<%@ Page Language="vb" AutoEventWireup="false" CodeBehind="ReservationStateTester.aspx.vb" Inherits="LabScheduler.ReservationStateTester" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>ReservationStateTester</title>
    <style>
        body {
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
                L - IsInLab<br />
                R - IsReserver<br />
                I - IsInvited<br />
                A - IsAuth<br />
                M - Before MCT<br />
                S - In Start Per
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
            <div>Unstarted reservation state:</div>
            <div class="reservation-state"></div>
        </div>
    </form>

    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery/jquery.min.js"></script>
    <script>
        function getReservationState(args) {
            return $.ajax({
                "url": "ReservationStateTester.aspx",
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

            getReservationState(args).done(function (data) {
                $(".reservation-state").html(data.stateText + " [" + data.state + "]");
            });
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
    </script>
</body>
</html>

