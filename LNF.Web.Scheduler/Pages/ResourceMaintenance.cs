using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using LNF.Web.Scheduler.Content;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Pages
{
    public class ResourceMaintenance : SchedulerPage
    {
        #region Controls
        protected HtmlGenericControl divRepairBeginDateTime;
        protected HtmlGenericControl divRepairStart;
        protected HtmlGenericControl divRepairTime;
        protected HtmlGenericControl divRepairDuration;
        protected HtmlGenericControl divInterlockState;
        protected HtmlGenericControl divStatusOptions;
        protected HtmlGenericControl divStatusText;
        protected HtmlInputRadioButton rdoStatusOffline;
        protected HtmlInputRadioButton rdoStatusLimited;
        protected HtmlInputRadioButton rdoRepairStartUnitMinutes;
        protected HtmlInputRadioButton rdoRepairStartUnitHours;
        protected HtmlInputRadioButton rdoRepairTimeUnitMinutes;
        protected HtmlInputRadioButton rdoRepairTimeUnitHours;
        protected Literal litStatus;
        protected TextBox txtRepairStart;
        protected TextBox txtRepairTime;
        protected TextBox txtNotes;
        protected Button btnStartRepair;
        protected Button btnUpdateRepair;
        protected Button btnEndRepair;
        protected HtmlInputReset btnReset;
        protected Literal litErrMsg;
        protected Literal litRepairBeginMessage;
        protected Literal litRepairEndMessage;
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            if (Request.QueryString["Update"] == "1")
            {
                Session["UpdateForgivenChargeOnFinOps"] = true;
                Response.Redirect(string.Format("~/ResourceMaintenance.aspx?Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate()), false);
            }
            else if (!Page.IsPostBack)
            {
                ResourceModel res = Request.SelectedPath().GetResource();
                LoadResourceStatus(res);
                LoadInterlockState(res);
                RegisterAsyncTask(new PageAsyncTask(() => UpdateForgivenChargeOnFinOps(res)));
            }
        }

        private void ClearRepairBeginDateTime()
        {
            divRepairBeginDateTime.Visible = false;
            litRepairBeginMessage.Text = string.Empty;
            litRepairEndMessage.Text = string.Empty;
        }

        private TimeSpan GetDuration(Reservation rsv)
        {
            TimeSpan result = new TimeSpan(0, 0, Convert.ToInt32(rsv.EndDateTime.Subtract(DateTime.Now).TotalSeconds));
            return result;
        }

        private void SetRepairBeginDateTime(Reservation rsv)
        {
            divRepairBeginDateTime.Visible = true;
            litRepairBeginMessage.Text = string.Format("<div>This repair activity started at <b>{0}</b>.</div>", rsv.ActualBeginDateTime);
            litRepairEndMessage.Text = string.Format("<div>Current scheduled end time: <b>{0} ({1} hours from now)</b>.</div>", rsv.EndDateTime, GetDuration(rsv).TotalHours.ToString("#0.00"));
        }

        private void LoadResourceStatus(ResourceModel res)
        {
            if (res.HasState(ResourceState.Online))
            {
                // Tool state is online
                ClearRepairBeginDateTime();
                divRepairStart.Visible = true;
                divRepairTime.Visible = true;
                divRepairDuration.Visible = true;
                divStatusOptions.Visible = true;
                divStatusText.Visible = false;

                txtRepairStart.Text = "0";
                txtRepairTime.Text = "0";
                btnStartRepair.Visible = true;
                btnUpdateRepair.Visible = false;
                btnEndRepair.Visible = false;
                btnReset.Visible = true;
            }
            else if (res.HasState(ResourceState.Offline))
            {
                // Tool state is offline
                Reservation rsv = DA.Scheduler.Reservation.GetRepairReservationInProgress(res.ResourceID);

                if (rsv != null)
                {
                    //We found a repair offline reservation
                    SetRepairBeginDateTime(rsv);
                    divRepairStart.Visible = false;
                    divRepairTime.Visible = true;
                    divRepairDuration.Visible = false;
                    divStatusOptions.Visible = false;
                    divStatusText.Visible = true;

                    litStatus.Text = "Offline";
                    txtRepairTime.Text = GetDuration(rsv).TotalHours.ToString("#0.00");
                    txtNotes.Text = rsv.Notes;
                    btnStartRepair.Visible = false;
                    btnUpdateRepair.Visible = true;
                    btnEndRepair.Visible = true;
                    btnReset.Visible = false;
                }
                else
                {
                    //Something is wrong, the resource state is different from the reservation table
                    btnReset.Visible = false;
                    btnStartRepair.Visible = false;
                    btnUpdateRepair.Visible = true;
                    btnUpdateRepair.Enabled = false;
                    btnEndRepair.Visible = true;
                    litErrMsg.Text = WebUtility.BootstrapAlert("danger", @"A problem has occurred: This resource is currenlty offline but there is no active repair reservation to modify. Please click the ""End Repair"" button to put the resource back online - then you will be able to create a new repair reservation.");
                }
            }
            else if (res.HasState(ResourceState.Limited))
            {
                //Tool state is limited
                ClearRepairBeginDateTime();
                divRepairStart.Visible = false;
                divRepairTime.Visible = false;
                divRepairDuration.Visible = false;
                divStatusOptions.Visible = false;
                divStatusText.Visible = true;

                litStatus.Text = "Limited";
                btnStartRepair.Visible = false;
                btnUpdateRepair.Visible = true;
                txtNotes.Text = res.StateNotes;
                btnReset.Visible = false;
                btnEndRepair.Visible = true;
            }
            else
            {
                //The state of tool is unknown, something is wrong
                Response.Redirect("~", false);
            }
        }

        private void LoadInterlockState(ResourceModel res)
        {
            divInterlockState.Attributes.Add("data-id", res.ResourceID.ToString());
        }

        private ResourceState GetSelectedState()
        {
            if (rdoStatusOffline.Checked)
                return ResourceState.Offline;
            else if (rdoStatusLimited.Checked)
                return ResourceState.Limited;
            else
                throw new InvalidOperationException("Either Offline or Limited must be selected.");
        }

        protected void ResourceStatus_Command(object sender, CommandEventArgs e)
        {
            litErrMsg.Text = string.Empty;

            ResourceModel res = Request.SelectedPath().GetResource();

            if (e.CommandName == "start")
                StartRepair(res);
            else if (e.CommandName == "update")
                UpdateRepair(res);
            else if (e.CommandName == "end")
                EndRepair(res);
            else
                throw new InvalidOperationException("Unknown command: " + e.CommandName);
        }

        private void StartRepair(ResourceModel res)
        {
            if (res.HasState(ResourceState.Online))
            {
                // Create new offline reservation or new limited mode status
                ResourceState resourceState = rdoStatusOffline.Checked ? ResourceState.Offline : ResourceState.Limited;

                if (ResourceState.Offline == GetSelectedState())
                {
                    // User wants to create new offline reservation

                    // Make sure needed data are entered by user
                    if ((string.IsNullOrEmpty(txtRepairStart.Text) && string.IsNullOrEmpty(txtRepairTime.Text)) || (txtRepairStart.Text == "0" && txtRepairTime.Text == "0"))
                    {
                        litErrMsg.Text = WebUtility.BootstrapAlert("danger", "You must specify the start time and estimated time to repair.", true);
                        return;
                    }

                    // Determine BeginDateTime for repair reservation
                    DateTime beginDatetime, endDateTime;
                    DateTime actualBeginDateTime = DateTime.Now;
                    DateTime actualEndDateTime = DateTime.Now;

                    // Calculate the actual time the machine went off
                    if (!string.IsNullOrEmpty(txtRepairStart.Text))
                        actualBeginDateTime = actualBeginDateTime.AddMinutes(Convert.ToDouble(txtRepairStart.Text) * (rdoRepairStartUnitMinutes.Checked ? -1.0 : -60.0));

                    // [2016-05-04 jg] The following code was already commented out prior to moving this to LNF.Web.Scheduler
                    // Check that this time is after the end of the last completed repair
                    //DateTime? lastRepairEndTime = Reservation.SelectLastRepairEndTime(res.ResourceID);
                    //if (lastRepairEndTime > actualBeginDateTime)
                    //{
                    //    // The tool is broken before we finished repairing it last time
                    //    // so we have to set the actual time to be right after that repair time
                    //    actualBeginDateTime = lastRepairEndTime.Value.AddSeconds(1);
                    //}

                    beginDatetime = res.GetNextGranularity(actualBeginDateTime, NextGranDir.Previous);

                    if (!string.IsNullOrEmpty(txtRepairTime.Text))
                        actualEndDateTime = actualEndDateTime.AddMinutes(Convert.ToDouble(txtRepairTime.Text) * (rdoRepairTimeUnitMinutes.Checked ? 1.0 : 60.0));

                    endDateTime = res.GetNextGranularity(actualEndDateTime, NextGranDir.Future);

                    // Find and End reservations that are in progress (Endable) for this resource
                    IList<Reservation> endableRsvQuery = DA.Scheduler.Reservation.SelectEndableReservations(res.ResourceID);
                    foreach (Reservation endableRsv in endableRsvQuery)
                    {
                        endableRsv.EndForRepair(CurrentUser.ClientID, CurrentUser.ClientID);
                        EmailUtility.EmailOnCanceledByRepair(endableRsv, false, "Offline", txtNotes.Text, endDateTime);
                    }

                    // Find and Remove any un-started reservations made during time of repair
                    IList<Reservation> unstartedReservations = DA.Scheduler.Reservation.SelectByResource(res.ResourceID, beginDatetime, endDateTime, false);
                    foreach (Reservation unstartedRsv in unstartedReservations)
                    {
                        // If the reservation has not begun
                        if (!unstartedRsv.ActualBeginDateTime.HasValue)
                        {
                            unstartedRsv.DeleteAndForgive(CurrentUser.ClientID);
                            EmailUtility.EmailOnCanceledByRepair(unstartedRsv, true, "Offline", txtNotes.Text, endDateTime);
                        }
                        else
                        {
                            // We have to disable all those reservations that have been activated by setting isActive to 0.  
                            // The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                            // have to charge the user for that reservation
                        }
                    }

                    // 2009-05-21 Make the old reservations that were covered by the repair to be forgiven
                    // Get all the past active reservations that were covered by this specific repair period
                    // [2013-05-20 jg] We also need cancelled reservations so booking fee is forgiven
                    IList<Reservation> query = ReservationUtility.SelectHistoryToForgiveForRepair(res.ResourceID, actualBeginDateTime, DateTime.Now);
                    ForgiveReservationsForRepair(query, actualBeginDateTime);

                    // Remove invitees and process info that might be in the session
                    CacheManager.Current.RemoveSessionValue("ReservationInvitees");
                    CacheManager.Current.RemoveSessionValue("ReservationProcessInfos");

                    // Insert the new repair reservation
                    DA.Scheduler.Reservation.InsertRepair(res.ResourceID, CurrentUser.ClientID, beginDatetime, endDateTime, actualBeginDateTime, txtNotes.Text, CurrentUser.ClientID);

                    // Set the state into resource table and session object
                    ResourceUtility.UpdateState(res.ResourceID, ResourceState.Offline, string.Empty);
                }
                else
                {
                    // User sets the tool into limited mode
                    // Set Resource State, txtNotes.Text is saved with Resource table only in limited mode, since limited mode has no reservation record
                    ResourceUtility.UpdateState(res.ResourceID, ResourceState.Limited, txtNotes.Text);
                }
            }

            RefreshAndRedirect(res);
        }

        private void UpdateRepair(ResourceModel res)
        {
            if (res.HasState(ResourceState.Offline))
            {
                // User set the tool into offline mode
                if ((string.IsNullOrEmpty(txtRepairStart.Text) && string.IsNullOrEmpty(txtRepairTime.Text)) || (txtRepairStart.Text == "0" && txtRepairTime.Text == "0"))
                {
                    litErrMsg.Text = WebUtility.BootstrapAlert("danger", "You must specify the start time and estimated time to repair.", true);
                    return;
                }

                // Determine BeginDateTime for repair reservation
                DateTime beginDatetime, endDateTime;
                DateTime actualBeginDateTime = DateTime.Now;
                DateTime actualEndDateTime = DateTime.Now;

                Reservation rsv = DA.Scheduler.Reservation.GetRepairReservationInProgress(res.ResourceID);
                beginDatetime = rsv.BeginDateTime;

                if (!string.IsNullOrEmpty(txtRepairTime.Text))
                    actualEndDateTime = actualEndDateTime.AddMinutes(Convert.ToDouble(txtRepairTime.Text) * (rdoRepairTimeUnitMinutes.Checked ? 1.0 : 60.0));

                endDateTime = res.GetNextGranularity(actualEndDateTime, NextGranDir.Future);

                // Find and End reservations that are in progress (Endable) for this resource
                IList<Reservation> endableRsvQuery = DA.Scheduler.Reservation.SelectEndableReservations(res.ResourceID);
                foreach (Reservation endableRsv in endableRsvQuery)
                {
                    if (endableRsv.ReservationID != rsv.ReservationID)
                    {
                        endableRsv.EndForRepair(CurrentUser.ClientID, CurrentUser.ClientID);
                        EmailUtility.EmailOnCanceledByRepair(endableRsv, false, "Offline", txtNotes.Text, endDateTime);
                    }
                }

                // Find and Remove any un-started reservations made during time of repair
                IList<Reservation> unstartedReservations = DA.Scheduler.Reservation.SelectByResource(res.ResourceID, beginDatetime, endDateTime, false);
                foreach (Reservation unstartedRsv in unstartedReservations)
                {
                    // If the reservation has not begun
                    if (!unstartedRsv.ActualBeginDateTime.HasValue)
                    {
                        if (unstartedRsv.ReservationID != rsv.ReservationID)
                        {
                            unstartedRsv.Delete(CurrentUser.ClientID);
                            EmailUtility.EmailOnCanceledByRepair(unstartedRsv, true, "Offline", txtNotes.Text, endDateTime);
                        }
                    }
                }

                IList<Reservation> query = ReservationUtility.SelectHistoryToForgiveForRepair(res.ResourceID, actualBeginDateTime, DateTime.Now);
                ForgiveReservationsForRepair(query, actualBeginDateTime);

                // Modify Existing Repair Reservation
                rsv.EndDateTime = endDateTime;
                rsv.Notes = txtNotes.Text;
                rsv.Update(CurrentUser.ClientID);
            }
            else
            {
                // modifying limited mode, only StateNotes is modifiable in this case
                ResourceUtility.UpdateState(res.ResourceID, ResourceState.Limited, txtNotes.Text);
            }

            RefreshAndRedirect(res);
        }

        private void EndRepair(ResourceModel res)
        {
            litErrMsg.Text = string.Empty;

            if (res.HasState(ResourceState.Offline))
            {
                Reservation rsv = DA.Scheduler.Reservation.GetRepairReservationInProgress(res.ResourceID);
                if (rsv != null)
                {
                    if (res.IsSchedulable)
                    {
                        // Set Scheduled EndDateTime = next grain boundary in future
                        rsv.EndDateTime = res.GetNextGranularity(DateTime.Now, NextGranDir.Future);
                        rsv.Update(CurrentUser.ClientID);

                        // End the repair reservation now
                        rsv.End(CurrentUser.ClientID, CurrentUser.ClientID);

                        // [2013-05-20 jg] Recheck for any reservations that were not forgiven that should have been.
                        // This can happen when a reservation has a start time that comes after the repair is started.
                        // When the repair is started DateTime.Now is used as the repair end date. So if a reservation
                        // has a begin date (for example) 5 minutes later it will not be included for forgiving because
                        // it starts 5 minutes after the repair "ends". Now we know the real end date so we can tell
                        // that the we need to forgive that reservation.
                        IList<Reservation> query = ReservationUtility.SelectHistoryToForgiveForRepair(res.ResourceID, rsv.ActualBeginDateTime.Value, rsv.ActualEndDateTime.Value);
                        ForgiveReservationsForRepair(query, rsv.ActualEndDateTime.GetValueOrDefault());
                    }
                }
            }

            // Set Resource State
            ResourceUtility.UpdateState(res.ResourceID, ResourceState.Online, string.Empty);

            RefreshAndRedirect(res);
        }

        private void ForgiveReservationsForRepair(IList<Reservation> reservations, DateTime actualBeginDateTime)
        {
            Session["UpdateForgivenChargeOnFinOps"] = false;

            double chargeMultiplier = 0;

            foreach (Reservation rsv in reservations)
            {
                // The following doesn't do anything because the variable chargeMultiplier will always be zero, I think this can be safely removed.
                double timediff = (rsv.BeginDateTime - actualBeginDateTime).TotalMinutes;
                if (timediff < 0)
                {
                    // If the repair starts right in the middle of a past reservation, should we just prorate it?  
                    chargeMultiplier = 0;
                }

                rsv.UpdateCharges(chargeMultiplier, true, CurrentUser.ClientID);

                // We have to delete those reservations as well, so it won't conflict with Repair and produce multiple reservation issue
                //rsvDB.Delete(ReservationID); // why was this commented out?
                rsv.Delete(CurrentUser.ClientID); // added this back, the previous line was commmented without explanation

                // Email User after everything is done.
                EmailUtility.EmailOnForgiveCharge(rsv, 100, true);

                // Make the change to two ToolData tables.
                // The session variable is set now and then checked for on the next page load.
                Session["UpdateForgivenChargeOnFinOps"] = true;
            }
        }

        private async Task UpdateForgivenChargeOnFinOps(ResourceModel res)
        {
            if (Session["UpdateForgivenChargeOnFinOps"] != null)
            {
                bool update = Convert.ToBoolean(Session["UpdateForgivenChargeOnFinOps"]);
                Session.Remove("UpdateForgivenChargeOnFinOps");

                if (update)
                {
                    Reservation rsv = DA.Scheduler.Reservation.GetRepairReservationInProgress(res.ResourceID);

                    if (rsv != null)
                    {
                        DateTime sd = rsv.BeginDateTime.FirstOfMonth();
                        DateTime ed = rsv.EndDateTime.FirstOfMonth().AddMonths(1);

                        var updateBillingResult = await ReservationHistoryUtility.UpdateBilling(sd, ed, 0);

                        if (!updateBillingResult.HasError())
                            throw new Exception(updateBillingResult.GetErrorMessage());
                    }
                }
            }
        }

        private void RefreshAndRedirect(ResourceModel res)
        {
            // Last, reload and refresh everything to reflect the new change made
            Response.Redirect(string.Format("~/ResourceMaintenance.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), Request.SelectedDate()), false);
        }
    }
}
