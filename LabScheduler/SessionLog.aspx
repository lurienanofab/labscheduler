<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="SessionLog.aspx.vb" Inherits="LabScheduler.SessionLog" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Session Log</title>

    <style>
        body {
            font-family: 'Courier New';
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:Repeater runat="server" ID="rptSessionLog">
            <ItemTemplate>
                <div class="log-message">
                    <%#Eval("Message")%>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </form>
</body>
</html>
