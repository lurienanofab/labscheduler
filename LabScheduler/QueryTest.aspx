<%@ Page Language="C#" %>

<%@ Import Namespace="LNF" %>
<%@ Import Namespace="LNF.Data" %>
<%@ Import Namespace="LNF.Impl.DataAccess" %>
<%@ Import Namespace="LNF.Impl.Repository.Scheduler" %>
<%@ Import Namespace="LNF.Repository" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Web" %>
<%@ Import Namespace="NHibernate" %>
<%@ Import Namespace="NHibernate.Criterion" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Diagnostics" %>

<script runat="server">
    void Page_Load(object sender, EventArgs e)
    {
        try
        {
            DateTime sd;
            DateTime ed;

            if (!DateTime.TryParse(Request.QueryString["sd"], out sd))
                sd = DateTime.Now.Date.AddDays(-1);

            if (!DateTime.TryParse(Request.QueryString["ed"], out ed))
                ed = sd.AddDays(1);

            litOutput.Text += "<div class=\"test\">" + string.Format("<div>sd: {0:yyyy-MM-dd}</div>", sd)
                + string.Format("<div>ed: {0:yyyy-MM-dd}</div>", ed) + "</div>";

            SelectByDateRange(sd, ed);
            //SystemDataTest(++n, sd, ed);
            //SelectByDateRangeTest1(sd, ed);
            //SelectByDateRangeTest2(sd, ed);
            //NamedQueryTest(sd, ed);
            //NHiberateQueryTest(++n, sd, ed);
            //NHiberateCriteriaTest(++n, sd, ed);
            //NHiberateQueryOverTest(++n, sd, ed);
            //NHiberateHqlTest(++n, sd, ed);
        }
        catch (Exception ex)
        {
            litOutput.Text += string.Format("<pre>{0}</pre>", ex.Message);
            litOutput.Text += "<hr>";
            litOutput.Text += string.Format("<pre>{0}</pre>", ex.StackTrace);
        }
    }
    
    void SelectByDateRange(DateTime sd, DateTime ed)
    {
        var provider = WebApp.Current.GetInstance<IProvider>();
        var sw = Stopwatch.StartNew();
        var reservations = provider.Scheduler.Reservation.SelectByDateRange(sd, ed, true);
        sw.Stop();

        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>{0}</div>", "SelectByDateRange")
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void SystemDataTest(int n, DateTime sd, DateTime ed)
    {
        string sql = "SELECT * FROM sselScheduler.dbo.v_ReservationItem rsv"
            + " WHERE rsv.BeginDateTime < @ed"
            + " AND rsv.EndDateTime > @sd"
            + " OR rsv.ActualBeginDateTime < @ed"
            + " AND rsv.ActualEndDateTime > @sd";

        using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
        using (var cmd = new SqlCommand(sql, conn) { CommandType = CommandType.Text })
        using (var adap = new SqlDataAdapter(cmd))
        {
            cmd.Parameters.AddWithValue("sd", sd);
            cmd.Parameters.AddWithValue("ed", ed);

            var dt = new DataTable();

            var sw = Stopwatch.StartNew();
            adap.Fill(dt);
            sw.Stop();

            var elapsed = sw.Elapsed;
            var count = dt.Rows.Count;

            var output = "<div class=\"test\">"
                + string.Format("<div>method #{0}</div>", n)
                + string.Format("<div>count: {0}</div>", count)
                + string.Format("<div>elapsed: {0}</div>", elapsed)
                + "</div>";

            litOutput.Text += output;

            conn.Close();
        }
    }

    void SelectByDateRangeTest1(DateTime sd, DateTime ed)
    {
        var sw = Stopwatch.StartNew();
        var reservations = SelectByDateRange1(sd, ed);
        sw.Stop();

        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>{0}</div>", "SelectByDateRangeTest1")
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void SelectByDateRangeTest2(DateTime sd, DateTime ed)
    {
        var sw = Stopwatch.StartNew();
        var reservations = SelectByDateRange2(sd, ed);
        sw.Stop();

        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>{0}</div>", "SelectByDateRangeTest2")
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void NHiberateQueryTest(int n, DateTime sd, DateTime ed)
    {
        var provider = WebApp.Current.GetInstance<IProvider>();
        var sw = Stopwatch.StartNew();
        var reservations = provider.Scheduler.Reservation.SelectByDateRange(sd, ed, true);
        sw.Stop();
        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>method #{0}</div>", n)
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void NHiberateQueryOverTest(int n, DateTime sd, DateTime ed)
    {
        var mgr = WebApp.Current.GetInstance<ISessionManager>();
        var session = mgr.Session;

        var sw = Stopwatch.StartNew();

        var reservations = session.QueryOver<ReservationInfo>().Where(Restrictions.Or(
            Restrictions.And(
                Restrictions.Lt("BeginDateTime", ed),
                Restrictions.Gt("EndDateTime", sd)),
            Restrictions.And(
                Restrictions.Lt("BeginDateTime", ed),
                Restrictions.Gt("EndDateTime", sd))
        )).List<ReservationInfo>();

        sw.Stop();
        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>method #{0}</div>", n)
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void NHiberateCriteriaTest(int n, DateTime sd, DateTime ed)
    {
        var mgr = WebApp.Current.GetInstance<ISessionManager>();
        var session = mgr.Session;

        var sw = Stopwatch.StartNew();

        ICriteria crit = session.CreateCriteria<ReservationInfo>().Add(Restrictions.Or(
            Restrictions.And(
                Restrictions.Lt("BeginDateTime", ed),
                Restrictions.Gt("EndDateTime", sd)),
            Restrictions.And(
                Restrictions.Lt("BeginDateTime", ed),
                Restrictions.Gt("EndDateTime", sd))
        ));

        var reservations = crit.List<IReservation>();

        sw.Stop();
        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>method #{0}</div>", n)
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void NHiberateHqlTest(int n, DateTime sd, DateTime ed)
    {
        var mgr = WebApp.Current.GetInstance<ISessionManager>();
        var session = mgr.Session;

        string hql = "from ReservationInfo rsv"
            + " where rsv.BeginDateTime < :ed"
            + " and rsv.EndDateTime > :sd"
            + " or rsv.ActualBeginDateTime < :ed"
            + " and rsv.ActualEndDateTime > :sd";

        var sw = Stopwatch.StartNew();

        var reservations = session.CreateQuery(hql)
            .SetDateTime("sd", sd)
            .SetDateTime("ed", ed)
            .List<IReservation>();

        sw.Stop();
        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>method #{0}</div>", n)
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    void NamedQueryTest(DateTime sd, DateTime ed)
    {
        var mgr = WebApp.Current.GetInstance<ISessionManager>();
        var session = mgr.Session;

        var sw = Stopwatch.StartNew();

        var reservations = session.GetNamedQuery("SelectByDateRange")
            .SetParameter("active", null, NHibernate.NHibernateUtil.Boolean)
            .SetDateTime("sd", sd)
            .SetDateTime("ed", ed)
            .List<ReservationItem>();

        sw.Stop();
        var elapsed = sw.Elapsed;
        var count = reservations.Count();

        var output = "<div class=\"test\">"
            + string.Format("<div>{0}</div>", "NamedQueryTest")
            + string.Format("<div>count: {0}</div>", count)
            + string.Format("<div>elapsed: {0}</div>", elapsed)
            + "</div>";

        litOutput.Text += output;
    }

    IEnumerable<IReservationItem> SelectByDateRange1(DateTime sd, DateTime ed)
    {
        string sql = "SELECT * FROM sselScheduler.dbo.v_ReservationItem"
            + " WHERE ((BeginDateTime < @ed AND EndDateTime > @sd) OR (ActualBeginDateTime < @ed AND ActualEndDateTime > @sd))";

        var dt = DataCommand.Create(CommandType.Text)
            .Param("sd", sd)
            .Param("ed", ed)
            .FillDataTable(sql);

        var result = CreateReservationItems(dt);

        return result;
    }

    IEnumerable<IReservationItem> SelectByDateRange2(DateTime sd, DateTime ed)
    {
        string sql = "SELECT * FROM sselScheduler.dbo.v_ReservationItem"
                + " WHERE ((BeginDateTime < @ed AND EndDateTime > @sd))";

        var dt = DataCommand.Create(CommandType.Text)
            .Param("sd", sd)
            .Param("ed", ed)
            .FillDataTable(sql);

        var result = CreateReservationItems(dt);

        return result;
    }

    IEnumerable<IReservationItem> CreateReservationItems(DataTable dt)
    {
        IList<IReservationItem> result = new List<IReservationItem>();
        foreach (DataRow dr in dt.Rows)
        {
            var item = CreateReservationItem(dr);
            result.Add(item);
        }
        return result;
    }

    IReservationItem CreateReservationItem(DataRow dr)
    {
        var item = new ReservationItem
        {
            ReservationID = dr.Field<int>("ReservationID"),
            ResourceID = dr.Field<int>("ResourceID"),
            ResourceName = dr.Field<string>("ResourceName"),
            Granularity = dr.Field<int>("Granularity"),
            Offset = dr.Field<int>("Offset"),
            MinReservTime = dr.Field<int>("MinReservTime"),
            MaxReservTime = dr.Field<int>("MaxReservTime"),
            MinCancelTime = dr.Field<int>("MinCancelTime"),
            GracePeriod = dr.Field<int>("GracePeriod"),
            ReservFence = dr.Field<int>("ReservFence"),
            AuthDuration = dr.Field<int>("AuthDuration"),
            AuthState = dr.Field<bool>("AuthState"),
            ResourceAutoEnd = dr.Field<int>("ResourceAutoEnd"),
            ProcessTechID = dr.Field<int>("ProcessTechID"),
            LabID = dr.Field<int>("LabID"),
            BuildingID = dr.Field<int>("BuildingID"),
            ActivityID = dr.Field<int>("ActivityID"),
            ActivityName = dr.Field<string>("ActivityName"),
            Editable = dr.Field<bool>("Editable"),
            IsFacilityDownTime = dr.Field<bool>("IsFacilityDownTime"),
            ActivityAccountType = dr.Field<ActivityAccountType>("ActivityAccountType"),
            StartEndAuth = dr.Field<ClientAuthLevel>("StartEndAuth"),
            ClientID = dr.Field<int>("ClientID"),
            UserName = dr.Field<string>("UserName"),
            Privs = dr.Field<ClientPrivilege>("Privs"),
            Email = dr.Field<string>("Email"),
            Phone = dr.Field<string>("Phone"),
            LName = dr.Field<string>("LName"),
            MName = dr.Field<string>("MName"),
            FName = dr.Field<string>("FName"),
            AccountID = dr.Field<int>("AccountID"),
            AccountName = dr.Field<string>("AccountName"),
            ShortCode = dr.Field<string>("ShortCode"),
            ChargeTypeID = dr.Field<int>("ChargeTypeID"),
            RecurrenceID = dr.Field<int?>("RecurrenceID"),
            GroupID = dr.Field<int?>("GroupID"),
            ClientIDBegin = dr.Field<int?>("ClientIDBegin"),
            ClientIDEnd = dr.Field<int?>("ClientIDEnd"),
            BeginDateTime = dr.Field<DateTime>("BeginDateTime"),
            EndDateTime = dr.Field<DateTime>("EndDateTime"),
            ActualBeginDateTime = dr.Field<DateTime?>("ActualBeginDateTime"),
            ActualEndDateTime = dr.Field<DateTime?>("ActualEndDateTime"),
            Duration = dr.Field<double>("Duration"),
            ChargeMultiplier = dr.Field<double>("ChargeMultiplier"),
            ApplyLateChargePenalty = dr.Field<bool>("ApplyLateChargePenalty"),
            ReservationAutoEnd = dr.Field<bool>("ReservationAutoEnd"),
            HasProcessInfo = dr.Field<bool>("HasProcessInfo"),
            HasInvitees = dr.Field<bool>("HasInvitees"),
            IsActive = dr.Field<bool>("IsActive"),
            IsStarted = dr.Field<bool>("IsStarted"),
            KeepAlive = dr.Field<bool>("KeepAlive"),
            Notes = dr.Field<string>("Notes"),
            CreatedOn = dr.Field<DateTime>("CreatedOn"),
            LastModifiedOn = dr.Field<DateTime>("LastModifiedOn"),
            CancelledDateTime = dr.Field<DateTime?>("CancelledDateTime")
        };

        return item;
    }
</script>

<!doctype html>
<html>
<head>
    <title>Query Test</title>

    <style>
        body {
            font-family: 'Courier New';
        }

        .test {
            margin-bottom: 20px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Literal runat="server" ID="litOutput"></asp:Literal>
    </form>
    
    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script src="//ssel-apps.eecs.umich.edu/static/lib/moment/moment.min.js"></script>
    
    <script>
        function runTest(url){
            var start = moment();
            console.log('started test at ' + start.format('YYYY-MM-DD HH:mm:ss') + ' [url: ' + url + ']');
            $.ajax({
                'url': url
            }).done(function(data){
                var end = moment();
                var diff = end.diff(start, 'milliseconds');
                console.log('completed at ' + end.format('YYYY-MM-DD HH:mm:ss') + ' [time taken: ' + diff + ' ms]');
            }).fail(function(err){
                console.log(err);
            });
        }
        
        function testLabOccupancy(){
            runTest('ProcessTech.aspx?Path=4-1-33&Date=2020-10-29');
        }
        
        function testLabLocation(){
            runTest('LabLocation.aspx?LocationPath=1-4&Date=2020-10-29');
        }
    </script>
</body>
</html>
