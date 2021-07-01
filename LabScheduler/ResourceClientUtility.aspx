<%@ Page Language="C#" %>

<%@ Import Namespace="LNF" %>
<%@ Import Namespace="LNF.Data" %>
<%@ Import Namespace="LNF.Impl.Repository.Scheduler" %>
<%@ Import Namespace="LNF.Scheduler" %>
<%@ Import Namespace="LNF.Web" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.Configuration" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Data.SqlClient" %>
<%@ Import Namespace="System.Linq" %>

<script runat="server">
    private SqlConnection _conn;

    public IProvider Provider
    {
        get { return WebApp.Current.GetInstance<IProvider>(); }
    }

    void Page_Load(object sender, EventArgs e)
    {
        using (_conn = CreateConnection())
        {
            _conn.Open();

            if (!Page.IsPostBack)
            {
                int resourceId = GetResourceID();
                int clientId = GetClientID();
                
                if (resourceId > 0)
                {
                    string command = Request.QueryString["command"];

                    if (command == "AddClient")
                    {
                        if (clientId > 0)
                        {
                            AddInvitee(reservationId, clientId);
                            RedirectToResource(reservationId);
                        }
                    }
                    else if (command == "RemoveClient")
                    {
                        if (clientId > 0)
                        {
                            RemoveInvitee(reservationId, clientId);
                            RedirectToResource(reservationId);
                        }
                    }

                    txtResourceID.Text = resourceId.ToString();
                    LoadResource(resourceId);
                }
                else
                {
                    txtResourceID.Text = string.Empty;
                }
            }

            _conn.Close();
        }
    }

    void AddInvitee(int reservationId, int inviteeId)
    {
        string sql = "INSERT sselScheduler.dbo.ReservationInvitee (ReservationID, InviteeID) VALUES (@ReservationID, @InviteeID)";
        using (var cmd = new SqlCommand(sql, _conn))
        {
            cmd.Parameters.AddWithValue("ReservationID", reservationId);
            cmd.Parameters.AddWithValue("InviteeID", inviteeId);
            cmd.ExecuteNonQuery();
        }
    }

    void RemoveInvitee(int reservationId, int inviteeId)
    {
        string sql = "DELETE sselScheduler.dbo.ReservationInvitee WHERE ReservationID = @ReservationID AND InviteeID = @InviteeID";
        using (var cmd = new SqlCommand(sql, _conn))
        {
            cmd.Parameters.AddWithValue("ReservationID", reservationId);
            cmd.Parameters.AddWithValue("InviteeID", inviteeId);
            cmd.ExecuteNonQuery();
        }
    }

    void BtnOK_Click(object sender, EventArgs e)
    {
        string rid = txtResourceID.Text;
        RedirectToResource(rid);
    }

    int GetResourceID()
    {
        return GetQueryStringValueAsInt32("ResourceID");
    }

    int GetClientID()
    {
        return GetQueryStringValueAsInt32("ClientID");
    }
    
    int GetQueryStringValueAsInt32(string key)
    {
        int result = 0;

        if (!string.IsNullOrEmpty(Request.QueryString[key]))
        {
            int val;
            if (int.TryParse(Request.QueryString[key], out val))
                result = val;
        }

        return result;
    }

    void RedirectToResource(object rid)
    {
        Response.Redirect(string.Format("~/ResourceClientUtility.aspx?ResourceID={0}", rid), false);
    }

    void LoadReservation(int reservationId)
    {
        var rsv = Provider.Scheduler.Reservation.GetReservationWithInvitees(reservationId);

        IPrivileged p = rsv;
        IResource res = rsv;

        var items = new[]
        {
            new InviteeUtilityItem
            {
                ReservationID = rsv.ReservationID,
                ClientID = p.ClientID,
                DisplayName = rsv.DisplayName,
                ResourceID = res.ResourceID,
                ResourceName = res.ResourceName,
                AccountID = rsv.AccountID,
                AccountName = rsv.AccountName,
                ActivityID = rsv.ActivityID,
                ActivityName = rsv.ActivityName
            }
        };

        rptReservation.DataSource = items;
        rptReservation.DataBind();

        //rptInvitees.DataSource = rsv.Invitees;
        rptInvitees.DataSource = GetInvitees(rsv.ReservationID);
        rptInvitees.DataBind();

        var available = Provider.Scheduler.Reservation.GetAvailableInvitees(rsv.ReservationID, res.ResourceID, rsv.ActivityID, p.ClientID);
        IEnumerable<AvailableInvitee> filtered;

        if (rsv.ActivityAccountType == ActivityAccountType.Invitee)
            filtered = FilterAvailableInvitees(rsv.AccountID, available);
        else
            filtered = available;

        rptAvailableInvitees.DataSource = filtered;
        rptAvailableInvitees.DataBind();
    }

    string GetInviteeDisplayName(DataRow dr)
    {
        return string.Format("{0}, {1}", dr["InviteeLName"], dr["InviteeFName"]);
    }

    string GetDisplayName(DataRow dr)
    {
        return string.Format("{0}, {1}", dr["LName"], dr["FName"]);
    }

    IEnumerable<IReservationInviteeItem> GetInvitees(int reservationId)
    {
        string sql = "SELECT * FROM sselScheduler.dbo.v_ReservationInviteeItem WHERE ReservationID = @ReservationID";
        using (var cmd = new SqlCommand(sql, _conn))
        using (var adap = new SqlDataAdapter(cmd))
        {
            cmd.Parameters.AddWithValue("ReservationID", reservationId);
            var dt = new DataTable();
            adap.Fill(dt);

            var result = dt.AsEnumerable().Select(x => new ReservationInviteeItem
            {
                InviteeID = x.Field<int>("InviteeID"),
                ReservationID = x.Field<int>("ReservationID"),
                ResourceID = x.Field<int>("ResourceID"),
                ProcessTechID = x.Field<int>("ProcessTechID"),
                BeginDateTime = x.Field<DateTime>("BeginDateTime"),
                EndDateTime = x.Field<DateTime>("EndDateTime"),
                ActualBeginDateTime = x.Field<DateTime?>("ActualBeginDateTime"),
                ActualEndDateTime = x.Field<DateTime?>("ActualEndDateTime"),
                IsStarted = x.Field<bool>("IsStarted"),
                IsActive = x.Field<bool>("IsActive"),
                InviteeActive = x.Field<bool>("InviteeActive"),
                InviteeLName = x.Field<string>("InviteeLName"),
                InviteeFName = x.Field<string>("InviteeFName"),
                InviteePrivs = x.Field<ClientPrivilege>("InviteePrivs")
            }).ToList();

            return result;
        }
    }

    DataTable GetActiveClientAccounts(int accountId)
    {
        string sql = "SELECT * FROM sselData.dbo.v_ClientAccountInfo WHERE AccountID = @AccountID AND ClientAccountActive = 1";
        using (var cmd = new SqlCommand(sql, _conn))
        using (var adap = new SqlDataAdapter(cmd))
        {
            cmd.Parameters.AddWithValue("AccountID", accountId);
            var dt = new DataTable();
            adap.Fill(dt);
            return dt;
        }
    }

    SqlConnection CreateConnection()
    {
        return new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);
    }

    IEnumerable<AvailableInvitee> FilterAvailableInvitees(int accountId, IEnumerable<AvailableInvitee> available)
    {
        DataTable dt = GetActiveClientAccounts(accountId);

        List<AvailableInvitee> result = new List<AvailableInvitee>();

        foreach (DataRow dr in dt.Rows)
        {
            var a = available.FirstOrDefault(x => x.ClientID == dr.Field<int>("ClientID"));
            if (a != null)
                result.Add(a);
        }

        return result;
    }

    class InviteeUtilityItem
    {
        public int ReservationID { get; set; }
        public int ClientID { get; set; }
        public string DisplayName { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public int ActivityID { get; set; }
        public string ActivityName { get; set; }
    }
</script>

<!doctype html>

<html>
<head>
    <title>ResourceClient Utility</title>

    <style>
        body, input, button {
            font-family: 'Courier New';
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Literal runat="server" ID="litDebug"></asp:Literal>
        <div>
            <span>ResourceID:</span>
            <asp:TextBox runat="server" ID="txtResourceID" />
        </div>
        <div>
            <span>ClientID:</span>
            <asp:TextBox runat="server" ID="txtClientID" />
        </div>
        <div>
            <asp:Button runat="server" ID="btnOK" Text="OK" OnClick="BtnOK_Click" />
        </div>
        <hr>
        <div>
            <table>
                <tbody>
                    <asp:Repeater runat="server" ID="rptReservation">
                        <ItemTemplate>
                            <tr>
                                <td style="text-align: right;"><strong>ReservationID:</strong></td>
                                <td><%#Eval("ReservationID")%></td>
                            </tr>
                            <tr>
                                <td style="text-align: right;"><strong>Client:</strong></td>
                                <td><%#Eval("DisplayName")%> [<%#Eval("ClientID")%>]</td>
                            </tr>
                            <tr>
                                <td style="text-align: right;"><strong>Resource:</strong></td>
                                <td><%#Eval("ResourceName")%> [<%#Eval("ResourceID")%>]</td>
                            </tr>
                            <tr>
                                <td style="text-align: right;"><strong>Activity:</strong></td>
                                <td><%#Eval("ActivityName")%> [<%#Eval("ActivityID")%>]</td>
                            </tr>
                            <tr>
                                <td style="text-align: right;"><strong>Account:</strong></td>
                                <td><%#Eval("AccountName")%> [<%#Eval("AccountID")%>]</td>
                            </tr>

                        </ItemTemplate>
                    </asp:Repeater>
                    <tr>
                        <td style="text-align: right; vertical-align: top;"><strong>Invitees:</strong></td>
                        <td>
                            <asp:Repeater runat="server" ID="rptInvitees">
                                <ItemTemplate>
                                    <div>[<asp:HyperLink runat="server" NavigateUrl='<%#string.Format("~/InviteeUtility.aspx?Command=RemoveInvitee&ReservationID={0}&ClientID={1}", Request.QueryString["ReservationID"], Eval("InviteeID"))%>'>del</asp:HyperLink>] <%#Eval("InviteeDisplayName")%> [<%#Eval("InviteeID")%>]</div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </td>
                    </tr>
                    <tr>
                        <td style="text-align: right; vertical-align: top;"><strong>Available:</strong></td>
                        <td>
                            <asp:Repeater runat="server" ID="rptAvailableInvitees">
                                <ItemTemplate>
                                    <div>[<asp:HyperLink runat="server" NavigateUrl='<%#string.Format("~/InviteeUtility.aspx?Command=AddInvitee&ReservationID={0}&ClientID={1}", Request.QueryString["ReservationID"], Eval("ClientID"))%>'>add</asp:HyperLink>] <%#Eval("DisplayName")%> [<%#Eval("ClientID")%>]</div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </form>
</body>
</html>
