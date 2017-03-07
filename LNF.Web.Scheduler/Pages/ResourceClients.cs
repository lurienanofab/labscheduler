using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Scheduler.Data;
using LNF.Web.Scheduler.Content;
using System;
using System.Data;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Pages
{
    public class ResourceClients : SchedulerPage
    {
        #region Controls
        protected Literal litErrMsg;
        protected Literal litCreateModifyResourceClient;
        protected Literal litClientName;
        protected Literal litNoTE;
        protected Literal litNoCheckout;
        protected Literal litNoTrainer;
        protected Literal litNoUser;
        protected Literal litClientListTitle;
        protected HyperLink hypEmailToolEngineers;
        protected HyperLink hypEmailCheckouts;
        protected HyperLink hypEmailTrainers;
        protected HyperLink hypEmailUsers;
        protected HyperLink hypEmailAll;
        protected PlaceHolder phAddUser;
        protected PlaceHolder phCheckouts;
        protected PlaceHolder phTrainers;
        protected PlaceHolder phUsers;
        protected PlaceHolder phEmailList;
        protected PlaceHolder phClientName;
        protected DropDownList ddlAuthLevel;
        protected DropDownList ddlClients;
        protected DropDownList ddlPager;
        protected DataGrid dgTEs;
        protected DataGrid dgTrainers;
        protected DataGrid dgCheckouts;
        protected DataGrid dgUsers;
        protected HiddenField hidClientID;
        protected Button btnSubmit;
        protected Button btnCancel;
        protected HtmlGenericControl divPager;
        protected Repeater rptClientList;
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            ResourceModel res = Request.SelectedPath().GetResource();

            if (res == null)
                Response.Redirect("~");

            if (!Page.IsPostBack)
            {
                Session.Remove("dtAvailClients");
                Session.Remove("dtAuthLevels");
                Session.Remove("dtClients");

                litErrMsg.Text = string.Empty;
                LoadResourceClients(res);
                LoadAuthLevels(res);
                EnableEdit(false);

                litClientListTitle.Text = string.Format("Client List for {0} [{1}]", res.ResourceName, res.ResourceID);
            }

            Session["ReturnFromEmail"] = SchedulerUtility.GetReturnUrl("ResourceClients.aspx", Request.SelectedPath(), 0, Request.SelectedDate());
        }

        private void LoadResourceClients(ResourceModel res)
        {
            // Load Available Clients and Resource Clients
            DataTable dtAvailClients = GetAvailClientsDataTable();
            DataTable dtClients = GetClientsDataTable();

            // Add email to "Everyone"
            DataRow drRC = FindRow(dtClients, "ClientID = {0}", -1);

            if (drRC != null)
                drRC["Email"] = EmailEveryone(dtClients);

            // Bind Avail Clients
            dtAvailClients.DefaultView.Sort = "DisplayName ASC";
            ddlClients.DataSource = dtAvailClients.DefaultView;
            ddlClients.DataBind();

            // Bind Engineers
            DataView dv = new DataView(dtClients);
            dv.Sort = "DisplayName";
            dv.RowFilter = string.Format("AuthLevel = {0}", Convert.ToInt32(ClientAuthLevel.ToolEngineer));
            dgTEs.DataSource = dv;
            dgTEs.DataBind();

            if (res.IsSchedulable)
            {
                // Bind Trainers
                dv.RowFilter = string.Format("AuthLevel = {0}", Convert.ToInt32(ClientAuthLevel.Trainer));
                dgTrainers.DataSource = dv;
                dgTrainers.DataBind();

                // Bind Super User
                dv.RowFilter = string.Format("AuthLevel = {0}", Convert.ToInt32(ClientAuthLevel.SuperUser));
                dgCheckouts.DataSource = dv;
                dgCheckouts.DataBind();

                // Bind Users
                dv.RowFilter = string.Format("AuthLevel = {0}", Convert.ToInt32(ClientAuthLevel.AuthorizedUser));
                dgUsers.DataSource = dv;
                dgUsers.DataBind();
            }
            else
            {
                phCheckouts.Visible = false;
                phTrainers.Visible = false;
                phUsers.Visible = false;
                phEmailList.Visible = false;
            }

            DataTable dtClientList = CreateClientListDataTable(dtClients);
            rptClientList.DataSource = dtClientList;
            rptClientList.DataBind();
        }

        private DataTable CreateClientListDataTable(DataTable dtClients)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ClientID", typeof(int));
            dt.Columns.Add("AuthLevel", typeof(int));
            dt.Columns.Add("AuthLevelText", typeof(string));
            dt.Columns.Add("DisplayName", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("SortOrder", typeof(int));

            foreach (DataRow dr in dtClients.Rows)
            {
                int clientId = dr.Field<int>("ClientID");

                if (clientId > 0)
                {
                    ClientAuthLevel authLevel = dr.Field<ClientAuthLevel>("AuthLevel");
                    ClientAuthLevel includedAuthLevels = ClientAuthLevel.ToolEngineer | ClientAuthLevel.Trainer | ClientAuthLevel.SuperUser | ClientAuthLevel.AuthorizedUser;

                    if ((authLevel & includedAuthLevels) > 0)
                    {
                        DataRow ndr = dt.NewRow();

                        ndr["ClientID"] = clientId;
                        ndr["AuthLevel"] = (int)authLevel;
                        ndr["AuthLevelText"] = GetAuthLevelText(authLevel);
                        ndr["DisplayName"] = dr["DisplayName"];
                        ndr["Email"] = dr["Email"];
                        ndr["SortOrder"] = GetAuthLevelSortOrder(authLevel);

                        dt.Rows.Add(ndr);
                    }
                }
            }

            dt.DefaultView.Sort = "SortOrder ASC, DisplayName ASC";

            return dt;
        }

        private DataRow FindRow(DataTable dt, string where, params object[] values)
        {
            DataRow dr = null;
            DataRow[] rows = dt.Select(string.Format(where, values));
            if (rows.Length > 0)
                dr = rows[0];
            return dr;
        }

        private string EmailEveryone(DataTable dt)
        {
            string result = string.Empty;
            string comma = string.Empty;
            foreach (DataRow dr in dt.Rows)
            {
                if (dr.Field<int>("ClientID") != -1)
                {
                    result += comma + dr["Email"].ToString();
                    comma = ", ";
                }
            }
            return result;
        }

        private void LoadAuthLevels(ResourceModel res)
        {
            DataTable dtAuthLevels = GetAuthLevelsDataTable();

            ClientAuthLevel currentAuthLevel = GetCurrentAuthLevel();

            // If resource is not schedulable, then only TE can be authorized by TE
            if (!res.IsSchedulable)
            {
                dtAuthLevels.DefaultView.RowFilter = string.Format("AuthLevelID = {0}", Convert.ToInt32(ClientAuthLevel.ToolEngineer));
                if (currentAuthLevel < ClientAuthLevel.Trainer)
                    phAddUser.Visible = false;
            }
            else
            {
                switch (currentAuthLevel)
                {
                    case ClientAuthLevel.ToolEngineer:
                        // Engineers can authorize any type of users
                        dtAuthLevels.DefaultView.RowFilter = string.Empty;
                        break;
                    case ClientAuthLevel.Trainer:
                        // Checkouts can only authorize regular users
                        dtAuthLevels.DefaultView.RowFilter = string.Format("AuthLevelID = {0}", Convert.ToInt32(ClientAuthLevel.AuthorizedUser));
                        break;
                    default:
                        // Others can't authorize anyone
                        break;
                }

                if (currentAuthLevel < ClientAuthLevel.Trainer)
                    phAddUser.Visible = false;
            }

            ddlAuthLevel.DataSource = dtAuthLevels.DefaultView;
            ddlAuthLevel.DataBind();
            ddlAuthLevel_SelectedIndexChanged(null, null);
        }

        protected void ddlAuthLevel_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dtAvailClients = GetAvailClientsDataTable();

            string selectedClient = ddlClients.SelectedValue;

            ClientAuthLevel selectedAuthLevel = (ClientAuthLevel)Convert.ToInt32(ddlAuthLevel.SelectedValue);

            switch (selectedAuthLevel)
            {
                case ClientAuthLevel.ToolEngineer:
                    // Display Staffs (only staffs are eligible to be tool engineers)
                    ddlClients.Items.Clear();
                    foreach (DataRow dr in dtAvailClients.Rows)
                    {
                        if (dr["Privs"] != DBNull.Value && (Convert.ToInt32(dr["Privs"]) & (int)ClientPrivilege.Staff) > 0)
                            ddlClients.Items.Add(new ListItem(dr["DisplayName"].ToString(), dr["ClientID"].ToString()));
                    }
                    break;
                case ClientAuthLevel.SuperUser:
                case ClientAuthLevel.Trainer:
                    // Display all available clients, except "Everyone"
                    dtAvailClients.DefaultView.RowFilter = "ClientID <> -1";
                    ddlClients.DataSource = dtAvailClients.DefaultView;
                    ddlClients.DataBind();
                    break;
                default:
                    // Display all available clients
                    dtAvailClients.DefaultView.RowFilter = string.Empty;
                    ddlClients.DataSource = dtAvailClients.DefaultView;
                    ddlClients.DataBind();
                    break;
            }

            ListItem item = ddlClients.Items.FindByValue(selectedClient);

            if (item != null)
                item.Selected = true;
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            litErrMsg.Text = string.Empty;

            ResourceModel res = Request.SelectedPath().GetResource();

            // Error Checking
            if (ddlClients.Items.Count == 0)
            {
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", "There are no available clients to add.");
                return;
            }

            // Update Dataset
            try
            {
                DataTable dtClients = GetClientsDataTable();
                DataTable dtAvailClients = GetAvailClientsDataTable();

                DataRow drClient = null;
                DataRow drAvailC = null;
                if (btnSubmit.CommandName == "Insert")
                {
                    drClient = dtClients.NewRow();
                    drClient["ResourceID"] = res.ResourceID;
                    drClient["ClientID"] = ddlClients.SelectedValue;
                    drClient["DisplayName"] = ddlClients.SelectedItem.Text;
                    drAvailC = dtAvailClients.Rows.Find(ddlClients.SelectedValue);
                    drClient["Privs"] = drAvailC["Privs"];
                    drClient["Email"] = drAvailC["Email"];
                    dtAvailClients.Rows.Remove(drAvailC);
                    dtAvailClients.AcceptChanges();
                }
                else if (btnSubmit.CommandName == "Update")
                {
                    drClient = FindRow(dtClients, "ClientID = {0}", hidClientID.Value);
                }

                // Only Auth Users needs to set expiration date
                drClient["Authlevel"] = ddlAuthLevel.SelectedValue;
                if ((ClientAuthLevel)drClient["AuthLevel"] == ClientAuthLevel.AuthorizedUser)
                    drClient["Expiration"] = DateTime.Now.AddMonths(res.AuthDuration);
                else
                    drClient["Expiration"] = DBNull.Value;

                if (btnSubmit.CommandName == "Insert")
                    dtClients.Rows.Add(drClient);

                ResourceClientData.Update(dtClients);

                // clear cache so it gets reloaded
                CacheManager.Current.ClearResourceClients(res.ResourceID);

                LoadResourceClients(res);

                EnableEdit(false);
            }
            catch (Exception ex)
            {
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", ex.Message);
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            EnableEdit(false);
        }

        protected void dg_DataBinding(object sender, EventArgs e)
        {
            ResourceModel res = Request.SelectedPath().GetResource();

            DataGrid dgClient = (DataGrid)sender;
            Literal litNoData = null;
            switch (dgClient.ID)
            {
                case "dgTEs":
                    // Tool Engineers
                    litNoData = litNoTE;
                    hypEmailToolEngineers.NavigateUrl = string.Format("~/Contact.aspx?Privs=16&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate());
                    break;
                case "dgCheckouts":
                    // Super Users
                    litNoData = litNoCheckout;
                    hypEmailCheckouts.NavigateUrl = string.Format("~/Contact.aspx?Privs=4&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate());
                    break;
                case "dgTrainers":
                    // Trainers
                    litNoData = litNoTrainer;
                    hypEmailTrainers.NavigateUrl = string.Format("~/Contact.aspx?Privs=8&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate());
                    break;
                case "dgUsers":
                    // Users
                    litNoData = litNoUser;
                    hypEmailUsers.NavigateUrl = string.Format("~/Contact.aspx?Privs=2&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate());
                    break;
            }

            // All
            hypEmailAll.NavigateUrl = string.Format("~/Contact.aspx?Privs=62&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate());

            DataView dvClients = (DataView)dgClient.DataSource;

            if (dvClients.Count == 0)
            {
                dgClient.Visible = false;
                litNoData.Visible = true;

                if (dgClient.ID == "dgUsers")
                    divPager.Visible = false;
            }
            else
            {
                dgClient.Visible = true;
                litNoData.Visible = false;

                // Pager for dgUser
                if (dgClient.ID == "dgUsers")
                {
                    ddlPager.Items.Clear();
                    int pSize = dgUsers.PageSize;
                    for (int i = 0; i <= dvClients.Count - 1; i += pSize)
                    {
                        ListItem pagerItem = new ListItem();
                        pagerItem.Value = (i / pSize).ToString();
                        int j = (i + (pSize - 1) >= dvClients.Count) ? dvClients.Count - 1 : i + (pSize - 1);
                        pagerItem.Text = dvClients[i].Row["DisplayName"].ToString() + " ... " + dvClients[j].Row["DisplayName"].ToString();
                        ddlPager.Items.Add(pagerItem);
                    }
                    ddlPager.SelectedValue = dgUsers.CurrentPageIndex.ToString();
                }
            }
        }

        protected void dg_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                try
                {
                    ResourceModel res = Request.SelectedPath().GetResource();

                    string emailLinkName = null;
                    string editName = null;
                    string deleteName = null;

                    string senderId = ((DataGrid)sender).ID;

                    switch (senderId)
                    {
                        case "dgTEs":
                            emailLinkName = "hypToolEngineer";
                            editName = "ibtnEditTE";
                            deleteName = "ibtnDeleteTE";
                            break;
                        case "dgCheckouts":
                            emailLinkName = "hypCheckout";
                            editName = "ibtnEditCheckout";
                            deleteName = "ibtnDeleteCheckout";
                            break;
                        case "dgTrainers":
                            emailLinkName = "hypTrainer";
                            editName = "ibtnEditTrainer";
                            deleteName = "ibtnDeleteTrainer";
                            break;
                        case "dgUsers":
                            emailLinkName = "hypUser";
                            editName = "ibtnEditUser";
                            deleteName = "ibtnDeleteUser";
                            break;
                    }

                    ClientAuthLevel authLevel = GetCurrentAuthLevel();

                    DataItemHelper di = new DataItemHelper(e.Item.DataItem);
                    HyperLink emailLink = (HyperLink)e.Item.FindControl(emailLinkName);
                    emailLink.Text = di["DisplayName"].ToString();
                    emailLink.NavigateUrl = string.Format("~/Contact.aspx?ClientID={0}&Path={1}&Date={2:yyyy-MM-dd}", di["ClientID"], Request.SelectedPath().UrlEncode(), Request.SelectedDate());
                    emailLink.Attributes.Add("title", di["Email"].ToString());

                    if (authLevel == ClientAuthLevel.Trainer || authLevel == ClientAuthLevel.ToolEngineer)
                    {
                        if (authLevel == ClientAuthLevel.ToolEngineer || senderId == "dgUsers")
                        {
                            ImageButton btnDelete = (ImageButton)e.Item.FindControl(deleteName);
                            btnDelete.Attributes.Add("onclick", "return confirm('Are you sure you want to delete this user?');");
                        }

                        if (senderId == "dgUsers")
                        {
                            // if we are in dgUsers, then we have to see if we need to show the E button
                            ImageButton btnExtend = (ImageButton)e.Item.FindControl("ibtnExtend");

                            DateTime expiration;
                            if (di["Expiration"] == DBNull.Value)
                                expiration = DateTime.Now.AddDays(10);
                            else
                                expiration = Convert.ToDateTime(di["Expiration"]);

                            if (DateTime.Now > expiration.AddDays(-30 * Properties.Current.AuthExpWarning * res.AuthDuration))
                                btnExtend.Visible = true;
                            else
                                btnExtend.Visible = false;
                        }
                    }
                    else
                    {
                        ((ImageButton)e.Item.FindControl(editName)).Visible = false;
                        ((ImageButton)e.Item.FindControl(deleteName)).Visible = false;
                        if (senderId == "dgUsers")
                            ((ImageButton)e.Item.FindControl("ibtnExtend")).Visible = false;
                    }
                }
                catch (Exception ex)
                {
                    litErrMsg.Text = WebUtility.BootstrapAlert("danger", ex.Message);
                }
            }
        }

        protected void dg_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            ResourceModel res = Request.SelectedPath().GetResource();

            string senderId = ((DataGrid)source).ID;

            try
            {
                DataTable dtClients = GetClientsDataTable();
                DataTable dtAvailClients = GetAvailClientsDataTable();

                DataRow drClient = dtClients.Select(string.Format("ClientID = {0}", e.Item.Cells[0].Text))[0];

                if (e.CommandName == "Edit")
                {
                    EnableEdit(false);
                    hidClientID.Value = drClient["ClientID"].ToString();
                    litClientName.Text = drClient["DisplayName"].ToString();
                    ddlAuthLevel.Items.FindByValue(drClient["AuthLevel"].ToString()).Selected = true;
                    EnableEdit(true);
                }
                else if (e.CommandName == "Delete")
                {
                    // Insert into available clients
                    int clientId = Convert.ToInt32(drClient["ClientID"]);
                    if (clientId != -1)
                    {
                        DataRow dr = dtAvailClients.NewRow();
                        dr["ClientID"] = clientId;
                        dr["Privs"] = drClient["Privs"];
                        dr["DisplayName"] = drClient["DisplayName"];
                        dr["Email"] = drClient["Email"];
                        dtAvailClients.Rows.Add(dr);
                    }

                    // Delete from resource clients
                    drClient.Delete();
                    ResourceClientData.Update(dtClients);
                    LoadResourceClients(res);
                }
                else if (e.CommandName == "Extend")
                {
                    drClient["Expiration"] = DateTime.Now.AddMonths(res.AuthDuration);
                    ResourceClientData.Update(dtClients);
                    LoadResourceClients(res);
                }
            }
            catch (Exception ex)
            {
                litErrMsg.Text = WebUtility.BootstrapAlert("danger", ex.Message);
            }
        }

        protected void ddlPager_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgUsers.CurrentPageIndex = Convert.ToInt32(ddlPager.SelectedValue); // selectedItem should also work
            ResourceModel res = Request.SelectedPath().GetResource();
            LoadResourceClients(res);
        }

        private void EnableEdit(bool enable)
        {
            if (enable)
            {
                litCreateModifyResourceClient.Text = "Modify Client Authorization";
                btnSubmit.Text = "Modify Client Authorization";
                btnSubmit.CommandName = "Update";
                ddlClients.Visible = false;
                phClientName.Visible = true;
            }
            else
            {
                litCreateModifyResourceClient.Text = "Authorize Client";
                btnSubmit.Text = "Authorize Client";
                btnSubmit.CommandName = "Insert";
                ddlClients.Visible = true;
                phClientName.Visible = false;
                ddlClients.ClearSelection();
                ddlAuthLevel.ClearSelection();
            }
        }

        private DataTable GetAvailClientsDataTable()
        {
            if (Session["dtAvailClients"] == null)
                Session["dtAvailClients"] = ResourceClientData.SelectAvailClients(Request.SelectedPath().ResourceID);
            return (DataTable)Session["dtAvailClients"];
        }

        private DataTable GetAuthLevelsDataTable()
        {
            if (Session["dtAuthLevels"] == null)
                base.Session["dtAuthLevels"] = AuthLevelData.SelectAuthorizable();

            return (DataTable)Session["dtAuthLevels"];
        }

        private DataTable GetClientsDataTable()
        {
            if (Session["dtClients"] == null)
                Session["dtClients"] = ResourceClientData.SelectByResource(Request.SelectedPath().ResourceID);
            return (DataTable)Session["dtClients"];
        }

        private ClientAuthLevel GetCurrentAuthLevel()
        {
            ClientAuthLevel result = CacheManager.Current.GetAuthLevel(Request.SelectedPath().ResourceID, CacheManager.Current.ClientID);
            return result;
        }

        protected string GetAuthLevelText(ClientAuthLevel value)
        {
            return Enum.GetName(typeof(ClientAuthLevel), value);
        }

        protected int GetAuthLevelSortOrder(ClientAuthLevel value)
        {
            // this order matches the order each group of clients is displayed on the page

            if ((value & ClientAuthLevel.ToolEngineer) > 0)
                return 1;

            if ((value & ClientAuthLevel.Trainer) > 0)
                return 2;

            if ((value & ClientAuthLevel.SuperUser) > 0)
                return 3;

            if ((value & ClientAuthLevel.AuthorizedUser) > 0)
                return 4;

            return 9999;
        }
    }
}
