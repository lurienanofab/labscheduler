using LNF.Data;
using LNF.Impl.Repository.Data;
using LNF.Repository;
using LNF.Scheduler;
using LNF.Web.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler
{
    public static class SchedulerUtility
    {
        // Fix this dependency
        public static IProvider Provider => ServiceProvider.Current;

        public static ReservationState GetReservationCell(CustomTableCell rsvCell, IReservation rsv, ReservationClient client, DateTime now)
        {
            int reservationId = rsv.ReservationID;
            int resourceId = rsv.ResourceID;

            // Reservation State
            var args = ReservationStateArgs.Create(rsv, client);
            var util = Reservations.Create(Provider, now);
            var state = util.GetReservationState(args);

            // 2008-08-15 temp
            if (reservationId == -1 && state == ReservationState.Repair)
                state = ReservationState.Meeting;

            // Tooltip Caption and Text
            string caption = Reservations.GetReservationCaption(state);
            string toolTip = Reservations.Create(Provider, now).GetReservationToolTip(rsv, state);
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
            //var res = CacheManager.Current.ResourceTree().GetResource(rsv.ResourceID);

            if (state == ReservationState.Editable || state == ReservationState.StartOrDelete || state == ReservationState.StartOnly || (userAuth == ClientAuthLevel.ToolEngineer && DateTime.Now < rsv.BeginDateTime && rsv.ActualBeginDateTime == null && state != ReservationState.Repair))
            {
                var hypDelete = new HyperLink
                {
                    NavigateUrl = $"~/ReservationController.ashx?Command=DeleteReservation&ReservationID={rsv.ReservationID}&Date={rsvCell.CellDate:yyyy-MM-dd}&Time={rsvCell.CellDate.TimeOfDay.TotalMinutes}&State={state}&Path={PathInfo.Create(rsv.BuildingID, rsv.LabID, rsv.ProcessTechID, rsv.ResourceID)}",
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
            if (state == ReservationState.Editable || state == ReservationState.StartOrDelete || state == ReservationState.StartOnly)
            {
                var hypModify = new HyperLink
                {
                    NavigateUrl = $"~/ReservationController.ashx?Command=ModifyReservation&ReservationID={rsv.ReservationID}&Date={rsvCell.CellDate:yyyy-MM-dd}&Time={rsvCell.CellDate.TimeOfDay.TotalMinutes}&State={state}&Path={PathInfo.Create(rsv.BuildingID, rsv.LabID, rsv.ProcessTechID, rsv.ResourceID)}",
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

        public static void GetMultipleReservationCell(CustomTableCell rsvCell, IEnumerable<IReservation> reservs)
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
        public static bool LoadAccounts(List<ClientAccountInfo> accts, ActivityAccountType acctType, IClient client, IEnumerable<IReservationInvitee> invitees, string username)
        {
            bool mustAddInvitee = false;

            //IList<ClientAccountItem> activeAccounts = new List<ClientAccountItem>();
            IEnumerable<ClientAccountInfo> activeAccounts = new List<ClientAccountInfo>();

            if (acctType == ActivityAccountType.Reserver || acctType == ActivityAccountType.Both)
                /// Loads reserver's accounts
                activeAccounts = DA.Current.Query<ClientAccountInfo>().Where(x => x.ClientAccountActive && x.ClientOrgActive && x.UserName == username).ToList(); //CacheManager.Current.GetClientAccounts(clientId).ToList();

            if (acctType == ActivityAccountType.Invitee || acctType == ActivityAccountType.Both)
            {
                // Loads each of the invitee's accounts

                if (invitees != null)
                {
                    var invited = invitees.Where(x => !x.Removed).ToList();

                    if (invited.Count > 0)
                    {
                        var inviteeClientIds = invited.Select(x => x.InviteeID).ToArray();
                        activeAccounts = DA.Current.Query<ClientAccountInfo>().Where(x => x.ClientAccountActive && x.ClientOrgActive && inviteeClientIds.Contains(x.ClientID)).ToList();
                    }
                    else
                        mustAddInvitee = true;
                }
            }

            var orderedAccts = ClientPreferenceUtility.OrderListByUserPreference(client, activeAccounts, x => x.AccountID, x => x.AccountName);

            accts.AddRange(orderedAccts);

            return mustAddInvitee;
        }

        public static string GetReservationViewReturnUrl(ViewType view, bool confirm = false, int reservationId = 0)
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

        public static LocationPathInfo GetLocationPath(HttpContextBase context)
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
                result = LocationPathInfo.Parse(context.Request.QueryString["LocationPath"]);
            }

            return result;
        }

        public static string GetReservationReturnUrl(PathInfo pathInfo, int reservationId, DateTime date, TimeSpan time)
        {
            var result = GetReturnUrl("Reservation.aspx", pathInfo, reservationId, date);
            result += $"&Time={time.TotalMinutes}"; // at least date will be in the query string, so definitely use '&' here
            return result;
        }

        public static string GetReturnUrl(string page, PathInfo pathInfo, int reservationId, DateTime date)
        {
            string result = $"~/{page}";

            string separator = "?";

            if (reservationId > 0)
            {
                result += $"{separator}ReservationID={reservationId}";
                separator = "&";
            }

            if (!pathInfo.IsEmpty())
            {
                result += $"{separator}Path={pathInfo.UrlEncode()}";
                separator = "&";
            }

            result += $"{separator}Date={date:yyyy-MM-dd}";
            separator = "&";

            return result;
        }
    }
}
