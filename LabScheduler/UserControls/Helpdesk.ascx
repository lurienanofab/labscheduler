<%--
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
--%>

<%@ Control Language="C#" Inherits="LNF.Web.Scheduler.Controls.Helpdesk" %>

<%@ Register TagPrefix="lnf" Namespace="LNF.Web.Controls" Assembly="LNF.Web" %>

<asp:PlaceHolder runat="server" ID="phNoResource" Visible="false">
    <div class="alert alert-danger" role="alert">
        No resource specified.
    </div>
</asp:PlaceHolder>
<asp:PlaceHolder runat="server" ID="phHelpdesk" Visible="true">
    <div class="helpdesk">
        <lnf:BootstrapModal runat="server" ID="BootstrapModal1" CssClass="ticket-dialog" Title="Ticket Details" Size="Large">
            <BodyTemplate>
                <iframe class="helpdesk-frame" src="about:blank" seamless="seamless" style="width: 100%; height: 600px;"></iframe>
            </BodyTemplate>
            <FooterTemplate>
                <button type="button" class="lnf btn btn-default" data-dismiss="modal">Close</button>
            </FooterTemplate>
        </lnf:BootstrapModal>

        <input type="hidden" runat="server" id="hidAjaxUrl" class="ajax-url" />
        <input type="hidden" runat="server" id="hidHelpdeskQueue" class="helpdesk-queue" />
        <input type="hidden" runat="server" id="hidHelpdeskResource" class="helpdesk-resource" />
        <input type="hidden" runat="server" id="hidHelpdeskFromEmail" class="helpdesk-from-email" />
        <input type="hidden" runat="server" id="hidHelpdeskFromName" class="helpdesk-from-name" />

        <div class="lnf panel panel-default repair">
            <div class="panel-heading">
                <h3 class="panel-title">Open Tickets</h3>
            </div>
            <div class="panel-body">
                <div class="tickets" style="min-height: 24px;">
                    <img src="<%=Page.GetStaticUrl("images/ajax-loader-4.gif")%>" border="0" alt="Loading..." />
                </div>
            </div>
        </div>

        <div class="lnf panel panel-default repair">
            <div class="panel-heading">
                <h3 class="panel-title">Create Ticket</h3>
            </div>
            <div class="panel-body">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Reservation</label>
                        <div class="col-sm-5">
                            <asp:DropDownList ID="ddlReservations" runat="server" TabIndex="4" CssClass="form-control reservations">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Type</label>
                        <div class="col-sm-2">
                            <asp:DropDownList runat="server" ID="ddlTicketType" CssClass="form-control type">
                                <asp:ListItem Text="General Question" Value="0"></asp:ListItem>
                                <asp:ListItem Text="Process Issue" Value="1"></asp:ListItem>
                                <asp:ListItem Text="Hardware Issue" Value="2"></asp:ListItem>
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Subject</label>
                        <div class="col-sm-4">
                            <asp:TextBox runat="server" ID="txtSubject" CssClass="form-control subject"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="col-sm-2 control-label">Message</label>
                        <div class="col-sm-4">
                            <asp:TextBox ID="txtMessage" TextMode="MultiLine" Columns="10" Rows="10" runat="server" CssClass="form-control message"></asp:TextBox>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <a href="#" class="lnf btn btn-default create-ticket">Create Ticket</a>
                            <div class="create-ticket-message"></div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div style="margin-top: 20px;">
            <asp:Literal runat="server" ID="litErrMsg"></asp:Literal>
        </div>

        <div class="helpdesk-dialog" style="display: none; padding: 0; overflow: hidden;">

            <div style="border-top: solid 1px #AAAAAA; height: 15px;"></div>
        </div>
    </div>
</asp:PlaceHolder>
