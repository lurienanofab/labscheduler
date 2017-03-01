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

<%@ Page Language="C#" Inherits="LNF.Web.Scheduler.Pages.CacheUtility" %>

<%@ Import Namespace="LNF.Cache" %>
<%@ Import Namespace="LNF.Scheduler" %>

<script runat="server">
    void Page_Load(object sender, EventArgs e)
    {
        litPingMessage.Text = string.Format("{0}", Ping());
    }

    void Key_Command(object sender, CommandEventArgs e)
    {
        lblKeyMessage.Text = string.Empty;

        if (!string.IsNullOrEmpty(txtKey.Text))
        {
            if (e.CommandName == "get")
                LoadGridView(txtKey.Text);

            if (e.CommandName == "delete")
            {
                DeleteKey(txtKey.Text);
                LoadGridView(txtKey.Text);
            }

            lblKeyMessage.Text = "OK";
        }
    }

    void LoadGridView(string key)
    {
        gv.DataSource = txtKey.Text;
        gv.DataBind();
    }
</script>

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
    </style>
</head>

<body>
    <form runat="server" id="form1">
        <strong>Ping: </strong>
        <asp:Literal runat="server" ID="litPingMessage"></asp:Literal>

        <hr />

        <strong>Key: </strong>
        <asp:TextBox runat="server" ID="txtKey" Width="400"></asp:TextBox>
        <asp:Button runat="server" ID="btnGetKey" Text="Get" OnCommand="Key_Command" CommandName="get" />
        <asp:Button runat="server" ID="btnDeleteKey" Text="Delete" OnCommand="Key_Command" CommandName="delete" />
        <asp:Label runat="server" ID="lblKeyMessage" ForeColor="#008000" Font-Bold="true"></asp:Label>

        <asp:GridView runat="server" ID="gv" AutoGenerateColumns="true"></asp:GridView>
    </form>
</body>

</html>
