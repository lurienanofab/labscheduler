using LNF.Cache;
using LNF.Data;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Controllers
{
    public class ReservationController : IHttpHandler, IRequiresSessionState
    {
        [Inject] protected IProvider Provider { get; set; }

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            HttpContextBase ctx = new HttpContextWrapper(context);
            SchedulerContextHelper helper = GetContextHelper(ctx);

            string command = GetCommand(ctx);

            ViewType currentView = GetView(ctx);
            ctx.SetCurrentViewType(currentView);

            IClient currentUser = helper.CurrentUser();

            string redirectUrl;

            var util = SchedulerUtility.Create(Provider);

            helper.AppendLog($"ReservationController.ProcessRequest: url = {ctx.Request.Url}");

            try
            {
                ctx.Session.Remove("ErrorMessage");

                if (command == "ReservationAction")
                {
                    redirectUrl = GetReservationAction(ctx);
                }
                else
                {
                    IReservationItem rsv;
                    IResource res;

                    switch (command)
                    {
                        case "ChangeHourRange":
                            string range = ctx.Request.QueryString["Range"];

                            helper.AppendLog($"ReservationController.ProcessRequest: range = {range}");

                            if (range == "FullDay")
                                ctx.SetDisplayDefaultHours(false);
                            else
                                ctx.SetDisplayDefaultHours(true);

                            redirectUrl = util.GetReservationViewReturnUrl(currentView);
                            break;
                        case "NewReservation":
                            var canCreate = CanCreateNewReservation(ctx);

                            helper.AppendLog($"ReservationController.ProcessRequest: canCreate = {canCreate}");

                            if (canCreate)
                                redirectUrl = SchedulerUtility.GetReservationReturnUrl(ctx.Request.SelectedPath(), ctx.Request.SelectedLocationPath(), 0, ctx.Request.SelectedDate(), GetReservationTime(ctx), currentView);
                            else
                                redirectUrl = util.GetReservationViewReturnUrl(currentView);
                            break;
                        case "ModifyReservation":
                            rsv = helper.GetReservationWithInvitees();
                            res = helper.GetResourceTreeItemCollection().GetResource(rsv.ResourceID);
                            var currentDate = ctx.Request.SelectedDate();
                            var currentTime = GetReservationTime(ctx);
                            redirectUrl = SchedulerUtility.GetReservationReturnUrl(PathInfo.Create(res), ctx.Request.SelectedLocationPath(), rsv.ReservationID, currentDate, currentTime, currentView);
                            break;
                        case "DeleteReservation":
                            rsv = helper.GetReservationWithInvitees();
                            Reservations.Create(Provider, DateTime.Now).Delete(rsv, currentUser.ClientID);

                            redirectUrl = util.GetReservationViewReturnUrl(currentView);
                            break;
                        default:
                            throw new NotImplementedException($"Command not implemented: {command}");
                    }
                }
            }
            catch (Exception ex)
            {
                string errmsg = ex.Message;

                ctx.Session["ErrorMessage"] = errmsg;

                helper.AppendLog($"ReservationController.ProcessRequest: errmsg = {errmsg}");

                try
                {
                    redirectUrl = util.GetReservationViewReturnUrl(currentView);
                }
                catch
                {
                    redirectUrl = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(redirectUrl))
                redirectUrl = "~";

            helper.AppendLog($"ReservationController.ProcessRequest: redirectUrl = {redirectUrl}");

            ctx.Response.Redirect(redirectUrl);
        }

        private string GetCommand(HttpContextBase context)
        {
            if (string.IsNullOrEmpty(context.Request.QueryString["Command"]))
                throw new InvalidOperationException("Required parameter missing: Command");

            return context.Request.QueryString["Command"];
        }

        private ViewType GetView(HttpContextBase context)
        {
            if (string.IsNullOrEmpty(context.Request.QueryString["View"]))
                throw new InvalidOperationException("Required parameter missing: View");

            var result = (ViewType)Enum.Parse(typeof(ViewType), context.Request.QueryString["View"], true);

            return result;
        }

        private TimeSpan GetReservationTime(HttpContextBase context)
        {
            if (int.TryParse(context.Request.QueryString["Time"], out int result))
                return TimeSpan.FromMinutes(result);

            throw new Exception($"Missing required querystring parameter: Time [{context.Request.QueryString}]");
        }

        /// <summary>
        /// The requested state. May differ from the actual current reservation state so be sure to check and throw an appropriate error if necessary.
        /// </summary>
        private ReservationState GetReservationState(HttpContextBase context)
        {
            if (!string.IsNullOrEmpty(context.Request.QueryString["State"]))
                return (ReservationState)Enum.Parse(typeof(ReservationState), context.Request.QueryString["State"], true);

            throw new Exception("Missing required querystring parameter: State");
        }

        public string GetReservationAction(HttpContextBase context)
        {
            SchedulerContextHelper helper = GetContextHelper(context);

            context.Session.Remove("ActiveReservationMessage");
            context.Session.Remove("ShowStartConfirmationDialog");

            var now = DateTime.Now;
            var util = Reservations.Create(Provider, now);
            var requestedState = GetReservationState(context);
            var rsv = helper.GetReservation();
            var res = helper.GetResource(rsv.ResourceID);
            var client = helper.GetReservationClient(rsv);
            var args = ReservationStateArgs.Create(rsv, client, now);
            var state = ReservationStateUtility.Create(now).GetReservationState(args);
            var currentView = GetView(context);
            var currentUser = context.CurrentUser(Provider);

            bool confirm = false;
            int reservationId = 0;

            helper.AppendLog($"ReservationController.GetReservationAction: reservationId = {rsv.ReservationID}, requestedState = {requestedState}, state = {state}, currentView = {currentView}");

            switch (requestedState)
            {
                case ReservationState.StartOnly:
                case ReservationState.StartOrDelete:
                    // If there are previous unended reservations, then ask for confirmation
                    var endable = Provider.Scheduler.Reservation.SelectEndableReservations(rsv.ResourceID);

                    if (endable.Count() > 0)
                    {
                        var endableReservations = string.Join(",", endable.Select(x => x.ReservationID));
                        context.Session["ActiveReservationMessage"] = $"[Previous ReservationID: {endableReservations}, Current ReservationID: {rsv.ReservationID}]";
                        confirm = true;
                        reservationId = rsv.ReservationID;
                    }
                    else
                    {
                        util.Start(rsv, helper.GetReservationClient(rsv), currentUser.ClientID);
                    }
                    break;
                case ReservationState.Endable:
                    // End reservation
                    if (state == ReservationState.Endable)
                    {
                        util.End(rsv, DateTime.Now, currentUser.ClientID);
                    }
                    else
                    {
                        string actualBeginDateTime = rsv.ActualBeginDateTime.HasValue ? rsv.ActualBeginDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null";
                        throw new InvalidOperationException($"ReservationID {rsv.ReservationID} state is {state}, not Endable. ActualBeginDateTime: {actualBeginDateTime}");
                    }
                    break;
                case ReservationState.PastSelf:
                    if (currentView == ViewType.DayView || currentView == ViewType.WeekView)
                        context.SetWeekStartDate(rsv.BeginDateTime.Date);
                    return SchedulerUtility.GetReturnUrl("ReservationRunNotes.aspx", PathInfo.Create(res), context.Request.SelectedLocationPath(), rsv.ReservationID, context.Request.SelectedDate());
                case ReservationState.Other:
                case ReservationState.Invited:
                case ReservationState.PastOther:
                    return SchedulerUtility.GetReturnUrl("Contact.aspx", PathInfo.Create(res), context.Request.SelectedLocationPath(), rsv.ReservationID, context.Request.SelectedDate());
                default:
                    // invalid state detected!
                    // throw new NotImplementedException($"ReservationState = {state} is not implemented");
                    context.Session["ErrorMessage"] = $"The current reservation state, {state}, is invalid. No actions are defined for this state. [RequestedState = {requestedState}, ReservationID = {reservationId}]";

                    var vars = new Dictionary<string, object>
                    {
                        ["current state"] = state,
                        ["requested state"] = requestedState,
                        ["reservationId"] = reservationId,
                        ["tool_engineer"] = args.IsToolEngineer,
                        ["inlab"] = args.IsInLab,
                        ["reserver"] = args.IsReserver,
                        ["invited"] = args.IsInvited,
                        ["authorized"] = args.IsAuthorized,
                        ["before_mct"] = args.IsBeforeMinCancelTime(),
                        ["startable"] = args.IsStartable()
                    };

                    helper.SendDebugEmail("ReservationController.GetReservationAction", "Invalid state detected!", "Invalid state detected!", vars);

                    return SchedulerUtility.Create(Provider).GetReservationViewReturnUrl(currentView, false, reservationId);
            }

            string result = SchedulerUtility.Create(Provider).GetReservationViewReturnUrl(currentView, confirm, reservationId);

            return result;
        }

        private bool CanCreateNewReservation(HttpContextBase context)
        {
            var currentView = context.GetCurrentViewType();

            // copied from the old EmptyCell_Click event handler in ReservationView.ascx.vb

            if (currentView == ViewType.UserView)
                return false;

            DateTime date = context.Request.SelectedDate();
            TimeSpan time = GetReservationTime(context);

            // Check for past reservation date times
            if (date.Add(time) < DateTime.Now)
            {
                throw new Exception("You cannot create reservations in the past.");
            }

            // Check if user has accounts
            if (GetCurrentUserActiveClientAccountsCount(context) == 0)
            {
                throw new Exception("You do not have any accounts with which to make reservations.");
            }

            // The reservation fence cannot truly be checked until the activity type is selected
            // however, authorized users are always impacted by it/

            var pathInfo = context.Request.SelectedPath();
            IResource res = Provider.Scheduler.Resource.GetResource(pathInfo.ResourceID);
            IClient currentUser = context.CurrentUser(Provider);

            ClientAuthLevel authLevel = CacheManager.Current.GetAuthLevel(res.ResourceID, currentUser);

            if (authLevel < ClientAuthLevel.SuperUser)
            {
                if (DateTime.Now.AddMinutes(res.ReservFence) < date)
                {
                    throw new Exception("You are trying to make a reservation that is too far in the future.");
                }
            }

            if (currentView == ViewType.DayView || currentView == ViewType.WeekView)
            {
                context.SetWeekStartDate(GetWeekStartDate(context));
            }

            return true;
        }

        private int GetCurrentUserActiveClientAccountsCount(HttpContextBase context)
        {
            string un = context.User.Identity.Name;
            return Provider.Data.Client.GetActiveClientAccounts(un).Count();
        }

        private DateTime GetWeekStartDate(HttpContextBase context)
        {
            // This makes it so whenever the date is changed by clicking the calendar, the week start date
            // (the first column in the grid) is the selected date. Not sure if this is intended behavior or not.
            // An alternative would be to figure out the previous Sunday. For example:
            //      var dow = CacheManager.Current.CurrentUserState().Date.DayOfWeek;
            //      return CacheManager.Current.CurrentUserState().Date.AddDays(-(int)dow);

            return Convert.ToDateTime(context.Request.QueryString["Date"]);
        }

        private SchedulerContextHelper GetContextHelper(HttpContextBase context)
        {
            return new SchedulerContextHelper(context, Provider);
        }
    }
}
