<%@ Page Title="Preferences" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="Preference.aspx.vb" Inherits="LabScheduler.Pages.Preference" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="//ssel-apps.eecs.umich.edu/static/lib/jquery-ui/themes/smoothness/jquery-ui.min.css" />

    <style>
        .preferences {
            margin-bottom: 40px;
        }

        .settings {
            border-collapse: collapse;
        }

            .settings td {
                padding: 5px;
                border-bottom: solid 1px #dadada;
            }

                .settings td table td, .settings tr.last td {
                    border-bottom: none;
                }

        .accounts {
            cursor: pointer;
        }

        .radio-inline > label,
        .checkbox-inline > label {
            font-weight: normal;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="preferences">
        <h5 style="margin-bottom: 20px;">Preferences</h5>

        <asp:PlaceHolder runat="server" ID="phErrorMessage" Visible="false">
            <div class="alert alert-danger" role="alert" style="margin-bottom: 20px;">
                <asp:Literal runat="server" ID="litErrorMessage"></asp:Literal>
            </div>
        </asp:PlaceHolder>

        <asp:PlaceHolder runat="server" ID="phSuccessMessage" Visible="false">
            <div class="alert alert-success" role="alert" style="margin-bottom: 20px;">
                <asp:Literal runat="server" ID="litSuccessMessage"></asp:Literal>
            </div>
        </asp:PlaceHolder>

        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">My Preferences</h3>
            </div>
            <div class="panel-body">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Home Building</label>
                        <div class="col-sm-2">
                            <asp:DropDownList runat="server" ID="ddlBuilding" AutoPostBack="true" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Home Lab</label>
                        <div class="col-sm-2">
                            <asp:DropDownList runat="server" ID="ddlLab" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Preferred View</label>
                        <div class="col-sm-10">
                            <div class="radio-inline">
                                <label>
                                    <input type="radio" runat="server" id="rdoDefaultViewDay" name="DefaultView" />
                                    Day View
                                </label>
                            </div>
                            <div class="radio-inline">
                                <label>
                                    <input type="radio" runat="server" id="rdoDefaultViewWeek" name="DefaultView" />
                                    Week View
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Working Hours</label>
                        <div class="col-sm-10">
                            <div class="radio">
                                <label>
                                    <input type="radio" runat="server" id="rdoHoursAllDay" name="WorkingHours" class="working-hours allday" />
                                    All Day
                                </label>
                            </div>
                            <div class="form-inline">
                                <div class="radio-inline">
                                    <label>
                                        <input type="radio" runat="server" id="rdoHoursRange" name="WorkingHours" class="working-hours range" />
                                        Hour Range:
                                    </label>
                                    <div class="form-group">
                                        <span>from</span>
                                        <asp:DropDownList runat="server" ID="ddlBeginHour" CssClass="hour-range form-control" Width="80">
                                        </asp:DropDownList>
                                    </div>
                                    <div class="form-group">
                                        <span>to</span>
                                        <asp:DropDownList runat="server" ID="ddlEndHour" CssClass="hour-range form-control" Width="80">
                                        </asp:DropDownList>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Working Days</label>
                        <div class="col-sm-10">
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysSun" />
                                    Sunday
                                </label>
                            </div>
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysMon" />
                                    Monday
                                </label>
                            </div>
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysTue" />
                                    Tuesday
                                </label>
                            </div>
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysWed" />
                                    Wednesday
                                </label>
                            </div>
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysThu" />
                                    Thursday
                                </label>
                            </div>
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysFri" />
                                    Friday
                                </label>
                            </div>
                            <div class="checkbox-inline">
                                <label>
                                    <input type="checkbox" runat="server" id="chkWorkingDaysSat" />
                                    Saturday
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Show Treeview Images</label>
                        <div class="col-sm-10">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" runat="server" id="chkShowTreeViewImages" />
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- /.form-horizontal -->
            </div>
            <!-- /.panel-body -->
        </div>
        <!-- /.panel -->

        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">Account Order</h3>
            </div>
            <div class="panel-body">
                <div class="accounts">
                    <div>Drag and drop accounts below to set your prefered account sorting.</div>
                    <asp:Label ID="lblAccounts" CssClass="lblAccounts" Style="visibility: hidden" runat="server"></asp:Label>
                    <asp:Label ID="lblAccountsNames" CssClass="lblAccountsNames" Style="visibility: hidden" runat="server"></asp:Label>
                    <ul id="listAccountSortable" class="listAccountSortable">
                    </ul>
                    <div class="accounts-result">
                        <asp:HiddenField runat="server" ID="hidAccountsResult" Value="bbbb" />
                    </div>
                </div>
            </div>
            <!-- /.panel-body -->
        </div>
        <!-- /.panel -->

        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">My Email Notifications</h3>
            </div>
            <div class="panel-body">
                <div class="checkbox">
                    <label>
                        <input type="checkbox" runat="server" id="chkCreateReserv" />
                        When I create my reservations
                    </label>
                </div>
                <div class="checkbox">
                    <label>
                        <input type="checkbox" runat="server" id="chkModifyReserv" />
                        When I modify my reservations
                    </label>
                </div>
                <div class="checkbox">
                    <label>
                        <input type="checkbox" runat="server" id="chkDeleteReserv" />
                        When I delete my reservations
                    </label>
                </div>
                <div class="checkbox">
                    <label>
                        <input type="checkbox" runat="server" id="chkInviteReserv" />
                        When I'm invited or uninvited to a reservation
                    </label>
                </div>

                <hr />

                <h6>Check those resources for which you want to be notified when a time becomes available.</h6>

                <asp:DataGrid runat="server" ID="dgResources" DataKeyField="ResourceID" AutoGenerateColumns="false" CellPadding="3" BorderColor="#4682B4" CssClass="Table" Width="60%">
                    <AlternatingItemStyle BackColor="AliceBlue" />
                    <HeaderStyle Font-Bold="true" HorizontalAlign="Center" ForeColor="White" BackColor="#336699" />
                    <Columns>
                        <asp:BoundColumn DataField="ResourceClientID" Visible="false"></asp:BoundColumn>
                        <asp:BoundColumn DataField="BuildingName" HeaderText="Building"></asp:BoundColumn>
                        <asp:BoundColumn DataField="LabName" HeaderText="Laboratory"></asp:BoundColumn>
                        <asp:BoundColumn DataField="ProcessTechName" HeaderText="Process Technology"></asp:BoundColumn>
                        <asp:BoundColumn DataField="ResourceName" HeaderText="Resource"></asp:BoundColumn>
                        <asp:TemplateColumn HeaderText="Email">
                            <ItemStyle HorizontalAlign="Center" Width="150" />
                            <ItemTemplate>
                                <asp:HiddenField runat="server" ID="hidCurrentValue" />
                                <asp:DropDownList runat="server" ID="ddlNotify" Width="100%">
                                    <asp:ListItem Selected="true" Value="0">Never Notify</asp:ListItem>
                                    <asp:ListItem Value="1">Always Notify</asp:ListItem>
                                    <asp:ListItem Value="2">Notify on Opening</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
                <asp:PlaceHolder runat="server" ID="phResourcesNoData" Visible="false">
                    <div class="text-muted">-- You are authorized on any tools --</div>
                </asp:PlaceHolder>

                <hr />

                <h6>Check those resources for which you want to be notified when a practice reservation is made (for tool engineeris only)</h6>

                <asp:DataGrid runat="server" ID="dgResourcePractice" DataKeyNames="ResourceID" AutoGenerateColumns="false" CellPadding="3" BorderColor="#4682B4" CssClass="Table" Width="60%">
                    <AlternatingItemStyle BackColor="AliceBlue" />
                    <HeaderStyle Font-Bold="true" HorizontalAlign="Center" ForeColor="White" BackColor="#336699" />
                    <Columns>
                        <asp:BoundColumn DataField="ResourceClientID" Visible="false"></asp:BoundColumn>
                        <asp:BoundColumn DataField="BuildingName" HeaderText="Building"></asp:BoundColumn>
                        <asp:BoundColumn DataField="LabName" HeaderText="Laboratory"></asp:BoundColumn>
                        <asp:BoundColumn DataField="ProcessTechName" HeaderText="Process Technology"></asp:BoundColumn>
                        <asp:BoundColumn DataField="ResourceName" HeaderText="Resource"></asp:BoundColumn>
                        <asp:TemplateColumn HeaderText="Email">
                            <ItemStyle HorizontalAlign="Center" Width="150" />
                            <ItemTemplate>
                                <asp:HiddenField runat="server" ID="hidResourceClientID" />
                                <asp:HiddenField runat="server" ID="hidCurrentValue" />
                                <asp:DropDownList runat="server" ID="ddlNotify" Width="100%">
                                    <asp:ListItem Selected="true" Value="0">Never Notify</asp:ListItem>
                                    <asp:ListItem Value="1">Always Notify</asp:ListItem>
                                </asp:DropDownList>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
                <asp:PlaceHolder runat="server" ID="phResourcePracticeNoData" Visible="false">
                    <div class="text-muted">-- You are not tool engineer of any tools --</div>
                </asp:PlaceHolder>
            </div>
            <!-- /.panel-body -->
        </div>
        <!-- /.panel -->

        <asp:Button runat="server" ID="btnSubmit" Text="Change My Preferences" CssClass="lnf btn btn-default pref-submit" />
        <button type="reset" class="lnf btn btn-default">Reset</button>
    </div>
    <!-- /.preferences -->
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="//ssel-apps.eecs.umich.edu/static/lib/jquery-ui/jquery-ui.min.js"></script>
</asp:Content>
