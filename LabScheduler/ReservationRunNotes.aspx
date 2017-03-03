<%@ Page Title="Run Notes" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ReservationRunNotes.aspx.vb" Inherits="LabScheduler.Pages.ReservationRunNotes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Edit Reservation Run Notes</h3>
        </div>
        <div class="panel-body">
            <div class="form-horizontal">
                <div class="form-group">
                    <label class="col-sm-1 control-label">Resource</label>
                    <div class="col-sm-11">
                        <p class="form-control-static">
                            <asp:Literal runat="server" ID="litResourceName"></asp:Literal>
                        </p>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-1 control-label">Begin Time</label>
                    <div class="col-sm-11">
                        <p class="form-control-static">
                            <asp:Literal runat="server" ID="litBeginTime"></asp:Literal>
                        </p>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-1 control-label">End Time</label>
                    <div class="col-sm-11">
                        <p class="form-control-static">
                            <asp:Literal runat="server" ID="litEndTime"></asp:Literal>
                        </p>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-1 control-label">Notes</label>
                    <div class="col-sm-4">
                        <p class="form-control-static">
                            <asp:TextBox runat="server" ID="txtNotes" TextMode="MultiLine" Rows="15" Columns="10" CssClass="form-control"></asp:TextBox>
                        </p>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-sm-offset-1 col-sm-11">
                        <asp:Button ID="btnSubmit" runat="server" CssClass="lnf btn btn-default" Text="Update Notes"></asp:Button>
                        <asp:HyperLink runat="server" ID="hypCancel" CssClass="lnf btn btn-default">Cancel</asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
