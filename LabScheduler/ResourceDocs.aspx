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

<%@ Page Title="Documents" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ResourceDocs.aspx.vb" Inherits="LabScheduler.Pages.ResourceDocs" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .resource-docs-table td {
            padding: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu1" SelectedIndex="6" />
    <asp:PlaceHolder runat="server" ID="phMessage"></asp:PlaceHolder>
    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Resource Documents</h3>
        </div>
        <div class="panel-body">
            <asp:DataGrid runat="server" ID="dgDocs" ShowFooter="True" CellPadding="3" AutoGenerateColumns="False" BorderColor="#4682B4" CssClass="resource-docs-table">
                <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                <FooterStyle BackColor="#ccffcc"></FooterStyle>
                <Columns>
                    <asp:BoundColumn DataField="DocID" ReadOnly="True" Visible="False" />
                    <asp:BoundColumn DataField="FileName" ReadOnly="True" Visible="False" />
                    <asp:TemplateColumn>
                        <ItemStyle HorizontalAlign="Center"></ItemStyle>
                        <ItemTemplate>
                            <asp:ImageButton ID="ibtnEdit" CommandName="Edit" ImageUrl="~/images/edit.gif" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnDelete" CommandName="Delete" ImageUrl="~/images/delete.gif" runat="server"></asp:ImageButton>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:ImageButton ID="ibtnUpdate" CommandName="Update" ImageUrl="~/images/update.gif" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnCancel" CommandName="Cancel" ImageUrl="~/images/cancel.gif" runat="server"></asp:ImageButton>
                        </EditItemTemplate>
                        <FooterStyle HorizontalAlign="Center"></FooterStyle>
                        <FooterTemplate>
                            <asp:Button ID="btnAdd" CommandName="Add" runat="server" CssClass="lnf btn btn-default" Text="Add Doc"></asp:Button>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Doc Name">
                        <ItemTemplate>
                            <asp:Label ID="lblDocName" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txbDocName" Columns="30" MaxLength="500" runat="server"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txbNewDocName" Columns="30" MaxLength="500" runat="server"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Upload File">
                        <ItemTemplate>
                            <asp:HyperLink ID="hplView" runat="server" Target="_blank">Click to view doc</asp:HyperLink>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <input type="file" id="fileDoc" runat="server" />
                        </EditItemTemplate>
                        <FooterTemplate>
                            <input type="file" id="fileNewDoc" runat="server" />
                        </FooterTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
