using LNF.Billing;
using LNF.Billing.Process;
using LNF.CommonTools;
using LNF.Control;
using LNF.Data;
using LNF.Scheduler;
using Newtonsoft.Json;
using System;
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

            object result = null;

            int clientId;
            DateTime period;

            if (command == "update-billing")
            {
                clientId = Utility.ConvertTo(ctx.Request["ClientID"], 0);
                period = Convert.ToDateTime(ctx.Request["Period"]);

                var logs = Provider.Billing.Process.UpdateBilling(new UpdateBillingArgs
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
                DataResult dataResult = null;
                Step1Result step1Result = null;
                
                try
                {
                    clientId = Utility.ConvertTo(ctx.Request["ClientID"], 0);
                    period = Convert.ToDateTime(ctx.Request["Period"]);

                    // ToolDataClean is updated in IReservationRepository.SaveReservationHistory prior to this request.

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
                        IsTemp = Utility.IsCurrentPeriod(period)
                    });
                }
                catch (Exception ex)
                {
                    error = true;
                    msg = ex.Message;
                }

                result = new { Error = error, Message = msg, DataResult = dataResult, Step1Result = step1Result };
            }
            else if (int.TryParse(ctx.Request["ReservationID"], out int reservationId))
            {
                var rsv = Provider.Scheduler.Reservation.GetReservation(reservationId);

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

                        var model = new ReservationHistoryUpdate()
                        {
                            ReservationID = reservationId,
                            AccountID = accountId,
                            ChargeMultiplier = chargeMultiplier,
                            Notes = notes,
                            EmailClient = emailClient
                        };

                        bool updateResult = Provider.Scheduler.Reservation.UpdateReservationHistory(model);
                        string msg = updateResult ? "OK" : "An error occurred.";
                        result = new { Error = !updateResult, Message = msg };

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
                throw new Exception("Missing parameter: id");
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
                    util.Start(rsv, helper.GetReservationClientItem(rsv, client), context.CurrentUser(provider).ClientID);
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

        public static StartReservationItem CreateStartReservationItem(HttpContextBase context, IProvider provider, IReservation rsv, IClient client)
        {
            var now = DateTime.Now;
            var util = Reservations.Create(provider, now);

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
            var args = ReservationStateArgs.Create(reservationItem, helper.GetReservationClientItem(reservationItem), now);
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
