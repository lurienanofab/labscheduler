<%--
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
--%>

<%@ Page Title="Utility" Language="C#" %>

<%@ Import Namespace="System.Net" %>
<%@ Import Namespace="System.Net.Mail" %>
<%@ Import Namespace="LNF.Cache" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Models.Scheduler" %>
<%@ Import Namespace="LNF.Repository" %>
<%@ Import Namespace="LNF.Repository.Scheduler" %>
<%@ Import Namespace="LNF.Web.Scheduler.TreeView" %>
<%@ Import Namespace="LabScheduler.AppCode" %>

<script runat="server">
    //note: this page does not have a separate CodeBehind file so that server side code can be edited in production

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

    public void Page_Load(object sender, EventArgs e)
    {
        string tab = Request.QueryString["tab"];
        if (string.IsNullOrEmpty(tab))
            tab = "reservation-utility";

        liReservationUtility.Attributes.Remove("class");
        liProperties.Attributes.Remove("class");
        liInterlocks.Attributes.Remove("class");
        liEmail.Attributes.Remove("class");
        liJobs.Attributes.Remove("class");
        panReservationUtility.Visible = false;
        panProperties.Visible = false;
        panInterlocks.Visible = false;
        panEmail.Visible = false;
        panJobs.Visible = false;

        switch (tab)
        {
            case "reservation-utility":
                liReservationUtility.Attributes.Add("class", "active");
                panReservationUtility.Visible = true;
                LoadReservation(GetReservationID(), Request.QueryString["command"]);
                break;
            case "properties":
                liProperties.Attributes.Add("class", "active");
                panProperties.Visible = true;
                LoadPropertiesData(Request.QueryString["load"]);
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
        }
    }

    private ReservationProcessInfoItem GetReservationProcessInfoItem(ReservationProcessInfo rpi)
    {
        var pil = rpi.ProcessInfoLine;
        var pi = DA.Current.Single<LNF.Repository.Scheduler.ProcessInfo>(pil.ProcessInfoID);
        return new ReservationProcessInfoItem() { ReservationProcessInfo = rpi, ProcessInfoLine = pil, ProcessInfo = pi };
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

        Reservation rsv = DA.Scheduler.Reservation.Single(reservationId);

        if (rsv != null)
        {
            if (command == "history")
            {
                IList<ReservationHistory> history = DA.Scheduler.ReservationHistory.Query().Where(x => x.Reservation == rsv).ToList();

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
                IList<ReservationInvitee> invitees = rsv.GetInvitees().ToList();

                if (invitees.Count > 0)
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
                IList<ReservationProcessInfo> procinfo = DA.Current.Query<ReservationProcessInfo>().Where(x => x.Reservation.ReservationID == reservationId).ToList();

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
                    ReservationUtility.DeleteReservation(reservationId);
                }
                else if (command == "delete")
                {
                    // do a full purge, use at your own risk!
                    IList<ReservationHistory> history = DA.Scheduler.ReservationHistory.Query().Where(x => x.Reservation == rsv).ToList();

                    using (var dba = DA.Current.GetAdapter())
                    {
                        var param = dba.SelectCommand.CreateParameter();
                        param.ParameterName = "@ReservationID";
                        param.Value = reservationId;
                        dba.SelectCommand.Parameters.Add(param);

                        dba.SelectCommand.CommandType = System.Data.CommandType.Text;

                        dba.SelectCommand.CommandText = "DELETE sselScheduler.dbo.ReservationHistory WHERE ReservationID = @ReservationID";
                        dba.SelectCommand.ExecuteNonQuery();

                        dba.SelectCommand.CommandText = "DELETE sselScheduler.dbo.Reservation WHERE ReservationID = @ReservationID";
                        dba.SelectCommand.ExecuteNonQuery();

                        dba.SelectCommand.CommandText = "DELETE sselScheduler.dbo.ReservationInvitee WHERE ReservationID = @ReservationID";
                        dba.SelectCommand.ExecuteNonQuery();

                        dba.SelectCommand.CommandText = "DELETE sselScheduler.dbo.ReservationProcessInfo WHERE ReservationID = @ReservationID";
                        dba.SelectCommand.ExecuteNonQuery();
                    }

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

    private void LoadPropertiesData(string load = null)
    {
        if (!string.IsNullOrEmpty(load) && load == "properties")
            Properties.Load();

        litProperitesTimestamp.Text = string.Format("Timestamp: {0:yyyy-MM-dd HH:mm:ss}", Properties.Current.Timestamp());

        rptProperties.DataSource = new[] { Properties.Current };
        rptProperties.DataBind();
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

    protected string GetStaticUrl(string path)
    {
        return LNF.CommonTools.Utility.GetStaticUrl(path);
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
        var c = rh.GetModifiedByClient();
        if (c == null)
            return "[unknown]";
        else
            return c.DisplayName;
    }

    public class ReservationProcessInfoItem
    {
        public ReservationProcessInfo ReservationProcessInfo { get; set; }
        public ProcessInfoLine ProcessInfoLine { get; set; }
        public LNF.Repository.Scheduler.ProcessInfo ProcessInfo { get; set; }
    }
</script>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title></title>

    <!-- Bootstrap -->
    <link href="<%=GetStaticUrl("styles/bootstrap/themes/courier/bootstrap.min.css")%>" rel="stylesheet">

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
    </style>
</head>
<body>
    <form id="form1" runat="server" class="form-horizontal" role="form">
        <div class="container">
            <div class="page-header">
                <h1>Scheduler Utility Page</h1>
            </div>

            <ul class="nav nav-tabs">
                <li runat="server" id="liReservationUtility" role="presentation"><a href="Utility.aspx?tab=reservation-utility">Reservation Utility</a></li>
                <li runat="server" id="liProperties" role="presentation"><a href="Utility.aspx?tab=properties">Properties</a></li>
                <li runat="server" id="liInterlocks" role="presentation"><a href="Utility.aspx?tab=interlocks">Interlocks</a></li>
                <li runat="server" id="liEmail" role="presentation"><a href="Utility.aspx?tab=email">Email</a></li>
                <li runat="server" id="liJobs" role="presentation"><a href="Utility.aspx?tab=jobs">Jobs</a></li>
            </ul>

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

            <asp:Panel runat="server" ID="panProperties" Visible="false" CssClass="tab-panel">
                <div style="padding-left: 5px;">
                    <asp:Literal runat="server" ID="litProperitesTimestamp"></asp:Literal>
                    |
                    <a href="?tab=properties&load=properties">Reload</a>
                </div>
                <asp:Repeater runat="server" ID="rptProperties">
                    <HeaderTemplate>
                        <table>
                            <tbody>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <th style="width: 250px;">LateChargePenaltyMultiplier</th>
                            <td><%#Eval("LateChargePenaltyMultiplier") %></td>
                        </tr>
                        <tr>
                            <th>AuthExpWarning</th>
                            <td><%#Eval("AuthExpWarning") %></td>
                        </tr>
                        <tr>
                            <th>Admin</th>
                            <td><%#Eval("Admin.DisplayName") %></td>
                        </tr>
                        <tr>
                            <th>ResourceIPPrefix</th>
                            <td><%#Eval("ResourceIPPrefix") %></td>
                        </tr>
                        <tr>
                            <th>AlwaysOnKiosk</th>
                            <td><%#Eval("AlwaysOnKiosk") %></td>
                        </tr>
                        <tr>
                            <th>SchedulerEmail</th>
                            <td><%#Eval("SchedulerEmail") %></td>
                        </tr>
                        <tr>
                            <th>LabAccount</th>
                            <td><%#Eval("LabAccount.Name") %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                        </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
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
                        <a href="hangfire">Hangfire Dashboard</a>
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
        </div>
    </form>

    <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="<%=GetStaticUrl("lib/jquery/jquery.min.js")%>"></script>

    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="<%=GetStaticUrl("lib/bootstrap/js/bootstrap.min.js")%>"></script>

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
        }(jQuery));
    </script>
</body>
</html>
