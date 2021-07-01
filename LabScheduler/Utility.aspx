<%@ Page Title="Utility" Language="C#" Async="true" Inherits="LNF.Web.Content.LNFPage" %>

<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Net.Mail" %>
<%@ Import Namespace="System.Threading.Tasks" %>
<%@ Import Namespace="LNF" %>
<%@ Import Namespace="LNF.Billing" %>
<%@ Import Namespace="LNF.Billing.Process" %>
<%@ Import Namespace="LNF.Cache" %>
<%@ Import Namespace="LNF.CommonTools" %>
<%@ Import Namespace="LNF.Data" %>
<%@ Import Namespace="LNF.DataAccess" %>
<%@ Import Namespace="LNF.Impl.Repository.Data" %>
<%@ Import Namespace="LNF.Impl.Repository.Scheduler" %>
<%@ Import Namespace="LNF.PhysicalAccess" %>
<%@ Import Namespace="LNF.Repository" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Web" %>
<%@ Import Namespace="LNF.Web.Scheduler" %>
<%@ Import Namespace="LNF.Web.Scheduler.TreeView" %>
<%@ Import Namespace="LabScheduler.AppCode" %>
<%@ Import Namespace="System.Data" %>

<script runat="server">
    //note: this page does not have a separate CodeBehind file so that server side code can be edited in production

    public IReservationRepository ReservationManager { get { return Provider.Scheduler.Reservation; } }

    private HttpContextBase _contextBase;

    public HttpContextBase ContextBase { get { return _contextBase; } }

    public IClient CurrentUser { get { return ContextBase.CurrentUser(Provider); } }

    public ISession DataSession { get { return Provider.DataAccess.Session; } }

    public enum AlertType
    {
        Success = 1,
        Info = 2,
        Warning = 3,
        Danger = 4
    }

    private int GetResourceID()
    {
        int result = 0;
        int.TryParse(Request.QueryString["resourceId"], out result);
        return result;
    }

    private int GetReservationID()
    {
        int result = 0;
        int.TryParse(Request.QueryString["reservationId"], out result);
        return result;
    }

    private IClient GetLogInAsOriginalUser()
    {
        if (Session["LogInAsOriginalUser"] != null)
        {
            string un = Session["LogInAsOriginalUser"].ToString();
            return Provider.Data.Client.GetClient(un);
        }

        return null;
    }

    private bool HasAccess()
    {
        IClient logInAsOriginalUser = GetLogInAsOriginalUser();

        if (CurrentUser == null)
            return false;

        if (CurrentUser.HasPriv(ClientPrivilege.Developer))
            return true;

        if (logInAsOriginalUser == null)
            return false;

        if (logInAsOriginalUser.HasPriv(ClientPrivilege.Developer))
            return true;

        return false;
    }

    public void Page_Load(object sender, EventArgs e)
    {
        _contextBase = new HttpContextWrapper(Context);

        if (!HasAccess())
        {
            phNoAccess.Visible = true;
            phUtility.Visible = false;
            return;
        }

        string tab = Request.QueryString["tab"];

        if (string.IsNullOrEmpty(tab))
            tab = "user-report";

        liUserReport.Attributes.Remove("class");
        liReservationUtility.Attributes.Remove("class");
        liInterlocks.Attributes.Remove("class");
        liEmail.Attributes.Remove("class");
        liJobs.Attributes.Remove("class");
        liBilling.Attributes.Remove("class");

        panUserReport.Visible = false;
        panReservationUtility.Visible = false;
        panInterlocks.Visible = false;
        panEmail.Visible = false;
        panJobs.Visible = false;
        panBilling.Visible = false;

        switch (tab)
        {
            case "user-report":
                liUserReport.Attributes.Add("class", "active");
                panUserReport.Visible = true;
                if (!Page.IsPostBack)
                {
                    var clientId = GetClientID();

                    var client = clientId == 0 ? CurrentUser : Provider.Data.Client.GetClient(clientId);

                    if (client == null)
                        throw new Exception(string.Format("Cannot find a Client with ClientID = {0}", clientId));

                    LoadUserReport(client);
                    LoadInLabReport();
                }
                break;
            case "reservation-utility":
                liReservationUtility.Attributes.Add("class", "active");
                panReservationUtility.Visible = true;
                LoadReservation(GetReservationID(), Request.QueryString["command"]);
                break;
            case "interlocks":
                liInterlocks.Attributes.Add("class", "active");
                panInterlocks.Visible = true;
                break;
            case "email":
                liEmail.Attributes.Add("class", "active");
                panEmail.Visible = true;
                break;
            case "jobs":
                liJobs.Attributes.Add("class", "active");
                panJobs.Visible = true;
                break;
            case "billing":
                liBilling.Attributes.Add("class", "active");
                panBilling.Visible = true;
                UpdateBilling();
                break;
        }
    }

    private XReservationProcessInfoItem GetReservationProcessInfoItem(ReservationProcessInfo rpi)
    {
        var pil = DataSession.Single<ProcessInfoLine>(rpi.ProcessInfoLineID);
        var pi = DataSession.Single<LNF.Impl.Repository.Scheduler.ProcessInfo>(pil.ProcessInfoID);
        return new XReservationProcessInfoItem() { ReservationProcessInfo = rpi, ProcessInfoLine = pil, ProcessInfo = pi };
    }

    private bool SwitchUser(IClient client)
    {
        if (Request.QueryString["login"] == "true")
        {
            if (CurrentUser.ClientID == client.ClientID)
            {
                // same user is logging in as themself?
                return false;
            }

            var orig = Convert.ToString(Session["LogInAsOriginalUser"]);

            if (orig == client.UserName)
            {
                // the original user is logging back in
                Session["LogInAsOriginalUser"] = null;
            }
            else
            {
                if (Session["LogInAsOriginalUser"] == null)
                    Session["LogInAsOriginalUser"] = CurrentUser.UserName;
            }

            FormsAuthentication.SignOut();

            HttpCookie authCookie = FormsAuthentication.GetAuthCookie(client.UserName, true);
            FormsAuthenticationTicket formInfoTicket = FormsAuthentication.Decrypt(authCookie.Value);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(formInfoTicket.Version, formInfoTicket.Name, formInfoTicket.IssueDate, formInfoTicket.Expiration, formInfoTicket.IsPersistent, string.Join("|", client.Roles()), formInfoTicket.CookiePath);
            authCookie.Value = FormsAuthentication.Encrypt(ticket);
            authCookie.Expires = formInfoTicket.Expiration;
            Response.Cookies.Add(authCookie);

            Response.Redirect("~/Utility.aspx?tab=user-report", true);

            return true;
        }

        return false;
    }

    private void LoadUserReport(IClient client)
    {
        if (client == null)
            throw new ArgumentNullException("client");

        if (SwitchUser(client)) return;

        var originalUser = GetLogInAsOriginalUser();

        litCurrentUser.Text = string.Format("{0} [{1}]", CurrentUser.DisplayName, CurrentUser.ClientID);

        if (originalUser != null)
        {
            phLogInAsOriginalUser.Visible = true;
            litLogInAsOriginalUser.Text = string.Format("{0} [{1}]", originalUser.DisplayName, originalUser.ClientID);
            hypLogInAsOriginalUser.NavigateUrl = string.Format("~/Utility.aspx?tab=user-report&clientId={0}&login=true", originalUser.ClientID);
        }

        ddlCurrentUser.DataSource = Provider.Data.Client.GetClients().OrderBy(x => x.DisplayName).Select(x => new { x.ClientID, DisplayName = string.Format("{0} [{1}]", x.DisplayName, x.ClientID) }).ToList();
        ddlCurrentUser.DataBind();

        var listItem = ddlCurrentUser.Items.FindByValue(client.ClientID.ToString());
        if (listItem != null) listItem.Selected = true;

        var ipaddr = Request.UserHostAddress;
        var inlab = Provider.PhysicalAccess.GetCurrentlyInArea("all");

        var paUtil = new PhysicalAccessUtility(inlab, ipaddr, Provider.Scheduler.Kiosk);
        var kiosks = Kiosks.Create(Provider.Scheduler.Kiosk);
        var isKiosk = kiosks.IsKiosk(ipaddr);
        var onKiosk = kiosks.IsOnKiosk(ipaddr);
        var isInLab = paUtil.IsInLab(client.ClientID);
        var clientInLab = paUtil.ClientInLab(client.ClientID);

        var dataSource = new[] { new
        {
            client.ClientID,
            client.UserName,
            client.DisplayName,
            client.Email,
            client.MaxChargeTypeID,
            client.MaxChargeTypeName,
            IsStaff = client.HasPriv(ClientPrivilege.Staff),
            IPAddress = ipaddr,
            IsKiosk = isKiosk,
            OnKiosk = onKiosk,
            IsInLab = isInLab,
            ClientInLab = clientInLab
        }};

        rptUserReport1.DataSource = dataSource;
        rptUserReport1.DataBind();

        rptUserReport2.DataSource = dataSource;
        rptUserReport2.DataBind();
    }

    private void LoadInLabReport()
    {
        var inlab = Provider.PhysicalAccess.GetCurrentlyInArea("all");

        rptInLabReport.DataSource = inlab.Select(x => new
        {
            LabDisplayName = GetLabDisplayName(x.AltDescription),
            x.ClientID,
            DisplayName = Clients.GetDisplayName(x.LastName, x.FirstName),
            AccessDateTime = x.CurrentAccessTime,
            HoursInLab = (DateTime.Now - x.CurrentAccessTime.Value).TotalHours
        }).OrderBy(x => x.LabDisplayName).ThenBy(x => x.DisplayName).ToList();

        rptInLabReport.DataBind();
    }

    private void LoadReservation(int reservationId, string command)
    {
        rptReservation.Visible = false;
        phNoReservation.Visible = false;
        rptReservationHistory.Visible = false;
        phNoReservationHistory.Visible = false;
        rptReservationInvitees.Visible = false;
        phNoReservationInvitees.Visible = false;
        rptReservationProcessInfo.Visible = false;
        phNoReservationProcessInfo.Visible = false;

        if (reservationId == 0)
        {
            txtReservationID.Text = string.Empty;
            return;
        }

        txtReservationID.Text = reservationId.ToString();

        IReservation rsv = DataSession.Single<ReservationInfo>(reservationId);

        if (rsv != null)
        {
            if (command == "history")
            {
                var history = DataSession.Query<ReservationHistory>().Where(x => x.Reservation.ReservationID == rsv.ReservationID).ToList();

                if (history.Count > 0)
                {
                    rptReservationHistory.Visible = true;
                    rptReservationHistory.DataSource = history;
                    rptReservationHistory.DataBind();
                }
                else
                {
                    phNoReservationHistory.Visible = true;
                }
            }
            else if (command == "invitees")
            {
                IEnumerable<IReservationInviteeItem> invitees = DataSession.Query<ReservationInviteeInfo>().Where(x => x.ReservationID == rsv.ReservationID).ToList();

                if (invitees.Count() > 0)
                {
                    rptReservationInvitees.Visible = true;
                    rptReservationInvitees.DataSource = invitees;
                    rptReservationInvitees.DataBind();
                }
                else
                {
                    phNoReservationInvitees.Visible = true;
                }
            }
            else if (command == "procinfo")
            {
                IList<ReservationProcessInfo> procinfo = DataSession.Query<ReservationProcessInfo>().Where(x => x.ReservationID == reservationId).ToList();

                if (procinfo.Count > 0)
                {
                    rptReservationProcessInfo.Visible = true;
                    rptReservationProcessInfo.DataSource = procinfo.Select(GetReservationProcessInfoItem).ToList();
                    rptReservationProcessInfo.DataBind();
                }
                else
                {
                    phNoReservationProcessInfo.Visible = true;
                }
            }
            else
            {
                if (command == "cancel")
                {
                    // same method called in ReservationView.ascx.vb when the red X is clicked on the calendar
                    ReservationManager.CancelReservation(rsv.ReservationID, CurrentUser.ClientID);
                }
                else if (command == "delete")
                {
                    // do a full purge, use at your own risk!
                    IList<ReservationHistory> history = DataSession.Query<ReservationHistory>().Where(x => x.Reservation.ReservationID == rsv.ReservationID).ToList();

                    DataCommand(CommandType.Text).Batch(x =>
                    {
                        x.Select.AddParameter("ReservationID", reservationId);

                        x.Select.SetCommandText("DELETE sselScheduler.dbo.ReservationHistory WHERE ReservationID = @ReservationID");
                        x.Select.ExecuteNonQuery();

                        x.Select.SetCommandText("DELETE sselScheduler.dbo.Reservation WHERE ReservationID = @ReservationID");
                        x.Select.ExecuteNonQuery();

                        x.Select.SetCommandText("DELETE sselScheduler.dbo.ReservationInvitee WHERE ReservationID = @ReservationID");
                        x.Select.ExecuteNonQuery();

                        x.Select.SetCommandText("DELETE sselScheduler.dbo.ReservationProcessInfo WHERE ReservationID = @ReservationID");
                        x.Select.ExecuteNonQuery();
                    });

                    Response.Redirect("~/Utility.aspx?tab=reservation-utility");
                }

                rptReservation.Visible = true;
                rptReservation.DataSource = new[] { rsv };
                rptReservation.DataBind();
            }
        }
        else
        {
            phNoReservation.Visible = true;
        }
    }

    private void DisplayAlert(AlertType alertType, string msg)
    {
        string className = string.Format("alert-{0}", Enum.GetName(typeof(AlertType), alertType).ToLower());
        litEmailError.Text += string.Format("<div class=\"alert {0}\" role=\"alert\" style=\"margin-top: 10px;\">{1}</div>", className, msg);
    }

    protected void btnSendEmail_Click(object sender, EventArgs e)
    {
        litEmailError.Text = string.Empty;

        int errors = 0;

        if (string.IsNullOrEmpty(txtEmail.Value))
        {
            DisplayAlert(AlertType.Danger, "Recipient Email is required.");
            errors++;
        }

        if (string.IsNullOrEmpty(txtSubject.Value))
        {
            DisplayAlert(AlertType.Danger, "Subject is required.");
            errors++;
        }

        if (string.IsNullOrEmpty(txtBody.Value))
        {
            DisplayAlert(AlertType.Danger, "Body is required.");
            errors++;
        }

        if (errors > 0)
            return;

        string email = string.Empty;

        MailAddress recip;

        try
        {
            recip = new MailAddress(txtEmail.Value);
        }
        catch (Exception ex)
        {
            DisplayAlert(AlertType.Danger, ex.Message);
            return;
        }

        var host = string.IsNullOrEmpty(ConfigurationManager.AppSettings["Email.Host"]) ? "127.0.0.1" : ConfigurationManager.AppSettings["Email.Host"];
        var fromAddr = string.IsNullOrEmpty(ConfigurationManager.AppSettings[""]) ? "lnf-scheduler@umich.edu" : ConfigurationManager.AppSettings[""];
        var smtp = new SmtpClient(host, 25);
        var from = new MailAddress(fromAddr);
        var mm = new MailMessage(from, recip);
        mm.Subject = txtSubject.Value;
        mm.Body = txtBody.Value;

        try
        {
            smtp.Send(mm);
            DisplayAlert(AlertType.Success, "Email was sent successfully!");
        }
        catch (Exception ex)
        {
            DisplayAlert(AlertType.Danger, ex.ToString());
        }
    }

    protected string GetActualDateRange(Reservation rsv)
    {
        string result = string.Empty;

        if (rsv.ActualBeginDateTime.HasValue)
            result = string.Format("{0:yyyy-MM-dd HH:mm:ss} to ", rsv.ActualBeginDateTime.Value);
        else
            result = "? to ";

        if (rsv.ActualEndDateTime.HasValue)
            result += string.Format("{0:yyyy-MM-dd HH:mm:ss}", rsv.ActualEndDateTime.Value);
        else
            result += "?";

        return result;
    }

    protected string GetCanceledOn(Reservation rsv)
    {
        if (rsv.CancelledDateTime.HasValue)
            return rsv.CancelledDateTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
        else
            return "--";
    }

    protected string GetModifiedByClientDisplayName(ReservationHistory rh)
    {
        IClient c = null;

        if (rh.ModifiedByClientID.HasValue)
            c = Provider.Data.Client.GetClient(rh.ModifiedByClientID.Value);

        if (c == null)
            return "[unknown]";
        else
            return c.DisplayName;
    }

    private string GetCurrentUserName()
    {
        var name = Context.User.Identity.Name;
        return name;
    }

    protected void UpdateBilling()
    {
        string command = Request.QueryString["command"];

        DateTime period = GetPeriod();
        int clientId = GetClientID();

        txtBillingClientID.Value = clientId.ToString();
        txtBillingPeriod.Value = period.ToString("yyyy-MM-dd");

        if (command == "update")
        {
            IEnumerable<string> response = Provider.Billing.Process.UpdateBilling(new UpdateBillingArgs { Periods = new[] { period }, ClientID = clientId, BillingCategory = BillingCategory.Tool | BillingCategory.Room });
            litBillingOutput.Text = string.Join("<br>", response);

            //var result = await ReservationHistoryUtility.UpdateBilling(sd, ed, clientId);
            //litBillingOutput.Text = "<hr><ul class=\"list-group\">";

            //AppendBillingProcessResult(result.ToolDataClean, "ToolDataClean");
            //AppendBillingProcessResult(result.RoomDataClean, "RoomDataClean");

            //AppendBillingProcessResult(result.ToolData, "ToolData");
            //AppendBillingProcessResult(result.RoomData, "RoomData");

            //AppendBillingProcessResult(result.ToolStep1, "ToolStep1");
            //AppendBillingProcessResult(result.RoomStep1, "RoomStep1");

            //AppendBillingProcessResult(result.Subsidy, "Subsidy");

            //litBillingOutput.Text += "</ul>";

            //litBillingOutput.Text += string.Format("<hr><div><strong>Total Time Taken: {0}</strong></div>", result.TotalTimeTaken());
        }
    }

    private void AppendBillingProcessResult(ProcessResult result, string label)
    {

        litBillingOutput.Text += "<li>";
        litBillingOutput.Text += string.Format("<strong>{0}:</strong>", label);
        if (result != null)
        {
            litBillingOutput.Text += string.Format("<div><pre>{0}</pre></div>", result.LogText);
            litBillingOutput.Text += string.Format("<div style=\"margin-bottom: 20px;\">Completed in {0:0.00} seconds", (DateTime.Now - result.Start).TotalSeconds);
        }
        else
        {
            litBillingOutput.Text += " n/a";
        }
        litBillingOutput.Text += "</li>";

    }

    private DateTime GetPeriod()
    {
        DateTime result;
        if (DateTime.TryParse(Request.QueryString["period"], out result))
            return result;
        else
            return DateTime.Now.FirstOfMonth().AddMonths(-1);
    }

    private int GetClientID()
    {
        int result;
        if (int.TryParse(Request.QueryString["clientId"], out result))
            return result;
        else
            return 0;
    }

    private string GetLabDisplayName(string value)
    {
        switch (value)
        {
            case "Wet Chemistry":
                return "ROBIN";
            case "Clean Room":
                return "Clean Room";
        }

        throw new Exception("Unknown value: " + value);
    }

    protected void BtnCurrentUserOK_Click(object sender, EventArgs e)
    {
        int clientId;
        if (int.TryParse(ddlCurrentUser.SelectedValue, out clientId))
        {
            Response.Redirect(string.Format("~/Utility.aspx?tab=user-report&clientId={0}", clientId), true);
        }
    }

    protected void BtnCurrentUserLogin_Click(object sender, EventArgs e)
    {
        int clientId;
        if (int.TryParse(ddlCurrentUser.SelectedValue, out clientId))
        {
            Response.Redirect(string.Format("~/Utility.aspx?tab=user-report&clientId={0}&login=true", clientId), true);
        }
    }

    public class XReservationProcessInfoItem
    {
        public ReservationProcessInfo ReservationProcessInfo { get; set; }
        public ProcessInfoLine ProcessInfoLine { get; set; }
        public LNF.Impl.Repository.Scheduler.ProcessInfo ProcessInfo { get; set; }
    }
