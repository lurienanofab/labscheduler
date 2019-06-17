<%@ Page Title="Recurring Reservations" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="UserRecurringReservationEdit.aspx.vb" Inherits="LabScheduler.Pages.UserRecurringReservationEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="recurring-reservations">

        <h5>Modify Recurring Reservation</h5>

        <em class="text-muted" style="margin-top: 20px; display: block;">Note: Modifing these settings will not alter existing reservations. You can edit existing reservations by clicking the ID link in the list below.</em>

        <div style="margin-top: 20px;">
            <asp:Repeater runat="server" ID="rptRecurrence" OnItemDataBound="RptRecurrence_ItemDataBound">
                <ItemTemplate>
                    <div class="lnf panel panel-default" style="margin-top: 20px;">
                        <div class="panel-heading">
                            <h3 class="panel-title">Modify Recurring Reservation</h3>
                        </div>
                        <div class="panel-body">
                            <div class="form-horizontal">
                                <div class="form-group">
                                    <label class="control-label col-lg-2">Resource</label>
                                    <div class="col-lg-10">
                                        <p class="form-control-static">
                                            <asp:HyperLink runat="server" ID="hypResource" CssClass="resource-link"></asp:HyperLink>
                                        </p>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label col-lg-2">Start Time</label>
                                    <div class="col-lg-10">
                                        <div class="form-inline">
                                            <div class="form-group">
                                                <asp:DropDownList ID="ddlStartTimeHour" runat="server" AutoPostBack="True" CssClass="form-control">
                                                </asp:DropDownList>
                                            </div>
                                            <div class="form-group">
                                                <asp:DropDownList ID="ddlStartTimeMin" runat="server" AutoPostBack="True" CssClass="form-control">
                                                </asp:DropDownList>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label col-lg-2">Duration</label>
                                    <div class="col-lg-2">
                                        <asp:TextBox runat="server" ID="txtDuration" CssClass="form-control"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-offset-2 col-sm-10">
                                        <div class="checkbox">
                                            <label>
                                                <input type="checkbox" runat="server" id="chkKeepAlive" />
                                                Keep my reservation after grace period (do not auto-cancel)
                                            </label>
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-sm-offset-2 col-sm-5">
                                        <div class="checkbox">
                                            <label>
                                                <input type="checkbox" runat="server" id="chkAutoEnd" />
                                                Automatically end the reservation after the end time
                                            </label>
                                        </div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="control-label col-lg-2">Notes</label>
                                    <div class="col-lg-2">
                                        <asp:TextBox runat="server" ID="txtNotes" CssClass="form-control" TextMode="MultiLine"></asp:TextBox>
                                    </div>
                                </div>
                            </div>

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
                                    <div class="weekly-panel" style="display: none;">
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

                                    <div class="monthly-panel form-inline" style="display: none;">
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
                                        <asp:TextBox runat="server" ID="txtStartDate" CssClass="bs-datepicker form-control"></asp:TextBox>
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
                </ItemTemplate>
            </asp:Repeater>

            <asp:Literal runat="server" ID="litMessage"></asp:Literal>

            <asp:Button runat="server" ID="btnSave" Text="Save" CssClass="lnf btn btn-default" OnClick="BtnSave_Click" />

            <asp:HyperLink runat="server" ID="hypCancel" CssClass="lnf btn btn-default">Cancel</asp:HyperLink>
        </div>

        <div style="margin-top: 20px;">
            <div class="lnf panel panel-default" style="margin-top: 20px;">
                <div class="panel-heading">
                    <h3 class="panel-title">Recent Reservations</h3>
                </div>
                <div class="panel-body">
                    <asp:Repeater runat="server" ID="rptExistingReservations">
                        <HeaderTemplate>
                            <table class="table table-striped">
                                <thead>
                                    <tr>
                                        <th>ID</th>
                                        <th>BeginDateTime</th>
                                        <th>EndDateTime</th>
                                        <th>ActualBeginDateTime</th>
                                        <th>ActualEndDateTime</th>
                                        <th class="text-center">Cancelled</th>
                                        <th>&nbsp;</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%#GetReservationLink(Container.DataItem)%></td>
                                <td><%#Eval("BeginDateTime", "{0:MM/dd/yyyy h:mm:ss tt}")%></td>
                                <td><%#Eval("EndDateTime", "{0:MM/dd/yyyy h:mm:ss tt}")%></td>
                                <td><%#Eval("ActualBeginDateTime", "{0:MM/dd/yyyy h:mm:ss tt}")%></td>
                                <td><%#Eval("ActualEndDateTime", "{0:MM/dd/yyyy h:mm:ss tt}")%></td>
                                <td class="text-center"><%#GetIsCancelled(Container.DataItem)%></td>
                                <td><asp:LinkButton runat="server" ID="btnCancelReservation" OnCommand="BtnCancelReservation_Command" CommandArgument='<%#Eval("ReservationID")%>' Visible='<%#GetCancelReservationLinkVisible(Container.DataItem)%>'>Cancel</asp:LinkButton></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="//ssel-apps.eecs.umich.edu/static/lib/bootstrap/plugins/bootstrap-datepicker/js/bootstrap-datepicker.min.js"></script>

    <script>
        $(".recurring-reservations").each(function () {
            var $this = $(this);

            var togglePattern = function () {
                //show the week day radio list when the 'Weekly' radio is selected
                if ($('.weekly-radio', $this).is(':checked')) {
                    $('.monthly-panel', $this).hide();
                    $('.weekly-panel', $this).show();
                }
                //show the monthly options when the 'Monthly' radio is selected
                if ($('.monthly-radio', $this).is(':checked')) {
                    $('.weekly-panel', $this).hide();
                    $('.monthly-panel', $this).show();
                }
            };

            var toggleRange = function () {
                //when 'End By' radio is selected enable the 'End By' datepicker
                if ($('.endby-radio', $this).is(':checked'))
                    $('.endby-textbox', $this).removeAttr('disabled').css('background-color', '');
                //when 'Infinite' radio is selected disable the 'End By' datepicker
                if ($('.infinite-radio', $this).is(':checked'))
                    $('.endby-textbox', $this).prop('disabled', true).val('');
            };

            togglePattern();

            toggleRange();

            $(".bs-datepicker", $this).datepicker();

            $this.on('change', '.weekly-radio', function (e) {
                togglePattern();
            }).on('change', '.monthly-radio', function (e) {
                togglePattern();
            }).on('change', '.infinite-radio', function (e) {
                toggleRange();
            }).on('change', '.endby-radio', function (e) {
                toggleRange();
            });
        });
    </script>
</asp:Content>
