<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReservationStateTester.aspx.vb" Inherits="LabScheduler.ReservationStateTester" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ReservationStateTester</title>
    <style>
        body, input, button {
            font-family: 'Courier New';
        }

        table {
            border-collapse: collapse;
        }

        th {
            padding: 10px 10px 0 10px;
            border-bottom: solid 1px #808080;
        }

        td {
            padding: 5px 10px 10px 10px;
        }

        .section {
            padding: 10px;
            border-bottom: solid 1px #ccc;
        }

        .reservation-state {
            font-weight: bold;
            padding: 5px;
            font-size: larger;
        }

        .errmsg {
            color: #ff0000;
            font-weight: bold;
            padding: 5px 0 5px 10px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="section">
            <label>
                <input type="checkbox" id="tool_engineer" />
                Tool Engineer
            </label>
        </div>
        <div class="section">
            <div style="padding: 0 10px 10px;">
                This is the truth table for non tool engineer<br />
                note that having both R and I true is meaningless<br />
                <br />
                <div class="inlab">L - IsInLab</div>
                <div class="reserver">R - IsReserver</div>
                <div class="invited">I - IsInvited</div>
                <div class="authorized">A - IsAuth</div>
                <div class="before_mct">M - Before MCT</div>
                <div class="startable">S - In Start Per</div>
            </div>
            <table>
                <thead>
                    <tr>
                        <th>L</th>
                        <th>R</th>
                        <th>I</th>
                        <th>A</th>
                        <th>M</th>
                        <th>S</th>
                    </tr>
                </thead>
                <tbody>
                    <tr class="inputs">
                        <td>
                            <input type="checkbox" id="inlab" /></td>
                        <td>
                            <input type="checkbox" id="reserver" /></td>
                        <td>
                            <input type="checkbox" id="invited" /></td>
                        <td>
                            <input type="checkbox" id="authorized" /></td>
                        <td>
                            <input type="checkbox" id="before_mct" /></td>
                        <td>
                            <input type="checkbox" id="startable" /></td>
                    </tr>
                </tbody>
            </table>
        </div>

        <div class="section">
            <table>
                <tr>
                    <td>Now:</td>
                    <td>
                        <input type="text" id="now" placeholder="YYYY-MM-DD HH:MM:SS" />
                    </td>
                </tr>
                <tr>
                    <td>ClientID:</td>
                    <td>
                        <input type="text" id="client_id" placeholder="12345" />
                    </td>
                </tr>
                <tr>
                    <td>ReservationID:</td>
                    <td>
                        <input type="text" id="reservation_id" placeholder="12345" />
                    </td>
                </tr>
                <tr>
                    <td>IsInLab:</td>
                    <td>
                        <input type="checkbox" id="get_state_inlab" />
                    </td>
                </tr>
                <tr>
                    <td>UseActual:</td>
                    <td>
                        <input type="checkbox" id="use_actual" />
                    </td>
                </tr>
                <tr>
                    <td colspan="2">
                        <button type="button" id="get_state">Get Reservation State</button>
                    </td>
                </tr>
            </table>
            <div class="errmsg"></div>
        </div>

        <div class="section">
            <div class="reservation-state-label"></div>
            <div class="reservation-state"></div>
        </div>
    </form>

    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script src="//ssel-apps.eecs.umich.edu/static/lib/moment/moment.min.js"></script>

    <script>
        function getReservationState(args, command) {
            return $.ajax({
                "url": "ReservationStateTester.aspx?command=" + command,
                "method": "POST",
                "data": JSON.stringify(args),
                "dateType": "json",
                "contentType": "application/json"
            });
        }

        function refresh() {
            var args = {
                tool_engineer: $("#tool_engineer").prop("checked") === true,
                inlab: $("#inlab").prop("checked") === true,
                reserver: $("#reserver").prop("checked") === true,
                invited: $("#invited").prop("checked") === true,
                authorized: $("#authorized").prop("checked") === true,
                before_mct: $("#before_mct").prop("checked") === true,
                startable: $("#startable").prop("checked") === true
            };

            update(args);

            getReservationState(args, "calc-state")
                .done(displayState)
                .fail(handleErrorResponse);
        }

        function update(args) {
            $(".inlab").css("font-weight", "normal");
            $(".reserver").css("font-weight", "normal");
            $(".invited").css("font-weight", "normal");
            $(".authorized").css("font-weight", "normal");
            $(".before_mct").css("font-weight", "normal");
            $(".startable").css("font-weight", "normal");

            if (args.inlab)
                $(".inlab").css("font-weight", "bold");

            if (args.reserver)
                $(".reserver").css("font-weight", "bold");

            if (args.invited)
                $(".invited").css("font-weight", "bold");

            if (args.authorized)
                $(".authorized").css("font-weight", "bold");

            if (args.before_mct)
                $(".before_mct").css("font-weight", "bold");

            if (args.startable)
                $(".startable").css("font-weight", "bold");
        }

        function handleStateResponse(data) {
            $(".errmsg").html("");

            displayState(data);

            $("#tool_engineer").prop("checked", data.tool_engineer);
            $("#inlab").prop("checked", data.inlab);
            $("#reserver").prop("checked", data.reserver);
            $("#invited").prop("checked", data.invited);
            $("#authorized").prop("checked", data.authorized);
            $("#before_mct").prop("checked", data.before_mct);
            $("#startable").prop("checked", data.startable);

            update(data);
        }

        function handleErrorResponse(err) {
            $(".errmsg").html(err.responseJSON.Message);
        }

        function displayState(args) {
            var label = args.started ? "Started reservation state:" : "Unstarted reservation state:";
            $(".reservation-state-label").html(label);
            $(".reservation-state").html(args.stateText + " [" + args.state + "]");
        }

        $("#tool_engineer").on("change", function (e) {
            if ($(this).prop("checked")) {
                $("#inlab").prop("checked", false).prop("disabled", true);
                $("#reserver").prop("checked", false).prop("disabled", true);
                $("#invited").prop("checked", false).prop("disabled", true);
                $("#authorized").prop("checked", false).prop("disabled", true);
            } else {
                $("#inlab").prop("disabled", false);
                $("#reserver").prop("disabled", false);
                $("#invited").prop("disabled", false);
                $("#authorized").prop("disabled", false);
            }

            refresh();
        });

        $(".inputs").on("change", "input[type='checkbox']", function (e) {
            refresh();
        });

        refresh();

        $("#get_state").on("click", function () {
            var args = {
                now: $("#now").val(),
                clientId: $("#client_id").val(),
                reservationId: $("#reservation_id").val(),
                inlab: $("#get_state_inlab").prop("checked"),
                useActual: $("#use_actual").prop("checked")
            };

            localStorage.setItem('get_state_args', JSON.stringify(args));

            getReservationState(args, "get-state")
                .done(handleStateResponse)
                .fail(handleErrorResponse);
        });

        var url = new URL(window.location);

        var defArgs = {
            now: moment().format('YYYY-MM-DD HH:mm:ss'),
            clientId: null,
            reservationId: null,
            inlab: false,
            useActual: false
        };

        var storageArgs = JSON.parse(localStorage.getItem('get_state_args'));

        var queryArgs = {};
        if (url.searchParams.has("Now")) queryArgs.now = url.searchParams.get("Now");
        if (url.searchParams.has("ClientID")) queryArgs.clientId = url.searchParams.get("ClientID");
        if (url.searchParams.has("ReservationID")) queryArgs.reservationId = url.searchParams.get("ReservationID");
        if (url.searchParams.has("InLab")) queryArgs.inlab = url.searchParams.get("InLab");
        if (url.searchParams.has("UseActual")) queryArgs.useActual = url.searchParams.get("UseActual");

        var args = $.extend({}, defArgs, storageArgs, queryArgs);

        console.log(args);

        $("#now").val(args.now);
        $("#client_id").val(args.clientId);
        $("#reservation_id").val(args.reservationId);
        $("#get_state_inlab").prop("checked", args.inlab);
        $("#use_actual").prop("checked", args.useActual);

        if (args.reservationId) {
            getReservationState(args, "get-state")
                .done(handleStateResponse)
                .fail(handleErrorResponse);
        }
    </script>
</body>
</html>

