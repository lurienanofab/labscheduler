<%@ Page Language="C#" Title="LNF Scheduler Status" %>

<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Threading.Tasks" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Repository" %>
<%@ Import Namespace="LNF.Impl.Repository.Scheduler" %>
<%@ Import Namespace="LNF.CommonTools" %>
<%@ Import Namespace="LNF.Web" %>

<script runat="server">
    ReservationItem[] currentReservations;
    ReservationItem[] nextReservations;

    void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString[null] == "poke")
        {
            Response.Clear();
            Response.ContentType = "text/plain";
            Response.Write("ouch");
            Response.End();
            return;
        }

        currentReservations = DA.Current.Query<Reservation>()
                .Where(x => x.IsActive && x.IsStarted && x.ActualBeginDateTime != null && x.ActualEndDateTime == null)
                .OrderBy(x => x.ActualBeginDateTime).ToArray()
                .Select(GetReservationItem).ToArray();

        nextReservations = DA.Current.Query<Reservation>()
            .Where(x => x.IsActive && !x.IsStarted && x.ActualBeginDateTime == null && x.ActualEndDateTime == null && x.EndDateTime >= DateTime.Now)
            .OrderBy(x => x.BeginDateTime).ToArray()
            .Select(GetReservationItem).ToArray();

        LoadToolStatus();
    }

    void LoadToolStatus()
    {
        DataTable dt = DA.Command(CommandType.Text).FillDataTable("SELECT ResourceID, ResourceName FROM sselScheduler.dbo.Resource WHERE IsActive = 1 ORDER BY ResourceName");
        WagoInterlock.AllToolStatus(dt);
        rptToolStatus.DataSource = dt.AsEnumerable().Select(GetToolStatusItem);
        rptToolStatus.DataBind();
    }

    ReservationItem GetReservationItem(Reservation rsv)
    {
        return new ReservationItem()
        {
            ResourceID = rsv.Resource.ResourceID,
            ResourceName = string.Format("{0} [{1}]", rsv.Resource.ResourceName, rsv.Resource.ResourceID),
            DisplayName = rsv.Client.DisplayName,
            Scheduled = string.Format("from {0:M/d/yyyy h:mm:ss tt} to {1:M/d/yyyy h:mm:ss tt}", rsv.BeginDateTime, rsv.EndDateTime),
            Activity = rsv.Activity.ActivityName,
            Started = rsv.ActualBeginDateTime.HasValue ? string.Format("{0:M/d/yyyy hh:mm:ss tt}", rsv.ActualBeginDateTime.Value) : string.Empty
        };
    }

    bool GetBooleanValue(DataRow dr, string key)
    {
        bool result = false;
        if (dr[key] == DBNull.Value)
            return result;
        else
            return dr.Field<bool>(key);
    }

    ToolStatusItem GetToolStatusItem(DataRow dr)
    {
        string status = dr.Field<string>("InterlockStatus");
        bool state = GetBooleanValue(dr, "InterlockState");
        bool error = GetBooleanValue(dr, "InterlockError");
        bool interlocked = GetBooleanValue(dr, "IsInterlocked");

        if (interlocked)
        {
            if (error)
            {
                status = string.Format("<span class=\"label label-warning\">{0}</span>", status);
            }
            else
            {
                if (state)
                    status = string.Format("<span class=\"label label-success\">{0}</span>", "Interlock ON");
                else
                    status = string.Format("<span class=\"label label-danger\">{0}</span>", "Interlock OFF");
            }
        }
        else
        {
            status = string.Format("<span class=\"label label-default\">{0}</span>", status);
        }

        ToolStatusItem result = new ToolStatusItem()
        {
            ResourceID = dr.Field<int>("ResourceID"),
            ResourceName = string.Format("{0} [{1}] {2}", dr.Field<string>("ResourceName"), dr.Field<int>("ResourceID"), status),
            Status = status
        };

        ReservationItem rsv = currentReservations.FirstOrDefault(x => x.ResourceID == result.ResourceID);

        if (rsv != null)
        {
            result.Activity = rsv.Activity;
            result.DisplayName = rsv.DisplayName;
            result.ResourceID = rsv.ResourceID;
        }

        return result;
    }

    protected void rptToolStatus_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
        {
            Repeater rptCurrentReservation = (Repeater)e.Item.FindControl("rptCurrentReservation");
            Panel panNoCurrentReservation = (Panel)FindControlRecursive(e.Item, "panNoCurrentReservation");

            Repeater rptNextReservation = (Repeater)e.Item.FindControl("rptNextReservation");
            Panel panNoNextReservation = (Panel)FindControlRecursive(e.Item, "panNoNextReservation");

            ToolStatusItem item = (ToolStatusItem)e.Item.DataItem;
            ReservationItem rsv;

            if (item != null)
            {
                rsv = currentReservations.FirstOrDefault(x => x.ResourceID == item.ResourceID);
                if (rsv != null)
                {
                    if (rptCurrentReservation != null)
                    {
                        rptCurrentReservation.DataSource = new ReservationItem[] { rsv };
                        rptCurrentReservation.DataBind();
                    }
                }
                else
                {
                    if (panNoCurrentReservation != null)
                    {
                        panNoCurrentReservation.Visible = true;
                    }
                }

                rsv = nextReservations.FirstOrDefault(x => x.ResourceID == item.ResourceID);
                if (rsv != null)
                {
                    if (rptNextReservation != null)
                    {
                        rptNextReservation.DataSource = new ReservationItem[] { rsv };
                        rptNextReservation.DataBind();
                    }
                }
                else
                {
                    if (panNoNextReservation != null)
                    {
                        panNoNextReservation.Visible = true;
                    }
                }
            }
        }
    }

    private Control FindControlRecursive(Control root, string id)
    {
        if (root.ID == id)
            return root;

        foreach (Control c in root.Controls)
        {
            Control t = FindControlRecursive(c, id);
            if (t != null)
                return t;
        }

        return null;
    }

    public class ReservationItem
    {
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public string DisplayName { get; set; }
        public string Scheduled { get; set; }
        public string Activity { get; set; }
        public string Started { get; set; }
    }

    public class ToolStatusItem : ReservationItem
    {
        public string Status { get; set; }
    }
