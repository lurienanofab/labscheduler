<%@ Page Title="Facility Down Time" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ReservationFacilityDownTimeNew.aspx.vb" Inherits="LabScheduler.Pages.ReservationFacilityDownTimeNew" %>

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

        .new-reservation-tabpanel .edit-show,
        .new-reservation-tabpanel.editing .edit-hide {
            display: none;
        }

        .new-reservation-tabpanel.editing .edit-show {
            display: block;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5 style="margin-bottom: 20px;">Facility Down Time Reservations</h5>

    <div runat="server" id="divFDT" class="fdt" data-ajax-url="ajax/facility-downtime.ashx">
        <div class="alert alert-dismissible alert-danger" role="alert" style="display: none;">
            <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
            <div class="alert-text"></div>
        </div>

        <div class="tabs" style="display: none;">
            <ul class="nav nav-tabs" role="tablist" id="tabs1">
                <li role="presentation" class="active"><a href="#new-reservation" class="new-reservation-tab" aria-controls="new-reservation" role="tab" data-toggle="tab">Make New Reservation</a></li>
                <li role="presentation"><a href="#manage-reservations" class="manage-reservations-tab" aria-controls="manage-reservations" role="tab" data-toggle="tab">Manage Reservations</a></li>
            </ul>

            <!-- Tab panes -->
            <div class="tab-content">
                <div role="tabpanel" class="tab-pane active new-reservation-tabpanel" id="new-reservation">
                    <div class="row">
                        <div class="col-sm-4">
                            <div style="margin-top: 20px;">
                                <h4 class="created-by edit-show">Reservation Created By: <span class="display-name"></span>
                                </h4>
                                <div class="form-group edit-hide">
                                    <label>Filter by Lab:</label>
                                    <select class="form-control labs"></select>
                                </div>
                                <div class="form-group edit-hide">
                                    <input type="text" class="form-control search" placeholder="Search" />
                                </div>
                                <div class="form-group">
                                    <label class="edit-hide" style="color: #ff0000;">Hold the "Ctrl" key to select multiple tools</label>
                                    <select multiple class="form-control tools" style="height: 300px;"></select>
                                </div>
                                <div class="checkbox edit-hide">
                                    <label>
                                        <input type="checkbox" class="check-all">
                                        Select all tools
                                    </label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="form-horizontal" style="margin-top: 20px;">
                        <div class="form-group start-date">
                            <label class="col-sm-1 control-label">Start Date</label>
                            <div class="col-sm-2">
                                <div class="input-group">
                                    <div class="input-group-addon">
                                        <asp:Image runat="server" ImageUrl="~/images/calendar.gif" />
                                    </div>
                                    <input type="text" class="bs-datepicker form-control date-text" />
                                </div>
                            </div>
                            <label class="col-sm-1 control-label">Time</label>
                            <div class="col-sm-8">
                                <div class="form-inline">
                                    <select class="form-control time-select hour-select" style="width: 60px;">
                                    </select>
                                    <select class="form-control time-select min-select" style="width: 60px;">
                                    </select>
                                    <select class="form-control time-select ampm-select" style="width: 60px;">
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="form-group end-date">
                            <label class="col-sm-1 control-label">End Date</label>
                            <div class="col-sm-2">
                                <div class="input-group">
                                    <div class="input-group-addon">
                                        <asp:Image runat="server" ImageUrl="~/images/calendar.gif" />
                                    </div>
                                    <input type="text" class="bs-datepicker form-control date-text" />
                                </div>
                            </div>
                            <label class="col-sm-1 control-label">Time</label>
                            <div class="col-sm-8">
                                <div class="form-inline">
                                    <select class="form-control time-select hour-select" style="width: 60px;">
                                    </select>
                                    <select class="form-control time-select min-select" style="width: 60px;">
                                    </select>
                                    <select class="form-control time-select ampm-select" style="width: 60px;">
                                    </select>
                                </div>
                            </div>
                        </div>
                        <div class="form-group">
                            <label class="col-sm-1 control-label">Notes</label>
                            <div class="col-sm-5">
                                <textarea class="form-control notes" style="height: 100px;"></textarea>
                                <div class="alert alert-warning edit-show" role="alert">
                                    <strong>Warning:</strong> Modifying notes on this page will overwrite all existing reservation notes for reservations related to this facility down time instance. No changes to notes will be made if left blank.
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="edit-hide">
                        <button type="button" class="lnf btn btn-default make-reservation">Make Reservation</button>
                        <asp:Button runat="server" ID="btnBack" CssClass="lnf btn btn-default" Text="Back" OnClick="BtnBack_Click" />
                    </div>
                    <div class="edit-show">
                        <button type="button" class="lnf btn btn-default modify-reservation">Update Reservation</button>
                        <button type="button" class="lnf btn btn-default cancel-modify-reservation">Cancel</button>
                    </div>
                </div>
                <div role="tabpanel" class="tab-pane" id="manage-reservations">

                    <em class="text-muted" style="margin-top: 10px; margin-bottom: 10px; display: block;">Sorted in reverse chronological order.</em>

                    <table class="table table-hover groups-table">
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
                        </tbody>
                    </table>

                    <div class="modal fade delete-confirmation-dialog" tabindex="-1" role="dialog">
                        <div class="modal-dialog" role="document">
                            <div class="modal-content">
                                <div class="modal-header">
                                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                                    <h4 class="modal-title">Confirm Delete</h4>
                                </div>
                                <div class="modal-body">
                                    <p style="font-size: larger;"><strong>Are you sure you want to delete this record?</strong></p>
                                    <div class="text-muted">[GroupID: <span class="group-id"></span>]</div>
                                </div>
                                <div class="modal-footer">
                                    <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
                                    <button type="button" class="btn btn-primary delete-group-confirm-button">Delete</button>
                                </div>
                            </div>
                            <!-- /.modal-content -->
                        </div>
                        <!-- /.modal-dialog -->
                    </div>
                    <!-- /.modal -->

                    <em class="text-muted no-reservations" style="display: none;">-- There are no Facility Down Time reservations --</em>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="scripts/facility-downtime/facility-downtime.js"></script>
    <script>
        $(".fdt").facilityDowntime();
    </script>
</asp:Content>
