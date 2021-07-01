<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ProcessInfo.aspx.vb" Inherits="LabScheduler.ProcessInfo" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Process Info</title>
    <link type="text/css" rel="stylesheet" href="scripts/process-info/process-info.css" />
</head>
<body>
    <form id="form1" runat="server">
        <div style="padding: 20px;">
            <asp:PlaceHolder runat="server" ID="phSelectTool" Visible="true">
                <span>ResourceID:</span>
                <asp:TextBox runat="server" ID="txtResourceID"></asp:TextBox>
                <asp:Button runat="server" ID="btnSelectTool" Text="OK" OnClick="BtnSelectTool_Click" />
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="phProcessInfo" Visible="false">
                <div style="margin-bottom: 20px;">
                    <asp:HyperLink runat="server" ID="hypSelectTool" NavigateUrl="~/ProcessInfo.aspx">Select a different tool</asp:HyperLink>
                </div>
                <div runat="server" id="divProcessInfo" class="process-info-config"></div>
            </asp:PlaceHolder>
        </div>
    </form>

    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script src="//ssel-apps.eecs.umich.edu/static/lib/handlebars/handlebars.runtime.min-v4.7.7.js"></script>
    <script src="scripts/process-info/templates/processinfo.precompiled.js"></script>
    <script src="scripts/process-info/templates/processinfoline.precompiled.js"></script>
    <script src="scripts/process-info/process-info.js"></script>
    <script>
        var cfg = $(".process-info-config").processInfo();
    </script>
</body>
</html>