</script>

<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta name="viewport" content="width=device-width, initial-scale=1">

    <title>LNF Scheduler Status</title>

    <link rel="stylesheet" href="//ssel-apps.eecs.umich.edu/static/styles/bootstrap/themes/lnf/bootstrap.css" />

    <style>
        span.form-text {
            margin-bottom: 0;
            margin-top: 0;
            padding-top: 5px;
            display: inline-block;
        }

            span.form-text > .error {
                color: #ff0000;
            }

        .section-title {
            border: solid 1px #eee;
            background-color: #f8f8f8;
            border-radius: 5px;
            font-weight: bold;
            padding: 5px;
        }

        .no-reservation {
            padding: 10px;
        }

        .chart-container {
            margin-top: 20px;
        }
    </style>
</head>
<body>
    <form runat="server" id="form1">
        <div class="container">
            <div class="page-header">
                <h1>System Status</h1>
            </div>
            <a href="/sselscheduler/Status.aspx" class="btn btn-primary" style="margin-bottom: 20px;">Refresh</a>
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h3 class="panel-title">Server Uptime Monitor</h3>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-sm-6">
                            <div class="section-title">ssel-sched.eecs.umich.edu</div>
                            <div class="chart-container">
                                <script>
                                    monitis_embed_module_id = "271831357";
                                    monitis_embed_module_width = "500";
                                    monitis_embed_module_height = "350";
                                    monitis_embed_module_readonlyChart = "false";
                                    monitis_embed_module_readonlyDateRange = "true";
                                    monitis_embed_module_detailedError = "false";
                                </script>
                                <script src="http://dashboard.monitor.us/sharedModule/shareModule.js"></script>
                                <noscript><a href="http://www.monitor.us">Monitoring by Monitor.Us. Please enable JavaScript to see the report!</a> </noscript>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <div class="section-title">ssel-apps.eecs.umich.edu</div>
                            <div class="chart-container">
                                <script>
                                    monitis_embed_module_id = "786259110";
                                    monitis_embed_module_width = "500";
                                    monitis_embed_module_height = "350";
                                    monitis_embed_module_readonlyChart = "false";
                                    monitis_embed_module_readonlyDateRange = "true";
                                    monitis_embed_module_detailedError = "false";
                                </script>
                                <script src="http://dashboard.monitor.us/sharedModule/shareModule.js"></script>
                                <noscript><a href="http://www.monitor.us">Monitoring by Monitor.Us. Please enable JavaScript to see the report!</a> </noscript>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="panel panel-default">
                <div class="panel-heading">
                    <h3 class="panel-title">Tool Status</h3>
                </div>
                <asp:Repeater runat="server" ID="rptToolStatus" OnItemDataBound="rptToolStatus_ItemDataBound">
                    <HeaderTemplate>
                        <ul class="list-group">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="list-group-item">
                            <h4><%#Eval("ResourceName")%></h4>
                            <div class="row" style="margin-top: 20px;">
                                <div class="col-sm-6">
                                    <div class="section-title">Current Reservation</div>
                                    <asp:Panel runat="server" ID="panNoCurrentReservation" Visible="false" CssClass="no-reservation">
                                        <em class="text-muted">There is no current reservation on this resource.</em>
                                    </asp:Panel>
                                    <asp:Repeater runat="server" ID="rptCurrentReservation">
                                        <HeaderTemplate>
                                            <div class="form-horizontal" style="padding: 10px;">
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">User</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("DisplayName")%></span>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Activity</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("Activity")%></span>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Scheduled</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("Scheduled")%></span>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Started</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("Started")%></span>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </div>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </div>
                                <div class="col-sm-6">
                                    <div class="section-title">Next Reservation</div>
                                    <asp:Panel runat="server" ID="panNoNextReservation" Visible="false" CssClass="no-reservation">
                                        <em class="text-muted">There is no future reservation scheduled on this resource.</em>
                                    </asp:Panel>
                                    <asp:Repeater runat="server" ID="rptNextReservation">
                                        <HeaderTemplate>
                                            <div class="form-horizontal" style="padding: 10px;">
                                        </HeaderTemplate>
                                        <ItemTemplate>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">User</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("DisplayName")%></span>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Activity</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("Activity")%></span>
                                                </div>
                                            </div>
                                            <div class="form-group">
                                                <label class="col-sm-2 control-label">Scheduled</label>
                                                <div class="col-sm-10">
                                                    <span class="form-text"><%#Eval("Scheduled")%></span>
                                                </div>
                                            </div>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            </div>
                                        </FooterTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
        </div>
    </form>

    <!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>

    <!-- Include all compiled plugins (below), or include individual files as needed -->
    <script src="//ssel-apps.eecs.umich.edu/static/lib/bootstrap/js/bootstrap.min.js"></script>
</body>
</html>
