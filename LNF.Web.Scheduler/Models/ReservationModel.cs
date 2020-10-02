﻿using LNF.Cache;
using LNF.Data;
using LNF.Impl;
using LNF.Impl.Repository.Scheduler;
using LNF.Repository;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.Models
{
    public class ReservationModel
    {
        private IEnumerable<IReservation> _overwriteReservations;

        public DateTime Now { get; }
        public SchedulerContextHelper Helper { get; }
        public IClient CurrentUser => Helper.CurrentUser();
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

        public ReservationModel(SchedulerContextHelper helper, DateTime now)
        {
            Helper = helper;
            Now = now;

            if (!string.IsNullOrEmpty(helper.Context.Request.QueryString["ReservationID"]))
            {
                if (int.TryParse(helper.Context.Request.QueryString["ReservationID"], out int reservationId))
                {
                    var rsv = helper.Provider.Scheduler.Reservation.GetReservation(reservationId);
                    var res = helper.Provider.Scheduler.Resource.GetResource(rsv.ResourceID);

                    Reservation = rsv;
                    ActivityID = rsv.ActivityID;
                    AccountID = rsv.AccountID;
                    RecurrenceID = rsv.RecurrenceID;
                    Notes = rsv.Notes;
                    AutoEnd = rsv.AutoEnd;
                    KeepAlive = rsv.KeepAlive;
                    Resource = res;
                }
            }

            if (Resource == null)
            {
                var resourceId = helper.Context.Request.SelectedPath().ResourceID;
                Resource = helper.Provider.Scheduler.Resource.GetResource(resourceId);
            }
        }

        public IReservation CreateOrModifyReservation()
        {
            var rd = GetReservationDuration();
            return CreateOrModifyReservation(rd);
        }

        public IReservation CreateOrModifyReservation(ReservationDuration duration)
        {
            var isCreating = IsCreating();

            Helper.AppendLog($"ReservationModel.CreateOrModifyReservation: isCreating = {isCreating}");

            Helper.Context.Session.Remove("ReservationProcessInfoJsonData");

            if (IsThereAlreadyAnotherReservation(duration, out string alert))
            {
                throw new Exception(alert);
            }

            Helper.Context.Session["ReservationProcessInfoJsonData"] = ReservationProcessInfoJson;

            // this will be the result reservation - either a true new reservation when creating, a new reservation for modification, or the existing rsv when modifying non-duration data
            IReservation result = null;

            // Overwrite other reservations
            OverwriteReservations();

            var data = GetReservationData(duration);
            var util = Reservations.Create(Helper.Provider, Now);

            if (isCreating)
                result = util.CreateReservation(data);
            else
                result = util.ModifyReservation(Reservation, data);

            // Remove session data to avoid accidently reusing on another reservation. It also gets removed when Reservation.aspx loads
            // but there have been cases where ReservationProcessInfo from one reservation mysteriously shows up on another reservation.
            // Note that the session data is retrieved when GetReservationData is called above.
            Helper.Context.Session.Remove("ReservationProcessInfos");
            Helper.Context.Session.Remove("ReservationInvitees");
            Helper.Context.Session.Remove("AvailableInvitees");

            Helper.AppendLog($"ReservationModel.CreateOrModifyReservation: result.ReservationID = {result.ReservationID}");

            return result;
        }

        public bool IsThereAlreadyAnotherReservation(ReservationDuration rd, out string alert)
        {
            // This is called twice: once after btnSubmit is clicked (before the confirmation message is shown) and again 
            // after btnConfirmYes is clicked (immediately before creating the reservation).

            // Check for other reservations made during this time
            // Select all reservations for this resource during the time of current reservation

            if (Helper.Provider == null)
                throw new Exception("Provider is null.");

            if (Resource == null)
                throw new Exception("Resource is null.");

            int reservationId = 0;

            if (Reservation != null)
                reservationId = Reservation.ReservationID;

            Helper.AppendLog($"ReservationModel.IsThereAlreadyAnotherReservation: Reservation is {(Reservation == null ? "null" : "not null")}, reservationId = {reservationId}");

            var authLevel = GetCurrentAuthLevel();

            var otherReservations = Helper.Provider.Scheduler.Reservation.SelectOverwritable(Resource.ResourceID, rd.BeginDateTime, rd.EndDateTime) ?? new List<IReservation>();

            var count = otherReservations.Count();

            Helper.AppendLog($"ReservationModel.IsThereAlreadyAnotherReservation: count = {count}");

            if (count > 0)
            {
                bool isSameReservation;

                if (count == 1)
                {
                    var other = otherReservations.First();
                    isSameReservation = other.ReservationID == reservationId;
                }
                else
                {
                    isSameReservation = false;
                }

                Helper.AppendLog($"ReservationModel.IsThereAlreadyAnotherReservation: isSameReservation = {isSameReservation}, authLevel = {authLevel}");

                if (!isSameReservation)
                {
                    if (authLevel == ClientAuthLevel.ToolEngineer)
                    {
                        _overwriteReservations = otherReservations;
                    }
                    else
                    {
                        int[] ids = otherReservations.Select(x => x.ReservationID).ToArray();
                        string idList = string.Join(", ", ids);
                        alert = $"Cannot {(IsCreating() ? "create" : "modify")}. Another reservation has already been made for this time. [ReservationID: {reservationId}, Conflicts: {idList}]";
                        return true;
                    }
                }
            }

            alert = string.Empty;
            return false;
        }

        public void OverwriteReservations()
        {
            var reservationId = Reservation == null ? 0 : Reservation.ReservationID;

            if (_overwriteReservations != null)
            {
                foreach (var rsv in _overwriteReservations)
                {
                    if (rsv.ReservationID != reservationId)
                    {
                        // Delete Reservation
                        Helper.Provider.Scheduler.Reservation.CancelReservation(rsv.ReservationID, CurrentUser.ClientID);

                        // Send email to reserver informing them that their reservation has been canceled
                        Helper.Provider.Scheduler.Email.EmailOnToolEngDelete(rsv, CurrentUser, CurrentUser.ClientID);
                    }
                }
            }
        }

        public ReservationData GetReservationData(ReservationDuration rd)
        {
            return new ReservationData(GetReservationProcessInfos(), GetInvitees())
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

        public List<Invitee> GetInvitees()
        {
            List<Invitee> result;

            if (Helper.Context.Session["ReservationInvitees"] == null)
            {
                var reservationId = Reservation == null ? 0 : Reservation.ReservationID;

                if (reservationId > 0)
                    result = ReservationInvitees.Create(Helper.Provider).SelectInvitees(reservationId);
                else
                    result = new List<Invitee>();

                Helper.Context.Session["ReservationInvitees"] = result;
            }
            else
            {
                result = (List<Invitee>)Helper.Context.Session["ReservationInvitees"];
            }

            return result;
        }

        public List<AvailableInvitee> GetAvailableInvitees()
        {
            int reservationId;
            int resourceId;
            int clientId;
            int activityId;
            
            if (Reservation != null)
            {
                reservationId = Reservation.ReservationID;
                resourceId = Reservation.ResourceID;
                clientId = Reservation.ClientID;
                activityId = Reservation.ActivityID;
            }
            else
            {
                reservationId = 0;
                resourceId = Resource.ResourceID;
                clientId = CurrentUser.ClientID;
                activityId = Convert.ToInt32(ActivityID);
            }

            List<AvailableInvitee> result;

            if (Helper.Context.Session["AvailableInvitees"] == null)
            {
                result = ReservationInvitees.Create(Helper.Provider).SelectAvailable(reservationId, resourceId, activityId, clientId);
                Helper.Context.Session["AvailableInvitees"] = result;
            }
            else
            {
                result = (List<AvailableInvitee>)Helper.Context.Session["AvailableInvitees"];
            }

            return result;
        }

        public List<IReservationProcessInfo> GetReservationProcessInfos()
        {
            List<IReservationProcessInfo> result;

            if (Helper.Context.Session["ReservationProcessInfos"] == null)
            {
                var reservationId = Reservation == null ? 0 : Reservation.ReservationID;

                if (reservationId > 0)
                    result = Helper.Provider.Scheduler.ProcessInfo.GetReservationProcessInfos(reservationId).ToList();
                else
                    result = new List<IReservationProcessInfo>();

                Helper.Context.Session["ReservationProcessInfos"] = result;
            }
            else
            {
                result = (List<IReservationProcessInfo>)Helper.Context.Session["ReservationProcessInfos"];
            }

            return result;
        }

        public ReservationDuration GetReservationDuration()
        {
            var beginDateTime = Helper.Context.Request.SelectedDate().AddHours(Convert.ToInt32(StartTimeHourSelectedValue)).AddMinutes(Convert.ToInt32(StartTimeMinuteSelectedValue));
            var duration = GetCurrentDurationMinutes();
            var result = new ReservationDuration(beginDateTime, TimeSpan.FromMinutes(duration));
            return result;
        }

        public string GetRedirectUrl()
        {
            string redirectUrl;

            if (Helper.Context.Session["ReturnTo"] != null && !string.IsNullOrEmpty(Helper.Context.Session["ReturnTo"].ToString()))
                redirectUrl = Helper.Context.Session["ReturnTo"].ToString();
            else
            {
                ViewType view = Helper.Context.GetCurrentViewType();

                if (view == ViewType.UserView)
                    redirectUrl = $"~/UserReservations.aspx?Date={Helper.Context.Request.SelectedDate():yyyy-MM-dd}";
                else if (view == ViewType.ProcessTechView)
                {
                    // When we come from ProcessTech.aspx the full path is used (to avoid a null object error). When returning we just want the ProcessTech path.
                    IProcessTech pt = Helper.GetCurrentProcessTech();
                    PathInfo path = PathInfo.Create(pt);
                    redirectUrl = $"~/ProcessTech.aspx?Path={path.UrlEncode()}&Date={Helper.Context.Request.SelectedDate():yyyy-MM-dd}";
                }
                else //ViewType.DayView OrElse Scheduler.ViewType.WeekView
                    redirectUrl = $"~/ResourceDayWeek.aspx?Path={Helper.Context.Request.SelectedPath().UrlEncode()}&Date={Helper.Context.Request.SelectedDate():yyyy-MM-dd}";
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
            var resourceClients = Helper.GetResourceClients(Resource.ResourceID);
            return Reservations.GetAuthLevel(resourceClients, CurrentUser);
        }

        public IActivity GetCurrentActivity()
        {
            // always get from the select - even when modifying
            return CacheManager.Current.GetActivity(ActivityID);
        }

        private bool IsCreating()
        {
            return Reservation == null;
        }
    }
}
