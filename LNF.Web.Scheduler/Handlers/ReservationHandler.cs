using LNF.Billing;
using LNF.Billing.Process;
using LNF.CommonTools;
using LNF.Control;
using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Scheduler.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Handlers
{
    public class ReservationHandler : IHttpHandler, IReadOnlySessionState
    {
        [Inject] public IProvider Provider { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            HttpContextBase ctx = new HttpContextWrapper(context);

            ctx.Response.ContentType = "application/json";

            string command = ctx.Request["Command"];

            int clientId = Utility.ConvertTo(ctx.Request["ClientID"], 0);

            DateTime period;

            object result;

            if (command == "update-billing")
            {
                period = Convert.ToDateTime(ctx.Request["Period"]);

                Provider.Billing.Process.UpdateBilling(new UpdateBillingArgs
                {
                    BillingCategory = BillingCategory.Tool | BillingCategory.Room,
                    ClientID = clientId,
                    Periods = new[] { period }
                });

                result = new { Error = false, Message = "ok" };
            }
            else if (command == "reservation-history-billing-update")
            {
                bool error = false;
                string msg = "OK!";
                DataCleanResult dataCleanResult = null;
                DataResult dataResult = null;
                Step1Result step1Result = null;
                PopulateSubsidyBillingResult step4Result = null;

                bool isTemp;

                try
                {
                    period = Convert.ToDateTime(ctx.Request["Period"]);

                    isTemp = Utility.IsCurrentPeriod(period);

                    dataCleanResult = Provider.Billing.Process.DataClean(new DataCleanCommand
                    {
                        BillingCategory = BillingCategory.Tool | BillingCategory.Room,
                        ClientID = clientId,
                        StartDate = period,
                        EndDate = period.AddMonths(1)
                    });

                    dataResult = Provider.Billing.Process.Data(new DataCommand
                    {
                        BillingCategory = BillingCategory.Tool | BillingCategory.Room,
                        ClientID = clientId,
                        Period = period,
                        Record = 0
                    });

                    step1Result = Provider.Billing.Process.Step1(new Step1Command
                    {
                        BillingCategory = BillingCategory.Tool | BillingCategory.Room,
                        ClientID = clientId,
                        Period = period,
                        Record = 0,
                        Delete = true,
                        IsTemp = isTemp
                    });

                    // [2022-01-06 jg] Adding subsidy calculation, not sure why this wasn't already being done. Definitely need to recalculate in case account is changed from
                    //      internal to external or vice versa.

                    step4Result = Provider.Billing.Process.Step4(new Step4Command
                    {
                        ClientID = clientId,
                        Period = period,
                        Command = "subsidy"
                    });

                    // [2022-01-06 jg] This error check does not work for staff because repair reservations exist in ToolDataClean
                    //      but are not added to ToolData (and subsequently ToolBilling), so ToolDataClean rows loaded
                    //      can be greater than ToolData rows loaded. So now it will be skipped if the client is staff.

                    var c = Provider.Data.Client.GetClient(clientId);
                    if (!c.HasPriv(ClientPrivilege.Staff))
                    {
                        var toolDataRowsLoaded = dataResult.WriteToolDataProcessResult.RowsLoaded;
                        var toolDataCleanRowsLoaded = dataCleanResult.WriteToolDataCleanProcessResult.RowsLoaded;
                        var step1RowsLoaded = step1Result.PopulateToolBillingProcessResult.RowsLoaded;

                        string errmsg = string.Empty;

                        if (toolDataRowsLoaded < toolDataCleanRowsLoaded)
                            errmsg += $" Tool data row count ({toolDataRowsLoaded}) is less than tool data clean row count ({toolDataCleanRowsLoaded}).";

                        if (step1RowsLoaded < toolDataRowsLoaded)
                            errmsg += $" Step1 row count ({step1RowsLoaded}) is less than tool data row count ({toolDataRowsLoaded}).";

                        var errorCheck =
                            toolDataRowsLoaded >= toolDataCleanRowsLoaded
                            && step1RowsLoaded >= toolDataRowsLoaded;

                        if (!errorCheck)
                            throw new Exception($"A problem occurred when trying to update billing data. {errmsg} Period: {period:yyyy-MM-dd:HH:mm:ss}, ClientID: {clientId}, Rows Loaded: DataClean = {dataCleanResult.WriteToolDataCleanProcessResult.RowsLoaded}, Data = {dataResult.WriteToolDataProcessResult.RowsLoaded}, Step1 = {step1Result.PopulateToolBillingProcessResult.RowsLoaded}");
                    }
                }
                catch (Exception ex)
                {
                    error = true;
                    msg = ex.Message;
                    SendEmail.SendDebugEmail(clientId, "LNF.Web.Scheduler.Handlers.ReservationHandler.ProcessRequest", $"[DEBUG:{DateTime.Now:yyyy-MM-dd HH:mm:ss}] An error occurred while updating billing from the Reservation History page.", msg);
                }

                result = new { Error = error, Message = msg, DataCleanResult = dataCleanResult, DataResult = dataResult, Step1Result = step1Result, Step4Result = step4Result };
            }
            else if (command == "get-clients-for-reservation-history")
            {
                bool isok = clientId > 0;
                DateTime sd, ed;

                if (string.IsNullOrEmpty(ctx.Request.Form["StartDate"]))
                    sd = Reservations.MinReservationBeginDate;
                else
                    isok &= DateTime.TryParse(ctx.Request.Form["StartDate"], out sd);

                if (string.IsNullOrEmpty(ctx.Request.Form["EndDate"]))
                    ed = Reservations.MaxReservationEndDate;
                else
                    isok &= DateTime.TryParse(ctx.Request.Form["EndDate"], out ed);

                IEnumerable<ReservationHistoryClient> clients = null;

                if (isok)
                {
                    clients = ReservationHistoryUtility.Create(Provider).SelectReservationHistoryClients(sd, ed, clientId);
                    result = new { Error = false, Message = "OK", Clients = clients };
                }
                else
                {
                    result = new { Error = true, Message = "One or more parameters are invalid.", Clients = clients };
                }
            }
            else if (int.TryParse(ctx.Request["ReservationID"], out int reservationId))
            {
                switch (command)
                {
                    case "start-reservation":
                        clientId = ctx.CurrentUser(Provider).ClientID;
                        result = ReservationHandlerUtility.Start(ctx, Provider, reservationId, clientId);
                        break;
                    case "get-reservation":
                        result = ReservationHandlerUtility.GetReservation(ctx, Provider, reservationId);
                        break;
                    case "save-reservation-history":
                        string notes = ctx.Request["Notes"];
                        double forgivenPct = Utility.ConvertTo(ctx.Request["ForgivenPct"], 0D);
                        int accountId = Utility.ConvertTo(ctx.Request["AccountId"], 0);
                        bool emailClient = Utility.ConvertTo(ctx.Request["EmailClient"], false);

                        double chargeMultiplier = 1.00 - (forgivenPct / 100.0);

                        clientId = ctx.CurrentUser(Provider).ClientID;

                        var model = new ReservationHistoryUpdate()
                        {
                            ReservationID = reservationId,
                            AccountID = accountId,
                            ChargeMultiplier = chargeMultiplier,
                            Notes = notes,
                            EmailClient = emailClient,
                            ClientID = clientId
                        };

                        bool updateResult = Provider.Scheduler.Reservation.UpdateReservationHistory(model);
                        string msg = updateResult ? "OK" : "An error occurred.";
                        result = new
                        {
                            Error = !updateResult,
                            Message = msg
                        };

                        break;
                    case "test":
                        result = new { Error = false, Message = "ok" };
                        break;
                    case "":
                        throw new Exception("Missing parameter: command");
                    default:
                        throw new Exception("Invalid command: " + command);
                }
            }
            else
            {
                throw new Exception("Missing parameter: command");
            }

            ctx.Response.Write(JsonConvert.SerializeObject(result));
        }

        public bool IsReusable => false;
    }

    public static class ReservationHandlerUtility
    {
        public static object Start(HttpContextBase context, IProvider provider, int reservationId, int clientId)
        {
            try
            {
                var helper = new SchedulerContextHelper(context, provider);
                var util = Reservations.Create(provider, DateTime.Now);
                var rsv = provider.Scheduler.Reservation.GetReservationWithInvitees(reservationId);
                var client = provider.Data.Client.GetClient(clientId);

                if (rsv != null)
                {
                    util.Start(rsv, helper.GetReservationClient(rsv, client), context.CurrentUser(provider).ClientID);
                    return new { Error = false, Message = "OK" };
                }
                else
                {
                    return new { Error = true, Message = $"Cannot find record for ReservationID {reservationId}" };
                }
            }
            catch (Exception ex)
            {
                return new { Error = true, ex.Message };
            }
        }

        public static object GetReservation(HttpContextBase context, IProvider provider, int reservationId)
        {
            var rsv = provider.Scheduler.Reservation.GetReservation(reservationId);
            var client = context.CurrentUser(provider);

            if (rsv != null)
                return CreateStartReservationItem(context, provider, rsv, client);
            else
                return new { Error = true, Message = string.Format("Cannot find record for ReservationID {0}", reservationId) };
        }

        public static StartReservationItem CreateStartReservationItem(HttpContextBase context, IProvider provider, IReservationItem rsv, IClient client)
        {
            var now = DateTime.Now;

            var item = new StartReservationItem
            {
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.ResourceID,
                ResourceName = rsv.ResourceName,
                ReservedByClientID = rsv.ClientID,
                ReservedByClientName = string.Format("{0} {1}", rsv.FName, rsv.LName)
            };

            var currentUser = context.CurrentUser(provider);

            if (rsv.ClientIDBegin.HasValue)
            {
                if (rsv.ClientIDBegin.Value > 0)
                {
                    IClient startedBy = provider.Data.Client.GetClient(rsv.ClientIDBegin.Value);
                    item.StartedByClientID = startedBy.ClientID;
                    item.StartedByClientName = string.Format("{0} {1}", startedBy.FName, startedBy.LName);
                }
                else
                {
                    item.StartedByClientID = 0;
                    item.StartedByClientName = string.Empty;
                }
            }
            else
            {
                item.StartedByClientID = currentUser.ClientID;
                item.StartedByClientName = string.Format("{0} {1}", currentUser.FName, currentUser.LName);
            }

            var reservationItem = provider.Scheduler.Reservation.GetReservationWithInvitees(rsv.ReservationID);
            var helper = new SchedulerContextHelper(context, provider);
            var args = ReservationStateArgs.Create(reservationItem, helper.GetReservationClient(reservationItem), now);
            var stateUtil = ReservationStateUtility.Create(now);
            ReservationState state = stateUtil.GetReservationState(args);

            item.Startable = stateUtil.IsStartable(state);
            item.NotStartableMessage = GetNotStartableMessage(state);

            var inst = ActionInstances.Find(ActionType.Interlock, rsv.ResourceID);
            item.HasInterlock = inst != null;

            var res = provider.Scheduler.Resource.GetResource(rsv.ResourceID);
            item.ReturnUrl = GetResourceUrl(context, res);

            return item;
        }

        public static string GetNotStartableMessage(ReservationState state)
        {
            string result = @"<div class=""not-startable-message"">{0}</div>";
            switch (state)
            {
                case ReservationState.Undefined:
                    return string.Format(result, "Reservation is not startable at this time.");
                case ReservationState.Editable:
                case ReservationState.PastSelf:
                case ReservationState.Other:
                case ReservationState.Invited:
                case ReservationState.PastOther:
                    return string.Format(result, "Reservation has already ended.");
                case ReservationState.Endable:
                    return string.Format(result, "Reservation is already in progress.");
                case ReservationState.Repair:
                    return string.Format(result, "Resource offline for repair");
                case ReservationState.NotInLab:
                    return string.Format(result, "You must be in the lab to start the reservation");
                case ReservationState.UnAuthToStart:
                    return string.Format(result, "You are not authorized to start this reservation");
                case ReservationState.Meeting:
                    return string.Format(result, "Reservation takes place during regular meeting time.");
                default:
                    return string.Empty;
            }
        }

        public static string GetResourceUrl(HttpContextBase context, IResource res)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), context.Request.SelectedDate()));
        }
    }

    public class StartReservationItem
    {
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public int ReservedByClientID { get; set; }
        public string ReservedByClientName { get; set; }
        public int StartedByClientID { get; set; }
        public string StartedByClientName { get; set; }
        public bool Startable { get; set; }
        public string NotStartableMessage { get; set; }
        public bool HasInterlock { get; set; }
        public string ReturnUrl { get; set; }
    }
}
