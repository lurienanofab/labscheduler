<%@ Page Title="Client List" Language="vb" AutoEventWireup="false" CodeBehind="ResourceClientList.aspx.vb" Inherits="LabScheduler.Pages.ResourceClientList" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Resource - Client List</title>
    <link rel="stylesheet" href="styles/main.css?v=20201104" />
    <style>
        .tbl {
            border-collapse: collapse;
        }

            .tbl td {
                padding: 5px;
            }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <table class="tbl" border="0">
                <tr>
                    <td>
                        <h5>Resource Client List</h5>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:DataGrid ID="dgRC" runat="server" CellPadding="3" AutoGenerateColumns="False" BorderColor="#4682B4">
                            <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                            <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                            <Columns>
                                <asp:BoundColumn DataField="DisplayName" HeaderText="Client"></asp:BoundColumn>
                                <asp:BoundColumn DataField="OrgName" HeaderText="Organization"></asp:BoundColumn>
                                <asp:BoundColumn DataField="Email" HeaderText="Email"></asp:BoundColumn>
                            </Columns>
                        </asp:DataGrid>
                    </td>
                </tr>
            </table>
        </div>
    </form>
</body>
</html>
