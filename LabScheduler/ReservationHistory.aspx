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

        /* this is for the reservation list */
        .history {
            width: 100%;
        }

        .history-date {
            width: 130px;
        }

        /* this is for the edit form */
        .edit-history {
            width: 800px;
        }

            .edit-history .group {
                width: 100%;
                border-collapse: collapse;
                margin-bottom: 10px;
            }

                .edit-history .group .row-label {
                    border: solid 1px #aaa;
                    background-color: #ddd;
                    color: #003366;
                    font-size: 10pt;
                    font-weight: bold;
                    font-family: Arial;
                    width: 20%;
                    padding: 5px;
                }

                .edit-history .group .row-data {
                    border: solid 1px #aaa;
                    padding: 5px;
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

        .controls {
            margin-top: 15px;
        }

            .controls .save-button {
                margin-right: 5px;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5>Reservation History</h5>

    <hr />

    <div class="form-inline">
        <div class="form-group">
            <span>Show reservations for</span>
            <asp:DropDownList runat="server" ID="ddlClients" AutoPostBack="False" DataTextField="DisplayName" DataValueField="ClientID" CssClass="form-control"></asp:DropDownList>
        </div>
    </div>

    <div class="form-inline" style="margin-top: 10px;">
        <div class="form-group">
            <span>Show reservations in the past</span>
            <span class="date-manager">
                <asp:DropDownList runat="server" ID="ddlRange" AutoPostBack="False" CssClass="form-control daterange-select">
                    <asp:ListItem Value="0" Selected="True">30 days</asp:ListItem>
                    <asp:ListItem Value="1">3 months</asp:ListItem>
                    <asp:ListItem Value="2">1 year</asp:ListItem>
                    <asp:ListItem Value="3">All</asp:ListItem>
                </asp:DropDownList>
                <asp:TextBox runat="server" ID="txtStartDate" CssClass="sdate datepicker form-control" Width="85"></asp:TextBox>
                <asp:TextBox runat="server" ID="txtEndDate" CssClass="edate datepicker form-control" Width="85"></asp:TextBox>
            </span>
        </div>
    </div>

    <div style="margin-top: 10px;">
        <asp:Button runat="server" ID="btnSearchHistory" Text="Search" OnClick="BtnSearchHistory_Click" CssClass="lnf btn btn-default" />
    </div>

    <hr />

    <asp:PlaceHolder runat="server" ID="phSelectHistory">
        <div class="history-container" style="visibility: hidden;">
            <table class="history datatable table table-striped">
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
                    <asp:Repeater runat="server" ID="rptHistory">
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
                                    <asp:Image runat="server" ID="imgCanBeForgiven" ImageUrl="//ssel-apps.eecs.umich.edu/static/images/flag-green.png" AlternateText="green flag" Visible='<%#IsBeforeForgiveCutoff(Convert.ToInt32(Eval("ReservationID")))%>'></asp:Image>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>

            <hr />

            <div style="padding-bottom: 5px;">
                <img src="//ssel-apps.eecs.umich.edu/static/images/flag-green.png" alt="green flag" />
                = Can be forgiven
            </div>

            <asp:Literal runat="server" ID="litShowCanceledForModificationMessage"></asp:Literal>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phEditHistory" Visible="false">
        <div class="edit-history">
            <asp:PlaceHolder runat="server" ID="phSaveAlert" Visible="false">
                <div runat="server" id="divSaveAlert" class="alert alert-danger alert-dismissible" role="alert">
                    <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <asp:Literal runat="server" ID="litSaveAlertText"></asp:Literal>
                </div>
            </asp:PlaceHolder>

            <table class="group">
                <tr>
                    <th class="row-label">Resource</th>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litResourceName"></asp:Literal>
                    </td>
                </tr>
            </table>

            <table class="group">
                <tr>
                    <td class="row-label">Reservation ID</td>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litReservationID"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="row-label">Activity</td>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litActivityName"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="row-label">Started</td>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litIsStarted"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="row-label">Canceled</td>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litIsCanceled"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="row-label">Invitee(s)</td>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litInvitees"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="row-label">Account</td>
                    <td class="row-data">
                        <asp:Literal runat="server" ID="litCurrentAccount"></asp:Literal>
                    </td>
                </tr>
                <tr runat="server" id="trAccount" visible="false">
                    <td class="row-data" colspan="2">
                        <div style="padding: 5px;">
                            <div style="margin-bottom: 3px;">
                                <strong><em class="text-muted">Modify Account</em></strong>
                            </div>
                            <div>
                                <asp:DropDownList runat="server" ID="ddlEditReservationAccount" DataValueField="AccountID" DataTextField="Name" Width="100%" CssClass="reservation-account-id form-control"></asp:DropDownList>
                                <asp:Literal runat="server" ID="litEditReservationAccountMessage"></asp:Literal>
                            </div>
                        </div>
                    </td>
                </tr>
            </table>

            <table class="group">
                <tr>
                    <td class="row-label text-center" colspan="3">&nbsp;</td>
                    <td class="row-label text-center" colspan="2">Duration (Minutes)</td>
                </tr>
                <tr>
                    <td class="row-label text-center">&nbsp;</td>
                    <td class="row-label text-center">Begin Time</td>
                    <td class="row-label text-center">End Time</td>
                    <td class="row-label text-center">Regular</td>
                    <td class="row-label text-center">Overtime</td>
                </tr>
                <tr>
                    <td class="row-label">Reserved</td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litReservedBeginDateTime"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litReservedEndDateTime"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litReservedRegularDuration"></asp:Literal>
                    </td>
                    <td class="row-data text-right">--</td>
                </tr>
                <tr>
                    <td class="row-label">Actual</td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litActualBeginDateTime"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litActualEndDateTime"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litActualRegularDuration"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litActualOvertimeDuration"></asp:Literal>
                    </td>
                </tr>
                <tr>
                    <td class="row-label">Chargeable</td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litChargeableBeginDateTime"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litChargeableEndDateTime"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litChargeableRegularDuration"></asp:Literal>
                    </td>
                    <td class="row-data text-right">
                        <asp:Literal runat="server" ID="litChargeableOvertimeDuration"></asp:Literal>
                    </td>
                </tr>
            </table>

            <table class="group">
                <tr>
                    <td class="row-label">Forgiven Percentage</td>
                    <td class="row-data forgiven-percentage">
                        <asp:Literal runat="server" ID="litForgiveAmount"></asp:Literal>
                    </td>
                </tr>
                <tr runat="server" id="trForgiveForm" visible="false">
                    <td class="row-data" colspan="2">
                        <div style="padding: 5px;">
                            <div style="margin-bottom: 3px;">
                                <strong><em class="text-muted">Modify Forgiven Percentage</em></strong>
                            </div>
                            <div class="form-inline">
                                <div class="form-group">
                                    <span>Forgive</span>
                                    <asp:TextBox runat="server" ID="txtForgiveAmount" Width="60" CssClass="form-control reservation-forgiven-percentage text-right"></asp:TextBox>
                                    <span>%</span>
                                </div>
                            </div>
                        </div>
                    </td>
                </tr>
            </table>

            <table class="group">
                <tr>
                    <td class="row-label">Reservation Notes</td>
                </tr>
                <tr>
                    <td class="row-data">
                        <div style="padding: 5px;">
                            <asp:TextBox runat="server" ID="txtNotes" TextMode="MultiLine" Width="100%" CssClass="reservation-notes form-control" MaxLength="500"></asp:TextBox>
                            <div class="text-muted">
                                <em>500 character limit</em>
                            </div>
                        </div>
                    </td>
                </tr>
            </table>
        </div>

        <div style="margin-top: 10px;">
            <asp:CheckBox ID="chkEmailClient" runat="server" Text="Send email notification to user after update" Checked="true" CssClass="email-client" />
        </div>

        <div class="controls">
            <asp:Button runat="server" ID="btnEditSave" Text="Save" Width="65" OnCommand="ReservationHistory_Command" CommandName="save" CssClass="lnf btn btn-default save-button" />
            <asp:Button runat="server" ID="btnEditCancel" Text="Done" Width="65" OnCommand="ReservationHistory_Command" CommandName="cancel" CssClass="lnf btn btn-default done-button" />
        </div>

        <asp:Literal runat="server" ID="litEditMessage"></asp:Literal>
    </asp:PlaceHolder>

    <asp:Literal runat="server" ID="litDebug"></asp:Literal>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="scripts/jquery.dateManager.js?v=20161115"></script>

    <script>
        $(".date-manager").dateManager();

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
                window.localStorage.setItem('ReservationHistoryDatatableState', JSON.stringify(data));
            },
            'stateLoadCallback': function (settings) {
                return $.parseJSON(window.localStorage.getItem('ReservationHistoryDatatableState'));
            },
            'order': [[1, 'desc']],
            'columnDefs': [
                { 'orderable': false, 'searchable': false, 'targets': [9] },
                { 'type': 'moment', 'targets': [1, 2] }
            ],
            'language': {
                'emptyTable': 'No past reservations were found'
            },
            'initComplete': function (settings, json) {
                $('.history-container').css({ 'visibility': 'visible' });
            }
        });
    </script>
</asp:Content>
