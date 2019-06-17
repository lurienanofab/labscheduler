<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="Contact.ascx.vb" Inherits="LabScheduler.UserControls.Contact" %>

<div class="lnf panel panel-default">
    <div class="panel-heading">
        <h3 class="panel-title">Contact</h3>
    </div>
    <div class="panel-body">
        <div class="alert alert-warning" role="alert"><strong>Note:</strong> LNF staff may be copied on your email.</div>
        <div class="form-horizontal">
            <div class="form-group">
                <label class="col-sm-2 control-label">Send To</label>
                <asp:PlaceHolder runat="server" ID="phSendTo">
                    <div class="col-sm-3">
                        <asp:DropDownList runat="server" ID="ddlSendTo" TabIndex="1" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                </asp:PlaceHolder>
                <asp:PlaceHolder runat="server" ID="phSendToText">
                    <div class="col-sm-10">
                        <p class="form-control-static">
                            <asp:Literal runat="server" ID="litSendTo"></asp:Literal>
                        </p>
                    </div>
                </asp:PlaceHolder>
            </div>
            <div class="form-group">
                <div class="col-sm-offset-2 col-sm-10">
                    <div class="checkbox">
                        <label>
                            <input type="checkbox" runat="server" id="chkCC" checked tabindex="3" />
                            CC Self
                        </label>
                    </div>
                </div>
            </div>
            <asp:PlaceHolder runat="server" ID="phCreateHelpdeskTicket" Visible="false">
                <div class="form-group">
                <div class="col-sm-offset-2 col-sm-10">
                    <div class="checkbox">
                        <label>
                            <input type="checkbox" runat="server" id="chkHelpdesk" checked tabindex="2" />
                            Create Helpdesk Ticket
                        </label>
                    </div>
                </div>
            </div>
            </asp:PlaceHolder>
            <asp:PlaceHolder runat="server" ID="phReservation" Visible="false">
                <div class="form-group">
                    <label class="col-sm-2 control-label">Reservation</label>
                    <div class="col-sm-5">
                        <asp:DropDownList runat="server" ID="ddlReservations" TabIndex="4" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                </div>
            </asp:PlaceHolder>
            <div class="form-group">
                <label class="col-sm-2 control-label">Subject *</label>
                <div class="col-sm-5">
                    <asp:TextBox runat="server" ID="txtSubject" Columns="60" TabIndex="5" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="form-group">
                <label class="col-sm-2 control-label">Body</label>
                <div class="col-sm-5">
                    <asp:TextBox runat="server" ID="txtBody" TextMode="MultiLine" Columns="50" Rows="10" TabIndex="6" CssClass="form-control"></asp:TextBox>
                </div>
            </div>
            <div class="form-group">
                <div class="col-sm-offset-2 col-sm-10">
                    <asp:Button runat="server" ID="btnSend" Text="Send Email Now!" CssClass="lnf btn btn-default" OnClick="BtnSend_Click"></asp:Button>
                    <asp:HyperLink runat="server" ID="hypCancel" CssClass="lnf btn btn-default">Cancel</asp:HyperLink>
                </div>
            </div>
        </div>
        <asp:PlaceHolder runat="server" ID="phErrorMessage" Visible="false">
            <div class="alert alert-danger" role="alert">
                <asp:Literal runat="server" ID="litErrorMessage"></asp:Literal>
            </div>
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phSuccessMessage" Visible="false">
            <div class="alert alert-success" role="alert">
                <asp:Literal runat="server" ID="litSuccessMessage"></asp:Literal>
            </div>
        </asp:PlaceHolder>
    </div>
</div>
