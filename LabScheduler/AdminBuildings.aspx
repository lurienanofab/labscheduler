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

<%@ Page Title="Admin: Buildings" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="AdminBuildings.aspx.vb" Inherits="LabScheduler.Pages.AdminBuildings" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td class="tabs">
                <lnf:AdminTabMenu runat="server" ID="AdminTabMenu1" SelectedIndex="1" />
            </td>
        </tr>
        <tr>
            <td class="view">
                <asp:Panel ID="pEditBuilding" runat="server">
                    <table class="Table" border="0">
                        <tr class="TableHeader">
                            <td>
                                <b><asp:Label ID="lblAction" runat="server"></asp:Label>&nbsp;Building</b>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table border="0">
                                    <tr>
                                        <td>Building Name *</td>
                                        <td>
                                            <asp:TextBox ID="txbBuildingName" runat="server" Columns="20" MaxLength="50"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rfvBuildingName" runat="server" ControlToValidate="txbBuildingName" ErrorMessage="This is a required field." Display="Dynamic"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Picture</td>
                                        <td>
                                            <input id="filePic" type="file" runat="server" />
                                            <asp:Image ID="imgPic" runat="server"></asp:Image>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Description</td>
                                        <td>
                                            <asp:TextBox ID="txbDesc" runat="server" Columns="40" Rows="5" TextMode="MultiLine"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td>
                                            <asp:Button ID="btnAdd" runat="server" CssClass="Button" Text="Save"></asp:Button>
                                            <asp:Button ID="btnAddAnother" runat="server" CssClass="Button" Text="Save and Add Another"></asp:Button>
                                            <asp:Button ID="btnUpdate" runat="server" CssClass="Button" Text="Update"></asp:Button>
                                            <asp:Button ID="btnCancel" runat="server" CssClass="Button" Text="Cancel" CausesValidation="False"></asp:Button>
                                        </td>
                                    </tr>
                                </table>
                                <asp:Label ID="lblErrMsg" runat="server" ForeColor="Red"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <asp:Panel ID="pListBuilding" runat="server">
                    <table class="Table" border="0">
                        <tr class="TableHeader">
                            <td>
                                <b>Building List</b>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnNewBuilding" runat="server" CssClass="Button" Text="Add Buildings"></asp:Button>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:DataGrid ID="dgBuildings" runat="server" BorderColor="#4682B4" AutoGenerateColumns="False" CellPadding="3" AllowPaging="True" AllowSorting="True" PageSize="20" DataKeyField="BuildingID">
                                    <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                                    <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699" Wrap="False"></HeaderStyle>
                                    <PagerStyle HorizontalAlign="Right" Mode="NumericPages"></PagerStyle>
                                    <Columns>
                                        <asp:BoundColumn DataField="BuildingID" Visible="False"></asp:BoundColumn>
                                        <asp:TemplateColumn ItemStyle-HorizontalAlign="Center" ItemStyle-Wrap="False">
                                            <ItemTemplate>
                                                <asp:ImageButton ID="ibtnEdit" ImageUrl="~/images/edit.gif" CommandName="Edit" ToolTip="Edit" runat="server" CausesValidation="False"></asp:ImageButton>
                                                <asp:ImageButton ID="ibtnDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" ToolTip="Delete" CausesValidation="False" runat="server"></asp:ImageButton>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:BoundColumn DataField="BuildingName" SortExpression="BuildingName" HeaderText="Building"></asp:BoundColumn>
                                        <asp:TemplateColumn HeaderText="Picture" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <asp:Image ID="Picture" runat="server"></asp:Image>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:BoundColumn DataField="Description" HeaderText="Description"></asp:BoundColumn>
                                    </Columns>
                                </asp:DataGrid>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>