using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Controls;
using LNF.Web.Scheduler.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler
{
    public class SchedulerUtility
    {
        public IProvider Provider { get; }

        private SchedulerUtility(IProvider provider)
        {
            Provider = provider;
        }

        public static SchedulerUtility Create(IProvider provider)
        {
            return new SchedulerUtility(provider);
        }

        public ReservationState GetReservationCell(CustomTableCell rsvCell, IReservationItem rsv, ReservationClient client, IEnumerable<IReservationProcessInfo> reservationProcessInfos, IEnumerable<IReservationInviteeItem> invitees, LocationPathInfo locationPath, ViewType view, DateTime now)
        {
            int reservationId = rsv.ReservationID;
            int resourceId = rsv.ResourceID;

            // Reservation State
            var args = ReservationStateArgs.Create(rsv, client, now);
            var state = ReservationStateUtility.Create(now).GetReservationState(args);

            // Tooltip Caption and Text
            string caption = Reservations.GetReservationCaption(state);
            string toolTip = Reservations.Create(Provider, now).GetReservationToolTip(rsv, state, reservationProcessInfos, invitees);
            rsvCell.Attributes["data-tooltip"] = toolTip;
            rsvCell.Attributes["data-caption"] = caption;

            // Remove the create reservation link if it was added.
            if (rsvCell.Controls.Count > 0)
                rsvCell.Controls.Clear();

            // BackGround color and cursor - set by CSS
            rsvCell.CssClass = state.ToString();

            var div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "reservation-container");

            var cellText = rsv.DisplayName;

            if (rsv.RecurrenceID.GetValueOrDefault(-1) > 0)
                cellText += " [R]";

            // Reservation Text
            Literal litReserver = new Literal
            {
                Text = $"<div class=\"cell-text\">{cellText}</div>"
            };

            div.Controls.Add(litReserver);

            // Delete Button
            // 2/11/05 - GPR: allow tool engineers to cancel any non-started, non-repair reservation in the future
            ClientAuthLevel userAuth = args.UserAuth;

            PathInfo path = PathInfo.Create(rsv.BuildingID, rsv.LabID, rsv.ProcessTechID, rsv.ResourceID);
            string navurl;

            //if (state == ReservationState.Editable || state == ReservationState.StartOrDelete || state == ReservationState.StartOnly || (userAuth == ClientAuthLevel.ToolEngineer && DateTime.Now < rsv.BeginDateTime && rsv.ActualBeginDateTime == null && state != ReservationState.Repair))
            // [2020-09-18 jg] StartOnly should not allow delete and NotInLab should allow delete
            if (CanDeleteReservation(state, args, now))
            {
                navurl = UrlUtility.GetDeleteReservationUrl(rsv.ReservationID, rsvCell.CellDate, state, view);
                var hypDelete = new HyperLink
                {
                    NavigateUrl = NavigateUrl(navurl, path, locationPath),
                    ImageUrl = "~/images/deleteGrid.gif",
                    CssClass = "ReservDelete"
                };

                hypDelete.Attributes["data-tooltip"] = "Click to cancel reservation";
                hypDelete.Attributes["data-caption"] = "Cancel this reservation";

                div.Controls.Add(hypDelete);

                rsvCell.HorizontalAlign = HorizontalAlign.Left;
                rsvCell.VerticalAlign = VerticalAlign.Top;
            }

            // 2011/04/03 Modify button
            // [2020-09-18 jg] StartOnly should not allow modification (also NotInLab should not allow modification)
            //if (state == ReservationState.Editable || state == ReservationState.StartOrDelete || state == ReservationState.StartOnly)
            if (CanModifyReservation(state, args, now))
            {
                navurl = UrlUtility.GetModifyReservationUrl(rsv.ReservationID, rsvCell.CellDate, state, view);
                var hypModify = new HyperLink
                {
                    NavigateUrl = NavigateUrl(navurl, path, locationPath),
                    ImageUrl = "~/images/edit.png",
                    CssClass = "ReservModify"
                };

                hypModify.Attributes["data-tooltip"] = "Click to modify reservation";
                hypModify.Attributes["data-caption"] = "Modify this reservation";

                div.Controls.Add(hypModify);

                rsvCell.HorizontalAlign = HorizontalAlign.Left;
                rsvCell.VerticalAlign = VerticalAlign.Top;
            }

            rsvCell.Controls.Add(div);

            return state;
        }

        public static bool CanDeleteReservation(ReservationState state, ReservationStateArgs args, DateTime now)
        {
            // [2020-09-29 jg] invitee and tool engineer can also delete

            if (state == ReservationState.StartOrDelete)
                return true;

            if (state == ReservationState.Editable)
                return true;

            if (state == ReservationState.Invited)
                return true;

            if (state == ReservationState.NotInLab)
                return true;

            return false;
        }

        public static bool CanModifyReservation(ReservationState state, ReservationStateArgs args, DateTime now)
        {
            // [2020-09-29 jg] Editable means Delete or Modify so we must check here for isReserver.

            // only reserver can modify
            if (!args.IsReserver)
                return false;

            if (state == ReservationState.StartOrDelete)
                return true;

            if (state == ReservationState.Editable)
                return true;

            if (state == ReservationState.NotInLab)
                return true;

            return false;
        }

        public static void GetMultipleReservationCell(CustomTableCell rsvCell, IEnumerable<IReservationItem> reservs)
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
                toolTip += $"<div>[{rsv.ReservationID}] <b>{Clients.GetDisplayName(rsv.LName, rsv.FName)}</b> ";

                if (rsv.ActualEndDateTime == null)
                    toolTip += rsv.BeginDateTime.ToShortTimeString() + " - ";
                else
                    toolTip += rsv.ActualBeginDateTime.Value.ToShortTimeString() + " - " + rsv.ActualEndDateTime.Value.ToShortTimeString();
            }

            rsvCell.Attributes["data-tooltip"] = toolTip;
            rsvCell.Attributes["data-caption"] = caption;
        }

        /// <summary>
        /// Loads Reservation Billing Account Dropdownlist
        /// </summary>
        public bool LoadAccounts(List<IClientAccount> accts, ActivityAccountType acctType, IClient client, IEnumerable<Invitee> invitees, string username)
        {
            bool mustAddInvitee = false;

            //IList<ClientAccountItem> activeAccounts = new List<ClientAccountItem>();
            IEnumerable<IClientAccount> activeAccounts = new List<IClientAccount>();

            if (acctType == ActivityAccountType.Reserver || acctType == ActivityAccountType.Both)
                /// Loads reserver's accounts
                activeAccounts = Provider.Data.Client.GetActiveClientAccounts(username);

            if (acctType == ActivityAccountType.Invitee || acctType == ActivityAccountType.Both)
            {
                // Loads each of the invitee's accounts

                if (invitees != null)
                {
                    var invited = invitees.Where(x => !x.Removed).ToList();

                    if (invited.Count > 0)
                    {
                        var inviteeClientIds = invited.Select(x => x.InviteeID).ToArray();
                        activeAccounts = Provider.Data.Client.GetActiveClientAccounts(inviteeClientIds);
                    }
                    else
                        mustAddInvitee = true;
                }
            }

            var orderedAccts = ClientPreferenceUtility.OrderListByUserPreference(client, activeAccounts, x => x.AccountID, x => x.AccountName);

            accts.AddRange(orderedAccts);

            return mustAddInvitee;
        }

        public string GetReservationViewReturnUrl(ViewType view, bool confirm = false, int reservationId = 0)
        {
            string result;
            string separator;

            HttpContextBase ctx = new HttpContextWrapper(HttpContext.Current);

            var path = ctx.Request.SelectedPath();
            var date = ctx.Request.SelectedDate();

            switch (view)
            {
                case ViewType.DayView:
                case ViewType.WeekView:
                    result = $"~/ResourceDayWeek.aspx?Path={path.UrlEncode()}&Date={date:yyyy-MM-dd}";
                    separator = "&";
                    break;
                case ViewType.ProcessTechView:
                    result = $"~/ProcessTech.aspx?Path={path.UrlEncode()}&Date={date:yyyy-MM-dd}";
                    separator = "&";
                    break;
                case ViewType.UserView:
                    result = $"~/UserReservations.aspx?Date={date:yyyy-MM-dd}";
                    separator = "&";
                    break;
                case ViewType.LocationView:
                    LocationPathInfo locationPath = GetLocationPath(ctx);
                    result = $"~/LabLocation.aspx?LocationPath={locationPath.UrlEncode()}&Date={date:yyyy-MM-dd}";
                    separator = "&";
                    break;
                default:
                    throw new ArgumentException($"Invalid view: {view}");
            }

            if (confirm && reservationId > 0)
                result += $"{separator}Confirm=1&ReservationID={reservationId}";

            return result;
        }

        public LocationPathInfo GetLocationPath(HttpContextBase context)
        {
            LocationPathInfo result;

            if (string.IsNullOrEmpty(context.Request.QueryString["LocationPath"]))
            {
                // This can happen when ending a reservation from LabLocation.aspx. The QueryString passed to this controller does not contain
                // the LocationPath variable but this is needed for the redirect url when View == ViewType.LocationView.
                var labId = context.Request.SelectedPath().LabID;
                var resourceId = context.Request.SelectedPath().ResourceID;
                var loc = Provider.Scheduler.LabLocation.GetResourceLabLocationByResource(resourceId);
                if (loc == null)
                    result = LocationPathInfo.Create(Provider.Scheduler.Resource.GetLab(labId));
                else
                    result = LocationPathInfo.Create(labId, loc.LabLocationID);
            }
            else
            {
                result = context.Request.SelectedLocationPath();
            }

            return result;
        }

        public static string GetReservationReturnUrl(PathInfo path, LocationPathInfo locationPath, int reservationId, DateTime date, TimeSpan time, ViewType view)
        {
            var result = GetReturnUrl("Reservation.aspx", path, locationPath, reservationId, date);
            result += $"&Time={time.TotalMinutes}&View={view}"; // at least date will be in the query string, so definitely use '&' here
            return result;
        }

        public static string GetReturnUrl(string page, PathInfo path, LocationPathInfo locationPath, int reservationId, DateTime date)
        {
            string result = $"~/{page}";

            string separator = "?";

            if (reservationId > 0)
            {
                result += $"{separator}ReservationID={reservationId}";
                separator = "&";
            }

            if (!path.IsEmpty())
            {
                result += $"{separator}Path={path.UrlEncode()}";
                separator = "&";
            }

            if (!locationPath.IsEmpty())
            {
                result += $"{separator}LocationPath={locationPath.UrlEncode()}";
                separator = "&";
            }

            result += $"{separator}Date={date:yyyy-MM-dd}";
            separator = "&";

            return result;
        }

        public bool ShowLabCleanWarning(DateTime beginDateTime, DateTime endDateTime)
        {
            bool showWarning;

            string setting = ConfigurationManager.AppSettings["ShowLabCleanWarning"];

            if (string.IsNullOrEmpty(setting))
                showWarning = true;
            else
                bool.TryParse(setting, out showWarning);

            if (showWarning)
            {
                LabCleanConfiguration config = LabCleanConfiguration.GetCurrentConfiguration();

                DateTime sdate = beginDateTime.Date;
                DateTime edate = endDateTime.Date.AddDays(1);
                DateTime currentDate = sdate;

                while (currentDate < edate)
                {
                    DateTime yesterday = currentDate.AddDays(-1); //need this to determine lab clean days that are moved by holidays

                    DateTime labCleanBegin;
                    DateTime labCleanEnd;

                    foreach (var item in config.Items)
                    {
                        if (item.Active)
                        {
                            // current day is not a holiday and is included
                            // or previous day is a holiday and is included
                            bool isLabCleanDay =
                                (!Provider.Data.Holiday.IsHoliday(currentDate) && item.Days.Contains((int)currentDate.DayOfWeek)) //we have a standard non-holiday lab clean day
                                || (Provider.Data.Holiday.IsHoliday(yesterday) && item.Days.Contains((int)yesterday.DayOfWeek)); //we have a non standard lab clean day due to holiday

                            if (isLabCleanDay)
                            {
                                // Sandrine wants a 45 minute padding after the labclean end time as of 2018-10-22 - jg

                                var sPad = TimeSpan.FromMinutes(item.StartPadding);
                                var ePad = TimeSpan.FromMinutes(item.EndPadding);

                                labCleanBegin = currentDate
                                    .Add(item.StartTime) //e.g. 08:30
                                    .Subtract(sPad); //e.g. 10 (minutes)

                                labCleanEnd = currentDate
                                    .Add(item.EndTime) //e.g. 09:30
                                    .Add(ePad); //e.g. 45 (minutes)

                                if (beginDateTime < labCleanEnd && endDateTime > labCleanBegin)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }
            }

            return false;
        }

        public static string NavigateUrl(string url, PathInfo path, LocationPathInfo locationPath)
        {
            if (!path.IsEmpty())
                url += $"&Path={path.UrlEncode()}";
            if (!locationPath.IsEmpty())
                url += $"&LocationPath={locationPath.UrlEncode()}";
            return url;
        }
    }
}
