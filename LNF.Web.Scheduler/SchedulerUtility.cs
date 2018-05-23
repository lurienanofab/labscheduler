﻿using LNF.Cache;
using LNF.Data;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using LNF.Web.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler
{
    public static class SchedulerUtility
    {
        public static IReservationManager ReservationManager => DA.Use<IReservationManager>();
        public static IReservationInviteeManager ReservationInviteeManager => DA.Use<IReservationInviteeManager>();
        public static IEmailManager EmailManager => DA.Use<IEmailManager>();

        public class ReservationData
        {
            public int ClientID { get; set; }
            public int ResourceID { get; set; }
            public int ActivityID { get; set; }
            public int AccountID { get; set; }
            public ReservationDuration ReservationDuration { get; set; }
            public string Notes { get; set; }
            public bool AutoEnd { get; set; }
            public bool KeepAlive { get; set; }
        }

        public static Reservation CreateNewReservation(ReservationData data)
        {
            var result = GetNewReservation(data, data.ReservationDuration.Duration);

            UpdateReservation(result, data);

            if (HandleFacilityDowntimeResrvation(result, data))
                ReservationManager.InsertFacilityDownTime(result, data.ClientID);
            else
            {
                ReservationManager.Insert(result, data.ClientID);
                HandlePracticeReservation(result, data);
            }

            InsertReservationInvitees(result.ReservationID);
            InsertReservationProcessInfos(result.ReservationID);

            EmailManager.EmailOnUserCreate(result);
            EmailManager.EmailOnInvited(result, CacheManager.Current.ReservationInvitees());

            return result;
        }

        public static Reservation ModifyExistingReservation(Reservation rsv, ReservationData data)
        {
            var result = GetReservationForModification(rsv, data, out bool insert);

            UpdateReservation(result, data);

            if (HandleFacilityDowntimeResrvation(result, data))
                ReservationManager.UpdateFacilityDownTime(result, data.ClientID);
            else
            {
                if (insert)
                {
                    ReservationManager.InsertForModification(result, rsv.ReservationID, data.ClientID);
                    rsv.AppendNotes(string.Format("Canceled for modification. New ReservationID: {0}", result.ReservationID));
                }
                else
                    ReservationManager.Update(result, data.ClientID);

                HandlePracticeReservation(result, data);
            }

            if (insert)
            {
                InsertReservationInvitees(result.ReservationID);
                InsertReservationProcessInfos(result.ReservationID);
            }
            else
            {
                UpdateReservationInvitees();
                UpdateReservationProcessInfos();
            }

            EmailManager.EmailOnUserUpdate(result);
            EmailManager.EmailOnInvited(result, CacheManager.Current.ReservationInvitees(), ReservationModificationType.Modified);
            EmailManager.EmailOnUninvited(rsv, CacheManager.Current.RemovedInvitees());

            return result;
        }

        public static void UpdateReservationInvitees()
        {
            var removed = CacheManager.Current.RemovedInvitees();

            if (removed != null)
            {
                foreach (var item in removed)
                    ReservationInviteeManager.Delete(item.ReservationID, item.InviteeID);
            }

            var invitees = CacheManager.Current.ReservationInvitees();

            if (invitees != null)
            {
                foreach (var item in invitees)
                    ReservationInviteeManager.Insert(item.ReservationID, item.InviteeID);
            }
        }

        public static void InsertReservationInvitees(int reservationId)
        {
            var invitees = CacheManager.Current.ReservationInvitees();

            if (invitees != null)
            {
                foreach (var item in invitees)
                {
                    item.ReservationID = reservationId;
                    ReservationInviteeManager.Insert(item.ReservationID, item.InviteeID);
                }
            }
        }

        /// <summary>
        /// Saves any changes to the current process info session items.
        /// </summary>
        public static void UpdateReservationProcessInfos()
        {
            foreach (var item in CacheManager.Current.ReservationProcessInfos())
                item.Update();
        }

        /// <summary>
        /// Creates new process info records for the specified reservation using the current process info session items.
        /// </summary>
        public static void InsertReservationProcessInfos(int reservationId)
        {
            foreach (var item in CacheManager.Current.ReservationProcessInfos())
            {
                if (item.ReservationID == reservationId)
                    throw new Exception("Tried to copy a ReservationProcessInfo to the same reservation. This will cause a duplicate record.");

                item.ReservationID = reservationId;
                item.Insert();
            }
        }

        public static Reservation GetNewReservation(ReservationData data, TimeSpan maxReservedDuration)
        {
            var result = new Reservation
            {
                IsActive = true,
                Resource = DA.Current.Single<Resource>(data.ResourceID),
                Client = DA.Current.Single<Client>(data.ClientID),
                RecurrenceID = -1, //always -1 for non-recurring reservation
                MaxReservedDuration = maxReservedDuration.TotalMinutes,
                Activity = DA.Current.Single<Activity>(data.ActivityID),
                CreatedOn = DateTime.Now
            };

            return result;
        }

        public static Reservation GetReservationForModification(Reservation rsv, ReservationData data, out bool insert)
        {
            Reservation result = null;

            if (CreateReservationForModification(rsv, data.ReservationDuration.BeginDateTime, data.ReservationDuration.Duration))
            {
                insert = true;

                DateTime originalBeginDateTime = rsv.OriginalBeginDateTime.GetValueOrDefault(rsv.BeginDateTime);
                DateTime originalEndDateTime = rsv.OriginalEndDateTime.GetValueOrDefault(rsv.EndDateTime);
                DateTime originalModifiedOn = rsv.OriginalModifiedOn.GetValueOrDefault(rsv.LastModifiedOn);

                // New Update mechanism: Cancel the current reservation and create a new reservation
                ReservationManager.Delete(rsv, data.ClientID);

                // Now we need to create a new reservation object
                double maxReservedMinutes = Math.Max(data.ReservationDuration.Duration.TotalMinutes, rsv.MaxReservedDuration);
                result = GetNewReservation(data, TimeSpan.FromMinutes(maxReservedMinutes));

                // Copy existing properties
                result.ChargeMultiplier = rsv.ChargeMultiplier;
                result.ApplyLateChargePenalty = rsv.ApplyLateChargePenalty;
                result.IsStarted = rsv.IsStarted;
                result.IsUnloaded = rsv.IsUnloaded;
                result.RecurrenceID = rsv.RecurrenceID;
                result.OriginalBeginDateTime = originalBeginDateTime;
                result.OriginalEndDateTime = originalEndDateTime;
                result.OriginalModifiedOn = originalModifiedOn;
            }
            else
            {
                // A new reservation is not needed because the duration is not modified
                insert = false;
                result = rsv;
            }

            return result;
        }

        public static bool CreateReservationForModification(Reservation rsv, DateTime beginDateTime, TimeSpan duration)
        {
            // if Time And Duration modified create new reservation else change existing
            if (rsv.BeginDateTime != beginDateTime || rsv.Duration != duration.TotalMinutes)
                return true;
            else
                return false;
        }

        public static bool HandleFacilityDowntimeResrvation(Reservation rsv, ReservationData data)
        {
            // 2009-06-21 If it's Facility downtime, we must delete the reservations that has been made during that period
            if (rsv.Activity.ActivityID == Properties.Current.Activities.FacilityDownTime.ActivityID)
            {
                // Facility down time must not need to be activated manually by person
                rsv.ActualBeginDateTime = rsv.BeginDateTime;
                rsv.ActualEndDateTime = rsv.EndDateTime;

                // Find and Remove any un-started reservations made during time of repair
                var query = ReservationManager.SelectByResource(rsv.Resource.ResourceID, rsv.BeginDateTime, rsv.EndDateTime, false);

                foreach (var existing in query)
                {
                    // Only if the reservation has not begun
                    if (existing.ActualBeginDateTime == null)
                    {
                        ReservationManager.Delete(existing, data.ClientID);
                        EmailManager.EmailOnCanceledByRepair(existing, true, "LNF Facility Down", "Facility is down, thus we have to disable the tool.", rsv.EndDateTime);
                    }
                    else
                    {
                        // We have to disable all those reservations that have been activated by setting IsActive to 0. 
                        // The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                        // have to charge the user for that reservation
                    }
                }

                return true;
            }
            else
                return false;
        }

        public static bool HandlePracticeReservation(Reservation rsv, ReservationData data)
        {
            // 2009-09-16 Practice reservation : we must also check if tool engineers want to receive the notify email
            if (rsv.Activity.ActivityID == Properties.Current.Activities.Practice.ActivityID)
            {
                LNF.Scheduler.ReservationInviteeItem invitee = null;

                var invitees = CacheManager.Current.ReservationInvitees();

                if (invitees != null)
                    invitee = invitees.FirstOrDefault();

                if (invitee == null)
                    throw new InvalidOperationException("A practice reservation must have at least one invitee.");

                EmailManager.EmailOnPracticeRes(rsv, invitee.DisplayName);

                return true;
            }
            else
                return false;
        }

        public static void UpdateReservation(Reservation rsv, ReservationData data)
        {
            int inviteeCount = 0;

            var invitees = CacheManager.Current.ReservationInvitees();

            if (invitees != null)
                inviteeCount = invitees.Count();

            rsv.BeginDateTime = data.ReservationDuration.BeginDateTime;
            rsv.EndDateTime = data.ReservationDuration.EndDateTime;
            rsv.Duration = data.ReservationDuration.Duration.TotalMinutes;
            rsv.LastModifiedOn = DateTime.Now;
            rsv.Account = DA.Current.Single<Account>(data.AccountID);
            rsv.Notes = data.Notes;
            rsv.AutoEnd = data.AutoEnd;
            rsv.HasProcessInfo = CacheManager.Current.ReservationProcessInfos().Count() > 0;
            rsv.HasInvitees = inviteeCount > 0;
            rsv.KeepAlive = data.KeepAlive;
        }

        public static ReservationState GetReservationCell(CustomTableCell rsvCell, Reservation rsv, int clientId, bool isInLab)
        {
            int reservationId = rsv.ReservationID;
            int resourceId = rsv.Resource.ResourceID;

            // Reservation State
            var state = ReservationManager.GetReservationState(reservationId, clientId, isInLab);

            // 2008-08-15 temp
            if (reservationId == -1 && state == ReservationState.Repair)
                state = ReservationState.Meeting;

            // Tooltip Caption and Text
            string caption = ReservationManager.GetReservationCaption(state);
            string toolTip = ReservationManager.GetReservationToolTip(rsv, state);
            rsvCell.Attributes["data-tooltip"] = toolTip;
            rsvCell.Attributes["data-caption"] = caption;

            // BackGround color and cursor - set by CSS
            rsvCell.CssClass = state.ToString();

            // Reservation Text
            Literal litReserver = new Literal
            {
                Text = string.Format("<div>{0}</div>", rsv.Client.DisplayName)
            };

            rsvCell.Controls.Add(litReserver);

            // Delete Button
            // 2/11/05 - GPR: allow tool engineers to cancel any non-started, non-repair reservation in the future
            ClientAuthLevel authLevel = CacheManager.Current.GetAuthLevel(resourceId, clientId);
            ResourceModel res = CacheManager.Current.ResourceTree().GetResource(rsv.Resource.ResourceID);

            if (state == ReservationState.Editable || state == ReservationState.StartOrDelete || state == ReservationState.StartOnly || (authLevel == ClientAuthLevel.ToolEngineer && DateTime.Now < rsv.BeginDateTime && rsv.ActualBeginDateTime == null && state != ReservationState.Repair))
            {
                var hypDelete = new HyperLink
                {
                    NavigateUrl = string.Format("~/ReservationController.ashx?Command=DeleteReservation&ReservationID={0}&Date={1:yyyy-MM-dd'T'HH:mm:ss}&State={2}&Path={3}", rsv.ReservationID, rsvCell.CellDate, state, PathInfo.Create(res)),
                    ImageUrl = "~/images/deleteGrid.gif",
                    CssClass = "ReservDelete"
                };

                hypDelete.Attributes["data-tooltip"] = "Click to cancel reservation";
                hypDelete.Attributes["data-caption"] = "Cancel this reservation";
                rsvCell.Controls.Add(hypDelete);
                rsvCell.HorizontalAlign = HorizontalAlign.Left;
                rsvCell.VerticalAlign = VerticalAlign.Top;
            }

            // 2011/04/03 Modify button
            if (state == ReservationState.Editable || state == ReservationState.StartOrDelete || state == ReservationState.StartOnly)
            {
                var hypModify = new HyperLink
                {
                    NavigateUrl = string.Format("~/ReservationController.ashx?Command=ModifyReservation&ReservationID={0}&Date={1:yyyy-MM-dd'T'HH:mm:ss}&State={2}&Path={3}", rsv.ReservationID, rsvCell.CellDate, state, PathInfo.Create(res)),
                    ImageUrl = "~/images/edit.png",
                    CssClass = "ReservModify"
                };

                hypModify.Attributes["data-tooltip"] = "Click to modify reservation";
                hypModify.Attributes["data-caption"] = "Modify this reservation";
                //ibtnModify.Attributes.Add("onclick", "event.cancelBubble=true;return true;");
                rsvCell.Controls.Add(hypModify);
                rsvCell.HorizontalAlign = HorizontalAlign.Left;
                rsvCell.VerticalAlign = VerticalAlign.Top;
            }

            return state;
        }

        public static void GetMultipleReservationCell(CustomTableCell rsvCell, IList<Reservation> reservs)
        {
            // Display multiple reservations
            rsvCell.HorizontalAlign = HorizontalAlign.Center;
            rsvCell.VerticalAlign = VerticalAlign.Middle;
            rsvCell.Text = "Multiple Reservations";
            rsvCell.BackColor = Color.Gold;

            // Tooltip Caption and Text
            string caption = "Multiple Reservations";
            string toolTip = string.Empty;
            foreach (var rsv in reservs)
            {
                toolTip += string.Format("<div>[{0}] <b>{1}</b> ", rsv.ReservationID, rsv.Client.DisplayName);
                if (rsv.ActualEndDateTime == null)
                    toolTip += rsv.BeginDateTime.ToShortTimeString() + " - ";
                else
                    toolTip += rsv.ActualBeginDateTime.Value.ToShortTimeString() + " - " + rsv.ActualEndDateTime.Value.ToShortTimeString();
            }

            rsvCell.Attributes["data-tooltip"] = toolTip;
            rsvCell.Attributes["data-caption"] = caption;
        }

        public static void LoadReservationInvitees(int reservationId)
        {
            var items = LNF.Scheduler.ReservationInviteeItem.Create(DA.Current.Query<ReservationInvitee>().Where(x => x.Reservation.ReservationID == reservationId));
            CacheManager.Current.ReservationInvitees(items);
        }

        public static void LoadAvailableInvitees(int reservationId, int resourceId, int activityId, int clientId)
        {
            var items = ReservationInviteeUtility.SelectAvailable(reservationId, resourceId, activityId, clientId);
            CacheManager.Current.AvailableInvitees(items);
        }

        public static void LoadRemovedInvitees()
        {
            var items = new List<LNF.Scheduler.ReservationInviteeItem>();
            CacheManager.Current.RemovedInvitees(items);
        }

        public static void LoadProcessInfo(int reservationId)
        {
            // Reservation Process Info
            var items = ReservationProcessInfoUtility.SelectByReservation(reservationId).Select(ReservationProcessInfoItem.Create).ToList();
            CacheManager.Current.ReservationProcessInfos(items);
        }

        public static void AddReservationProcessInfo(int resourceId, int processInfoId, int processInfoLineId, int reservationId, string valueText, bool special)
        {
            var pi = CacheManager.Current.ProcessInfos(resourceId).FirstOrDefault(x => x.ProcessInfoID == processInfoId);

            if (pi == null)
                throw new Exception(string.Format("Cannot find a ProcessInfo record with ProcessInfoID = {0}", processInfoId));

            var pil = CacheManager.Current.ProcessInfoLines(processInfoId).FirstOrDefault(x => x.ProcessInfoLineID == processInfoLineId);

            if (pil == null)
                throw new Exception(string.Format("Cannot find a ProcessInfoLine record with ProcessInfoID = {0} and ProcessInfoLineID = {1}", processInfoId, processInfoLineId));

            if (!double.TryParse(valueText, out double value))
                throw new Exception(string.Format("Please enter a floating-point number for Process Info {0}", pi.ProcessInfoName));

            if (value < pil.MinValue || value > pil.MaxValue)
                throw new Exception("Process Info value is out of range.");

            bool isSpecial = !string.IsNullOrEmpty(pi.Special);

            // ReservationProcessInfo
            ReservationProcessInfoItem rpi;

            rpi = CacheManager.Current.ReservationProcessInfos().FirstOrDefault(x => x.ProcessInfoLineID == processInfoLineId);

            if (rpi == null)
            {
                // First check for an existing line for this pi
                rpi = CacheManager.Current.ReservationProcessInfos().FirstOrDefault(x => x.ProcessInfoID == processInfoId);

                // If an existing line is not found add it
                if (rpi == null)
                {
                    // Create new ProcessInfo
                    rpi = new ReservationProcessInfoItem()
                    {
                        ReservationProcessInfoID = 0,
                        ReservationID = reservationId,
                        ProcessInfoLineID = processInfoLineId,
                        ProcessInfoID = processInfoId,
                        RunNumber = 0,
                        ChargeMultiplier = 1,
                        Active = true
                    };

                    var list = CacheManager.Current.ReservationProcessInfos().ToList();
                    list.Add(rpi);
                    CacheManager.Current.ReservationProcessInfos(list);
                }
                else
                {
                    // just update the line
                    rpi.ProcessInfoLineID = processInfoLineId;
                    rpi.ProcessInfoID = processInfoId;
                }
            }

            // Update Existing ProcessInfo
            rpi.Value = value;
            rpi.Special = isSpecial && special;
        }

        public static void RemoveReservationProcessInfo(int processInfoId)
        {
            var rpi = CacheManager.Current.ReservationProcessInfos().FirstOrDefault(x => x.ProcessInfoID == processInfoId);

            if (rpi != null)
            {
                // this will cause it to be deleted later in CacheReservationProcessInfo.Update();
                rpi.ProcessInfoLineID = 0;
            }
        }

        /// <summary>
        /// Loads Reservation Billing Account Dropdownlist
        /// </summary>
        public static bool LoadAccounts(List<ClientAccountItem> accts, ActivityAccountType acctType, int clientId)
        {
            bool mustAddInvitee = false;

            IList<ClientAccountItem> activeAccounts = new List<ClientAccountItem>();

            if (acctType == ActivityAccountType.Reserver || acctType == ActivityAccountType.Both)
                /// Loads reserver's accounts
                activeAccounts = CacheManager.Current.ActiveClientAccounts(clientId).ToList();

            if (acctType == ActivityAccountType.Invitee || acctType == ActivityAccountType.Both)
            {
                // Loads each of the invitee's accounts

                IEnumerable<ClientAccountItem> inviteeAccounts = null;

                var invitees = CacheManager.Current.ReservationInvitees();

                if (invitees != null && invitees.Count() > 0)
                {
                    foreach (var inv in invitees)
                    {
                        inviteeAccounts = CacheManager.Current.ActiveClientAccounts(inv.InviteeID);
                        foreach (var invAcct in inviteeAccounts)
                        {
                            if (!activeAccounts.Any(x => x.AccountID == invAcct.AccountID))
                                activeAccounts.Add(invAcct);
                        }
                    }
                }
                else
                    mustAddInvitee = true;
            }

            var orderedAccts = ClientPreferenceUtility.OrderListByUserPreference(clientId, activeAccounts, x => x.AccountID, x => x.AccountName);

            accts.AddRange(orderedAccts);

            return mustAddInvitee;
        }

        public static string GetReservationViewReturnUrl(ViewType view, bool confirm = false, int reservationId = 0)
        {
            string result;
            string separator;

            switch (view)
            {
                case ViewType.DayView:
                case ViewType.WeekView:
                    result= string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpContext.Current.Request.SelectedPath().UrlEncode(), HttpContext.Current.Request.SelectedDate());
                    separator = "&";
                    break;
                case ViewType.ProcessTechView:
                    result = string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpContext.Current.Request.SelectedPath().UrlEncode(), HttpContext.Current.Request.SelectedDate());
                    separator = "&";
                    break;
                case ViewType.UserView:
                    result = string.Format("~/UserReservations.aspx?Date={0:yyyy-MM-dd}", HttpContext.Current.Request.SelectedDate());
                    separator = "&";
                    break;
                default:
                    throw new ArgumentException(string.Format("Invalid view: {0}", view));
            }

            if (confirm && reservationId > 0)
                result += string.Format("{0}Confirm=1&ReservationID={1}", separator, reservationId);

            return result;
        }

        public static string GetReturnUrl(string page, PathInfo pathInfo, int reservationId, DateTime date)
        {
            string result = string.Format("~/{0}", page);

            string separator = "?";

            if (reservationId > 0)
            {
                result += string.Format("{0}ReservationID={1}", separator, reservationId);
                separator = "&";
            }

            if (!pathInfo.IsEmpty())
            {
                result += string.Format("{0}Path={1}", separator, pathInfo.UrlEncode());
                separator = "&";
            }

            result += string.Format("{0}Date={1:yyyy-MM-dd}", separator, date);
            separator = "&";

            return result;
        }
    }
}
