<%@ Page Language="C#" %>

<%@ Import Namespace="LNF" %>
<%@ Import Namespace="LNF.CommonTools" %>
<%@ Import Namespace="LNF.Data" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Web.Scheduler" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Collections.Generic" %>

<script runat="server">
    private IClient _currentUser;

    public IProvider Provider { get { return LabScheduler.Global.ContainerContext.GetInstance<IProvider>(); } }

    public IClient CurrentUser
    {
        get
        {
            if (_currentUser == null)
            {
                _currentUser = Provider.Data.Client.GetClient(Context.User.Identity.Name);
            }

            if (_currentUser == null)
                throw new Exception(string.Format("No current user. [username = {0}]", Context.User.Identity.Name));

            return _currentUser;
        }
    }

    void Page_Load(object sender, EventArgs e)
    {

    }

    void BtnSearch_Click(object sender, EventArgs e)
    {
        DateTime sd;
        DateTime ed;
        int clientId;
        int accountId;
        string errmsg;

        ShowAlert(string.Empty);

        int errors = Validate(out sd, out ed, out clientId, out accountId, out errmsg);

        if (errors == 0)
            LoadReservations(sd, ed, clientId, accountId);
        else
            ShowAlert(errmsg);
    }

    void BtnUpdate_Click(object sender, EventArgs e)
    {
        DateTime sd;
        DateTime ed;
        int clientId;
        int accountId;
        int updateAccountId;
        string appendNote;
        string errmsg;

        ShowAlert(string.Empty);

        int errors = Validate(out sd, out ed, out clientId, out accountId, out errmsg);

        if (!int.TryParse(txtUpdateAccountID.Text, out updateAccountId))
        {
            errmsg += "<div>&bull;Invalid Update AccountID</div>";
            errors += 1;
        }

        if (errors == 0)
        {
            appendNote = txtAppendNote.Text;
            int updates = UpdateReservations(sd, ed, clientId, accountId, updateAccountId, appendNote);
            LoadReservations(sd, ed, clientId, updateAccountId);
            ShowAlert(string.Format("Updated row count: {0}", updates), "success");
        }
        else
            ShowAlert(errmsg);
    }

    int Validate(out DateTime sd, out DateTime ed, out int clientId, out int accountId, out string errmsg)
    {
        //DateTime sd;
        //DateTime ed;
        //int clientId;
        //int accountId;

        int errors = 0;
        errmsg = string.Empty;

        if (!DateTime.TryParse(txtStartDate.Text, out sd))
        {
            errmsg += "<div>&bull;Invalid Start Date</div>";
            errors += 1;
        }

        if (!DateTime.TryParse(txtEndDate.Text, out ed))
        {
            errmsg += "<div>&bull;Invalid End Date</div>";
            errors += 1;
        }

        if (!int.TryParse(txtClientID.Text, out clientId))
        {
            errmsg += "<div>&bull;Invalid ClientID</div>";
            errors += 1;
        }

        if (!int.TryParse(txtAccountID.Text, out accountId))
            accountId = 0;

        return errors;
    }

    int UpdateReservations(DateTime sd, DateTime ed, int clientId, int accountId, int updateAccountId, string appendNote)
    {
        var dt = GetReservationData(sd, ed, clientId, accountId);

        foreach (DataRow dr in dt.Rows)
        {
            dr["AccountID"] = updateAccountId;
            AppendNote(dr, appendNote);
        }

        string sql = "UPDATE sselScheduler.dbo.Reservation SET AccountID = @UpdateAccountID, Notes = @Notes WHERE ReservationID = @ReservationID";

        using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
        using (var cmd = new SqlCommand(sql, conn))
        using (var adap = new SqlDataAdapter() { UpdateCommand = cmd })
        {
            cmd.Parameters.Add("ReservationID", SqlDbType.Int, 0, "ReservationID");
            cmd.Parameters.Add("UpdateAccountID", SqlDbType.Int, 0, "AccountID");
            cmd.Parameters.Add("Notes", SqlDbType.NVarChar, 500, "Notes");

            int result = adap.Update(dt);
            return result;
        }
    }

    void AppendNote(DataRow dr, string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            string notes = dr["Notes"].ToString().Trim();

            if (string.IsNullOrEmpty(notes))
            {
                notes = text;
            }
            else
            {
                if (!notes.EndsWith("."))
                    notes += ". " + text;
                else
                    notes += " " + text;
            }

            dr["Notes"] = notes;
        }
    }

    DataTable GetReservationData(DateTime sd, DateTime ed, int clientId, int accountId)
    {
        string sql = "SELECT rsv.ReservationID, rsv.ClientID, c.LName, c.FName, rsv.AccountID, a.[Name] AS AccountName, a.ShortCode"
            + ", rsv.BeginDateTime, rsv.EndDateTime, rsv.ActualBeginDateTime, rsv.ActualEndDateTime, rsv.IsStarted, rsv.IsActive, ISNULL(rsv.Notes, '') AS Notes"
            + " FROM sselScheduler.dbo.Reservation rsv"
            + " INNER JOIN sselData.dbo.Client c ON c.ClientID = rsv.ClientID"
            + " INNER JOIN sselData.dbo.Account a ON a.AccountID = rsv.AccountID"
            + " WHERE rsv.ClientID = @ClientID AND rsv.AccountID = ISNULL(@AccountID, rsv.AccountID) AND ((rsv.BeginDateTime < @ed AND rsv.EndDateTime > @sd) OR (rsv.ActualBeginDateTime < @ed AND rsv.ActualEndDateTime > @sd))";

        using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
        using (var cmd = new SqlCommand(sql, conn))
        using (var adap = new SqlDataAdapter(cmd))
        {
            cmd.Parameters.AddWithValue("sd", sd);
            cmd.Parameters.AddWithValue("ed", ed);
            cmd.Parameters.AddWithValue("ClientID", clientId);

            if (accountId > 0)
                cmd.Parameters.AddWithValue("AccountID", accountId);
            else
                cmd.Parameters.AddWithValue("AccountID", DBNull.Value);

            var dt = new DataTable();
            adap.Fill(dt);

            return dt;
        }
    }

    void LoadReservations(DateTime sd, DateTime ed, int clientId, int accountId)
    {
        txtUpdateAccountID.Text = string.Empty;
        txtAppendNote.Text = string.Empty;

        var dt = GetReservationData(sd, ed, clientId, accountId);

        if (dt.Rows.Count == 0)
        {
            phNoData.Visible = true;
            phReservations.Visible = false;
        }
        else
        {
            PrepareData(dt);
            phNoData.Visible = false;
            phReservations.Visible = true;
            rptReservations.DataSource = dt;
            rptReservations.DataBind();
            litRowCount.Text = dt.Rows.Count.ToString();
        }
    }

    void PrepareData(DataTable dt)
    {
        dt.Columns.Add("Client", typeof(string));
        dt.Columns.Add("Account", typeof(string));

        foreach (DataRow dr in dt.Rows)
        {
            dr["Client"] = string.Format("<div>#{0}</div><div>{1}, {2}</div>", dr["ClientID"], dr["LName"], dr["FName"]);

            string shortCode = string.Empty;
            if (dr["ShortCode"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["ShortCode"].ToString()))
                shortCode = dr["ShortCode"].ToString().Trim() + ":";

            dr["Account"] = string.Format("<div>#{0}</div><div>{1}{2}</div>", dr["AccountID"], shortCode, dr["AccountName"]);
        }
    }

    void ShowAlert(string errmsg, string alertType = "danger")
    {
        phAlert.Visible = !string.IsNullOrEmpty(errmsg);
        litAlertMessage.Text = errmsg;
        divAlert.Attributes["class"] = "alert alert-" + alertType;
    }
