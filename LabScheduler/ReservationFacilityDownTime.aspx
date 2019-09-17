<%@ Page Title="Facility Down Time" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ReservationFacilityDownTime.aspx.vb" Inherits="LabScheduler.Pages.ReservationFacilityDownTime" %>

<%@ Register TagName="CalendarPopup" TagPrefix="uc" Src="~/UserControls/CalendarPopup.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        #tabs1 {
            width: unset;
        }

        .fdt-reservations th,
        .fdt-reservations td {
            padding: 4px;
        }

        .fdt-reservations th {
            background-color: #eaeaea;
        }

        .time-select {
            margin-right: 5px;
        }

        .update-modify-button {
            margin-right: 5px;
        }

        .alert {
            margin-top: 10px;
            margin-bottom: 10px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5 style="margin-bottom: 20px;">Facility Down Time Reservations</h5>

    <div class="fdt" style="display: none;">
        <input type="hidden" runat="server" id="hidSelectedTabIndex" value="0" class="selected-tab-index" />

        <asp:Literal runat="server" ID="litAlert1"></asp:Literal>

        <ul class="nav nav-tabs" role="tablist" id="tabs1">
            <li role="presentation" class="active"><a href="#new-reservation" aria-controls="home" role="tab" data-toggle="tab">Make New Reservation</a></li>
            <li role="presentation"><a href="#manage-reservations" aria-controls="profile" role="tab" data-toggle="tab">Manage Reservations</a></li>
        </ul>

        <!-- Tab panes -->
        <div class="tab-content">
            <div role="tabpanel" class="tab-pane active" id="new-reservation">
                <div class="row">
                    <div class="col-sm-4">
                        <div class="form-group" style="margin-top: 20px;">
                            <label>Filter by Lab:</label>
                            <asp:DropDownList ID="ddlLabs" AppendDataBoundItems="true" runat="server" DataSourceID="odsLabs" DataValueField="LabID" DataTextField="LabDisplayName" AutoPostBack="true" CssClass="form-control">
                                <asp:ListItem Text="All" Value="0">All</asp:ListItem>
                                <asp:ListItem Text="All" Value="-1">Clean Room & ROBIN</asp:ListItem>
                            </asp:DropDownList>
                            <asp:ObjectDataSource ID="odsLabs" runat="server" TypeName="LabScheduler.AppCode.DBAccess.LabDB" SelectMethod="SelectAll"></asp:ObjectDataSource>
                        </div>
                        <div class="form-group" style="margin-top: 0;">
                            <input type="text" class="form-control search" placeholder="Search" />
                        </div>
                        <div class="form-group">
                            <label style="color: #ff0000;">Hold the "Ctrl" key to select multiple tools</label>
                            <asp:ListBox runat="server" ID="lboxTools" SelectionMode="Multiple" DataSourceID="odsTool" DataValueField="ResourceID" DataTextField="ResourceName" CssClass="form-control tools" Height="300"></asp:ListBox>
                            <asp:ObjectDataSource ID="odsTool" runat="server" TypeName="LabScheduler.AppCode.DBAccess.ResourceDB" SelectMethod="SelectResourceListItemsByLab">
                                <SelectParameters>
                                    <asp:ControlParameter Type="Int32" ControlID="ddlLabs" DefaultValue="0" PropertyName="SelectedValue" Name="labId" />
                                </SelectParameters>
                            </asp:ObjectDataSource>
                        </div>
                        <div class="checkbox">
                            <label>
                                <input type="checkbox" class="check-all" onclick="toggleCheckAll();">
                                Select all tools
                            </label>
                        </div>
                    </div>
                </div>
                <div class="form-horizontal" style="margin-top: 20px;">
                    <div class="form-group">
                        <label class="col-sm-1 control-label">Start Date</label>
                        <div class="col-sm-2">
                            <div class="input-group">
                                <div class="input-group-addon">
                                    <asp:Image runat="server" ImageUrl="~/images/calendar.gif" />
                                </div>
                                <asp:TextBox runat="server" ID="txtStartDate" CssClass="bs-datepicker form-control"></asp:TextBox>
                            </div>
                        </div>
                        <label class="col-sm-1 control-label">Time</label>
                        <div class="col-sm-8">
                            <div class="form-inline">
                                <asp:DropDownList ID="ddlHour" runat="server" DataTextField="HourText" DataValueField="HourValue" CssClass="form-control time-select" Width="60" OnDataBound="DdlHour_DataBound">
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlMin" runat="server" DataTextField="MinText" DataValueField="MinValue" CssClass="form-control time-select" Width="60" OnDataBound="DdlMin_DataBound">
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlAMPM" runat="server" DataTextField="Name" DataValueField="Name" CssClass="form-control time-select" Width="60" OnDataBound="DdlAMPM_DataBound">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-1 control-label">End Date</label>
                        <div class="col-sm-2">
                            <div class="input-group">
                                <div class="input-group-addon">
                                    <asp:Image runat="server" ImageUrl="~/images/calendar.gif" />
                                </div>
                                <asp:TextBox runat="server" ID="txtEndDate" CssClass="bs-datepicker form-control"></asp:TextBox>
                            </div>
                        </div>
                        <label class="col-sm-1 control-label">Time</label>
                        <div class="col-sm-8">
                            <div class="form-inline">
                                <asp:DropDownList ID="ddlHourEnd" runat="server" DataTextField="HourText" DataValueField="HourValue" CssClass="form-control time-select" Width="60" OnDataBound="ddlHour_DataBound">
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlMinEnd" runat="server" DataTextField="MinText" DataValueField="MinValue" CssClass="form-control time-select" Width="60" OnDataBound="ddlMin_DataBound">
                                </asp:DropDownList>
                                <asp:DropDownList ID="ddlAMPMEnd" runat="server" DataTextField="Name" DataValueField="Name" CssClass="form-control time-select" Width="60" OnDataBound="ddlAMPM_DataBound">
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-1 control-label">Notes</label>
                        <div class="col-sm-4">
                            <asp:TextBox ID="txtNotes" runat="server" TextMode="MultiLine" CssClass="form-control" Height="100"></asp:TextBox>
                        </div>
                    </div>
                </div>
                <asp:Button runat="server" ID="btnReserve" CssClass="lnf btn btn-default" Text="Make Reservation" OnClick="BtnReserve_Click" />
                <asp:Button runat="server" ID="btnBack" CssClass="lnf btn btn-default" Text="Back" OnClick="BtnBack_Click" />
            </div>
            <div role="tabpanel" class="tab-pane" id="manage-reservations">
                <asp:PlaceHolder runat="server" ID="phGrid">
                    <em class="text-muted" style="margin-top: 10px; margin-bottom: 10px; display: block;">Sorted in reverse chronological order.</em>
                    <asp:Repeater runat="server" ID="rptFDT" OnItemDataBound="RptFDT_ItemDataBound">
                        <HeaderTemplate>
                            <table class="table table-hover">
                                <thead>
                                    <tr>
                                        <th>GroupID</th>
                                        <th>Created By</th>
                                        <th>Begin Date</th>
                                        <th>End Date</th>
                                        <th style="text-align: center;">Delete</th>
                                        <th style="text-align: center;">Edit</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%#Eval("GroupID")%></td>
                                <td><%#Eval("DisplayName")%></td>
                                <td><%#Eval("BeginDateTime", "{0:MM/dd/yyyy HH:mm:ss}")%></td>
                                <td><%#Eval("EndDateTime", "{0:MM/dd/yyyy HH:mm:ss}")%></td>
                                <td style="text-align: center;">
                                    <asp:PlaceHolder runat="server" ID="phDeleteGroup">
                                        <a href="#" data-toggle="modal" data-target=".delete-confirmation-dialog" data-group-id="<%#Eval("GroupID")%>">
                                            <img src="images/im_delete.gif" />
                                        </a>
                                    </asp:PlaceHolder>
                                </td>
                                <td style="text-align: center;">
                                    <asp:ImageButton runat="server" ID="imgbtnEdit" AlternateText="Edit" ImageUrl="~/images/im_edit.gif" CommandName="Edit" CommandArgument='<%#Bind("GroupID")%>' OnCommand="Row_Command" />
                                </td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                    </table>
                        </FooterTemplate>
                    </asp:Repeater>

                    <div class="modal fade delete-confirmation-dialog" tabindex="-1" role="dialog">
                        <div class="modal-dialog" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                                    <h4 class="modal-title">Confirm Delete</h4>
                                </div>
                                <div class="modal-body">
                                    <p>Are you sure you want to delete this record?</p>
                                    <div class="text-muted">[GroupID: <span class="group-id"></span>]</div>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                                    <input type="hidden" runat="server" id="hidDeleteGroupID" class="delete-group-id" />
                                    <asp:Button runat="server" ID="btnConfirmDelete" Text="Delete" CssClass="btn btn-primary" OnCommand="Row_Command" CommandName="Delete" />
                                </div>
                            </div>
                            <!-- /.modal-content -->
                        </div>
                        <!-- /.modal-dialog -->
                    </div>
                    <!-- /.modal -->

                    <asp:PlaceHolder runat="server" ID="phNoData" Visible="false">
                        <em class="text-muted" style="display: block;">-- There are no Facility Down Time reservations --</em>
                    </asp:PlaceHolder>
                </asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="phModify" Visible="false">
                    <asp:HiddenField ID="hidGroupID" runat="server" />
                    <h4 style="margin-top: 15px;">
                        <span>Reservation Created By:</span>
                        <asp:Literal ID="litName" runat="server"></asp:Literal>
                    </h4>
                    <div class="row">
                        <div class="col-sm-4">
                            <div class="form-group" style="margin-top: 5px;">
                                <asp:ListBox runat="server" ID="lboxModify" SelectionMode="Multiple" DataValueField="ResourceID" DataTextField="ResourceName" CssClass="form-control" Height="300"></asp:ListBox>
                            </div>
                        </div>
                    </div>

                    <div class="form-horizontal" style="margin-top: 20px;">
                        <div class="form-group">
                            <label class="col-sm-1 control-label">Start Date</label>
                            <div class="col-sm-2">
                                <div class="input-group">
                                    <div class="input-group-addon">
                                        <asp:Image runat="server" ImageUrl="~/images/calendar.gif" />
                                    </div>
                                    <asp:TextBox runat="server" ID="txtStartDateModify" CssClass="bs-datepicker form-control"></asp:TextBox>
                                </div>
                            </div>
                            <label class="col-sm-1 control-label">Time</label>
                            <div class="col-sm-8">
                                <div class="form-inline">
                                    <asp:DropDownList ID="ddlHourModify" runat="server" DataTextField="HourText" DataValueField="HourValue" CssClass="form-control time-select" Width="60" OnDataBound="ddlHour_DataBound">
                                    </asp:DropDownList>
                                    <asp:DropDownList ID="ddlMinModify" runat="server" DataTextField="MinText" DataValueField="MinValue" CssClass="form-control time-select" Width="60" OnDataBound="ddlMin_DataBound">
                                    </asp:DropDownList>
                                    <asp:DropDownList ID="ddlAMPMModify" runat="server" DataTextField="Name" DataValueField="Name" CssClass="form-control time-select" Width="60" OnDataBound="ddlAMPM_DataBound">
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-sm-1 control-label">End Date</label>
                            <div class="col-sm-2">
                                <div class="input-group">
                                    <div class="input-group-addon">
                                        <asp:Image runat="server" ImageUrl="~/images/calendar.gif" />
                                    </div>
                                    <asp:TextBox runat="server" ID="txtEndDateModify" CssClass="bs-datepicker form-control"></asp:TextBox>
                                </div>
                            </div>
                            <label class="col-sm-1 control-label">Time</label>
                            <div class="col-sm-8">
                                <div class="form-inline">
                                    <asp:DropDownList ID="ddlHourEndModify" runat="server" DataTextField="HourText" DataValueField="HourValue" CssClass="form-control time-select" Width="60" OnDataBound="ddlHour_DataBound">
                                    </asp:DropDownList>
                                    <asp:DropDownList ID="ddlMinEndModify" runat="server" DataTextField="MinText" DataValueField="MinValue" CssClass="form-control time-select" Width="60" OnDataBound="ddlMin_DataBound">
                                    </asp:DropDownList>
                                    <asp:DropDownList ID="ddlAMPMEndModify" runat="server" DataTextField="Name" DataValueField="Name" CssClass="form-control time-select" Width="60" OnDataBound="ddlAMPM_DataBound">
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-sm-1 control-label">Notes</label>
                            <div class="col-sm-4">
                                <asp:TextBox ID="txtNotesModify" runat="server" TextMode="MultiLine" CssClass="form-control" Height="100"></asp:TextBox>
                            </div>
                        </div>
                    </div>
                    <asp:Button runat="server" ID="btnUpdateModify" CssClass="lnf btn btn-default update-modify-button" Text="Update" OnClick="BtnUpdateModify_Click" />
                    <asp:Button runat="server" ID="btnBackModify" CssClass="lnf btn btn-default" Text="Back" OnClick="BtnBackModify_Click" />
                </asp:PlaceHolder>
            </div>
        </div>
        <asp:Literal runat="server" ID="litAlert2"></asp:Literal>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        //var selectedTabIndex;

        //function BeginRequestHandler(sender, args) {
        //    var tabs = $("#tabs").tabs();
        //    selectedTabIndex = tabs.tabs('option', 'active');
        //    //$(".calendar-popup").datepicker();
        //}

        //function EndRequestHandler(sender, args) {
        //    var tabs = $("#tabs").tabs();
        //    $("#tabs").tabs("option", "active", selectedTabIndex);
        //    $(".calendar-popup").datepicker();
        //}

        //Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);
        //Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(BeginRequestHandler);
        //$("#tabs").tabs();
        //$(".calendar-popup").datepicker();

        $(".bs-datepicker").datepicker();

        function toggleCheckAll() {
            var checked = $(".check-all").prop("checked") === true;
            $('.tools option:visible').prop("selected", checked);
        }

        $(".delete-confirmation-dialog").on("show.bs.modal", function (e) {
            var button = $(e.relatedTarget);
            var groupId = button.data("group-id");
            var modal = $(this);
            $(".delete-group-id", modal).val(groupId);
            modal.find('.group-id').text(groupId);
        });

        var selectedTabIndex = parseInt($(".selected-tab-index").val());
        $("#tabs1 li").eq(selectedTabIndex).find("a").tab("show");
        $(".fdt").show();

        $(".search").on("keyup", function (e) {
            var $this = $(this);
            var current = $this.val().toLowerCase();
            if (current) {
                $(".tools").find("option").each(function () {
                    var $opt = $(this);
                    if ($opt.text().toLowerCase().search(current) >= 0)
                        $opt.show();
                    else
                        $opt.hide();
                })
            } else {
                $(".tools").find("option").show();
            }
        });
    </script>
</asp:Content>
