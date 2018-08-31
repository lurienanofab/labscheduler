<%@ Page Language="C#" Inherits="LNF.Web.Scheduler.Pages.CacheUtility" %>

<!DOCTYPE html>
<html>
<head>
    <title>Cache Utility</title>

    <style>
        body, input, select, textarea {
            font-family: 'Courier New';
        }

        table {
            border-collapse: collapse;
            margin-top: 10px;
        }

            table > thead > tr > th,
            table > tbody > tr > th {
                background-color: #90c697;
                border: solid 1px #808080;
                padding: 3px;
            }

            table > tbody > tr > td {
                border: solid 1px #808080;
                padding: 3px;
            }

            table > tbody > tr:nth-child(odd) > td {
                background-color: #ddd;
            }

        .key-form strong {
            display: inline-block;
            width: 80px;
        }

        .count-form strong {
            display: inline-block;
            width: 200px;
        }

        .count-form .count-label {
            display: inline-block;
            width: 80px;
        }
    </style>
</head>

<body>
    <form runat="server" id="form1">
        <div>
            <strong>Current Appoximate Size:</strong>
            <asp:Label runat="server" ID="lblApproximateSize"></asp:Label>
        </div>

        <hr />

        <div class="key-form">
            <div style="margin-bottom: 10px;">
                <strong>Key:</strong>
                <asp:TextBox runat="server" ID="txtKey" Width="400"></asp:TextBox>
            </div>

            <div style="margin-bottom: 10px;">
                <strong>Value:</strong>
                <asp:TextBox runat="server" ID="txtValue" Width="400"></asp:TextBox>
            </div>

            <div style="margin-bottom: 10px;">
                <strong>Expire:</strong>
                <asp:TextBox runat="server" ID="txtExpire" Width="400"></asp:TextBox>
            </div>

            <div style="margin-bottom: 10px;">
                <asp:Button runat="server" ID="btnGetKey" Text="Get" OnCommand="Key_Command" CommandName="get" />
                <asp:Button runat="server" ID="btnSetKey" Text="Set" OnCommand="Key_Command" CommandName="set" />
                <asp:Button runat="server" ID="btnDeleteKey" Text="Delete" OnCommand="Key_Command" CommandName="delete" />
                <asp:Button runat="server" ID="btnClearAll" Text="Clear All" OnCommand="Key_Command" CommandName="clear" ForeColor="Red" />
                <asp:Label runat="server" ID="lblKeyMessage" ForeColor="#008000" Font-Bold="true"></asp:Label>
            </div>
        </div>

        <hr />

        <div class="count-form">
            <div style="margin-bottom: 10px;">
                <strong>Clients:</strong>
                <asp:Label runat="server" ID="lblClientCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=Clients">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>Orgs:</strong>
                <asp:Label runat="server" ID="lblOrgCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=Orgs">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>Accounts:</strong>
                <asp:Label runat="server" ID="lblAccountCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=Accounts">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>ClientAccounts:</strong>
                <asp:Label runat="server" ID="lblClientAccountCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=ClientAccounts">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>ClientOrgs:</strong>
                <asp:Label runat="server" ID="lblClientOrgCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=ClientOrgs">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>Rooms:</strong>
                <asp:Label runat="server" ID="lblRoomCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=Rooms">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>Activities:</strong>
                <asp:Label runat="server" ID="lblActivityCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=Activities">refresh</a>]
            </div>

            <div style="margin-bottom: 10px;">
                <strong>SchedulerProperties:</strong>
                <asp:Label runat="server" ID="lblSchedulerPropertyCount" CssClass="count-label"></asp:Label>
                [<a href="CacheUtility.aspx?refresh=SchedulerProperties">refresh</a>]
            </div>
        </div>

        <hr />

        <asp:GridView runat="server" ID="gv" AutoGenerateColumns="true"></asp:GridView>
    </form>
</body>

</html>
