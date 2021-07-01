<%@ Page Title="Reservation" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="Reservation.aspx.vb" Inherits="LabScheduler.Pages.Reservation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="//ssel-apps.eecs.umich.edu/static/lib/bootstrap/plugins/bootstrap-datepicker/css/bootstrap-datepicker.min.css" />

    <style>
        .resource-name {
            color: #cc6633;
        }

        .confirm-button {
            font-weight: bold;
            background-color: #f2f2f2;
        }

        .btn {
            margin-right: 5px;
        }

            .btn:last-child {
                margin-right: 0;
            }

        .well.confirmation {
            border: solid 1px #aaaa66;
            background-color: #ffff66;
            border-radius: 5px;
            box-shadow: 2px 2px 3px #aaa;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5 style="margin-bottom: 20px;">
        <asp:Literal runat="server" ID="litHeader"></asp:Literal>
        <span class="resource-name">
            <asp:Literal runat="server" ID="litResourceName"></asp:Literal>
        </span>
    </h5>

    <asp:PlaceHolder runat="server" ID="phLoadError" Visible="false">
        <div class="alert alert-danger" role="alert">
            <asp:Literal runat="server" ID="litLoadError"></asp:Literal>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phReservationAlert" Visible="false">
        <div runat="server" id="divReservationAlert" class="alert alert-danger alert-dismissable" role="alert">
            <button runat="server" id="btnReservationAlertClose" type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
            <asp:Literal runat="server" ID="litReservationAlert"></asp:Literal>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phReservation">
        <div class="reservation">
            <div class="lnf panel panel-default">
                <div class="panel-heading">
                    <h3 class="panel-title">Reservation Information</h3>
                </div>
                <div class="panel-body">
                    <div class="form-horizontal">
                        <div class="form-group">
                            <label class="control-label col-sm-2">Scheduled By *</label>
                            <div class="col-sm-2">
                                <p class="form-control-static">
                                    <asp:Literal ID="litClientName" runat="server"></asp:Literal>
                                </p>
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="control-label col-sm-2">Start Date *</label>
                            <div class="col-sm-5">
                                <div class="form-control-static">
                                    <asp:Literal ID="litStartDate" runat="server"></asp:Literal>
                                    <span>(click on calendar to change date)</span>
                                    <asp:PlaceHolder runat="server" ID="phPastSelectedDateError" Visible="false">
                                        <div class="alert alert-danger" role="alert" style="margin-top: 10px; margin-bottom: 0;">
                                            <asp:Literal runat="server" ID="litPastSelectedDateError"></asp:Literal>
                                        </div>
                                    </asp:PlaceHolder>
                                </div>
                            </div>
                        </div>
                        <asp:PlaceHolder runat="server" ID="phStartTimeAndDuration" Visible="false">
                            <div class="form-group">
                                <label class="control-label col-sm-2">Start Time *</label>
                                <div class="col-sm-10">
                                    <div class="form-inline">
                                        <div class="form-group">
                                            <asp:DropDownList runat="server" ID="ddlStartTimeHour" AutoPostBack="True" CssClass="form-control" OnSelectedIndexChanged="DdlStartTimeHour_SelectedIndexChanged">
                                            </asp:DropDownList>
                                        </div>
                                        <div class="form-group">
                                            <asp:DropDownList runat="server" ID="ddlStartTimeMin" AutoPostBack="True" CssClass="form-control" OnSelectedIndexChanged="DdlStartTimeMin_SelectedIndexChanged">
                                            </asp:DropDownList>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="control-label col-sm-2">Duration *</label>
                                <asp:PlaceHolder runat="server" ID="phDurationSelect" Visible="false">
                                    <div class="col-sm-2">
                                        <asp:DropDownList runat="server" ID="ddlDuration" CssClass="form-control">
                                        </asp:DropDownList>
                                        <asp:Label runat="server" ID="lblMaxSchedLimit" Visible="false" ForeColor="#FF0000"></asp:Label>
                                    </div>
                                </asp:PlaceHolder>
                                <asp:PlaceHolder runat="server" ID="phDurationText" Visible="false">
                                    <div class="col-sm-2">
                                        <div class="input-group">
                                            <asp:TextBox runat="server" ID="txtDuration" MaxLength="5" CssClass="form-control"></asp:TextBox>
                                            <div class="input-group-addon">minutes</div>
                                        </div>
                                    </div>
                                </asp:PlaceHolder>
                            </div>
                        </asp:PlaceHolder>
                        <asp:PlaceHolder runat="server" ID="phRecurring">
                            <div class="form-group">
                                <div class="col-sm-offset-2 col-sm-10">
                                    <div class="checkbox">
                                        <asp:CheckBox runat="server" ID="chkIsRecurring" OnCheckedChanged="ChkIsRecurring_CheckedChanged" AutoPostBack="true" Text="Is Recurring" />
                                    </div>
                                </div>
                            </div>
                        </asp:PlaceHolder>
                        <div class="form-group">
                            <div class="col-sm-offset-2 col-sm-10">
                                <div class="checkbox">
                                    <label>
                                        <input type="checkbox" runat="server" id="chkKeepAlive" checked />
                                        Keep my reservation after grace period (do not auto-cancel)
                                    </label>
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <div class="col-sm-offset-2 col-sm-5">
                                <div class="checkbox">
                                    <label>
                                        <input type="checkbox" runat="server" id="chkAutoEnd" class="autoend-checkbox" />
                                        Automatically end the reservation after the end time
                                    </label>
                                </div>
                                <div class="autoend-warning alert alert-warning" role="alert" style="margin-top: 10px; margin-bottom: 0; display: none;">
                                    <strong>Warning:</strong> Checking this box will end your reservation automatically. Please make sure to complete your work before the scheduled end time.
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="control-label col-sm-2">Activity *</label>
                            <asp:PlaceHolder runat="server" ID="phActivity">
                                <div class="col-sm-2">
                                    <asp:DropDownList runat="server" ID="ddlActivity" AutoPostBack="True" DataTextField="ActivityName" DataValueField="ActivityID" CssClass="activity-select form-control" OnSelectedIndexChanged="DdlActivity_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </div>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" ID="phActivityName" Visible="false">
                                <div class="col-sm-10">
                                    <p class="form-control-static">
                                        <asp:Literal runat="server" ID="litActivityName"></asp:Literal>
                                    </p>
                                </div>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" ID="phActivityMessage" Visible="false">
                                <div class="col-sm-10">
                                    <p class="form-control-static">
                                        <em class="text-muted">
                                            <asp:Literal runat="server" ID="litActivityMessage"></asp:Literal>
                                        </em>
                                    </p>
                                </div>
                            </asp:PlaceHolder>
                        </div>
                        <div class="form-group">
                            <label class="control-label col-sm-2">Billing Account *</label>
                            <asp:PlaceHolder runat="server" ID="phBillingAccount">
                                <div class="col-sm-5">
                                    <asp:DropDownList runat="server" ID="ddlAccount" DataTextField="Name" DataValueField="AccountID" CssClass="account-select form-control">
                                    </asp:DropDownList>
                                </div>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder runat="server" ID="phBillingAccountMessage" Visible="false">
                                <div class="col-sm-10">
                                    <p class="form-control-static">
                                        <em class="text-muted">
                                            <asp:Literal runat="server" ID="litBillingAccountMessage"></asp:Literal>
                                        </em>
                                    </p>
                                </div>
                            </asp:PlaceHolder>
                        </div>
                        <div class="form-group">
                            <label class="control-label col-sm-2">Notes</label>
                            <div class="col-sm-5">
                                <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" CssClass="form-control" Height="80"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <asp:PlaceHolder runat="server" ID="phRecurringReservation" Visible="false">
                <div class="lnf panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Recurring Reservation</h3>
                    </div>
                    <div class="panel-body">
                        <h3>Pattern</h3>
                        <hr />
                        <label class="radio-inline">
                            <input type="radio" runat="server" id="rdoRecurringPatternWeekly" name="RecurringPattern" checked class="weekly-radio" />
                            Weekly
                        </label>
                        <label class="radio-inline">
                            <input type="radio" runat="server" id="rdoRecurringPatternMonthly" name="RecurringPattern" class="monthly-radio" />
                            Monthly
                        </label>
                        <div style="margin-left: 0px">
                            <div class="well" style="margin-top: 10px;">
                                <div class="weekly-panel">
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklySunday" name="RecurringPatternWeekly" />
                                        Sunday
                                    </label>
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklyMonday" name="RecurringPatternWeekly" />
                                        Monday
                                    </label>
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklyTuesday" name="RecurringPatternWeekly" />
                                        Tuesday
                                    </label>
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklyWednesday" name="RecurringPatternWeekly" />
                                        Wednesday
                                    </label>
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklyThursday" name="RecurringPatternWeekly" />
                                        Thursday
                                    </label>
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklyFriday" name="RecurringPatternWeekly" />
                                        Friday
                                    </label>
                                    <label class="radio-inline">
                                        <input type="radio" runat="server" id="rdoRecurringPatternWeeklySaturday" name="RecurringPatternWeekly" />
                                        Saturday
                                    </label>
                                </div>

                                <div class="monthly-panel form-inline">
                                    <span>The</span>
                                    <div class="form-group">
                                        <asp:DropDownList ID="ddlMonthly1" runat="server" CssClass="form-control">
                                            <asp:ListItem Text="First" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="Second" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="Third" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="Fourth" Value="4"></asp:ListItem>
                                            <asp:ListItem Text="Last" Value="5"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="form-group">
                                        <asp:DropDownList ID="ddlMonthly2" runat="server" CssClass="form-control">
                                            <asp:ListItem Text="Sunday" Value="0"></asp:ListItem>
                                            <asp:ListItem Text="Monday" Value="1"></asp:ListItem>
                                            <asp:ListItem Text="Tuesday" Value="2"></asp:ListItem>
                                            <asp:ListItem Text="Wednesday" Value="3"></asp:ListItem>
                                            <asp:ListItem Text="Thursday" Value="4"></asp:ListItem>
                                            <asp:ListItem Text="Friday" Value="5"></asp:ListItem>
                                            <asp:ListItem Text="Saturday" Value="6"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <span>of every month</span>
                                </div>
                            </div>
                        </div>

                        <h3>Range</h3>
                        <hr />
                        <div class="form-horizontal">
                            <div class="form-group">
                                <label class="col-sm-1 control-label">Start</label>
                                <div class="col-sm-2">
                                    <asp:TextBox runat="server" ID="txtRecurringStartDate" CssClass="bs-datepicker form-control"></asp:TextBox>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-1 control-label">Duration</label>
                                <div class="col-sm-11">
                                    <div class="radio">
                                        <label>
                                            <input type="radio" runat="server" id="rdoRecurringRangeInfinite" checked name="RecurringRange" class="infinite-radio" />
                                            Infinite
                                        </label>
                                    </div>
                                    <div class="radio">
                                        <label>
                                            <input type="radio" runat="server" id="rdoRecurringRangeEndBy" name="RecurringRange" class="endby-radio" />
                                            <span>End By:</span>
                                            <input type="text" runat="server" id="txtEndDate" class="endby-textbox bs-datepicker form-control" disabled style="display: unset; width: unset;" />
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </asp:PlaceHolder>

            <asp:PlaceHolder runat="server" ID="phProcessInfo">
                <div class="lnf panel panel-default">
                    <div class="panel-heading">
                        <h3 class="panel-title">Process Info</h3>
                    </div>
                    <div class="panel-body">
                        <asp:Repeater runat="server" ID="rptProcessInfo" OnItemDataBound="RptProcessInfo_ItemDataBound">
                            <HeaderTemplate>
                                <table border="0" class="reservation-process-info">
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>&nbsp;</td>
                                    <td class="param-name">
                                        <asp:HiddenField runat="server" ID="hidPIID" />
                                        <asp:Literal runat="server" ID="litParamName"></asp:Literal>
                                    </td>
                                    <td class="value-name">
                                        <asp:Literal runat="server" ID="litValueName"></asp:Literal>
                                    </td>
                                    <td>&nbsp;</td>
                                </tr>
                                <tr>
                                    <td class="pi-name">
                                        <asp:Literal runat="server" ID="litPIName"></asp:Literal>
                                    </td>
                                    <td class="pi-params">
                                        <asp:DropDownList ID="ddlParam" runat="server" CssClass="form-control">
                                        </asp:DropDownList>
                                    </td>
                                    <td class="pi-value">
                                        <asp:TextBox runat="server" ID="txtValue" MaxLength="5" CssClass="form-control"></asp:TextBox>
                                    </td>
                                    <td class="pi-special">
                                        <asp:CheckBox ID="chkSpecial" Text="Special" runat="server"></asp:CheckBox>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <SeparatorTemplate>
                                <tr>
                                    <td colspan="4" class="pi-separator">
                                        <hr>
                                    </td>
                                </tr>
                            </SeparatorTemplate>
                            <FooterTemplate>
                                </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                </div>

                <input type="hidden" runat="server" id="hidProcessInfoData" class="hidProcessInfoData" value="" />
            </asp:PlaceHolder>

            <div class="lnf panel panel-default">
                <div class="panel-heading">
                    <h3 class="panel-title">Invitees</h3>
                </div>
                <div class="panel-body">
                    <div class="row">
                        <div class="col-sm-4">
                            <asp:DataGrid ID="dgInvitees" runat="server" DataKeyField="InviteeID" ShowFooter="True" AutoGenerateColumns="False" BorderColor="#4682B4" CellPadding="3" ShowHeader="False" CssClass="Table" Width="100%" OnItemCommand="DgInvitees_ItemCommand" OnItemCreated="DgInvitees_ItemCreated" OnItemDataBound="DgInvitees_ItemDataBound">
                                <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                                <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                                <FooterStyle BackColor="#CCFFCC" HorizontalAlign="Center"></FooterStyle>
                                <Columns>
                                    <asp:TemplateColumn HeaderText="Invitee">
                                        <ItemTemplate>
                                            <asp:Label ID="lblInviteeID" Visible="False" runat="server" />
                                            <asp:Label ID="lblInviteeName" runat="server" />
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:DropDownList runat="server" ID="ddlInvitees" DataValueField="ClientID" DataTextField="DisplayName" CssClass="form-control">
                                            </asp:DropDownList>
                                        </FooterTemplate>
                                    </asp:TemplateColumn>
                                    <asp:TemplateColumn>
                                        <ItemStyle HorizontalAlign="Center" Width="50" />
                                        <FooterStyle HorizontalAlign="Center" Width="50" />
                                        <ItemTemplate>
                                            <asp:ImageButton ID="ibtnDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" runat="server"></asp:ImageButton>
                                        </ItemTemplate>
                                        <FooterTemplate>
                                            <asp:Button ID="btnInsert" CommandName="Insert" CssClass="lnf btn btn-default btn-sm" Text="Add" runat="server"></asp:Button>
                                        </FooterTemplate>
                                    </asp:TemplateColumn>
                                </Columns>
                            </asp:DataGrid>
                        </div>
                    </div>
                    <asp:PlaceHolder runat="server" ID="phInviteeWarning" Visible="false">
                        <div class="alert alert-warning" role="alert" style="margin-top: 10px; margin-bottom: 0;">
                            <asp:Literal runat="server" ID="litInviteeWarning"></asp:Literal>
                        </div>
                    </asp:PlaceHolder>
                </div>
            </div>

            <asp:Button runat="server" ID="btnSubmit" CssClass="lnf btn btn-default btn-lg" OnClick="BtnSubmit_Click" />
            <asp:Button runat="server" ID="btnCancel" CssClass="lnf btn btn-default btn-lg" Text="Back" OnClick="BtnCancel_Click" />
        </div>
        <!-- /.reservation -->
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phConfirm" Visible="false">
        <div class="row">
            <div class="col-xs-6">
                <div class="well confirmation">
                    <asp:Label runat="server" ID="lblConfirm" Font-Bold="true" />
                    <div style="text-align: center; margin-top: 10px;">
                        <asp:PlaceHolder runat="server" ID="phConfirmYesAndStart" Visible="false">
                            <asp:Button runat="server" ID="btnConfirmYesAndStart" Text="Yes and Start Reservation" CssClass="confirm-button btn btn-default" OnClientClick="disable('.confirm-button');" UseSubmitBehavior="false" OnClick="BtnConfirmYesAndStart_Click" />
                        </asp:PlaceHolder>
                        <asp:Button runat="server" ID="btnConfirmYes" Text="Yes" CssClass="confirm-button btn btn-default" OnClientClick="disable('.confirm-button');" UseSubmitBehavior="false" OnClick="BtnConfirmYes_Click" />
                        <asp:Button runat="server" ID="btnConfirmNo" Text="No" CssClass="confirm-button btn btn-default" OnClientClick="disable('.confirm-button');" UseSubmitBehavior="false" OnClick="BtnConfirmNo_Click" />
                    </div>
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phConfirmError" Visible="false">
        <div class="row">
            <div class="col-xs-6">
                <div class="alert alert-danger" role="alert">
                    <asp:Literal runat="server" ID="litConfirmError"></asp:Literal>
                </div>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="//ssel-apps.eecs.umich.edu/static/lib/bootstrap/plugins/bootstrap-datepicker/js/bootstrap-datepicker.min.js"></script>

    <script>
        $('.reservation').reservation();

        function disable(target) {
            $(target).prop("disabled", true);
            return true;
        }

        $(function () {
            initProcessInfo();
        });

        $(".bs-datepicker").datepicker();

        console.log("Current View: " + $(".current-view").val());
    </script>
</asp:Content>
