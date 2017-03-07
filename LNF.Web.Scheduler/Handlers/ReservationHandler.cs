using LNF.Cache;
using LNF.Control;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using OnlineServices.Api;
using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Handlers
{
    public class ReservationHandler : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            string command = context.Request["Command"];

            object result = null;

            int reservationId;
            int clientId;

            if (int.TryParse(context.Request["ReservationID"], out reservationId))
            {
                switch (command)
                {
                    case "start-reservation":
                        clientId = CacheManager.Current.CurrentUser.ClientID;
                        result = await ReservationManager.Start(reservationId, clientId);
                        break;
                    case "get-reservation":
                        result = ReservationManager.GetReservation(context, reservationId);
                        break;
                    case "save-reservation-history":
                        string notes = context.Request["Notes"];
                        double forgivenPct = RepositoryUtility.ConvertTo(context.Request["ForgivenPct"], 0D);
                        int accountId = RepositoryUtility.ConvertTo(context.Request["AccountId"], 0);
                        bool emailClient = RepositoryUtility.ConvertTo(context.Request["EmailClient"], false);

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

                            bool updateResult = await sc.UpdateHistory(model);
                            string msg = updateResult ? "OK" : "An error occurred.";
                            result = new { Error = !updateResult, Message = msg };
                        }

                        break;
                    case "update-billing":
                        clientId = RepositoryUtility.ConvertTo(context.Request["ClientID"], 0);
                        DateTime sd = Convert.ToDateTime(context.Request["StartDate"]);
                        DateTime ed = Convert.ToDateTime(context.Request["EndDate"]);

                        var updateBillingResult = await ReservationHistoryUtility.UpdateBilling(sd, ed, clientId);
                        result = new { Error = updateBillingResult.HasError(), Message = updateBillingResult.GetErrorMessage() };
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

            context.Response.Write(Providers.Serialization.Json.SerializeObject(result));
        }

        private OnlineServices.Api.ApiClientOptions GetClientOptions(HttpContext context)
        {
            var cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            string authToken = cookie.Value;

            return new OnlineServices.Api.ApiClientOptions()
            {
                AccessToken = authToken,
                Host = new Uri(ConfigurationManager.AppSettings["ApiHost"]),
                TokenType = "Forms"
            };
        }
    }

    public static class ReservationManager
    {
        public static async Task<object> Start(int reservationId, int clientId)
        {
            try
            {
                var rsv = DA.Scheduler.Reservation.Single(reservationId);
                if (rsv != null)
                {
                    await ReservationUtility.StartReservation(rsv, clientId);
                    return new { Error = false, Message = "OK" };
                }
                else
                {
                    return new { Error = true, Message = string.Format("Cannot find record for ReservationID {0}", reservationId) };
                }
            }
            catch (Exception ex)
            {
                return new { Error = true, Message = ex.Message };
            }
        }

        public static object GetReservation(HttpContext context, int reservationId)
        {
            var rsv = DA.Current.Single<Reservation>(reservationId);
            if (rsv != null)
                return CreateStartReservationItem(context, rsv);
            else
                return new { Error = true, Message = string.Format("Cannot find record for ReservationID {0}", reservationId) };
        }

        public static StartReservationItem CreateStartReservationItem(HttpContext context, Reservation rsv)
        {
            StartReservationItem item = new StartReservationItem();
            item.ReservationID = rsv.ReservationID;
            item.ResourceID = rsv.Resource.ResourceID;
            item.ResourceName = rsv.Resource.ResourceName;
            item.ReservedByClientID = rsv.Client.ClientID;
            item.ReservedByClientName = string.Format("{0} {1}", rsv.Client.FName, rsv.Client.LName);

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
                item.StartedByClientID = CacheManager.Current.CurrentUser.ClientID;
                item.StartedByClientName = string.Format("{0} {1}", CacheManager.Current.CurrentUser.FName, CacheManager.Current.CurrentUser.LName);
            }

            ReservationState state = ReservationUtility.GetReservationState(rsv.ReservationID, CacheManager.Current.CurrentUser.ClientID);
            item.Startable = ReservationUtility.IsStartable(state);
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
