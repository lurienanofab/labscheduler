<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="ReservationView.ascx.vb" Inherits="LabScheduler.UserControls.ReservationView" EnableViewState="true" %>

<%@ Register TagPrefix="uc" TagName="HelpdeskInfo" Src="~/UserControls/HelpdeskInfo.ascx" %>

<uc:HelpdeskInfo runat="server" ID="HelpdeskInfo1" />

<div class="reservation-view">
    <asp:PlaceHolder runat="server" ID="phErrorMessage" Visible="false">
        <div class="alert alert-danger" role="alert">
            <button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">&times;</span></button>
            <asp:Literal runat="server" ID="litErrorMessage"></asp:Literal>
        </div>
    </asp:PlaceHolder>

    <asp:Table runat="server" ID="tblSchedule" CssClass="reservation-view-table" CellSpacing="0" Visible="true">
        <asp:TableRow ID="trHeader">
            <asp:TableCell HorizontalAlign="Center" ID="tdHourRange" BackColor="White" Wrap="False">
                <asp:HyperLink runat="server" ID="hypHourRange">Full<br />Day</asp:HyperLink>
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>

    <div class="modal fade error-dialog" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">An error has occurred</h4>
                </div>
                <div class="modal-body">
                    <p class="error-message"></p>
                </div>
                <div class="modal-footer">
                    <asp:Button runat="server" ID="Button1" Text="OK" CssClass="btn btn-primary" OnCommand="DialogButton_OnCommand" CommandName="ok" />
                </div>
            </div>
            <!-- /.modal-content -->
        </div>
        <!-- /.modal-dialog -->
    </div>
    <!-- /.modal -->

    <div runat="server" id="divStartConfirmationDialog" class="modal fade start-confirmation-dialog" tabindex="-1" role="dialog" visible="false">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">Start Confirmation</h4>
                </div>
                <div class="modal-body">
                    <p>Someone still has an active reservation for this resource. Click OK to start your reservation (this will end the current active reservation, so please make sure you check the tool/log), otherwise click Cancel.</p>
                    <asp:Literal runat="server" ID="litActiveReservationMessage"></asp:Literal>
                </div>
                <div class="modal-footer">
                    <asp:Button runat="server" ID="btnConfirmOK" Text="OK" CssClass="btn btn-primary" OnCommand="DialogButton_OnCommand" CommandName="ok" />
                    <asp:Button runat="server" ID="btnConfirmCancel" Text="Cancel" CssClass="btn btn-default" OnCommand="DialogButton_OnCommand" CommandName="cancel" />
                </div>
            </div>
            <!-- /.modal-content -->
        </div>
        <!-- /.modal-dialog -->
    </div>
    <!-- /.modal -->
</div>

<asp:PlaceHolder runat="server" ID="phNoData" Visible="false">
    <div style="padding: 20px 0 20px 0; font-weight: bold; color: #990000;">
        <asp:Literal runat="server" ID="litNoData"></asp:Literal>
    </div>
</asp:PlaceHolder>