</script>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Scheduler Utility</title>

    <!-- Bootstrap -->
    <link href="//ssel-apps.eecs.umich.edu/static/styles/bootstrap/themes/courier/bootstrap.min.css" rel="stylesheet">

    <style>
        .tab-panel {
            padding: 20px;
        }

        .interlock-response {
            margin-top: 20px;
        }

        .interlock-info strong {
            width: 100px;
            display: inline-block;
        }

        .interlock-timetaken {
            margin-top: 10px;
            padding-top: 5px;
            border-top: solid 1px #eee;
        }

        .treeview-table {
            width: 100%;
            border-collapse: separate;
            border-spacing: 5px;
        }

            .treeview-table > tbody > tr > th {
                text-align: right;
                background-color: #f5f5f5;
                border-bottom: solid 1px #eee;
                border-right: solid 1px #eee;
                padding: 3px;
            }

            .treeview-table > tbody > tr > td {
                border-bottom: solid 1px #eee;
                border-right: solid 1px #eee;
                padding: 3px;
            }

        .jobs .form-group {
            margin-bottom: 0;
        }

        .reservation-history-detail {
            display: none;
            background-color: #f7f7f7;
        }

            .reservation-history-detail.toggled {
                display: table-row;
            }

        pre {
            font-family: 'Courier New';
        }

        .login-button {
            height: 33px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" class="form-horizontal" role="form">
        <div class="container">
            <div class="page-header">
                <h1>Scheduler Utility</h1>
            </div>

            <div style="margin-bottom: 20px;">
                <asp:HyperLink runat="server" ID="hypReturn" NavigateUrl="~">&larr; Return</asp:HyperLink>
            </div>

            <asp:PlaceHolder runat="server" ID="phNoAccess" Visible="false">
                <div class="alert alert-danger" role="alert">
                    You do not have access to this page.
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="phUtility">

                <ul class="nav nav-tabs">
                    <li runat="server" id="liUserReport" role="presentation"><a href="Utility.aspx?tab=user-report">User Report</a></li>
                    <li runat="server" id="liReservationUtility" role="presentation"><a href="Utility.aspx?tab=reservation-utility">Reservation Utility</a></li>
                    <li runat="server" id="liInterlocks" role="presentation"><a href="Utility.aspx?tab=interlocks">Interlocks</a></li>
                    <li runat="server" id="liEmail" role="presentation"><a href="Utility.aspx?tab=email">Email</a></li>
                    <li runat="server" id="liJobs" role="presentation"><a href="Utility.aspx?tab=jobs">Jobs</a></li>
                    <li runat="server" id="liBilling" role="presentation"><a href="Utility.aspx?tab=billing">Billing</a></li>
                </ul>

                <asp:Panel runat="server" ID="panUserReport" Visible="false" CssClass="tab-panel">
                    <div class="user-report">
                        <h4>Current User:
                            <asp:Literal runat="server" ID="litCurrentUser"></asp:Literal></h4>

                        <asp:PlaceHolder runat="server" ID="phLogInAsOriginalUser" Visible="false">
                            <h4>Original User:
                                <asp:Literal runat="server" ID="litLogInAsOriginalUser"></asp:Literal>
                                <span>
                                    <asp:HyperLink runat="server" ID="hypLogInAsOriginalUser" ImageUrl="~/images/icons8-enter-26.png" ImageHeight="18" ToolTip="Switch user..."></asp:HyperLink>
                                </span>
                            </h4>
                        </asp:PlaceHolder>
                        <hr />
                        <div class="row">
                            <div class="col-sm-8">
                                <div class="form-horizontal">
                                    <div class="form-group">
                                        <label class="control-label col-sm-3">ClientID</label>
                                        <div class="col-sm-6">
                                            <div class="input-group">
                                                <asp:DropDownList runat="server" ID="ddlCurrentUser" CssClass="form-control" DataValueField="ClientID" DataTextField="DisplayName"></asp:DropDownList>
                                                <span class="input-group-btn">
                                                    <asp:Button runat="server" ID="btnCurrentUserOK" Text="OK" CssClass="btn btn-default" OnClick="BtnCurrentUserOK_Click" />
                                                </span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="row">
                            <div class="col-sm-4">
                                <div class="form-horizontal">
                                    <asp:Repeater runat="server" ID="rptUserReport1">
                                        <ItemTemplate>
                                            <div class="form-group">
                                                <label class="control-label col-sm-6">Username</label>
                                                <div class="col-sm-6">
                                                    <p class="form-control-static">
                                                        <%#Eval("UserName")%>
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-6">Display Name</label>
                                                <div class="col-sm-6">
                                                    <p class="form-control-static">
                                                        <%#Eval("DisplayName")%>
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-6">Email</label>
                                                <div class="col-sm-6">
                                                    <p class="form-control-static">
                                                        <%#Eval("Email")%>
                                                    </p>
                                                </div>

                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-6">Max Charge Type</label>
                                                <div class="col-sm-6">
                                                    <p class="form-control-static">
                                                        <%#Eval("MaxChargeTypeName")%> [<%#Eval("MaxChargeTypeID")%>]
                                                    </p>
                                                </div>

                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-6">Is Staff</label>
                                                <div class="col-sm-6">
                                                    <p class="form-control-static">
                                                        <%#Eval("IsStaff")%>
                                                    </p>
                                                </div>

                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <div class="form-group">
                                        <div class="col-sm-offset-6 col-sm-6">
                                            <asp:Button runat="server" ID="btnCurrentUserLogin" OnClick="BtnCurrentUserLogin_Click" CssClass="btn btn-default" Text="Switch user..." />
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-sm-8">
                                <div class="form-horizontal">
                                    <asp:Repeater runat="server" ID="rptUserReport2">
                                        <ItemTemplate>
                                            <div class="form-group">
                                                <label class="control-label col-sm-4">IP Address</label>
                                                <div class="col-sm-8">
                                                    <p class="form-control-static">
                                                        <%#Eval("IPAddress")%>
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-4">Is Kiosk</label>
                                                <div class="col-sm-8">
                                                    <p class="form-control-static">
                                                        <%#Eval("IsKiosk")%>
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-4">On Kiosk</label>
                                                <div class="col-sm-8">
                                                    <p class="form-control-static">
                                                        <%#Eval("OnKiosk")%>
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-4">Is In Lab</label>
                                                <div class="col-sm-8">
                                                    <p class="form-control-static">
                                                        <%#Eval("IsInLab")%>
                                                    </p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="control-label col-sm-4">Client In Lab</label>
                                                <div class="col-sm-8">
                                                    <p class="form-control-static">
                                                        <%#Eval("ClientInLab")%>
                                                    </p>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </div>
                    </div>

                    <hr />

                    <div class="inlab-report">
                        <h4>Currently In Lab</h4>
                        <table class="table table-striped">
                            <thead>
                                <tr>
                                    <th>Lab</th>
                                    <th>Name</th>
                                    <th>Access Time</th>
                                    <th>Time In Lab</th>
                                    <th>&nbsp;</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater runat="server" ID="rptInLabReport">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%#Eval("LabDisplayName")%></td>
                                            <td>
                                                <a href='<%#VirtualPathUtility.ToAbsolute(Eval("ClientID", "~/Utility.aspx?tab=user-report&clientId={0}"))%>'>
                                                    <%#Eval("DisplayName")%> [<%#Eval("ClientID")%>]
                                                </a>
                                            </td>
                                            <td><%#Eval("AccessDateTime")%></td>
                                            <td><%#Eval("HoursInLab", "{0:0.00} hours")%></td>
                                            <td>
                                                <a href='<%#VirtualPathUtility.ToAbsolute(Eval("ClientID", "~/Utility.aspx?tab=user-report&clientId={0}&login=true"))%>'>
                                                    <img src="images/icons8-enter-26.png" border="0" title="Switch user..." style="height: 18px;" />
                                                </a>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="panReservationUtility" Visible="false" CssClass="tab-panel">
                    <div class="reservation">
                        <div class="form-group">
                            <label class="col-sm-2 control-label">ReservationID</label>
                            <div class="col-sm-2">
                                <asp:TextBox runat="server" ID="txtReservationID" CssClass="form-control reservation-id"></asp:TextBox>
                            </div>
                        </div>
                        <a href="#" class="btn btn-default command-button" data-command="view">View</a>
                        <a href="#" class="btn btn-default command-button" data-command="history">History</a>
                        <a href="#" class="btn btn-default command-button" data-command="invitees">Invitees</a>
                        <a href="#" class="btn btn-default command-button" data-command="procinfo">Process Info</a>
                        <a href="#" class="btn btn-default command-button" data-command="cancel">Cancel</a>
                        <a href="#" class="btn btn-default command-button" data-command="delete">Delete</a>

                        <div class="reservation-view">
                            <asp:Repeater runat="server" ID="rptReservation">
                                <HeaderTemplate>
                                    <hr />
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Resource</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("Resource.ResourceName")%> [<%#Eval("Resource.ResourceID")%>]</p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Client</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("Client.DisplayName")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Account</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#((Reservation)Container.DataItem).Account.GetFullAccountName()%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Created On</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("CreatedOn", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Last Modified On</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("LastModifiedOn", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Canceled On</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#GetCanceledOn((Reservation)Container.DataItem)%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Active</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("IsActive")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Started</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("IsStarted")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">AutoEnd</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("AutoEnd")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">KeepAlive</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("KeepAlive")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Scheduled</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#Eval("BeginDateTime", "{0:yyyy-MM-dd HH:mm:ss}")%> to <%#Eval("EndDateTime", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                        </div>
                                    </div>
                                    <div class="form-group">
                                        <label class="col-sm-2 control-label">Actual</label>
                                        <div class="col-sm-10">
                                            <p class="form-control-static"><%#GetActualDateRange((Reservation)Container.DataItem)%></p>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder runat="server" ID="phNoReservation" Visible="false">
                                <div class="alert alert-danger" role="alert" style="margin-top: 20px;">
                                    No reservation was found with this ReservationID.
                                </div>
                            </asp:PlaceHolder>
                        </div>

                        <div class="reservation-history">
                            <asp:Repeater runat="server" ID="rptReservationHistory">
                                <HeaderTemplate>
                                    <hr />
                                    <table class="table">
                                        <thead>
                                            <tr>
                                                <th>ID</th>
                                                <th>Action</th>
                                                <th>Source</th>
                                                <th>Modified By</th>
                                                <th>Modified On</th>
                                                <th>&nbsp;</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td><%#Eval("ReservationHistoryID")%></td>
                                        <td><%#Eval("UserAction")%></td>
                                        <td><%#Eval("ActionSource")%></td>
                                        <td><%#GetModifiedByClientDisplayName((ReservationHistory)Container.DataItem)%></td>
                                        <td><%#Eval("ModifiedDateTime", "{0:yyyy-MM-dd HH:mm:ss}")%></td>
                                        <td style="text-align: right;"><a href="#" class="reservation-history-detail-toggle" style="text-decoration: none;">+</a></td>
                                    </tr>
                                    <tr class="reservation-history-detail">
                                        <td colspan="6">
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Account</label>
                                                <div class="col-sm-10">
                                                    <p class="form-control-static"><%#((ReservationHistory)Container.DataItem).Account.GetNameWithShortCode()%></p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Scheduled</label>
                                                <div class="col-sm-10">
                                                    <p class="form-control-static"><%#Eval("BeginDateTime", "{0:yyyy-MM-dd HH:mm:ss}")%> to <%#Eval("EndDateTime", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Forgiven</label>
                                                <div class="col-sm-10">
                                                    <p class="form-control-static"><%#string.Format("{0:0.0%}", 1 - ((ReservationHistory)Container.DataItem).ChargeMultiplier)%></p>
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </tbody>
                            </table>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder runat="server" ID="phNoReservationHistory" Visible="false">
                                <div class="alert alert-danger" role="alert" style="margin-top: 20px;">
                                    No reservation history was found with this ReservationID.
                                </div>
                            </asp:PlaceHolder>
                        </div>

                        <div class="reservation-invitees">
                            <asp:Repeater runat="server" ID="rptReservationInvitees">
                                <HeaderTemplate>
                                    <hr />
                                    <table class="table table-striped">
                                        <thead>
                                            <tr>
                                                <th style="width: 100px;">ClientID</th>
                                                <th>Name</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td><%#Eval("Invitee.ClientID")%></td>
                                        <td><%#Eval("Invitee.DisplayName")%></td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </tbody>
                                </table>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder runat="server" ID="phNoReservationInvitees" Visible="false">
                                <div class="alert alert-danger" role="alert" style="margin-top: 20px;">
                                    No reservation invitees were found with this ReservationID.
                                </div>
                            </asp:PlaceHolder>
                        </div>

                        <div class="reservation-process-info">
                            <asp:Repeater runat="server" ID="rptReservationProcessInfo">
                                <HeaderTemplate>
                                    <hr />
                                    <table class="table table-striped">
                                        <thead>
                                            <tr>
                                                <th>ID</th>
                                                <th>Name</th>
                                                <th>Param</th>
                                                <th>Value</th>
                                                <th>Selected Param</th>
                                                <th>Selected Value</th>
                                                <th>Special</th>
                                                <th>Special Value</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <tr>
                                        <td><%#Eval("ReservationProcessInfo.ReservationProcessInfoID")%></td>
                                        <td><%#Eval("ProcessInfo.ProcessInfoName")%></td>
                                        <td><%#Eval("ProcessInfo.ParamName")%></td>
                                        <td><%#Eval("ProcessInfo.ValueName")%></td>
                                        <td><%#Eval("ProcessInfoLine.Param")%></td>
                                        <td><%#Eval("ReservationProcessInfo.Value")%></td>
                                        <td><%#Eval("ProcessInfo.Special")%></td>
                                        <td><%#Eval("ReservationProcessInfo.Special")%></td>
                                    </tr>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </tbody>
                                </table>
                                </FooterTemplate>
                            </asp:Repeater>
                            <asp:PlaceHolder runat="server" ID="phNoReservationProcessInfo" Visible="false">
                                <div class="alert alert-danger" role="alert" style="margin-top: 20px;">
                                    No reservation process info was found with this ReservationID.
                                </div>
                            </asp:PlaceHolder>
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="panInterlocks" Visible="false" CssClass="tab-panel">
                    <div class="interlock">
                        <div class="form-group resource-id">
                            <label for="resource_id" class="col-sm-1 control-label">ResourceID</label>
                            <div class="col-sm-2">
                                <input type="tel" class="form-control" id="resource_id" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-sm-offset-1 col-sm-11">
                                <div class="btn-group">
                                    <button type="button" class="btn btn-default get-state-button">Get State</button>
                                </div>
                                <div style="display: inline-block; border-left: solid 1px #ccc; padding-left: 10px;">
                                    Set State:
                        <div class="btn-group">
                            <button type="button" class="btn btn-default set-state-button" data-state="1">On</button>
                            <button type="button" class="btn btn-default set-state-button" data-state="0">Off</button>
                        </div>
                                </div>
                            </div>
                        </div>
                        <div class="interlock-response"></div>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="panEmail" Visible="false" CssClass="tab-panel">
                    <div class="email">
                        <div class="form-group">
                            <input runat="server" type="text" class="form-control" id="txtEmail" placeholder="Recipient Email">
                        </div>
                        <div class="form-group">
                            <input runat="server" type="text" class="form-control" id="txtSubject" placeholder="Subject">
                        </div>
                        <div class="form-group">
                            <textarea runat="server" id="txtBody" class="form-control" placeholder="Body" rows="5" cols="5"></textarea>
                        </div>
                        <div class="form-group">
                            <asp:Button runat="server" ID="btnSendEmail" Text="Send Email" CssClass="btn btn-primary" OnClick="btnSendEmail_Click" />
                        </div>
                        <asp:Literal runat="server" ID="litEmailError"></asp:Literal>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="panJobs" Visible="false" CssClass="tab-panel">
                    <div class="jobs">
                        <div style="margin-bottom: 10px;">
                            <a href="/tasks/hangfire">Hangfire Dashboard</a>
                        </div>
                        <asp:Repeater runat="server" ID="rptJobs">
                            <HeaderTemplate>
                                <ul class="list-group">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <li class="list-group-item">
                                    <h4><%#Eval("Id")%></h4>
                                    <div class="form-horizontal">
                                        <div class="form-group">
                                            <label class="control-label col-sm-2">created at</label>
                                            <div class="col-sm-10">
                                                <p class="form-control-static"><%#Eval("CreatedAt", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label col-sm-2">last execution</label>
                                            <div class="col-sm-10">
                                                <p class="form-control-static"><%#Eval("LastExecution", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label col-sm-2">next execution</label>
                                            <div class="col-sm-10">
                                                <p class="form-control-static"><%#Eval("NextExecution", "{0:yyyy-MM-dd HH:mm:ss}")%></p>
                                            </div>
                                        </div>
                                        <div class="form-group">
                                            <label class="control-label col-sm-2">cron</label>
                                            <div class="col-sm-10">
                                                <p class="form-control-static"><%#Eval("Cron")%></p>
                                            </div>
                                        </div>
                                    </div>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate>
                                </ul>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>

                <asp:Panel runat="server" ID="panBilling" Visible="false" CssClass="tab-panel">
                    <div class="billing">
                        <div class="form-horizontal">
                            <div class="form-group">
                                <label class="control-label col-sm-2">ClientID</label>
                                <div class="col-sm-2">
                                    <input runat="server" type="text" id="txtBillingClientID" class="form-control billing-client-id" />
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="control-label col-sm-2">Period</label>
                                <div class="col-sm-2">
                                    <input runat="server" type="text" id="txtBillingPeriod" class="form-control billing-period" />
                                </div>
                            </div>
                            <div class="form-group">
                                <div class="col-sm-offset-2 col-sm-10">
                                    <button type="button" class="btn btn-default command-button" data-command="update">Update Billing</button>
                                </div>
                            </div>
                        </div>
                        <asp:Literal runat="server" ID="litBillingOutput"></asp:Literal>
                    </div>
                </asp:Panel>

            </asp:PlaceHolder>
        </div>
    </form>

    <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>

    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="//ssel-apps.eecs.umich.edu/static/lib/bootstrap/js/bootstrap.min.js"></script>

    <script>
        (function ($) {
            var interlock = $(".interlock").each(function () {
                var $this = $(this);

                var getState = function (resourceId) {
                    return $.ajax({
                        "url": "ajax/interlock.ashx",
                        "data": { "command": "get-state", "id": resourceId },
                        "dataType": "json"
                    });
                }

                var setState = function (resourceId, state) {
                    return $.ajax({
                        "url": "ajax/interlock.ashx",
                        "data": { "command": "set-state", "id": resourceId, "state": state },
                        "dataType": "json"
                    });
                }

                var resourceId = function () {
                    var def = $.Deferred();

                    $(".resource-id", $this).removeClass("has-error");

                    var resourceId = parseInt($(".resource-id input", $this).val());

                    if (isNaN(resourceId)) {
                        $(".resource-id", $this).addClass("has-error");
                        def.reject();
                    } else {
                        def.resolve(resourceId);
                    }

                    return def.promise();
                }

                var stateLabel = function (state) {
                    if (state)
                        return $("<span/>", { "class": "label label-success" }).html("Enabled");
                    else
                        return $("<span/>", { "class": "label label-danger" }).html("Disabled");
                }

                $this.on("set-state", function (e, resourceId, state, refresh) {
                    var response = null;

                    $(".resource-id input", $this).val(resourceId);

                    setState(resourceId, state).done(function (data, textStatus, jqXHR) {
                        if (refresh) {
                            $this.trigger("get-state", resourceId);
                        } else {
                            response = $("<div/>", { "class": "panel panel-default" }).append(
                                $("<div/>", { "class": "panel-heading" }).append(
                                    $("<div/>", { "class": "panel-title" }).html("Set Point State")
                                )
                            ).append(
                                $("<div/>", { "class": "panel-body" }).append(
                                    $("<div/>", { "class": "interlock-info" }).html("<strong>Tool:</strong> " + data.InstanceName + " [" + data.ActionID + "]")
                                ).append(
                                    $("<div/>", { "class": "interlock-info" }).html("<strong>Wago Block:</strong> " + data.BlockName + " [" + data.BlockID + "]")
                                ).append(
                                    $("<div/>", { "class": "interlock-info" }).html("<strong>PointID:</strong> " + data.PointID)
                                ).append(
                                    $("<div/>", { "class": "interlock-timetaken" }).html("Completed in " + data.TimeTaken.toFixed(3) + " seconds")
                                )
                            );
                        }
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        response = $("<div/>", { "class": "alert alert-danger", "role": "alert" }).html($(jqXHR.responseText).filter("title").text() || errorThrown);
                    }).always(function () {
                        if (!refresh)
                            $(".interlock-response", $this).html(response);
                    });
                }).on("get-state", function (e, resourceId) {
                    var response = null;

                    $(".resource-id input", $this).val(resourceId);

                    getState(resourceId).done(function (data, textStatus, jqXHR) {
                        response = $("<div/>", { "class": "panel panel-default" }).append(
                            $("<div/>", { "class": "panel-heading" }).append(
                                $("<div/>", { "class": "panel-title" }).html("Get Point State")
                            )
                        ).append(
                            $("<div/>", { "class": "panel-body" }).append(
                                $("<div/>", { "class": "interlock-info" }).html("<strong>State:</strong> ").append(stateLabel(data.State))
                            ).append(
                                $("<div/>", { "class": "interlock-info" }).html("<strong>Tool:</strong> " + data.InstanceName + " [" + data.ActionID + "]")
                            ).append(
                                $("<div/>", { "class": "interlock-info" }).html("<strong>Wago Block:</strong> " + data.BlockName + " [" + data.BlockID + "]")
                            ).append(
                                $("<div/>", { "class": "interlock-info" }).html("<strong>PointID:</strong> " + data.PointID)
                            ).append(
                                $("<div/>", { "class": "interlock-timetaken" }).html("Completed in " + data.TimeTaken.toFixed(3) + " seconds")
                            )
                        );
                    }).fail(function (jqXHR, textStatus, errorThrown) {
                        response = $("<div/>", { "class": "alert alert-danger", "role": "alert" }).html($(jqXHR.responseText).filter("title").text() || errorThrown);
                    }).always(function () {
                        $(".interlock-response").html(response);
                    });
                }).on("click", ".set-state-button", function (e) {
                    var state = $(this).data("state") == 1;
                    resourceId().done(function (result) {
                        $this.trigger("set-state", [result, state, true]);
                    });
                }).on("click", ".get-state-button", function (e) {
                    resourceId().done(function (result) {
                        $this.trigger("get-state", result);
                    });
                });
            });

            $.interlock = {
                "setState": function (resourceId, state, refresh) {

                    if (refresh == null)
                        refresh = true;

                    interlock.trigger("set-state", [resourceId, state, refresh]);
                }
            };

            var reservation = $(".reservation").each(function () {
                console.log('test');
                var $this = $(this);

                $this.on("click", ".command-button", function (e) {
                    console.log('hi');
                    e.preventDefault();

                    var link = $(this);

                    var reservationId = parseInt($(".reservation-id", $this).val());

                    if (!isNaN(reservationId)) {
                        var command = link.data("command");
                        window.location = "Utility.aspx?tab=reservation-utility&reservationId=" + reservationId + "&command=" + command;
                    }
                }).on("click", ".reservation-history-detail-toggle", function (e) {
                    e.preventDefault();
                    var row = $(this).closest("tr");
                    var next = row.next(".reservation-history-detail");
                    next.toggleClass("toggled");

                    if ($(this).text() == "+")
                        $(this).text("-");
                    else
                        $(this).text("+");
                });
            });


            var billing = $(".billing").each(function () {
                var $this = $(this);

                $this.on("click", ".command-button", function (e) {
                    var command = $(this).data("command");
                    var clientId = $(".billing-client-id", $this).val();
                    var period = $(".billing-period", $this).val();
                    window.location = "Utility.aspx?tab=billing&command=" + command + "&clientId=" + clientId + "&period=" + period;
                });
            });
        }(jQuery));
    </script>
</body>
</html>
