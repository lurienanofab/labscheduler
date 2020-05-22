using LNF.CommonTools;
using LNF.Helpdesk;
using LNF.Scheduler;
using LNF.Web.Scheduler.Content;
using System;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class Helpdesk : SchedulerUserControl
    {
        #region Controls
        protected PlaceHolder phNoResource;
        protected PlaceHolder phHelpdesk;
        protected HtmlInputHidden hidAjaxUrl;
        protected HtmlInputHidden hidHelpdeskQueue;
        protected HtmlInputHidden hidHelpdeskResource;
        protected HtmlInputHidden hidHelpdeskFromEmail;
        protected HtmlInputHidden hidHelpdeskFromName;
        protected Literal litErrMsg;
        protected DropDownList ddlReservations;
        protected DropDownList ddlTicketType;
        protected TextBox txtSubject;
        protected TextBox txtMessage;
        #endregion

        public int ResourceID { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                IResource res = Provider.Scheduler.Resource.GetResource(ResourceID);

                if (res != null)
                {
                    string helpdeskEmail = res.HelpdeskEmail;

                    hidAjaxUrl.Value = "/ostclient/ajax.aspx";
                    hidHelpdeskQueue.Value = helpdeskEmail;
                    hidHelpdeskResource.Value = Provider.Utility.Serialization.Json.SerializeObject(new { id = res.ResourceID, name = res.ResourceName });
                    hidHelpdeskFromEmail.Value = CurrentUser.Email;
                    hidHelpdeskFromName.Value = string.Format("{0} {1}", CurrentUser.FName, CurrentUser.LName);

                    LoadReservations();
                    if (string.IsNullOrEmpty(helpdeskEmail))
                        litErrMsg.Text = WebUtility.BootstrapAlert("danger", "A ticket cannot be created because a helpdesk email is not configured for this resource.");
                }
                else
                {
                    phNoResource.Visible = true;
                    phHelpdesk.Visible = false;
                }
            }
        }

        private void LoadReservations()
        {
            var recentRsvQuery = Provider.Scheduler.Reservation.SelectRecentReservations(ResourceID);
            ddlReservations.Items.Clear();
            ddlReservations.Items.Add(new ListItem("None"));
            foreach (var recentRsv in recentRsvQuery)
            {
                var item = new ListItem
                {
                    Value = recentRsv.ReservationID.ToString(),
                    Text = recentRsv.BeginDateTime.ToString() + " - " + recentRsv.EndDateTime.ToString() + " Reserved by " + recentRsv.DisplayName
                };

                ddlReservations.Items.Add(item);
            }
        }

        private string GetHelpdeskEmail(IResource res)
        {
            if (res != null)
                return res.HelpdeskEmail;
            else
                return string.Empty;
        }

        protected void btnCreateTicket_Click(object sender, EventArgs e)
        {
            var res = Helper.GetResourceTreeItemCollection().GetResource(ResourceID);

            litErrMsg.Text = string.Empty;

            string helpdeskEmail = GetHelpdeskEmail(res);

            bool load = true;

            if (string.IsNullOrEmpty(txtSubject.Text))
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", "Please enter a subject.", true);
            else if (string.IsNullOrEmpty(txtMessage.Text))
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", "Please enter a message.", true);
            else if (string.IsNullOrEmpty(helpdeskEmail))
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", "An error occurred while creating the ticket. A helpdesk email is not configured for this resource.", true);
            else
            {
                IReservation rsv = null;
                if (ddlReservations.SelectedValue != "None")
                    rsv = Provider.Scheduler.Reservation.GetReservation(int.Parse(ddlReservations.SelectedValue));

                string subjectText = "[" + res.ResourceID.ToString() + ":" + res.ResourceName + "] " + txtSubject.Text;
                CreateTicketResult addTicketResult = HelpdeskUtility.CreateTicket(CurrentUser, res, rsv, CurrentUser.ClientID, ddlReservations.SelectedItem.Text, subjectText, txtMessage.Text, ddlTicketType.SelectedItem.Text, Request.Url);
                if (addTicketResult.Success)
                {
                    litErrMsg.Text = WebUtility.BootstrapAlert("success", string.Format("Your ticket has been created. A confirmation email has been sent to {0}.", CurrentUser.Email));
                    load = false;
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(string.Format("Create ticket failed. User: {0} ({1}). Resource: {2} ({3})", CurrentUser.DisplayName, CurrentUser.ClientID, res.ResourceName, res.ResourceID));
                    sb.AppendLine(string.Format("----------{0}{1}", Environment.NewLine, addTicketResult.Exception.Message));
                    SendEmail.SendDeveloperEmail("LNF.Web.Scheduler.Controls.Helpdesk.btnCreateTicket_Click", "Create Ticket Error", sb.ToString());
                    litErrMsg.Text = WebUtility.BootstrapAlert("danger", "Sorry, an error occurred and your ticket was not created. A notification has been sent to LNF staff.");
                }
            }

            if (load)
            {
                //LoadOpenTickets();
            }

            LoadReservations();
        }
    }
}
