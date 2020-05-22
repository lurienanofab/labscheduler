using LNF.Impl.Repository.Scheduler;
using LNF.Repository;
using LNF.Scheduler;
using LNF.Web.Scheduler.Content;
using System;
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
            if (!Page.IsPostBack)
            {
                IResource res = GetCurrentResource();
                LoadResourceStatus(res);
                LoadInterlockState(res);
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

        private void LoadResourceStatus(IResource res)
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
                var rip = Reservations.GetRepairReservationInProgress(Helper.GetResourceTreeItemCollection().GetResourceTree(res.ResourceID));

                Reservation rsv = null;

                if (rip != null)
                    rsv = DA.Current.Single<Reservation>(rip.ReservationID);

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

        private void LoadInterlockState(IResource res)
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
            try
            {
                litErrMsg.Text = string.Empty;

                IResource res = GetCurrentResource();
                IResourceTree treeItem = Helper.GetResourceTreeItemCollection().GetResourceTree(res.ResourceID);
                IReservation repair;

                var util = new RepairUtility(treeItem, CurrentUser, Provider);

                switch (e.CommandName)
                {
                    case "start":
                        var state = GetSelectedState();
                        repair = util.StartRepair(ContextBase, GetSelectedState(), GetRepairActualBeginDateTime(state), GetRepairActualEndDateTime(state), txtNotes.Text);
                        break;
                    case "update":
                        repair = util.UpdateRepair(GetRepairActualBeginDateTime(res.State), GetRepairActualEndDateTime(res.State), txtNotes.Text);
                        break;
                    case "end":
                        repair = util.EndRepair(DateTime.Now);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown command: {e.CommandName}");
                }

                RefreshAndRedirect(res);
            }
            catch (Exception ex)
            {
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", ex.Message, true);
            }
        }

        private DateTime GetRepairActualBeginDateTime(ResourceState state)
        {
            if (state == ResourceState.Offline)
                EnsureRepairDurationEnteredByUser();

            DateTime actualBeginDateTime = DateTime.Now;

            if (!string.IsNullOrEmpty(txtRepairStart.Text))
                actualBeginDateTime = actualBeginDateTime.AddMinutes(Convert.ToDouble(txtRepairStart.Text) * (rdoRepairStartUnitMinutes.Checked ? -1.0 : -60.0));

            return actualBeginDateTime;
        }

        private DateTime GetRepairActualEndDateTime(ResourceState state)
        {
            if (state == ResourceState.Offline)
                EnsureRepairDurationEnteredByUser();

            DateTime actualEndDateTime = DateTime.Now;

            if (!string.IsNullOrEmpty(txtRepairTime.Text))
                actualEndDateTime = actualEndDateTime.AddMinutes(Convert.ToDouble(txtRepairTime.Text) * (rdoRepairTimeUnitMinutes.Checked ? 1.0 : 60.0));

            return actualEndDateTime;
        }

        private void EnsureRepairDurationEnteredByUser()
        {
            // Make sure needed data are entered by user
            if ((string.IsNullOrEmpty(txtRepairStart.Text) && string.IsNullOrEmpty(txtRepairTime.Text)) || (txtRepairStart.Text == "0" && txtRepairTime.Text == "0"))
                throw new Exception("You must specify the start time and estimated time to repair.");
        }

        private void RefreshAndRedirect(IResource res)
        {
            // Last, reload and refresh everything to reflect the new change made
            Response.Redirect(string.Format("~/ResourceMaintenance.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), ContextBase.Request.SelectedDate()), false);
        }
    }
}
