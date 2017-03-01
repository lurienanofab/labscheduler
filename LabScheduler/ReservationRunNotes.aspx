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
