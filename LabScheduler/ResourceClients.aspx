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
        /*.email-table {
            border-collapse: collapse;
        }

            .email-table tr:hover {
                background-color: #f0ffff;
            }

            .email-table td {
                padding: 3px;
                border: solid 1px #C4C4C4;
            }

        .resource-client-datatable .dataTables_wrapper {
            padding: 0;
            background-color: unset;
            border: none;
            border-radius: 0;
        }

            .resource-client-datatable .dataTables_wrapper .dataTables_filter {
                float: unset;
            }

                .resource-client-datatable .dataTables_wrapper .dataTables_filter input {
                    font-weight: normal;
                    font-size: 10pt;
                    height: unset;
                    width: unset;
                    margin-left: 5px;
                    display: inline-block;
                }

            .resource-client-datatable .dataTables_wrapper .dataTables_info {
                display: block;
                width: unset;
            }

            .resource-client-datatable .dataTables_wrapper .dataTables_paginate {
                float: none;
                text-align: left;
                display: block;
                width: unset;
                margin: 15px 0 0 0;
            }

                .resource-client-datatable .dataTables_wrapper .dataTables_paginate .paginate_button {
                    padding: 5px 10px 5px 10px;
                    margin: 0;
                    border-radius: 0;
                    text-decoration: none;
                    border-top: solid 1px #ddd;
                    border-right: none;
                    border-bottom: solid 1px #ddd;
                    border-left: solid 1px #ddd;
                }

                    .resource-client-datatable .dataTables_wrapper .dataTables_paginate .paginate_button:hover {
                        background-color: #eee;
                    }

                    .resource-client-datatable .dataTables_wrapper .dataTables_paginate .paginate_button.current {
                        color: #fff;
                        background-color: #337ab7;
                        border-color: #337ab7;
                    }

                    .resource-client-datatable .dataTables_wrapper .dataTables_paginate .paginate_button.previous {
                        border-top-left-radius: 4px;
                        border-bottom-left-radius: 4px;
                    }

                    .resource-client-datatable .dataTables_wrapper .dataTables_paginate .paginate_button.next {
                        border-right: solid 1px #ddd;
                        border-top-right-radius: 4px;
                        border-bottom-right-radius: 4px;
                    }

                .resource-client-datatable .dataTables_wrapper .dataTables_paginate .ellipsis {
                    padding: 5px 10px 5px 10px;
                    border-top: solid 1px #ddd;
                    border-right: none;
                    border-bottom: solid 1px #ddd;
                    border-left: solid 1px #ddd;
                }

            .resource-client-datatable .dataTables_wrapper .dataTable {
                clear: both;
                border: none;
                border-radius: 0;
                border-collapse: collapse;
                border-spacing: 0;
                background-color: #fff;
            }*/
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <div class="modal fade" id="client-list-modal" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
                    <h4 class="modal-title">
                        <asp:Literal runat="server" ID="EmailListTitleLiteral">Client List</asp:Literal>
                    </h4>
                </div>
                <div class="modal-body">
                    <asp:Repeater runat="server" ID="EmailListRepeater">
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
                                <td><%#Eval("AuthLevel")%></td>
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

    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu1" SelectedIndex="2" />

    <asp:PlaceHolder runat="server" ID="AddUserPlaceHolder">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">Authorize Client</h3>
            </div>
            <div class="panel-body">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="control-label col-sm-2">Authorization *</label>
                        <div class="col-sm-2">
                            <asp:DropDownList runat="server" ID="AuthLevelDropDownList" DataValueField="AuthLevelID" DataTextField="AuthLevelName" CssClass="form-control auth-levels" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="control-label col-sm-2">User Name *</label>
                        <div class="col-sm-4">
                            <asp:DropDownList runat="server" ID="ClientsDropDownList" DataTextField="DisplayName" DataValueField="ClientID" CssClass="form-control clients" />
                            <asp:PlaceHolder runat="server" ID="ClientNamePlaceHolder">
                                <p class="form-control-static">
                                    <asp:Literal runat="server" ID="ClientNameLiteral"></asp:Literal>
                                    <asp:HiddenField runat="server" ID="ClientIdHiddenField"></asp:HiddenField>
                                </p>
                            </asp:PlaceHolder>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10 buttons">
                            <asp:Button ID="SubmitButton" runat="server" CssClass="lnf btn btn-default" Text="Authorize Client" CommandName="Authorize" OnCommand="SubmitButton_Command" />&nbsp;
                            <asp:Button ID="CancelButton" runat="server" CssClass="lnf btn btn-default" Text="Cancel" OnClick="CancelButton_Click" />
                        </div>
                    </div>
                </div>

                <asp:PlaceHolder runat="server" ID="ErrorMessagePlaceHolder" Visible="false">
                    <div class="alert alert-danger" role="alert">
                        <asp:Literal runat="server" ID="ErrorMessageLiteral"></asp:Literal>
                    </div>
                </asp:PlaceHolder>

            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="EmailListPlaceHolder">
        <div style="padding-left: 15px; margin-bottom: 20px; font-size: 14px;">
            <img src="images/email.gif" alt="email" />&nbsp;
            <asp:HyperLink runat="server" ID="EmailAllHyperLink" Font-Bold="true" ForeColor="Black" Font-Underline="true">Email All</asp:HyperLink>
            <span>|</span>
            <a href="#" data-toggle="modal" data-target="#client-list-modal" style="font-weight: bold; color: #000; text-decoration: underline;">Client List</a>
        </div>
    </asp:PlaceHolder>

    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">
                <img src="images/email.gif" alt="email" />&nbsp;
                <asp:HyperLink runat="server" ID="EmailToolEngineersHyperLink" Font-Bold="true" ForeColor="Black" Font-Underline="true">Tool Engineers</asp:HyperLink>
            </h3>
        </div>
        <div class="panel-body">
            <div class="row">
                <div class="col-md-5">
                    <asp:Repeater runat="server" ID="ToolEngineersRepeater">
                        <HeaderTemplate>
                            <table class="table table-hover">
                                <thead>
                                    <th></th>
                                    <th style="width: 60px;"></th>
                                    <th style="width: 60px;"></th>
                                </thead>
                                <tbody>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <asp:HyperLink runat="server" NavigateUrl='<%#Eval("ContactUrl")%>'><%#Eval("DisplayName")%></asp:HyperLink>
                                </td>
                                <td class="text-center" style="width: 60px;">
                                    <asp:ImageButton runat="server" CommandName="ToolEngineer" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Edit_Command" ImageUrl="~/images/edit.gif" Visible='<%#CanModify()%>' />
                                    <asp:ImageButton runat="server" CommandName="ToolEngineer" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Delete_Command" ImageUrl="~/images/delete.gif" Visible='<%#CanDelete()%>' />
                                </td>
                                <td class="text-center" style="width: 60px;">&nbsp;</td>
                            </tr>
                        </ItemTemplate>
                        <FooterTemplate>
                            </tbody>
                            </table>
                        </FooterTemplate>
                    </asp:Repeater>

                    <asp:PlaceHolder runat="server" ID="NoToolEngineersPlaceHolder" Visible="false">
                        <em class="text-muted">There are no tool engineers.</em>
                    </asp:PlaceHolder>
                </div>
            </div>
        </div>
    </div>

    <asp:PlaceHolder runat="server" ID="TrainersPlaceHolder">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <img src="images/email.gif" alt="email" />&nbsp;
                    <asp:HyperLink runat="server" ID="EmailTrainersHyperLink" Font-Bold="true" ForeColor="Black" Font-Underline="true">Authorized Staff</asp:HyperLink>
                </h3>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-5">
                        <asp:Repeater runat="server" ID="TrainersRepeater">
                            <HeaderTemplate>
                                <table class="table table-hover">
                                    <thead>
                                        <th></th>
                                        <th style="width: 60px;"></th>
                                        <th style="width: 60px;"></th>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <asp:HyperLink runat="server" NavigateUrl='<%#Eval("ContactUrl")%>'><%#Eval("DisplayName")%></asp:HyperLink>
                                    </td>
                                    <td class="text-center" style="width: 60px;">
                                        <asp:ImageButton runat="server" CommandName="Trainer" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Edit_Command" ImageUrl="~/images/edit.gif" Visible='<%#CanModify()%>' />
                                        <asp:ImageButton runat="server" CommandName="Trainer" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Delete_Command" ImageUrl="~/images/delete.gif" Visible='<%#CanDelete()%>' />
                                    </td>
                                    <td class="text-center" style="width: 60px;">&nbsp;</td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder runat="server" ID="NoTrainersPlaceHolder" Visible="false">
                            <em class="text-muted">There are no authorized staff.</em>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="SuperUsersPlaceHolder">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <img src="images/email.gif" alt="email" />&nbsp;
                    <asp:HyperLink runat="server" ID="EmailSuperUsersHyperLink" Font-Bold="true" ForeColor="Black" Font-Underline="true">Super Users</asp:HyperLink>
                </h3>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-5">
                        <asp:Repeater runat="server" ID="SuperUsersRepeater">
                            <HeaderTemplate>
                                <table class="table table-hover">
                                    <thead>
                                        <th></th>
                                        <th style="width: 60px;"></th>
                                        <th style="width: 60px;"></th>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <asp:HyperLink runat="server" NavigateUrl='<%#Eval("ContactUrl")%>'><%#Eval("DisplayName")%></asp:HyperLink>
                                    </td>
                                    <td class="text-center" style="width: 60px;">
                                        <asp:ImageButton runat="server" CommandName="SuperUser" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Edit_Command" ImageUrl="~/images/edit.gif" Visible='<%#CanModify()%>' />
                                        <asp:ImageButton runat="server" CommandName="SuperUser" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Delete_Command" ImageUrl="~/images/delete.gif" Visible='<%#CanDelete()%>' />
                                    </td>
                                    <td class="text-center" style="width: 60px;">&nbsp;</td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder runat="server" ID="NoSuperUsersPlaceHolder" Visible="false">
                            <em class="text-muted">There are no super users.</em>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:PlaceHolder runat="server" ID="AuthorizedUsersPlaceHolder">
        <div class="lnf panel panel-default">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <img src="images/email.gif" alt="email" />&nbsp;
                    <asp:HyperLink runat="server" ID="EmailAuthorizedUsersHyperLink" Font-Bold="true" ForeColor="Black" Font-Underline="true">Authorized Users</asp:HyperLink>
                </h3>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-5">
                        <asp:Repeater runat="server" ID="AuthorizedUsersRepeater">
                            <HeaderTemplate>
                                <table class="table table-hover authorized-users">
                                    <thead>
                                        <th></th>
                                        <th style="width: 60px;"></th>
                                        <th style="width: 60px;"></th>
                                    </thead>
                                    <tbody>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <td>
                                        <asp:HyperLink runat="server" NavigateUrl='<%#Eval("ContactUrl")%>'><%#Eval("DisplayName")%></asp:HyperLink>
                                    </td>
                                    <td class="text-center" style="width: 60px;">
                                        <asp:ImageButton runat="server" CommandName="AuthorizedUser" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Edit_Command" ImageUrl="~/images/edit.gif" Visible='<%#CanModify()%>' />
                                        <asp:ImageButton runat="server" CommandName="AuthorizedUser" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Delete_Command" ImageUrl="~/images/delete.gif" Visible='<%#CanDelete()%>' />
                                    </td>
                                    <td class="text-center" style="width: 60px;">
                                        <asp:PlaceHolder runat="server" Visible='<%#CanExtend()%>'>
                                            <asp:ImageButton runat="server" CommandName="AuthorizedUser" CommandArgument='<%#Eval("ClientID")%>' OnCommand="Extend_Command" ImageUrl="~/images/extend.gif" Visible='<%#Eval("ShowExtendButton")%>' />
                                        </asp:PlaceHolder>
                                        <asp:PlaceHolder runat="server" Visible='<%#!CanExtend()%>'>
                                            <asp:Label runat="server" Visible='<%#Eval("ShowExtendButton")%>'>E</asp:Label>
                                        </asp:PlaceHolder>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                </tbody>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>

                        <asp:PlaceHolder runat="server" ID="NoAuthorizedUsersPlaceHolder" Visible="false">
                            <em class="text-muted">There are no authorized users.</em>
                        </asp:PlaceHolder>
                    </div>
                </div>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        $(".authorized-users").DataTable({
            "ordering": false,
            "lengthChange": false,
            "pageLength": 10,
            "columns": [
                { "searchable": true },
                { "searchable": false },
                { "searchable": false }
            ],
            "initComplete": function (settings, json) {
                $(".dataTables_filter input").addClass("form-control");
            }
        });
    </script>
</asp:Content>
