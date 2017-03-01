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
