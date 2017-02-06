<%@ Page Title="Start Reservation" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="StartReservation.aspx.vb" Inherits="LabScheduler.Pages.StartReservation" %>

<asp:Content runat="server" ID="Content1" ContentPlaceHolderID="head">
    <style>
        .start-reservation {
            padding: 0 10px 10px 10px;
            border: solid 1px #ddd;
            border-radius: 6px;
            box-shadow: 3px 3px 3px #aaa;
            background-color: #fff;
        }

            .start-reservation > h4 {
                padding-bottom: 10px;
                border-bottom: solid 1px #ccc;
            }


            .start-reservation .elapsed-time {
                margin-top: 10px;
                font-style: italic;
                color: #808080;
            }

            .start-reservation .error {
                padding: 10px;
                border: solid 1px #800000;
                background-color: #ffcccc;
                color: #990000;
                border-radius: 6px;
                font-weight: bold;
            }

            .start-reservation table {
                width: 100%;
                border-collapse: separate;
                border-spacing: 5px;
                margin-top: 10px;
            }

                .start-reservation table > tbody > tr > th {
                    padding: 5px;
                    background-color: #fafafa;
                    text-align: right;
                    border-bottom: solid 1px #eee;
                }

                .start-reservation table > tbody > tr > td {
                    padding: 5px;
                    border-bottom: solid 1px #eee;
                }

            .start-reservation .not-startable-message {
                color: #990000;
                font-size: larger;
                font-weight: bold;
                padding: 10px;
            }

            .start-reservation .return-link {
                padding: 10px;
            }
    </style>
</asp:Content>

<asp:Content runat="server" ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1">
    <div>
        <asp:Literal runat="server" ID="litMessage"></asp:Literal>
        <asp:Repeater runat="server" ID="rptStartReservation">
            <ItemTemplate>
                <div class="start-reservation" data-id="<%#Eval("ReservationID")%>">
                    <h4>Starting Reservation #<%#Eval("ReservationID")%></h4>
                    <table class="reservation-info" style="display: none;">
                        <tbody>
                            <tr>
                                <th style="width: 140px;">resource</th>
                                <td>
                                    <span data-property="ResourceName"></span>
                                    [<span data-property="ResourceID"></span>]</td>
                            </tr>
                            <tr>
                                <th>reserved by</th>
                                <td>
                                    <span data-property="ReservedByClientName"></span>
                                </td>
                            </tr>
                            <tr>
                                <th>started by</th>
                                <td>
                                    <span data-property="StartedByClientName"></span>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                    <div data-property="NotStartableMessage"></div>
                    <div class="elapsed-time"></div>
                    <div class="starting" style="display: none;">
                        <table>
                            <tbody>
                                <tr>
                                    <th style="width: 140px;">reservation started</th>
                                    <td class="reservation-started">
                                        <img src="<%=GetStaticUrl("images/ajax-loader-5.gif")%>" />
                                    </td>
                                </tr>
                                <tr class="interlock" style="display: none;">
                                    <th>interlock enabled</th>
                                    <td class="interlock-enabled">
                                        <img src="<%=GetStaticUrl("images/ajax-loader-5.gif")%>" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                    <div class="return-link" style="display: none;">
                        <a href="#">&larr; Return</a>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        $(".start-reservation").each(function () {
            var $this = $(this);

            var elapsedTime = 0;
            var handle = null;

            var displayElapsedTime = function () {
                $(".elapsed-time", $this).html("Elapsed time: " + elapsedTime + " seconds");
                elapsedTime += 1;
            }

            var onComplete = function (error) {

                if (handle)
                    clearInterval(handle);

                if (error) {
                    $(".return-link", $this).show();
                } else {
                    var returnUrl = $(".return-link a", $this).attr("href");
                    $(".return-link a", $this).hide();
                    $(".return-link", $this).show().prepend($("<strong/>").css("color", "#008000").html("Your reservation has started, redirecting to Reservations page..."));
                    setTimeout(function () {
                        window.location = returnUrl;
                    }, 1000);
                }
            }

            var getReservation = function (id) {
                return $.ajax({
                    "url": "ajax/reservation.ashx",
                    "type": "POST",
                    "data": { "Command": "get-reservation", "ReservationID": id },
                    "dataType": "json"
                });
            }

            getReservation($this.data("id")).done(function (data) {

                var item = data;

                var reservationInfo = $(".reservation-info", $this);

                $.each(item, function (k, v) {
                    $("[data-property='" + k + "']", reservationInfo).html(v);
                });

                $(".return-link a", $this).attr("href", item.ReturnUrl || "UserReservations.aspx");

                reservationInfo.show();

                if (item.Startable) {
                    displayElapsedTime();
                    handle = setInterval(displayElapsedTime, 1000);

                    $(".starting").show();

                    setTimeout(function () {
                        $.ajax({ "url": "ajax/reservation.ashx", "type": "POST", "data": { "Command": "start-reservation", "ReservationID": $this.data("id") }, "dataType": "json" }).done(function (data) {

                            if (data.Error)
                                $(".reservation-started", $this).html($("<span/>").css("color", "#ff0000").html(data.Message));
                            else
                                $(".reservation-started", $this).html("OK");

                            if (!item.HasInterlock)
                                onComplete(data.Error === true);

                        }).fail(function (err) {
                            $(".reservation-started", $this).html($("<span/>").css("color", "#ff0000").html(err.statusText));
                            onComplete(true);
                        });
                    }, 1000);

                    if (item.HasInterlock) {
                        $(".interlock", $this).show();
                        setTimeout(function () {
                            $.ajax({ "url": "ajax/interlock.ashx", "type": "POST", "data": { "command": "set-state", "id": item.ResourceID, "state": true }, "dataType": "json" }).done(function (data) {

                                if (data.Error)
                                    $(".interlock-enabled", $this).html($("<span/>").css("color", "#ff0000").html(data.Message));
                                else
                                    $(".interlock-enabled", $this).html("OK");

                                onComplete(data.Error === true);

                            }).fail(function (err) {
                                $(".interlock-enabled", $this).html($("<span/>").css("color", "#ff0000").html(err.statusText));
                                onComplete(true);
                            });
                        }, 1500);
                    }
                } else {
                    $(".return-link", $this).show();
                }
            });
        });
    </script>
</asp:Content>
