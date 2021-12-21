<%@ Page Language="C#" %>

<%@ Import Namespace="LNF" %>
<%@ Import Namespace="LNF.CommonTools" %>
<%@ Import Namespace="LNF.Data" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Web.Scheduler" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script runat="server">
    private ReservationHistoryUtility _utility;
    private IClient _currentUser;

    public IProvider Provider { get { return LabScheduler.Global.ContainerContext.GetInstance<IProvider>(); } }

    public IClient CurrentUser
    {
        get
        {
            int clientId = 0;
            if (_currentUser == null)
            {
                clientId = GetClientID();
                if (clientId == 0)
                    _currentUser = Provider.Data.Client.GetClient(Context.User.Identity.Name);
                else
                    _currentUser = Provider.Data.Client.GetClient(clientId);
            }

            if (_currentUser == null)
                throw new Exception(string.Format("No current user. [clientId = {0}, username = {1}]", clientId, Context.User.Identity.Name));

            return _currentUser;
        }
    }

    public ReservationHistoryUtility ReservationHistoryUtility
    {
        get
        {
            if (_utility == null)
                _utility = ReservationHistoryUtility.Create(Provider);
            return _utility;
        }
    }

    public IReservation EditReservation
    {
        get
        {
            int reservationId = GetReservationID();
            if (reservationId == 0)
                return null;
            else
                return Provider.Scheduler.Reservation.GetReservation(reservationId);
        }
    }

    void Page_Load(object sender, EventArgs e)
    {
        DateTime sd = DateTime.Parse("2021-01-01");
        DateTime ed = DateTime.Parse("2022-01-01");

        IEnumerable<IHoliday> holidays = Utility.GetHolidays(sd, ed);
        var now = GetNow();
        var clientId = GetClientID();
        var reservationId = GetReservationID();
        var nextBusinessDay = Utility.NextBusinessDay(now, holidays);

        if (reservationId > 0)
        {
            CanChangeAccount(now, holidays);
        }
        else
        {
            GetNextBusinessDay(now, holidays);
        }

        if (!Page.IsPostBack)
        {
            txtNow.Text = now.ToString("yyyy-MM-dd");
            txtClientID.Text = clientId.ToString();
            txtReservationID.Text = reservationId.ToString();
        }
    }

    void BtnSubmit_Click(object sender, EventArgs e)
    {
        DateTime now;
        if (!DateTime.TryParse(txtNow.Text, out now))
            now = DateTime.Now;

        int clientId;
        if (!int.TryParse(txtClientID.Text, out clientId))
            clientId = 0;

        int reservationId;
        if (!int.TryParse(txtReservationID.Text, out reservationId))
            reservationId = 0;

        var redirectUrl = string.Format("~/util/business-day-calculator?Now={0:yyyy-MM-dd}", now);

        if (clientId > 0)
            redirectUrl += string.Format("&ClientID={0}", clientId);

        if (reservationId > 0)
            redirectUrl += string.Format("&ReservationID={0}", reservationId);

        Response.Redirect(redirectUrl);
    }

    void GetNextBusinessDay(DateTime now, IEnumerable<IHoliday> holidays)
    {
        var nextBusinessDay = Utility.NextBusinessDay(now, holidays);

        litDebug.Text += "<table class=\"table\"><tbody>";
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>now</strong></td><td>{0:yyyy-MM-dd}</td><td>&nbsp;</td></tr>", now);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>businessDays</strong></td><td>{0}</td><td># of business days after which changes cannot be made</td></tr>", GetBusinessDays());
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>nextBusinessDay</strong></td><td>{0:yyyy-MM-dd}</td><td>next business day (changes can be made before this date)</td></tr>", nextBusinessDay);
        litDebug.Text += "</tbody></table>";
    }

    private void CanChangeAccount(DateTime now, IEnumerable<IHoliday> holidays)
    {
        if (EditReservation == null)
        {
            litDebug.Text = string.Format("<div class=\"error\">Missing required parameter: ReservationID</div>");
            return;
        }

        var canChangeAcct = ReservationHistoryUtility.ReservationAccountCanBeChanged(CurrentUser, EditReservation, now, holidays);
        var maxDay = EditReservation.ActualBeginDateTime.GetValueOrDefault(EditReservation.EndDateTime);
        var d = maxDay.AddMonths(1).FirstOfMonth();
        var nextBusinessDay = Utility.NextBusinessDay(d, holidays);

        litDebug.Text += "<table class=\"table\"><tbody>";
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>now</strong></td><td>{0:yyyy-MM-dd}</td><td>&nbsp;</td></tr>", now);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>businessDays</strong></td><td>{0}</td><td># of business days after which changes cannot be made</td></tr>", GetBusinessDays());
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>current user</td><td>{0}, {1} [{2}]</td><td>&nbsp;</td></tr>", CurrentUser.LName, CurrentUser.FName, CurrentUser.ClientID);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>reservation</strong></td><td>#{0}</td><td>scheduled from {1:yyyy-MM-dd HH:mm:ss} to {2:yyyy-MM-dd HH:mm:ss}, actual from {3:yyyy-MM-dd HH:mm:ss} to {4:yyyy-MM-dd HH:mm:ss}</td><tr>", EditReservation.ReservationID, EditReservation.BeginDateTime, EditReservation.EndDateTime, EditReservation.ActualBeginDateTime, EditReservation.ActualEndDateTime);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>maxDay</strong></td><td>{0:yyyy-MM-dd}</td><td>reservation.ActualBeginDateTime.GetValueOrDefault(reservation.EndDateTime)</td></tr>", maxDay);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>d</strong></td><td>{0:yyyy-MM-dd}</td><td>maxDay.AddMonths(1).FirstOfMonth()</td></tr>", d);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>nextBusinessDay</strong></td><td>{0:yyyy-MM-dd}</td><td>next business day (changes can be made before this date)</td></tr>", nextBusinessDay);
        litDebug.Text += string.Format("<tr><td class=\"align-right\"><strong>canChangeAcct</strong></td><td>{0}</td><td>now < nextBusinessDay (and priv requirements are met)</td></tr>", canChangeAcct ? "Yes" : "No");
        litDebug.Text += "</tbody></table>";
    }

    private int GetBusinessDays()
    {
        // the property is BusinessDay but really should be BusinessDays
        // because this is the number of days after which changes cannot
        // be made (apportionment, reservation account, and forgiveness)
        return Provider.Data.Cost.GetActiveGlobalCost().BusinessDay;
    }

    private int GetReservationID()
    {
        if (!string.IsNullOrEmpty(Request.QueryString["ReservationID"]))
        {
            int result;
            if (int.TryParse(Request.QueryString["ReservationID"], out result))
                return result;
        }

        return 0;
    }

    private int GetClientID()
    {
        if (!string.IsNullOrEmpty(Request.QueryString["ClientID"]))
        {
            int result;
            if (int.TryParse(Request.QueryString["ClientID"], out result))
                return result;
        }

        return 0;
    }

    private DateTime GetNow()
    {
        if (!string.IsNullOrEmpty(Request.QueryString["Now"]))
        {
            DateTime result;
            if (DateTime.TryParse(Request.QueryString["Now"], out result))
                return result;
        }

        return DateTime.Now;
    }
