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

<%@ Page Title="Resource Clients" Language="C#" AutoEventWireup="true" MasterPageFile="~/MasterPageScheduler.Master" Inherits="LNF.Web.Scheduler.Pages.ResourceClients" %>

<%@ Register TagPrefix="lnf" Namespace="LNF.Web.Controls" Assembly="LNF.Web" %>
<%@ Register TagPrefix="lnf" Namespace="LNF.Web.Scheduler.Controls" Assembly="LNF.Web.Scheduler" %>

<script runat="server">
    void Page_PreInit(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            Session.Remove("dtClients");
            Session.Remove("dtAvailClients");
        }
    }
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .email-table {
            border-collapse: collapse;
        }

            .email-table td {
                padding: 3px;
                border: solid 1px #C4C4C4;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="modal fade" id="client-list-modal" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">
                        <asp:Literal runat="server" ID="litClientListTitle">Client List</asp:Literal>
                    </h4>
                </div>
                <div class="modal-body">
                    <asp:Repeater runat="server" ID="rptClientList">
                        <HeaderTemplate>
                            <table class="table table-striped" style="width: 100%;">
                                <thead>
                                    <tr>
                                        <th>Auth</th>
                                        <th>Name</th>
                                        <th>Email</th>
                                    </tr>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td><%#Eval("AuthLevelText")%></td>
                                <td><%#Eval("DisplayName")%></td>
                                <td><%#Eval("Email")%></td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>
            </div>
            <!-- /.modal-content -->
        </div>
        <!-- /.modal-dialog -->
    </div>
    <!-- /.modal -->

    <lnf:BootstrapModal runat="server" ID="bsmClientListModal" Title="Client List">
        <BodyTemplate>
        </BodyTemplate>
        <FooterTemplate>
            <button type="button" class="lnf btn btn-default" data-dismiss="modal">Close</button>
        </FooterTemplate>
    </lnf:BootstrapModal>

    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu1" SelectedIndex="2" />

    <asp:PlaceHolder runat="server" ID="phAddUser">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <asp:Literal runat="server" ID="litCreateModifyResourceClient"></asp:Literal>
                </h3>
            </div>
            <div class="panel-body">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="control-label col-sm-2">Authorization *</label>
                        <div class="col-sm-2">
                            <asp:DropDownList runat="server" ID="ddlAuthLevel" DataTextField="AuthLevelName" DataValueField="AuthLevelID" AutoPostBack="True" CssClass="form-control">
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="control-label col-sm-2">User Name *</label>
                        <div class="col-sm-4">
                            <asp:DropDownList runat="server" ID="ddlClients" DataTextField="DisplayName" DataValueField="ClientID" CssClass="form-control">
                            </asp:DropDownList>
                            <asp:PlaceHolder runat="server" ID="phClientName">
                                <p class="form-control-static">
                                    <asp:Literal runat="server" ID="litClientName"></asp:Literal>
                                    <asp:HiddenField runat="server" ID="hidClientID"></asp:HiddenField>
                                </p>
                            </asp:PlaceHolder>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <asp:Button ID="btnSubmit" runat="server" CssClass="lnf btn btn-default" OnClick="btnSubmit_Click"></asp:Button>&nbsp;
                            <asp:Button ID="btnCancel" runat="server" CssClass="lnf btn btn-default" Text="Cancel" OnClick="btnCancel_Click"></asp:Button>
                        </div>
                    </div>
                </div>
                <asp:Literal runat="server" ID="litErrMsg"></asp:Literal>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phEmailList">
        <div style="padding-left: 15px; margin-bottom: 20px; font-size: 14px;">
            <asp:Image runat="server" ID="imgEmailAll" ImageUrl="~/images/email.gif"></asp:Image>&nbsp;
            <asp:HyperLink runat="server" ID="hypEmailAll" Font-Bold="true" ForeColor="Black" Font-Underline="true">Email All</asp:HyperLink>
            <span>|</span>
            <a href="#" data-toggle="modal" data-target="#client-list-modal" style="font-weight: bold; color: #000; text-decoration: underline;">Client List</a>
            <%--<asp:Literal runat="server" ID="litRCListSeparator">&nbsp;|&nbsp;</asp:Literal>--%>
            <%--<asp:LinkButton runat="server" ID="lbtnRCList" Font-Bold="true" ForeColor="Black" Font-Underline="true">Client List</asp:LinkButton>--%>
        </div>
    </asp:PlaceHolder>

    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">
                <asp:Image ID="imgEmailTEs" runat="server" ImageUrl="~/images/email.gif"></asp:Image>&nbsp;
                <asp:HyperLink runat="server" ID="hypEmailToolEngineers" Font-Bold="true" ForeColor="Black" Font-Underline="true">Tool Engineers</asp:HyperLink>
            </h3>
        </div>
        <div class="panel-body">
            <asp:DataGrid runat="server" ID="dgTEs" CellSpacing="0" AutoGenerateColumns="False" ShowHeader="False" ShowFooter="False" GridLines="None" CssClass="email-table" OnDataBinding="dg_DataBinding" OnItemDataBound="dg_ItemDataBound" OnItemCommand="dg_ItemCommand">
                <Columns>
                    <asp:BoundColumn DataField="ClientID" Visible="False"></asp:BoundColumn>
                    <asp:TemplateColumn>
                        <ItemStyle Width="300"></ItemStyle>
                        <ItemTemplate>
                            <asp:HyperLink runat="server" ID="hypToolEngineer"></asp:HyperLink>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="ibtnEditTE" CommandName="Edit" ImageUrl="~/images/edit.gif" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnDeleteTE" CommandName="Delete" ImageUrl="~/images/delete.gif" runat="server"></asp:ImageButton>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
            <asp:Literal runat="server" ID="litNoTE" Visible="false"><em class="text-muted">There are no tool engineers.</em></asp:Literal>
        </div>
    </div>

    <asp:PlaceHolder runat="server" ID="phTrainers">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <asp:Image ID="imgEmailTrainers" runat="server" ImageUrl="~/images/email.gif"></asp:Image>&nbsp;
                    <asp:HyperLink runat="server" ID="hypEmailTrainers" Font-Bold="true" ForeColor="Black" Font-Underline="true">Trainers</asp:HyperLink>
                </h3>
            </div>
            <div class="panel-body">
                <asp:DataGrid runat="server" ID="dgTrainers" CellSpacing="0" AutoGenerateColumns="False" ShowHeader="False" ShowFooter="False" GridLines="None" CssClass="email-table" OnDataBinding="dg_DataBinding" OnItemDataBound="dg_ItemDataBound" OnItemCommand="dg_ItemCommand">
                    <Columns>
                        <asp:BoundColumn DataField="ClientID" Visible="False"></asp:BoundColumn>
                        <asp:TemplateColumn>
                            <ItemStyle Width="300"></ItemStyle>
                            <ItemTemplate>
                                <asp:HyperLink runat="server" ID="hypTrainer"></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:ImageButton ID="ibtnEditTrainer" CommandName="Edit" ImageUrl="~/images/edit.gif" runat="server"></asp:ImageButton>
                                <asp:ImageButton ID="ibtnDeleteTrainer" CommandName="Delete" ImageUrl="~/images/delete.gif" runat="server"></asp:ImageButton>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
                <asp:Literal runat="server" ID="litNoTrainer" Visible="false"><em class="text-muted">There are no trainers.</em></asp:Literal>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phCheckouts">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <asp:Image ID="imgEmailCheckouts" runat="server" ImageUrl="~/images/email.gif"></asp:Image>&nbsp;
                    <asp:HyperLink runat="server" ID="hypEmailCheckouts" Font-Bold="true" ForeColor="Black" Font-Underline="true">Super Users</asp:HyperLink>
                </h3>
            </div>
            <div class="panel-body">
                <asp:DataGrid runat="server" ID="dgCheckouts" CellSpacing="0" AutoGenerateColumns="False" ShowHeader="False" ShowFooter="False" GridLines="None" CssClass="email-table" OnDataBinding="dg_DataBinding" OnItemDataBound="dg_ItemDataBound" OnItemCommand="dg_ItemCommand">
                    <Columns>
                        <asp:BoundColumn DataField="ClientID" Visible="False"></asp:BoundColumn>
                        <asp:TemplateColumn>
                            <ItemStyle Width="300"></ItemStyle>
                            <ItemTemplate>
                                <asp:HyperLink runat="server" ID="hypCheckout"></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:ImageButton ID="ibtnEditCheckout" CommandName="Edit" ImageUrl="~/images/edit.gif" runat="server"></asp:ImageButton>
                                <asp:ImageButton ID="ibtnDeleteCheckout" CommandName="Delete" ImageUrl="~/images/delete.gif" runat="server"></asp:ImageButton>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
                <asp:Literal runat="server" ID="litNoCheckout" Visible="false"><em class="text-muted">There are no super users.</em></asp:Literal>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="phUsers">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <asp:Image ID="imgEmailUsers" runat="server" ImageUrl="~/images/email.gif"></asp:Image>&nbsp;
                    <asp:HyperLink runat="server" ID="hypEmailUsers" Font-Bold="true" ForeColor="Black" Font-Underline="true">Authorized Users</asp:HyperLink>
                </h3>
            </div>
            <div class="panel-body">
                <asp:DataGrid runat="server" ID="dgUsers" CellSpacing="0" AutoGenerateColumns="False" ShowHeader="False" ShowFooter="False" PageSize="20" AllowPaging="True" GridLines="None" CssClass="email-table" OnDataBinding="dg_DataBinding" OnItemDataBound="dg_ItemDataBound" OnItemCommand="dg_ItemCommand">
                    <Columns>
                        <asp:BoundColumn Visible="False" DataField="ClientID"></asp:BoundColumn>
                        <asp:TemplateColumn>
                            <ItemStyle Width="300"></ItemStyle>
                            <ItemTemplate>
                                <asp:HyperLink runat="server" ID="hypUser"></asp:HyperLink>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn ItemStyle-Width="50" ItemStyle-HorizontalAlign="Center">
                            <ItemTemplate>
                                <asp:ImageButton ID="ibtnEditUser" CommandName="Edit" ImageUrl="~/images/edit.gif" runat="server"></asp:ImageButton>
                                <asp:ImageButton ID="ibtnDeleteUser" CommandName="Delete" ImageUrl="~/images/delete.gif" runat="server"></asp:ImageButton>
                                <asp:ImageButton ID="ibtnExtend" CommandName="Extend" ImageUrl="~/images/extend.gif" runat="server"></asp:ImageButton>
                            </ItemTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                    <PagerStyle Visible="False"></PagerStyle>
                </asp:DataGrid>
                <div runat="server" id="divPager" class="form-inline" style="margin-top: 10px;">
                    <div class="form-group">
                        <label>Select page:</label>
                        <asp:DropDownList runat="server" ID="ddlPager" AutoPostBack="true" Width="200" OnSelectedIndexChanged="ddlPager_SelectedIndexChanged" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                </div>
                <asp:Literal runat="server" ID="litNoUser" Visible="false"><em class="text-muted">There are no authorized users.</em></asp:Literal>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
