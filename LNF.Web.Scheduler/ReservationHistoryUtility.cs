using LNF.CommonTools;
using LNF.Data;
using LNF.Models.Billing;
using LNF.Models.Billing.Process;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using OnlineServices.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace LNF.Web.Scheduler
{
    public static class ReservationHistoryUtility
    {
        public static IList<ReservationHistoryItem> GetReservationHistoryData(ClientModel client, DateTime? sd, DateTime? ed, bool includeCanceledForModification)
        {
            // Select Past Reservations
            IList<Reservation> reservations = ReservationUtility.SelectHistory(client.ClientID, sd.GetValueOrDefault(Reservation.MinReservationBeginDate), ed.GetValueOrDefault(Reservation.MaxReservationEndDate));
            IList<ReservationHistoryFilterItem> filtered = ReservationUtility.FilterCancelledReservations(reservations, includeCanceledForModification);
            var result = ReservationHistoryItem.CreateList(filtered);
            return result;
        }

        public static bool ReservationCanBeForgiven(ClientModel client, Reservation rsv, DateTime now)
        {
            // first, only admins and staff can possibly forgive
            if (!client.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Staff | ClientPrivilege.Developer))
                return false;

            // always let admins forgive even after business day cutoff
            if (client.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return true;

            return IsBeforeForgiveCutoff(rsv, now);
        }

        public static bool ReservationAccountCanBeChanged(ClientModel client, Reservation rsv, DateTime now)
        {
            // admins can always change the account
            if (client.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return true;

            // reserver and staff can before 3rd business day of next period
            if (client.HasPriv(ClientPrivilege.Staff) || client.ClientID == rsv.Client.ClientID)
                return IsBeforeChangeAccountCutoff(rsv, now);

            return false;
        }

        public static bool ReservationNotesCanBeChanged(ClientModel client, Reservation rsv)
        {
            // staff can always change notes
            if (client.HasPriv(ClientPrivilege.Staff | ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return true;

            // otherwise notes can be changed by the reserver
            return client.ClientID == rsv.Client.ClientID;
        }

        public static bool IsBeforeForgiveCutoff(Reservation rsv, DateTime now)
        {
            // Normal lab users cannot forgive reservations
            // Staff can forgive a reservation on the same day it ended, and during the following 3 three business days
            // Admins can always forgive reservations

            DateTime maxDay = rsv.ActualBeginDateTime.GetValueOrDefault(rsv.EndDateTime);
            int maxForgivenDay = int.Parse(ConfigurationManager.AppSettings["MaxForgivenDay"]);

            // start checking at the beginning of the next day
            DateTime cutoff = Utility.NextBusinessDay(maxDay.Date.AddDays(1), maxForgivenDay);

            // cutoff has now been incremented to the cutoff date - i.e. 3 (for example) business days after the reservation ended

            // Display Forgive Charge button 
            // extra if clause added by gpr, 1/3/05
            // 2011-07-11 Setting the cell visibility wasn't working, it's better to set the LinkButton control visibility.
            //       Also using cell index was getting messy because if a column is added all hell breaks loose. It's better
            //       to use FindControl on the whole row
            return now >= maxDay && now < cutoff && rsv.Activity.Editable;
        }

        public static bool IsBeforeChangeAccountCutoff(Reservation rsv, DateTime now)
        {
            // Normal lab users can modify their own reservation's account on the same day it ended through 3 business days after the 1st of the following month
            // Staff can modify any reservation's account on the same day it ended through 3 business days after the 1st of the following month
            // Admins can always modify a reservation's account

            DateTime maxDay = rsv.ActualBeginDateTime.GetValueOrDefault(rsv.EndDateTime);

            // need to see if current date is between maxDay and next period business day cutoff
            DateTime cutoff = Utility.NextBusinessDay(maxDay.AddMonths(1).FirstOfMonth());

            return now >= maxDay && now < cutoff && rsv.Activity.Editable;
        }

        public static DateTime GetStartDate(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Reservation.MinReservationBeginDate;

            DateTime d;
            if (DateTime.TryParse(input, out d))
                return d.Date;
            else
                return DateTime.Now.Date;
        }

        public static DateTime GetEndDate(string input)
        {
            if (string.IsNullOrEmpty(input))
                return Reservation.MaxReservationEndDate;

            DateTime d;
            if (DateTime.TryParse(input, out d))
                return d.Date.AddDays(1);
            else
                return DateTime.Now.Date.AddDays(1);
        }

        public static async Task<bool> SaveReservationHistory(int reservationId, int accountId, double forgivenPct, string notes, bool emailClient)
        {
            double chargeMultiplier = 1.00 - (forgivenPct / 100.0);

            using (var sc = await ApiProvider.NewSchedulerClient())
            {
                var model = new ReservationHistoryUpdate()
                {
                    ReservationID = reservationId,
                    AccountID = accountId,
                    ChargeMultiplier = chargeMultiplier,
                    Notes = notes,
                    EmailClient = emailClient
                };

                bool result = await sc.UpdateHistory(model);
                //string msg = updateResult ? "OK" : "An error occurred.";
                //var result = new { Error = !updateResult, Message = msg };
                return result;
            }
        }

        public static async Task<UpdateBillingResult> UpdateBilling(DateTime sd, DateTime ed, int clientId)
        {
            bool isTemp = sd == DateTime.Now.FirstOfMonth();

            using (var bc = await ApiProvider.NewBillingClient())
            {
                BillingProcessResult toolDataCleanResult = null;
                BillingProcessResult toolDataResult = null;
                BillingProcessResult toolStep1Result = null;
                BillingProcessResult roomDataCleanResult = null;
                BillingProcessResult roomDataResult = null;
                BillingProcessResult roomStep1Result = null;
                BillingProcessResult subsidyResult = null;

                // Tool
                toolDataCleanResult = await bc.BillingProcessDataClean(BillingCategory.Tool, sd, ed, clientId, 0);
                toolDataResult = await bc.BillingProcessData(BillingCategory.Tool, sd, ed, clientId, 0);
                toolStep1Result = await bc.BillingProcessStep1(BillingCategory.Tool, sd, ed, clientId, 0, isTemp, true);

                // Room
                roomDataCleanResult = await bc.BillingProcessDataClean(BillingCategory.Room, sd, ed, clientId, 0);
                roomDataResult = await bc.BillingProcessData(BillingCategory.Room, sd, ed, clientId, 0);
                roomStep1Result = await bc.BillingProcessStep1(BillingCategory.Room, sd, ed, clientId, 0, isTemp, true);

                // Subsidy
                if (!isTemp)
                    subsidyResult = await bc.BillingProcessStep4("subsidy", sd, clientId);

                var result = new UpdateBillingResult(toolDataCleanResult, toolDataResult, toolStep1Result, roomDataCleanResult, roomDataResult, roomStep1Result, subsidyResult);

                return result;
            }
        }

        public struct UpdateBillingResult
        {
            public UpdateBillingResult(BillingProcessResult toolDataClean, BillingProcessResult toolData, BillingProcessResult toolStep1, BillingProcessResult roomDataClean, BillingProcessResult roomData, BillingProcessResult roomStep1, BillingProcessResult subsidy)
            {
                ToolDataClean = toolDataClean;
                ToolData = toolData;
                ToolStep1 = toolStep1;
                RoomDataClean = roomDataClean;
                RoomData = roomData;
                RoomStep1 = roomStep1;
                Subsidy = subsidy;
            }

            public readonly BillingProcessResult ToolDataClean;
            public readonly BillingProcessResult ToolData;
            public readonly BillingProcessResult ToolStep1;
            public readonly BillingProcessResult RoomDataClean;
            public readonly BillingProcessResult RoomData;
            public readonly BillingProcessResult RoomStep1;
            public readonly BillingProcessResult Subsidy;

            public bool HasError()
            {
                bool result = !ToolDataClean.Success
                     || !ToolData.Success
                     || !ToolStep1.Success
                     || !RoomDataClean.Success
                     || !RoomData.Success
                     || !RoomStep1.Success;

                if (Subsidy != null)
                    result = result || !Subsidy.Success;

                return result;
            }

            public TimeSpan TotalTimeTaken()
            {
                double totalSeconds = ToolDataClean.TimeTaken
                    + ToolData.TimeTaken
                    + ToolStep1.TimeTaken
                    + RoomDataClean.TimeTaken
                    + RoomData.TimeTaken
                    + RoomStep1.TimeTaken;

                if (Subsidy != null)
                    totalSeconds += Subsidy.TimeTaken;

                TimeSpan result = TimeSpan.FromSeconds(totalSeconds);

                return result;
            }

            public string GetErrorMessage()
            {
                string result = "OK";

                List<string> errors = new List<string>();

                if (!ToolDataClean.Success)
                    errors.Add(ToolDataClean.ErrorMessage);

                if (!ToolData.Success)
                    errors.Add(ToolData.ErrorMessage);

                if (!ToolStep1.Success)
                    errors.Add(ToolStep1.ErrorMessage);

                if (!RoomDataClean.Success)
                    errors.Add(RoomDataClean.ErrorMessage);

                if (!RoomData.Success)
                    errors.Add(RoomData.ErrorMessage);

                if (!RoomStep1.Success)
                    errors.Add(RoomStep1.ErrorMessage);

                if (Subsidy != null)
                {
                    if (!Subsidy.Success)
                        errors.Add(Subsidy.ErrorMessage);
                }

                if (errors.Count > 0)
                    result = string.Join(", ", errors);

                return result;
            }
        }
    }
}