</script>

<!doctype html>
<html>
<head>
    <title>Business Day Calculator</title>
    <style>
        body, input, select, button {
            font-family: 'Courier New';
            padding: 10px;
        }

        hr {
            margin-top: 20px;
            margin-bottom: 20px;
        }

        .error {
            color: red;
        }

        .table {
            border-collapse: collapse;
        }

        .table td {
            padding: 10px;
            border: solid 1px #ccc;
        }

        .criteria.table td {
            border: none;
        }

        .align-right {
            text-align: right;
        }
    </style>
</head>
<body>
    <form runat="server" id="form1">
        <table class="criteria table">
            <tbody>
                <tr><td class="align-right"><strong>Now</strong></td><td><asp:TextBox runat="server" ID="txtNow" /></td></tr>
                <tr><td class="align-right"><strong>ClientID</strong></td><td><asp:TextBox runat="server" ID="txtClientID" /></td></tr>
                <tr><td class="align-right"><strong>ReservationID</strong></td><td><asp:TextBox runat="server" ID="txtReservationID" /></td></tr>
            </tbody>
        </table>
        <div style="margin-top: 10px;"><asp:Button runat="server" ID="btnSubmit" Text="Submit" OnClick="BtnSubmit_Click" /></div>
        <hr />
        <asp:Literal runat="server" ID="litDebug" />
    </form>
</body>
</html>