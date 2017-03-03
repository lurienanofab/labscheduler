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

<%@ Page Title="Admin: Labs" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="AdminLabs.aspx.vb" Inherits="LabScheduler.Pages.AdminLabs" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td class="tabs">
                <lnf:AdminTabMenu runat="server" ID="AdminTabMenu1" SelectedIndex="2" />
            </td>
        </tr>
        <tr>
            <td class="view">
                <asp:Panel runat="server" ID="pEditLab">
                    <table class="Table" border="0">
                        <tr class="TableHeader">
                            <td>
                                <b><asp:Label ID="lblAction" runat="server"></asp:Label>&nbsp;Laboratory</b>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table border="0">
                                    <tr>
                                        <td>Building *</td>
                                        <td>
                                            <asp:DropDownList ID="ddlBuilding" runat="server" DataTextField="BuildingName" DataValueField="BuildingID">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Lab Name *</td>
                                        <td>
                                            <asp:TextBox ID="txbLabName" runat="server" Columns="20" MaxLength="50"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rfvLabName" runat="server" ControlToValidate="txbLabName" ErrorMessage="This is a required field." Display="Dynamic"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Room *</td>
                                        <td>
                                            <asp:DropDownList ID="ddlRooms" runat="server" DataTextField="Room" DataValueField="RoomID">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Picture</td>
                                        <td>
                                            <input id="filePic" type="file" runat="server" />
                                            <asp:Image ID="imgPic" runat="server" Visible="False" BorderWidth="0" BorderStyle="Notset"></asp:Image>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Description</td>
                                        <td>
                                            <asp:TextBox ID="txbDesc" TabIndex="5" runat="server" Columns="40" Rows="5" TextMode="MultiLine"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Kiosks in Lab</td>
                                        <td>
                                            <asp:DataGrid ID="dgKioskLab" runat="server" BorderColor="#4682B4" AllowSorting="True" CellPadding="3" AutoGenerateColumns="False" ShowFooter="True" ShowHeader="True">
                                                <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                                                <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                                                <FooterStyle CssClass="GridText" BackColor="LightGreen" HorizontalAlign="Center"></FooterStyle>
                                                <Columns>
                                                    <asp:BoundColumn DataField="KioskLabID" ReadOnly="True" Visible="False"></asp:BoundColumn>
                                                    <asp:TemplateColumn HeaderText="Kiosk Name">
                                                        <ItemTemplate>
                                                            <asp:Label ID="lblKioskName" runat="server"></asp:Label>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:DropDownList ID="ddlKiosk" DataValueField="KioskID" DataTextField="KioskName" runat="server">
                                                            </asp:DropDownList>
                                                        </FooterTemplate>
                                                    </asp:TemplateColumn>
                                                    <asp:TemplateColumn ItemStyle-HorizontalAlign="Center">
                                                        <ItemTemplate>
                                                            <asp:ImageButton ID="ibtnKioskDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" ToolTip="Delete" CausesValidation="False" runat="server"></asp:ImageButton>
                                                        </ItemTemplate>
                                                        <FooterTemplate>
                                                            <asp:Button ID="btnAddRow" runat="server" Text="Add" CommandName="AddNewRow" CausesValidation="False"></asp:Button>
                                                        </FooterTemplate>
                                                    </asp:TemplateColumn>
                                                </Columns>
                                            </asp:DataGrid>
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
                <asp:Panel runat="server" ID="pListLab">
                    <table class="Table" border="0">
                        <tr class="TableHeader">
                            <td>
                                <b>Laboratory List</b>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnNewLab" runat="server" CssClass="Button" Text="Add Laboratories"></asp:Button>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:DataGrid ID="dgLabs" runat="server" BorderColor="#4682B4" AllowSorting="True" CellPadding="3" AutoGenerateColumns="False" DataKeyField="LabID" PageSize="20" AllowPaging="True">
                                    <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                                    <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699" Wrap="False"></HeaderStyle>
                                    <PagerStyle HorizontalAlign="Right" Mode="NumericPages"></PagerStyle>
                                    <Columns>
                                        <asp:BoundColumn DataField="LabID" Visible="False"></asp:BoundColumn>
                                        <asp:TemplateColumn ItemStyle-HorizontalAlign="Center" ItemStyle-Wrap="False">
                                            <ItemTemplate>
                                                <asp:ImageButton ID="ibtnEdit" ImageUrl="~/images/edit.gif" CommandName="Edit" ToolTip="Edit" runat="server" CausesValidation="False"></asp:ImageButton>
                                                <asp:ImageButton ID="ibtnDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" ToolTip="Delete" CausesValidation="False" runat="server"></asp:ImageButton>
                                            </ItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:BoundColumn DataField="BuildingName" SortExpression="BuildingName" HeaderText="Building"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="LabName" SortExpression="LabName" HeaderText="Lab"></asp:BoundColumn>
                                        <asp:BoundColumn DataField="Room" SortExpression="Room" HeaderText="Room"></asp:BoundColumn>
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