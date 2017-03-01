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

<%@ Page Title="Client List" Language="vb" AutoEventWireup="false" CodeBehind="ResourceClientList.aspx.vb" Inherits="LabScheduler.Pages.ResourceClientList" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Resource - Client List</title>
    <link rel="stylesheet" href="styles/main.css?v=20161220" />
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
