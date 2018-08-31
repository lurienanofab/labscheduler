using LNF.Cache;
using LNF.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Controllers
{
    public class ReservationController : HttpTaskAsyncHandler, IRequiresSessionState
    {
        protected IReservationManager ReservationManager => DA.Use<IReservationManager>();

        private string GetCommand(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Request.QueryString["Command"]))
                throw new InvalidOperationException("Required parameter missing: Command");

            return context.Request.QueryString["Command"];
        }

        private ReservationState GetReservationState(HttpContext context)
        {
            ReservationState state = ReservationState.Undefined;

            if (!string.IsNullOrEmpty(context.Request.QueryString["State"]))
                state = (ReservationState)Enum.Parse(typeof(ReservationState), context.Request.QueryString["State"], true);

            return state;
        }

        private int GetReservationID(HttpContext context)
        {
            if (!string.IsNullOrEmpty(context.Request.QueryString["ReservationID"]))
            {
                if (int.TryParse(context.Request.QueryString["ReservationID"], out int reservationId))
                {
                    return reservationId;
                }
            }

            return 0;
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            string command = GetCommand(context);

            string redirectUrl;

            var currentView = CacheManager.Current.CurrentViewType();

            try
            {
                context.Session.Remove("ErrorMessage");

                if (command == "ReservationAction")
                {
                    redirectUrl = await ReservationAction(context);
                }
                else
                {
                    Reservation rsv;

                    switch (command)
                    {
                        case "ChangeHourRange":
                            string range = context.Request.QueryString["Range"];

                            if (range == "FullDay")
                                CacheManager.Current.DisplayDefaultHours(false);
                            else
                                CacheManager.Current.DisplayDefaultHours(true);

                            redirectUrl = SchedulerUtility.GetReservationViewReturnUrl(currentView);
                            break;
                        case "NewReservation":
                            if (CanCreateNewReservation(context))
                                redirectUrl = SchedulerUtility.GetReturnUrl("Reservation.aspx", context.Request.SelectedPath(), 0, context.Request.SelectedDate());
                            else
                                redirectUrl = SchedulerUtility.GetReservationViewReturnUrl(currentView);
                            break;
                        case "ModifyReservation":
                            rsv = GetReservation(context);
                            var res = CacheManager.Current.ResourceTree().GetResource(rsv.Resource.ResourceID);
                            DateTime currentDate = context.Request.SelectedDate();

                            context.Session["ReservationSelectedTime"] = currentDate;

                            redirectUrl = SchedulerUtility.GetReturnUrl("Reservation.aspx", PathInfo.Create(res), rsv.ReservationID, currentDate);
                            break;
                        case "DeleteReservation":
                            rsv = GetReservation(context);
                            ReservationManager.DeleteReservation(rsv.ReservationID);

                            redirectUrl = SchedulerUtility.GetReservationViewReturnUrl(currentView);
                            break;
                        default:
                            throw new NotImplementedException(string.Format("Command not implemented: {0}", command));
                    }
                }
            }
            catch (Exception ex)
            {
                context.Session["ErrorMessage"] = ex.Message;

                try
                {
                    redirectUrl = SchedulerUtility.GetReservationViewReturnUrl(currentView);
                }
                catch
                {
                    redirectUrl = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(redirectUrl))
                context.Response.Redirect(redirectUrl);
            else
                context.Response.Redirect("~");
        }

        private Reservation GetReservation(HttpContext context)
        {
            if (int.TryParse(context.Request.QueryString["ReservationID"], out int reservationId))
            {
                var result = DA.Current.Single<Reservation>(reservationId);

                if (result == null)
                    throw new InvalidOperationException(string.Format("Cannot find a Reservation with ReservationID = {0}", reservationId));

                return result;
            }
            else
                throw new InvalidOperationException("Missing query string parameter: reservationId");
        }

        private async Task<string> ReservationAction(HttpContext context)
        {
            context.Session.Remove("ActiveReservationMessage");
            context.Session.Remove("ShowStartConfirmationDialog");

            Reservation rsv = GetReservation(context);
            ReservationState state = GetReservationState(context);

            bool confirm = false;
            int reservationId = 0;

            var currentView = CacheManager.Current.CurrentViewType();

            switch (state)
            {
                case ReservationState.StartOnly:
                case ReservationState.StartOrDelete:
                    // If there are previous unended reservations, then ask for confirmation
                    IList<Reservation> endable = ReservationManager.SelectEndableReservations(rsv.Resource.ResourceID);

                    if (endable.Count > 0)
                    {
                        context.Session["ActiveReservationMessage"] = string.Format(
                            "[Previous ReservationID: {0}, Current ReservationID: {1}]",
                            string.Join(",", endable.Select(x => x.ReservationID)),
                            rsv.ReservationID);

                        confirm = true;
                        reservationId = rsv.ReservationID;
                    }
                    else
                    {
                        var isInLab = CacheManager.Current.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID);
                        await ReservationManager.StartReservation(rsv, CacheManager.Current.CurrentUser.ClientID, isInLab);
                    }
                    break;
                case ReservationState.Endable:
                    // End reservation
                    if (state == ReservationState.Endable)
                    { 
                        await ReservationManager.EndReservation(rsv.ReservationID);
                    }
                    else
                        throw new InvalidOperationException(string.Format("ReservationID {0} state is {1}, not Endable. ActualBeginDateTime: {2}", rsv.ReservationID, state, rsv.ActualBeginDateTime.HasValue ? rsv.ActualBeginDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null"));
                    break;
                case ReservationState.PastSelf:
                    if (currentView == ViewType.DayView || currentView == ViewType.WeekView)
                        CacheManager.Current.WeekStartDate(rsv.BeginDateTime.Date);
                    return SchedulerUtility.GetReturnUrl("ReservationRunNotes.aspx", context.Request.SelectedPath(), rsv.ReservationID, context.Request.SelectedDate());
                case ReservationState.Other:
                case ReservationState.Invited:
                case ReservationState.PastOther:
                    return SchedulerUtility.GetReturnUrl("Contact.aspx", context.Request.SelectedPath(), rsv.ReservationID, context.Request.SelectedDate());
                default:
                    throw new NotImplementedException(string.Format("ReservationState = {0} is not implemented", state));
            }

            string result = SchedulerUtility.GetReservationViewReturnUrl(currentView, confirm, reservationId);

            return result;
        }

        private bool CanCreateNewReservation(HttpContext context)
        {
            var currentView = CacheManager.Current.CurrentViewType();

            // copied from the old EmptyCell_Click event handler in ReservationView.ascx.vb

            if (currentView == ViewType.UserView)
                return false;

            DateTime date = context.Request.SelectedDate();

            // Check for past reservation date times
            if (date < DateTime.Now)
            {
                throw new Exception("You cannot create reservations in the past.");
            }

            // Check if user has accounts
            if (GetCurrentUserActiveClientAccountsCount() == 0)
            {
                throw new Exception("You do not have any accounts with which to make reservations.");
            }

            // The reservation fence cannot truly be checked until the activity type is selected
            // however, authorized users are always impacted by it/

            var res = CacheManager.Current.ResourceTree().GetResource(context.Request.SelectedPath().ResourceID).GetResourceItem();

            ClientAuthLevel authLevel = GetAuthorization(res);

            if (authLevel < ClientAuthLevel.SuperUser)
            {
                if (DateTime.Now.Add(res.ReservFence) < date)
                {
                    throw new Exception("You're trying to make a reservation that's too far in the future.");
                }
            }

            if (currentView == ViewType.DayView || currentView == ViewType.WeekView)
            {
                //if (date.Date != userState.Date)
                //{
                //    userState.SetDate(date.Date);
                //    userState.AddAction("Changed Date to {0:yyyy-MM-dd} because QueryString date and UserState date do not match", date.Date);
                //}

                CacheManager.Current.WeekStartDate(GetWeekStartDate(context));
            }

            context.Session["ReservationSelectedTime"] = date;

            return true;
        }

        private int GetCurrentUserActiveClientAccountsCount()
        {
            return CacheManager.Current.GetCurrentUserClientAccounts().Count();
        }

        private ClientAuthLevel GetAuthorization(ResourceItem res)
        {
            ClientAuthLevel result = CacheManager.Current.GetAuthLevel(res.ResourceID, CacheManager.Current.CurrentUser.ClientID);
            return result;
        }

        private DateTime GetWeekStartDate(HttpContext context)
        {
            // This makes it so whenever the date is changed by clicking the calendar, the week start date
            // (the first column in the grid) is the selected date. Not sure if this is intended behavior or not.
            // An alternative would be to figure out the previous Sunday. For example:
            //      var dow = CacheManager.Current.CurrentUserState().Date.DayOfWeek;
            //      return CacheManager.Current.CurrentUserState().Date.AddDays(-(int)dow);

            return Convert.ToDateTime(context.Request.QueryString["Date"]);
        }
    }
}
