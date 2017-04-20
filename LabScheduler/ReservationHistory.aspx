<%@ Page Title="Reservation History" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="~/ReservationHistory.aspx.vb" Inherits="LabScheduler.Pages.ReservationHistory" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .reservation-item {
            border-collapse: collapse;
            width: 100%;
        }

            .reservation-item th, .reservation-item td {
                padding: 3px;
                text-align: left;
                border: solid 1px #AAAAAA;
                height: 25px;
            }

            .reservation-item th {
                vertical-align: middle;
                background-color: #DDDDDD;
            }

        .history-container {
            width: 1300px;
        }

        .history {
            width: 100%;
        }

        .history-date {
            width: 130px;
        }

        .history-edit {
            width: 40px;
            text-align: center;
        }

        .sdate, .edate {
            width: 80px;
        }

        .ui-widget {
            font-size: 10pt;
        }

        tr.canceled-for-modification td {
            color: #ff0000;
        }

        .canceled-for-modification-message {
            color: #ff0000;
            margin-top: 10px;
            display: block;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div runat="server" id="divReservationHistory" class="reservation-history" data-url="/scheduler/api/reservation" data-client-id="0">
        <input type="hidden" runat="server" id="hidClientID" class="client-id" />
        <table class="content-table">
            <tr>
                <td style="padding-bottom: 5px;">
                    <span style="font: bold 11pt Verdana; color: #003366;">Reservation History</span>
                </td>
            </tr>
            <tr>
                <td>
                    <table style="border-collapse: collapse;">
                        <tr runat="server" id="trUser">
                            <td style="padding: 10px 5px 5px 5px; border-top: solid 1px #808080;">Show reservations for
                            <asp:DropDownList runat="server" ID="ddlClients" AutoPostBack="False" DataTextField="DisplayName" DataValueField="ClientID" />
                            </td>
                        </tr>
                        <tr>
                            <td style="padding: 5px;">
                                <div class="date-manager">
                                    Show reservations in the past
                                <asp:DropDownList runat="server" ID="ddlRange" AutoPostBack="False" CssClass="daterange-select">
                                    <asp:ListItem Value="0" Selected="True">30 days</asp:ListItem>
                                    <asp:ListItem Value="1">3 months</asp:ListItem>
                                    <asp:ListItem Value="2">1 year</asp:ListItem>
                                    <asp:ListItem Value="3">All</asp:ListItem>
                                </asp:DropDownList>
                                    <asp:TextBox runat="server" ID="txtStartDate" CssClass="sdate datepicker"></asp:TextBox>
                                    <asp:TextBox runat="server" ID="txtEndDate" CssClass="edate datepicker"></asp:TextBox>
                                </div>
                            </td>
                        </tr>
                        <tr>
                            <td style="border-bottom: 1px solid #808080; padding: 5px 5px 10px 5px;">
                                <asp:Button runat="server" ID="btnSearchHistory" Text="Search" OnClick="btnSearchHistory_Click" />
                            </td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 5px 5px 5px;">
                                <asp:Panel runat="server" ID="panEditHistory" Visible="false">
                                    <input type="hidden" runat="server" id="hidSelectedClientID" class="selected-client-id" />
                                    <input type="hidden" runat="server" id="hidEditReservationID" class="reservation-id" />
                                    <input type="hidden" runat="server" id="hidStartDate" class="start-date" />
                                    <input type="hidden" runat="server" id="hidEndDate" class="end-date" />
                                    <div>
                                        <div>
                                            <table class="reservation-item">
                                                <tr>
                                                    <th style="width: 110px;">Resource</th>
                                                    <td>
                                                        <asp:Literal runat="server" ID="litResourceName"></asp:Literal>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                        <div style="margin-top: 10px;">
                                            <table class="reservation-item">
                                                <tr>
                                                    <th style="width: 110px;">Reservation ID</th>
                                                    <td>
                                                        <asp:Literal runat="server" ID="litReservationID"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>Activity</th>
                                                    <td>
                                                        <asp:Literal runat="server" ID="litActivityName"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>Started</th>
                                                    <td>
                                                        <asp:Literal runat="server" ID="litIsStarted"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>Canceled</th>
                                                    <td>
                                                        <asp:Literal runat="server" ID="litIsCanceled"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr runat="server" id="trInvitees" visible="false">
                                                    <th>Invitee(s)</th>
                                                    <td>
                                                        <asp:Literal runat="server" ID="litInvitees"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>Account</th>
                                                    <td class="current-account">
                                                        <asp:Literal runat="server" ID="litCurrentAccount"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr runat="server" id="trAccount" visible="false">
                                                    <td colspan="2" style="padding: 10px;">
                                                        <div style="font-weight: bold; color: #808080; padding-bottom: 2px; margin-bottom: 8px; border-bottom: solid 1px #808080; font-style: italic;">
                                                            Modify Account
                                                        </div>
                                                        <div>
                                                            <asp:DropDownList runat="server" ID="ddlEditReservationAccount" DataValueField="AccountID" DataTextField="Name" Width="100%" CssClass="reservation-account-id">
                                                            </asp:DropDownList>
                                                            <asp:Literal runat="server" ID="litEditReservationAccountMessage"></asp:Literal>
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                        <div style="margin-top: 10px;">
                                            <table class="reservation-item">
                                                <tr>
                                                    <th colspan="3">&nbsp;</th>
                                                    <th colspan="2" style="text-align: center;">Duration (Minutes)</th>
                                                </tr>
                                                <tr>
                                                    <th>&nbsp;</th>
                                                    <th style="text-align: center;">Begin Time</th>
                                                    <th style="text-align: center;">End Time</th>
                                                    <th style="text-align: center;">Regular</th>
                                                    <th style="text-align: center;">Overtime</th>
                                                </tr>
                                                <tr>
                                                    <th>Reserved</th>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litReservedBeginDateTime"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litReservedEndDateTime"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litReservedRegularDuration"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">--</td>
                                                </tr>
                                                <tr>
                                                    <th>Actual</th>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litActualBeginDateTime"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litActualEndDateTime"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litActualRegularDuration"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litActualOvertimeDuration"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <th>Chargeable</th>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litChargeableBeginDateTime"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litChargeableEndDateTime"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litChargeableRegularDuration"></asp:Literal>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <asp:Literal runat="server" ID="litChargeableOvertimeDuration"></asp:Literal>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                        <div style="margin-top: 10px;">
                                            <table class="reservation-item">
                                                <tr>
                                                    <th style="width: 150px;">Forgiven Percentage</th>
                                                    <td class="forgiven-percentage">
                                                        <asp:Literal runat="server" ID="litForgiveAmount"></asp:Literal>
                                                    </td>
                                                </tr>
                                                <tr runat="server" id="trForgiveForm" visible="false">
                                                    <td colspan="2" style="padding: 10px;">
                                                        <div style="font-weight: bold; color: #808080; padding-bottom: 2px; margin-bottom: 8px; border-bottom: solid 1px #808080; font-style: italic;">
                                                            Modify Forgiven Percentage
                                                        </div>
                                                        <div>
                                                            Forgive
                                                        <asp:TextBox runat="server" ID="txtForgiveAmount" Width="40" CssClass="reservation-forgiven-percentage"></asp:TextBox>
                                                            %
                                                        </div>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                        <div style="margin-top: 10px;">
                                            <table class="reservation-item">
                                                <tr>
                                                    <th>Reservation Notes</th>
                                                </tr>
                                                <tr>
                                                    <td style="padding: 10px 15px 10px 10px;">
                                                        <asp:Panel runat="server" ID="panNotes" Visible="false">
                                                            <asp:TextBox runat="server" ID="txtNotes" TextMode="MultiLine" Width="100%" CssClass="reservation-notes" MaxLength="500"></asp:TextBox>
                                                            <div style="font-style: italic; color: #808080;">
                                                                500 character limit
                                                            </div>
                                                        </asp:Panel>
                                                        <asp:Literal runat="server" ID="litNotes"></asp:Literal>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                    </div>
                                    <div style="margin-top: 10px;">
                                        <asp:CheckBox ID="chkEmailClient" runat="server" Text="Send email notification to user after updating the reservation" Checked="true" CssClass="email-client" />
                                    </div>
                                    <div style="margin-top: 10px;">
                                        <div class="controls">
                                            <input runat="server" id="btnEditSave" type="button" style="width: 65px;" class="save-button" value="Save" />
                                            <asp:Button runat="server" ID="btnEditCancel" Text="Done" Width="65" OnCommand="ReservationHistory_Command" CommandName="cancel" />
                                        </div>
                                        <div class="working-history" style="display: none; padding: 5px;">
                                            Updating reservation history
                                            <img src="<%=GetStaticUrl("images/ajax-loader-2.gif")%>" alt="Working..." />
                                        </div>
                                        <div class="working-billing" style="display: none; padding: 5px;">
                                            Updating billing
                                            <img src="<%=GetStaticUrl("images/ajax-loader-2.gif")%>" alt="Working..." />
                                        </div>
                                        <div class="message"></div>
                                    </div>
                                    <asp:Literal runat="server" ID="litEditMessage"></asp:Literal>
                                </asp:Panel>
                                <asp:Panel runat="server" ID="panHistory" Visible="true">
                                    <asp:Panel runat="server" ID="panCanForgiveNotice" Visible="false">
                                        <div style="padding-bottom: 5px; text-align: right;">
                                            <img src="<%=GetStaticUrl("images/flag-green.png")%>" alt="green flag" />
                                            = Can be forgiven
                                        </div>
                                    </asp:Panel>
                                    <div class="history-container" style="visibility: hidden;">
                                        <asp:Repeater runat="server" ID="rptHistory">
                                            <HeaderTemplate>
                                                <table class="history datatable">
                                                    <thead>
                                                        <tr>
                                                            <th style="width: 70px;">ID</th>
                                                            <th style="width: 100px;">Begin Time</th>
                                                            <th style="width: 100px;">End Time</th>
                                                            <th style="width: 200px;">Resource</th>
                                                            <th>Account</th>
                                                            <th style="width: 150px;">Activity</th>
                                                            <th style="width: 70px;">Started</th>
                                                            <th style="width: 80px;">Canceled</th>
                                                            <th style="width: 80px;">Forgiven</th>
                                                            <th style="width: 60px;">&nbsp;</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr class='<%#GetRowCssClass(CType(Container.DataItem, LNF.Web.Scheduler.ReservationHistoryItem))%>'>
                                                    <td>
                                                        <%#Eval("ReservationID")%>
                                                    </td>
                                                    <td>
                                                        <%#Eval("BeginDateTime", "{0:M/d/yyyy'<br/>'h:mm:ss tt}")%>
                                                    </td>
                                                    <td>
                                                        <%#Eval("EndDateTime", "{0:M/d/yyyy'<br/>'h:mm:ss tt}")%>
                                                    </td>
                                                    <td>
                                                        <%#Eval("ResourceName")%>
                                                    </td>
                                                    <td>
                                                        <%#Eval("AccountName")%>
                                                    </td>
                                                    <td>
                                                        <%#Eval("ActivityName")%>
                                                    </td>
                                                    <td style="text-align: center;">
                                                        <%#Eval("IsStarted")%>
                                                    </td>
                                                    <td style="text-align: center;">
                                                        <%#IsCanceled(Eval("IsActive"))%>
                                                    </td>
                                                    <td style="text-align: right;">
                                                        <%#Eval("ForgiveAmount")%>%
                                                    </td>
                                                    <td style="text-align: center; white-space: nowrap;">
                                                        <asp:HyperLink runat="server" ID="hypEditReservation" Text="Edit" NavigateUrl='<%#GetEditUrl(Container.DataItem)%>'></asp:HyperLink>
                                                        <asp:Image runat="server" ID="imgCanBeForgiven" ImageUrl='<%#GetStaticUrl("images/flag-green.png")%>' AlternateText="green flag" Visible='<%#IsBeforeForgiveCutoff(Convert.ToInt32(Eval("ReservationID")))%>'></asp:Image>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody> </table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </div>
                                    <asp:Literal runat="server" ID="litShowCanceledForModificationMessage"></asp:Literal>
                                    <asp:Label ID="lblNoData" runat="server" />
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        <asp:Literal runat="server" ID="litDebug"></asp:Literal>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="scripts/jquery.dateManager.js?v=20161115"></script>
    <script src="scripts/jquery.reservationHistory.js?v=20170420"></script>
    <script>
        $(".date-manager").dateManager();
        $(".reservation-history").reservationHistory();

        $.extend($.fn.dataTableExt.oSort, {
            'moment-pre': function (a) {
                var m = moment(a, 'M/D/YYYY[<br/>]h:mm:ss A');
                return m;
            },
            'moment-asc': function (a, b) {
                return a.isBefore(b) ? -1 : (a.isAfter(b) ? 1 : 0);
            },
            'moment-desc': function (a, b) {
                return a.isBefore(b) ? 1 : (a.isAfter(b) ? -1 : 0);
            }
        });

        $(".datatable").dataTable({
            'stateSave': true,
            'stateSaveCallback': function (settings, data) {

            },
            'stateLoadCallback': function (settings) {

            },
            'order': [[1, 'desc']],
            'columnDefs': [
                { 'orderable': false, 'searchable': false, 'targets': [9] },
                { 'type': 'moment', 'targets': [1, 2] }
            ],
            'initComplete': function (settings, json) {
                $('.history-container').css({ 'visibility': 'visible' });
            }
        });
    </script>
</asp:Content>
