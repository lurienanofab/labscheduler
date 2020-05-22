using LNF.Data;
using LNF.Impl.Repository.Data;
using LNF.Impl.Repository.Scheduler;
using LNF.Repository;
using LNF.Scheduler;
using LNF.Web.Scheduler.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Pages
{
    public class ResourceClients : SchedulerPage
    {
        #region Controls
        protected Literal ErrorMessageLiteral;
        protected Literal ClientNameLiteral;
        protected Literal EmailListTitleLiteral;
        protected HyperLink EmailAllHyperLink;
        protected HyperLink EmailToolEngineersHyperLink;
        protected HyperLink EmailTrainersHyperLink;
        protected HyperLink EmailSuperUsersHyperLink;
        protected HyperLink EmailAuthorizedUsersHyperLink;
        protected PlaceHolder NoToolEngineersPlaceHolder;
        protected PlaceHolder NoTrainersPlaceHolder;
        protected PlaceHolder NoSuperUsersPlaceHolder;
        protected PlaceHolder NoAuthorizedUsersPlaceHolder;
        protected PlaceHolder AddUserPlaceHolder;
        protected PlaceHolder TrainersPlaceHolder;
        protected PlaceHolder SuperUsersPlaceHolder;
        protected PlaceHolder AuthorizedUsersPlaceHolder;
        protected PlaceHolder EmailListPlaceHolder;
        protected PlaceHolder ClientNamePlaceHolder;
        protected PlaceHolder ErrorMessagePlaceHolder;
        protected DropDownList AuthLevelDropDownList;
        protected DropDownList ClientsDropDownList;
        protected Repeater ToolEngineersRepeater;
        protected Repeater TrainersRepeater;
        protected Repeater SuperUsersRepeater;
        protected Repeater AuthorizedUsersRepeater;
        protected Repeater EmailListRepeater;
        protected HiddenField ClientIdHiddenField;
        protected Button SubmitButton;
        protected Button CancelButton;
        #endregion

        private IList<ResourceClientItem> CurrentClients { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            var resourceId = GetCurrentResource().ResourceID;
            CurrentClients = CreateResourceClientItems(Provider.Scheduler.Resource.GetResourceClients(resourceId)).ToList();

            if (!Page.IsPostBack)
            {
                FillAuthLevels();
                FillClients();
                FillToolEngineers();

                if (GetCurrentResource().IsSchedulable)
                {
                    FillTrainers();
                    FillSuperUsers();
                    FillAuthorizedUsers();
                }
                else
                {
                    TrainersPlaceHolder.Visible = false;
                    SuperUsersPlaceHolder.Visible = false;
                    AuthorizedUsersPlaceHolder.Visible = false;
                    EmailListPlaceHolder.Visible = false;
                }

                FillEmailList();

                SetHyperLinks();
            }
        }

        private void FillAuthLevels()
        {
            if (CanAuthorize())
            {
                AuthLevelDropDownList.DataSource = Provider.Scheduler.Resource.GetAuthLevels().Where(x => x.Authorizable == 1).OrderBy(x => x.AuthLevelID);
                AuthLevelDropDownList.DataBind();
                AddUserPlaceHolder.Visible = true;
            }
            else
            {
                AddUserPlaceHolder.Visible = false;
            }
        }

        private void FillClients()
        {
            var existing = CurrentClients.Select(x => x.ClientID).ToArray();

            var p = ClientPrivilege.LabUser | ClientPrivilege.Staff;

            var query = DA.Current.Query<ClientInfo>().Where(x => (x.Privs & p) > 0 && x.ClientActive && !existing.Contains(x.ClientID)).OrderBy(x => x.LName).ThenBy(x => x.FName);

            ClientsDropDownList.DataSource = query;
            ClientsDropDownList.DataBind();

            if (!CurrentClients.Any(x => x.ClientID == -1))
                ClientsDropDownList.Items.Insert(0, new ListItem(" - Everyone - ", "-1"));
        }

        private void FillEmailList()
        {
            EmailListRepeater.DataSource = CurrentClients.Where(x => x.ClientID != -1).OrderBy(x => x.DisplayName);
            EmailListRepeater.DataBind();
        }

        private void FillToolEngineers()
        {
            var items = CurrentClients.Where(x => x.AuthLevel == ClientAuthLevel.ToolEngineer).OrderByDescending(x => x.IsEveryone()).ThenBy(x => x.DisplayName);
            ToolEngineersRepeater.DataSource = items;
            ToolEngineersRepeater.DataBind();
            NoToolEngineersPlaceHolder.Visible = items.Count() == 0;
            ToolEngineersRepeater.Visible = items.Count() > 0;
        }

        private void FillTrainers()
        {
            var items = CurrentClients.Where(x => x.AuthLevel == ClientAuthLevel.Trainer).OrderByDescending(x => x.IsEveryone()).ThenBy(x => x.DisplayName);
            TrainersRepeater.DataSource = items;
            TrainersRepeater.DataBind();
            NoTrainersPlaceHolder.Visible = items.Count() == 0;
            TrainersRepeater.Visible = items.Count() > 0;
        }

        private void FillSuperUsers()
        {
            var items = CurrentClients.Where(x => x.AuthLevel == ClientAuthLevel.SuperUser).OrderByDescending(x => x.IsEveryone()).ThenBy(x => x.DisplayName);
            SuperUsersRepeater.DataSource = items;
            SuperUsersRepeater.DataBind();
            NoSuperUsersPlaceHolder.Visible = items.Count() == 0;
            SuperUsersRepeater.Visible = items.Count() > 0;
        }

        private void FillAuthorizedUsers()
        {
            var items = CurrentClients.Where(x => x.AuthLevel == ClientAuthLevel.AuthorizedUser).OrderByDescending(x => x.IsEveryone()).ThenByDescending(x => x.ShowExtendButton).ThenBy(x => x.DisplayName);
            AuthorizedUsersRepeater.DataSource = items;
            AuthorizedUsersRepeater.DataBind();
            NoAuthorizedUsersPlaceHolder.Visible = items.Count() == 0;
            AuthorizedUsersRepeater.Visible = items.Count() > 0;
        }

        private ClientAuthLevel GetSelectedAuthLevel()
        {
            return (ClientAuthLevel)Enum.Parse(typeof(ClientAuthLevel), AuthLevelDropDownList.SelectedValue);
        }

        private IEnumerable<ResourceClientItem> CreateResourceClientItems(IEnumerable<IResourceClient> source)
        {
            return source.Select(x => new ResourceClientItem()
            {
                ResourceClientID = x.ResourceClientID,
                ClientID = x.ClientID,
                UserName = x.UserName,
                Privs = x.Privs,
                DisplayName = x.DisplayName,
                Email = x.Email,
                ContactUrl = GetContactUrl(x.ClientID),
                AuthLevel = x.AuthLevel,
                Expiration = x.Expiration,
                AuthDuration = GetCurrentResource().AuthDuration
            });
        }

        private string GetContactUrl(int clientId)
        {
            return $"~/Contact.aspx?ClientID={clientId}&Path={ContextBase.Request.SelectedPath()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}";
        }

        private void Fill(ClientAuthLevel authLevel)
        {
            if ((authLevel & ClientAuthLevel.ToolEngineer) > 0)
                FillToolEngineers();

            if ((authLevel & ClientAuthLevel.Trainer) > 0)
                FillTrainers();

            if ((authLevel & ClientAuthLevel.SuperUser) > 0)
                FillSuperUsers();

            if ((authLevel & ClientAuthLevel.AuthorizedUser) > 0)
                FillAuthorizedUsers();

            FillEmailList();
        }

        private void SetExpiration(ResourceClient rc)
        {
            if (rc.AuthLevel == ClientAuthLevel.AuthorizedUser)
                rc.Expiration = DateTime.Now.AddMonths(GetCurrentResource().AuthDuration);
        }

        private void CancelEdit()
        {
            ClientIdHiddenField.Value = string.Empty;
            ClientNameLiteral.Text = string.Empty;
            ClientNamePlaceHolder.Visible = false;
            ClientsDropDownList.Visible = true;
            SubmitButton.Text = "Authorize Client";
            SubmitButton.CommandName = "Authorize";
            AuthLevelDropDownList.SelectedIndex = 0;
        }

        private void SetHyperLinks()
        {
            var all = ClientAuthLevel.AuthorizedUser | ClientAuthLevel.SuperUser | ClientAuthLevel.Trainer | ClientAuthLevel.ToolEngineer | ClientAuthLevel.RemoteUser;
            SetHyperLinkNavigateUrl(EmailAllHyperLink, all);
            SetHyperLinkNavigateUrl(EmailToolEngineersHyperLink, ClientAuthLevel.ToolEngineer);
            SetHyperLinkNavigateUrl(EmailTrainersHyperLink, ClientAuthLevel.Trainer);
            SetHyperLinkNavigateUrl(EmailSuperUsersHyperLink, ClientAuthLevel.SuperUser);
            SetHyperLinkNavigateUrl(EmailAuthorizedUsersHyperLink, ClientAuthLevel.AuthorizedUser);
        }

        private void SetHyperLinkNavigateUrl(HyperLink hyp, ClientAuthLevel authLevel)
        {
            hyp.NavigateUrl = $"~/Contact.aspx?Privs={(int)authLevel}&Path={ContextBase.Request.SelectedPath()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}";
        }

        private string GetEmailAddress(int clientId)
        {
            var c = DA.Current.Single<ClientInfo>(clientId);

            if (c != null)
                return c.Email;
            else
                return string.Empty;
        }

        protected bool CanModify()
        {
            return CanAuthorize();
        }

        protected bool CanDelete()
        {
            return CanAuthorize();
        }

        protected bool CanExtend()
        {
            return CanAuthorize();
        }

        protected bool CanAuthorize()
        {
            var p = ClientAuthLevel.Trainer | ClientAuthLevel.ToolEngineer;
            var currentUserAuthLevel = Reservations.GetAuthLevel(CurrentClients, CurrentUser);
            return (currentUserAuthLevel & p) > 0;
        }

        protected void ShowErrorMessage(string msg)
        {
            ErrorMessageLiteral.Text = msg;
            ErrorMessagePlaceHolder.Visible = !string.IsNullOrEmpty(msg);
        }

        protected void SubmitButton_Command(object sender, CommandEventArgs e)
        {
            ShowErrorMessage(string.Empty);

            try
            {
                int clientId;
                var selectedAuthLevel = GetSelectedAuthLevel();
                var resourceId = GetCurrentResource().ResourceID;
                ClientAuthLevel refreshAuthLevel = selectedAuthLevel;

                if (e.CommandName == "Authorize")
                {
                    clientId = int.Parse(ClientsDropDownList.SelectedValue);
                    var rc = new ResourceClient()
                    {
                        ResourceID = resourceId,
                        ClientID = clientId,
                        AuthLevel = selectedAuthLevel,
                        Expiration = null,
                        EmailNotify = null,
                        PracticeResEmailNotify = null
                    };

                    SetExpiration(rc);

                    DA.Current.Insert(rc);

                    CurrentClients.Add(new ResourceClientItem()
                    {
                        ResourceClientID = rc.ResourceClientID,
                        ClientID = clientId,
                        AuthLevel = selectedAuthLevel,
                        DisplayName = ClientsDropDownList.SelectedItem.Text,
                        Expiration = rc.Expiration,
                        ContactUrl = GetContactUrl(clientId),
                        AuthDuration = GetCurrentResource().AuthDuration,
                        Email = GetEmailAddress(clientId)
                    });
                }
                else if (e.CommandName == "Modify")
                {
                    clientId = int.Parse(ClientIdHiddenField.Value);
                    var cc = CurrentClients.FirstOrDefault(x => x.ClientID == clientId);
                    if (cc != null)
                    {
                        refreshAuthLevel |= cc.AuthLevel;
                        var rc = DA.Current.Single<ResourceClient>(cc.ResourceClientID);
                        rc.AuthLevel = selectedAuthLevel;
                        cc.AuthLevel = selectedAuthLevel;
                        SetExpiration(rc);
                        cc.Expiration = rc.Expiration;

                        CancelEdit();
                    }
                }

                Fill(refreshAuthLevel);
                FillClients();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            ShowErrorMessage(string.Empty);

            try
            {
                CancelEdit();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message);
            }
        }

        protected void Edit_Command(object sender, CommandEventArgs e)
        {
            var clientId = Convert.ToInt32(e.CommandArgument);
            var cc = CurrentClients.FirstOrDefault(x => x.ClientID == clientId);

            if (cc != null)
            {
                ClientIdHiddenField.Value = clientId.ToString();
                ClientNameLiteral.Text = cc.DisplayName;
                ClientNamePlaceHolder.Visible = true;
                ClientsDropDownList.Visible = false;
                AuthLevelDropDownList.SelectedValue = e.CommandName;
                SubmitButton.Text = "Modify Client Authorization";
                SubmitButton.CommandName = "Modify";
            }
        }

        protected void Delete_Command(object sender, CommandEventArgs e)
        {
            var clientId = Convert.ToInt32(e.CommandArgument);
            var authLevel = (ClientAuthLevel)Enum.Parse(typeof(ClientAuthLevel), e.CommandName);
            var cc = CurrentClients.FirstOrDefault(x => x.ClientID == clientId);

            if (cc != null)
            {
                var rc = DA.Current.Single<ResourceClient>(cc.ResourceClientID);
                DA.Current.Delete(rc);
                CurrentClients.Remove(cc);
            }

            Fill(authLevel);
            FillClients();
        }

        protected void Extend_Command(object sender, CommandEventArgs e)
        {
            var clientId = Convert.ToInt32(e.CommandArgument);
            var authLevel = (ClientAuthLevel)Enum.Parse(typeof(ClientAuthLevel), e.CommandName);
            var cc = CurrentClients.FirstOrDefault(x => x.ClientID == clientId);

            if (cc != null)
            {
                var rc = DA.Current.Single<ResourceClient>(cc.ResourceClientID);
                rc.Expiration = DateTime.Now.AddMonths(GetCurrentResource().AuthDuration);
            }

            Fill(authLevel);
            FillClients();
        }
    }

    public class ResourceClientItem : IAuthorized
    {
        public int ResourceClientID { get; set; }
        public int ClientID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public ClientPrivilege Privs { get; set; }
        public ClientAuthLevel AuthLevel { get; set; }
        public DateTime? Expiration { get; set; }
        public string DisplayName { get; set; }
        public string ContactUrl { get; set; }
        public int AuthDuration { get; set; }
        public bool IsEveryone() => LNF.Scheduler.ResourceClients.IsEveryone(ClientID);
        public bool HasAuth(ClientAuthLevel auths) => LNF.Scheduler.ResourceClients.HasAuth(AuthLevel, auths);

        public bool ShowExtendButton
        {
            get
            {
                // check to see if we need to show the E button

                var value = Expiration.GetValueOrDefault(DateTime.Now.AddDays(10));

                if (DateTime.Now > value.AddDays(-30 * Properties.Current.AuthExpWarning * AuthDuration))
                    return true;
                else
                    return false;
            }
        }
    }
}
