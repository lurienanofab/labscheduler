using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.Models
{
    public class ReservationModel
    {
        private readonly HttpContextBase _context;
        private IEnumerable<IReservation> _overwriteReservations;

        public DateTime Now { get; }
        public IProvider Provider { get; }
        public IClient CurrentUser => _context.CurrentUser();
        public IReservation Reservation { get; }
        public IResource Resource { get; }

        public int ActivityID { get; set; }
        public int AccountID { get; set; }
        public int? RecurrenceID { get; set; }
        public string Notes { get; set; }
        public bool AutoEnd { get; set; }
        public bool KeepAlive { get; set; }
        public string ReservationProcessInfoJson { get; set; }
        public string DurationText { get; set; }
        public string DurationSelectedValue { get; set; }
        public string StartTimeHourSelectedValue { get; set; }
        public string StartTimeMinuteSelectedValue { get; set; }

        public ReservationModel(HttpContextBase context, IProvider provider, DateTime now)
        {
            _context = context;
            Provider = provider;
            Now = now;

            if (!string.IsNullOrEmpty(context.Request.QueryString["ReservationID"]))
            {
                if (int.TryParse(context.Request.QueryString["ReservationID"], out int reservationId))
                {
                    Reservation = provider.Scheduler.Reservation.GetReservation(reservationId);
                    ActivityID = Reservation.ActivityID;
                    AccountID = Reservation.AccountID;
                    RecurrenceID = Reservation.RecurrenceID;
                    Notes = Reservation.Notes;
                    AutoEnd = Reservation.AutoEnd;
                    KeepAlive = Reservation.KeepAlive;
                    Resource = provider.Scheduler.Resource.GetResource(Reservation.ResourceID);
                }
            }

            if (Resource == null)
                Resource = provider.Scheduler.Resource.GetResource(context.Request.SelectedPath().ResourceID);                
        }

        public IReservation CreateOrModifyReservation()
        {
            var rd = GetReservationDuration();
            return CreateOrModifyReservation(rd);
        }

        public IReservation CreateOrModifyReservation(ReservationDuration duration)
        {
            _context.Session.Remove("ReservationProcessInfoJsonData");

            if (IsThereAlreadyAnotherReservation(duration))
            {
                throw new Exception("Another reservation has already been made for this time.");
            }

            _context.Session["ReservationProcessInfoJsonData"] = ReservationProcessInfoJson;

            // this will be the result reservation - either a true new reservation when creating, a new reservation for modification, or the existing rsv when modifying non-duration data
            IReservation result = null;

            // Overwrite other reservations
            OverwriteReservations();

            var data = GetReservationData(duration);
            var util = GetReservationUtility();

            if (Reservation == null)
                result = util.Create(data);
            else
                result = util.Modify(Reservation, data);

            return result;
        }

        public bool IsThereAlreadyAnotherReservation(ReservationDuration rd)
        {
            // This is called twice: once after btnSubmit is clicked (before the confirmation message is shown) and again 
            // after btnConfirmYes is clicked (immediately before creating the reservation).

            // Check for other reservations made during this time
            // Select all reservations for this resource during the time of current reservation

            var authLevel = GetCurrentAuthLevel();

            var otherReservations = Provider.Scheduler.Reservation.SelectOverwritable(Resource.ResourceID, rd.BeginDateTime, rd.EndDateTime);

            if (otherReservations.Count() > 0)
            {
                if (!(otherReservations.Count() == 1 && otherReservations.ElementAt(0).ReservationID == Reservation.ReservationID))
                {
                    if (authLevel == ClientAuthLevel.ToolEngineer)
                        _overwriteReservations = otherReservations;
                    else
                        return true;
                }
            }

            return false;
        }

        public void OverwriteReservations()
        {
            if (_overwriteReservations != null)
            {
                foreach (var rsv in _overwriteReservations)
                {
                    // Delete Reservation
                    Provider.Scheduler.Reservation.CancelReservation(rsv.ReservationID, CurrentUser.ClientID);

                    // Send email to reserver informing them that their reservation has been canceled
                    Provider.EmailManager.EmailOnToolEngDelete(rsv, CurrentUser, CurrentUser.ClientID);
                }
            }
        }

        public ReservationData GetReservationData(ReservationDuration rd)
        {
            return new ReservationData(GetReservationProcessInfos(), GetReservationInvitees())
            {
                ClientID = CurrentUser.ClientID,
                ResourceID = Resource.ResourceID,
                ActivityID = ActivityID,
                AccountID = AccountID,
                RecurrenceID = RecurrenceID,
                Duration = rd,
                Notes = Notes,
                AutoEnd = AutoEnd,
                KeepAlive = KeepAlive
            };
        }

        public IEnumerable<IReservationInvitee> GetReservationInvitees()
        {
            if (_context.Session["ReservationInvitees"] == null)
            {
                var reservationId = Reservation == null ? 0 : Reservation.ReservationID;
                var query = DA.Current.Query<ReservationInviteeInfo>().Where(x => x.ReservationID == reservationId);
                _context.Session["ReservationInvitees"] = query.CreateModels<IReservationInvitee>().ToList();
            }

            var result = (List<IReservationInvitee>)_context.Session["ReservationInvitees"];

            return result;
        }

        public IEnumerable<IReservationProcessInfo> GetReservationProcessInfos()
        {
            if (_context.Session["ReservationProcessInfos"] == null)
            {
                var reservationId = Reservation == null ? 0 : Reservation.ReservationID;
                var query = DA.Current.Query<ReservationProcessInfo>().Where(x => x.Reservation.ReservationID == reservationId);
                _context.Session["ReservationProcessInfos"] = query.CreateModels<IReservationProcessInfo>().ToList();
            }

            var result = (List<IReservationProcessInfo>)_context.Session["ReservationProcessInfos"];

            return result;
        }

        public ReservationDuration GetReservationDuration()
        {
            var beginDateTime = _context.Request.SelectedDate().AddHours(Convert.ToInt32(StartTimeHourSelectedValue)).AddMinutes(Convert.ToInt32(StartTimeMinuteSelectedValue));
            var duration = GetCurrentDurationMinutes();
            var result = new ReservationDuration(beginDateTime, TimeSpan.FromMinutes(duration));
            return result;
        }

        public string GetRedirectUrl()
        {
            string redirectUrl;

            if (_context.Session["ReturnTo"] != null && !string.IsNullOrEmpty(_context.Session["ReturnTo"].ToString()))
                redirectUrl = _context.Session["ReturnTo"].ToString();
            else
            {
                ViewType view = _context.GetCurrentViewType();

                if (view == ViewType.UserView)
                    redirectUrl = $"~/UserReservations.aspx?Date={_context.Request.SelectedDate():yyyy-MM-dd}";
                else if (view == ViewType.ProcessTechView)
                {
                    // When we come from ProcessTech.aspx the full path is used (to avoid a null object error). When returning we just want the ProcessTech path.
                    ProcessTechItem pt = _context.GetCurrentProcessTech();
                    PathInfo path = PathInfo.Create(pt);
                    redirectUrl = $"~/ProcessTech.aspx?Path={path.UrlEncode()}&Date={_context.Request.SelectedDate():yyyy-MM-dd}";
                }
                else //ViewType.DayView OrElse Scheduler.ViewType.WeekView
                    redirectUrl = $"~/ResourceDayWeek.aspx?Path={_context.Request.SelectedPath().UrlEncode()}&Date={_context.Request.SelectedDate():yyyy-MM-dd}";
            }

            //_context.Response.Redirect(redirectUrl, false);
            return redirectUrl;
        }

        public int GetCurrentDurationMinutes()
        {
            if (GetDurationInputType() == DurationInputType.TextBox)
            {
                if (string.IsNullOrEmpty(DurationText.Trim()))
                    throw new Exception("Please enter the duration in minutes.");

                return Convert.ToInt32(DurationText.Trim());
            }
            else
            {
                if (string.IsNullOrEmpty(DurationSelectedValue))
                    return 0;
                else
                    return Convert.ToInt32(DurationSelectedValue);
            }
        }

        public DurationInputType GetDurationInputType()
        {
            //Right now, to have textbox, the authlevel must be included in NoMaxSchedAuth, and only only sched. maintenance and characterization

            var authLevel = GetCurrentAuthLevel();

            int[] textboxActivities = { 15, 18, 23 };

            var activity = GetCurrentActivity();

            if ((activity.NoMaxSchedAuth & (int)authLevel) > 0 && textboxActivities.Contains(activity.ActivityID))
                return DurationInputType.TextBox;
            else
                return DurationInputType.DropDown;
        }

        public ClientAuthLevel GetCurrentAuthLevel()
        {
            var resourceClients = _context.GetResourceClients(Resource.ResourceID);
            return ReservationUtility.GetAuthLevel(resourceClients, CurrentUser);
        }

        public IActivity GetCurrentActivity()
        {
            // always get from the select - even when modifying
            return CacheManager.Current.GetActivity(ActivityID);
        }

        public ReservationUtility GetReservationUtility()
        {
            return new ReservationUtility(Now, Provider);
        }
    }
}
