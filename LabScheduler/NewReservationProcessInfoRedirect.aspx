<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="NewReservationProcessInfoRedirect.aspx.vb" Inherits="LabScheduler.NewReservationProcessInfoRedirect" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <input type="hidden" runat="server" id="hidProcessInfoData" class="hidProcessInfoData" value=""/>
        <input type="hidden" runat="server" id="hidReservationID" class="reservation-id" value="-1" />
        <input type="hidden" runat="server" id="hidRedirectPath" class="hidRedirectPath" value="" />
    </form>
    <script src="http://lnf-dev.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script src="http://lnf-dev.eecs.umich.edu/static/lib/underscore/underscore-min.js"></script>

    <script src="scripts/underscore-min.js"></script>
    <script src="scripts/processinfo.js"></script>

        <script> 
        $(function () {
            submitPIJsonDataAndRedirect();
        });

    </script>
</body>
</html>