</script>

<!doctype html>
<html>
<head>
    <title>Batch Account Change</title>
    <style>
        body, input, select, button {
            font-family: 'Courier New';
            padding: 10px;
        }

        hr {
            margin-top: 20px;
            margin-bottom: 20px;
        }

        .alert {
            position: relative;
            padding: 1rem 1rem;
            margin-bottom: 1rem;
            border: 1px solid transparent;
            border-radius: .25rem;
        }

        .alert-danger {
            color: #842029;
            background-color: #f8d7da;
            border-color: #f5c2c7;
        }

        .alert-success {
            color: #0f5132;
            background-color: #d1e7dd;
            border-color: #badbcc;
        }

        .table {
            border-collapse: collapse;
        }

        .table td, .table th {
            padding: 10px;
            border: solid 1px #ccc;
        }

        .table th {
            text-align: center;
            background-color: #f7f7f7;
        }

        .criteria.table td {
            border: none;
        }

        .align-right {
            text-align: right;
        }

        .align-center {
            text-align: center;
        }

        .ml-10 {
            margin-left: 10px;
        }
    </style>
</head>
<body>
    <form runat="server" id="form1">
        <table class="criteria table">
            <tbody>
                <tr><td class="align-right"><strong>Start Date</strong></td><td><asp:TextBox runat="server" ID="txtStartDate" /></td></tr>
                <tr><td class="align-right"><strong>End Date</strong></td><td><asp:TextBox runat="server" ID="txtEndDate" /></td></tr>
                <tr><td class="align-right"><strong>ClientID</strong></td><td><asp:TextBox runat="server" ID="txtClientID" /></td></tr>
                <tr><td class="align-right"><strong>Current AccountID</strong></td><td><asp:TextBox runat="server" ID="txtAccountID" /></td></tr>
            </tbody>
        </table>

        <div style="margin-top: 10px;"><asp:Button runat="server" ID="btnSearch" Text="Search" OnClick="BtnSearch_Click" /></div>

        <hr />

        <asp:PlaceHolder runat="server" ID="phAlert" Visible="false">
            <div runat="server" id="divAlert" class="alert" role="alert">
                <asp:Literal runat="server" ID="litAlertMessage"></asp:Literal>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phNoData" Visible="false">
            <em>No reservations were found.</em>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phReservations" Visible="false">
            <table class="criteria table" style="margin-bottom: 20px;">
                <tbody>
                    <tr><td class="align-right"><strong>Append Note</strong></td><td><asp:TextBox runat="server" ID="txtAppendNote" Width="300" /></td></tr>
                    <tr><td class="align-right"><strong>Update AccountID</strong></td><td><asp:TextBox runat="server" ID="txtUpdateAccountID" /><asp:Button runat="server" ID="btnUpdate" Text="Update" OnClick="BtnUpdate_Click" CssClass="ml-10" /></td></tr>
                    <tr><td class="align-right"><strong>Row Count</strong></td><td><asp:Literal runat="server" ID="litRowCount"></asp:Literal></td></tr>
                </tbody>
            </table>

            <asp:Repeater runat="server" ID="rptReservations">
                <HeaderTemplate>
                    <table class="table">
                        <thead>
                            <tr>
                                <th>ReservationID</th>
                                <th>Client</th>
                                <th>Account</th>
                                <th>BeginDateTime</th>
                                <th>EndDateTime</th>
                                <th>ActualBeginDateTime</th>
                                <th>ActualEndDateTime</th>
                                <th>IsActive</th>
                                <th>IsStarted</th>
                                <th style="width: 400px;">Notes</th>
                            </tr>
                        </thead>
                        <thead>
                </HeaderTemplate>
                <ItemTemplate>
                            <tr>
                                <td><%#Eval("ReservationID")%></td>
                                <td><%#Eval("Client")%></td>
                                <td><%#Eval("Account")%></td>
                                <td><%#Eval("BeginDateTime", "{0:yyyy-MM-dd'<br>'HH:mm:ss}")%></td>
                                <td><%#Eval("EndDateTime", "{0:yyyy-MM-dd'<br>'HH:mm:ss}")%></td>
                                <td><%#Eval("ActualBeginDateTime", "{0:yyyy-MM-dd'<br>'HH:mm:ss}")%></td>
                                <td><%#Eval("ActualEndDateTime", "{0:yyyy-MM-dd'<br>'HH:mm:ss}")%></td>
                                <td class="align-center"><%#Eval("IsActive")%></td>
                                <td class="align-center"><%#Eval("IsStarted")%></td>
                                <td><%#Eval("Notes")%></td>
                            </tr>
                </ItemTemplate>
                <FooterTemplate>
                        </thead>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
        </asp:PlaceHolder>
    </form>
</body>
</html>