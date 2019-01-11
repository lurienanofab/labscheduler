using LNF.CommonTools;
using LNF.Control;
using LNF.Models.Scheduler;
using LNF.Models.Worker;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using OnlineServices.Api.Scheduler;
using System;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Handlers
{
    public class ReservationHandler : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            string command = context.Request["Command"];

            object result = null;

            int clientId;

            if (int.TryParse(context.Request["ReservationID"], out int reservationId))
            {
                var rsv = DA.Current.Single<Reservation>(reservationId);

                switch (command)
                {
                    case "start-reservation":
                        clientId = context.CurrentUser().ClientID;
                        result = ReservationHandlerUtility.Start(reservationId, clientId, context.Request.UserHostAddress);
                        break;
                    case "get-reservation":
                        result = ReservationHandlerUtility.GetReservation(context, reservationId);
                        break;
                    case "save-reservation-history":
                        string notes = context.Request["Notes"];
                        double forgivenPct = Utility.ConvertTo(context.Request["ForgivenPct"], 0D);
                        int accountId = Utility.ConvertTo(context.Request["AccountId"], 0);
                        bool emailClient = Utility.ConvertTo(context.Request["EmailClient"], false);

                        double chargeMultiplier = 1.00 - (forgivenPct / 100.0);

                        var sc = new SchedulerClient();

                        var model = new ReservationHistoryUpdate()
                        {
                            ReservationID = reservationId,
                            AccountID = accountId,
                            ChargeMultiplier = chargeMultiplier,
                            Notes = notes,
                            EmailClient = emailClient
                        };

                        bool updateResult = sc.UpdateReservationHistory(model);
                        string msg = updateResult ? "OK" : "An error occurred.";
                        result = new { Error = !updateResult, Message = msg };

                        break;
                    case "update-billing":
                        DateTime sd = Convert.ToDateTime(context.Request["StartDate"]);
                        DateTime ed = Convert.ToDateTime(context.Request["EndDate"]);
                        clientId = Utility.ConvertTo(context.Request["ClientID"], 0);
                        ServiceProvider.Current.Worker.Execute(new UpdateBillingWorkerRequest(sd, clientId, new[] { "tool", "room" }));
                        result = new { Error = false, Message = "ok" };
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

            context.Response.Write(ServiceProvider.Current.Serialization.Json.SerializeObject(result));
        }

        public bool IsReusable => false;
    }

    public static class ReservationHandlerUtility
    {
        public static IReservationManager ReservationManager => ServiceProvider.Current.Use<IReservationManager>();

        public static object Start(int reservationId, int clientId, string kioskIp)
        {
            try
            {
                var rsv = DA.Current.Single<Reservation>(reservationId);
                var client = DA.Current.Single<Client>(clientId);

                if (rsv != null)
                {
                    ReservationManager.StartReservation(rsv, client, kioskIp);
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

        public static object GetReservation(HttpContext context, int reservationId)
        {
            var rsv = DA.Current.Single<Reservation>(reservationId);
            var client = DA.Current.Single<Client>(context.CurrentUser().ClientID);

            if (rsv != null)
                return CreateStartReservationItem(context, rsv, client);
            else
                return new { Error = true, Message = string.Format("Cannot find record for ReservationID {0}", reservationId) };
        }

        public static StartReservationItem CreateStartReservationItem(HttpContext context, Reservation rsv, Client client)
        {
            var item = new StartReservationItem
            {
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.Resource.ResourceID,
                ResourceName = rsv.Resource.ResourceName,
                ReservedByClientID = rsv.Client.ClientID,
                ReservedByClientName = string.Format("{0} {1}", rsv.Client.FName, rsv.Client.LName)
            };

            if (rsv.ClientIDBegin.HasValue)
            {
                if (rsv.ClientIDBegin.Value > 0)
                {
                    Client startedBy = DA.Current.Single<Client>(rsv.ClientIDBegin.Value);
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
                item.StartedByClientID = context.CurrentUser().ClientID;
                item.StartedByClientName = string.Format("{0} {1}", context.CurrentUser().FName, context.CurrentUser().LName);
            }

            var args = ReservationManager.CreateReservationStateArgs(rsv, client, context.Request.UserHostAddress);
            ReservationState state = ReservationManager.GetReservationState(args);
            item.Startable = ReservationManager.IsStartable(state);
            item.NotStartableMessage = GetNotStartableMessage(state);

            var inst = ActionInstanceUtility.Find(ActionType.Interlock, rsv.Resource.ResourceID);
            item.HasInterlock = inst != null;

            item.ReturnUrl = GetResourceUrl(context, rsv.Resource);

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

        public static string GetResourceUrl(HttpContext context, Resource res)
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
