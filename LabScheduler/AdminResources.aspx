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

<%@ Page Title="Admin: Resources" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="AdminResources.aspx.vb" Inherits="LabScheduler.Pages.AdminResources" %>

<%@ Register TagPrefix="uc" TagName="AdminTabMenu" Src="~/UserControls/AdminTabMenu.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/admin.css" />
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td class="tabs">
                <uc:AdminTabMenu runat="server" ID="AdminTabMenu1" SelectedIndex="4" />
            </td>
        </tr>
        <tr>
            <td class="view">
                <input runat="server" id="hidBaseUrl" type="hidden" class="base-url" />
                <input runat="server" id="hidAjaxUrl" type="hidden" class="ajax-url" />
                <div class="general-error" style="padding-bottom: 5px;">
                    <asp:Label ID="lblErrMsg" runat="server" ForeColor="Red"></asp:Label>
                </div>
                <asp:Panel ID="pEditResource" runat="server">
                    <div class="resource-edit-form">
                        <input runat="server" id="hidResourceID" type="hidden" class="resource-id" value="" />
                        <input runat="server" id="hidProcTechID" type="hidden" class="proctech-id" value="" />
                        <input runat="server" id="hidLabID" type="hidden" class="lab-id" value="" />
                        <input runat="server" id="hidBuildingID" type="hidden" class="building-id" value="" />
                        <table class="Table" border="0" style="width: 500px;">
                            <tr class="TableHeader">
                                <td>
                                    <b>
                                        <asp:Label ID="lblAction" runat="server"></asp:Label>&nbsp;Resource</b>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <table border="0" style="width: 100%;">
                                        <tr>
                                            <td>Building *</td>
                                            <td>
                                                <select class="building-select" style="width: 150px;">
                                                </select>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Laboratory *</td>
                                            <td>
                                                <select class="lab-select" style="width: 150px;">
                                                </select>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Process Tech *</td>
                                            <td>
                                                <select class="proctech-select" style="width: 150px;">
                                                </select>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Resource ID *</td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtResourceID" MaxLength="6" Width="50" CssClass="resource-id-textbox"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="rfv1" ErrorMessage="This is a required field." runat="server" ControlToValidate="txtResourceID" ValidationGroup="none"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Resource Name *</td>
                                            <td>
                                                <asp:TextBox ID="txtResourceName" runat="server" MaxLength="50" Width="330" CssClass="resource-name-textbox"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="rfvResourceName" runat="server" ControlToValidate="txtResourceName" ErrorMessage="This is a required field." Display="Dynamic" ValidationGroup="regular"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Schedulable</td>
                                            <td>
                                                <asp:CheckBox ID="chkSchedulable" runat="server" Checked="True" CssClass="schedulable-checkbox"></asp:CheckBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Active</td>
                                            <td>
                                                <asp:CheckBox ID="chkActive" runat="server" Checked="True" CssClass="active-checkbox"></asp:CheckBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td style="vertical-align: top;">Description</td>
                                            <td>
                                                <asp:TextBox ID="txtDesc" TabIndex="16" runat="server" Columns="40" TextMode="MultiLine" Rows="5" CssClass="resource-description-textarea"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>Helpdesk Email</td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtHelpdeskEmail" Width="330" CssClass="resource-helpdesk-textbox"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <%--<tr>
                                            <td>
                                                Email Group
                                            </td>
                                            <td>
                                                <asp:TextBox runat="server" ID="txtEmailGroup" Width="330" CssClass="resource-emailgroup-textbox"></asp:TextBox>
                                            </td>
                                        </tr>--%>
                                        <tr>
                                            <td>&nbsp;</td>
                                            <td>
                                                <div class="edit-form-buttons" style="height: 25px;">
                                                    <input runat="server" id="btnAdd" type="button" class="Button edit-add-button" value="Save" />
                                                    <input runat="server" id="btnAddAnother" type="button" class="Button edit-add-another-button" value="Save and Add Another" />
                                                    <input runat="server" id="btnUpdate" type="button" class="Button edit-update-button" value="Update" />
                                                    <input type="button" class="Button edit-cancel-button" value="Cancel" />
                                                </div>
                                                <div class="validation-message" style="padding-top: 5px;">
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                        </table>
                        <table class="Table" border="0" style="width: 500px; margin-top: 10px;">
                            <tr class="TableHeader">
                                <td>
                                    <b>Tool Engineers</b>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 10px;">
                                    <asp:Literal runat="server" ID="litToolEngineer"></asp:Literal>
                                    <div class="tool-engineer-error" style="padding-top: 5px;">
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table class="Table" border="0" style="width: 500px; margin-top: 10px;">
                            <tr class="TableHeader">
                                <td>
                                    <b>Tool Image</b>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding: 10px;">
                                    <asp:Panel runat="server" ID="panResourceImage">
                                        <input id="filePic" type="file" name="resimg" class="image-upload" />
                                        <input type="button" class="edit-upload-button" value="Upload" />
                                        <asp:Image ID="imgPic" runat="server" BorderStyle="Notset" BorderWidth="0" Visible="False" CssClass="resource-image"></asp:Image>
                                    </asp:Panel>
                                    <asp:Literal runat="server" ID="litResourceImage"></asp:Literal>
                                    <div class="tool-image-error" style="padding-top: 5px;">
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pListResource" runat="server">
                    <div class="resource-list">
                        <table class="Table" border="0">
                            <tr class="TableHeader">
                                <td>
                                    <b>Resource List</b>
                                </td>
                            </tr>
                            <tr>
                                <td style="padding-bottom: 10px;">
                                    <input type="button" class="Button add-resource-button" value="Add Resources" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <span class="resource-list-working nodata">Retrieving resources... </span>
                                    <div class="resource-table-container">
                                        <asp:Repeater runat="server" ID="rptResources">
                                            <HeaderTemplate>
                                                <table class="resource-table">
                                                    <thead>
                                                        <tr>
                                                            <th class="action-links-header">&nbsp;</th>
                                                            <th class="building-name-header">Building</th>
                                                            <th class="lab-name-header">Lab</th>
                                                            <th class="process-tech-header">Process Tech</th>
                                                            <th class="resource-id-header">ResourceID</th>
                                                            <th class="resource-name-header">Resource</th>
                                                            <th class="tool-engineer-header">Tool Engineer</th>
                                                            <th class="schedulable-header">Schedulable</th>
                                                            <th class="picture-header">Picture</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <tr>
                                                    <td class="action-links-item">
                                                        <%#Eval("ActionLinks")%>
                                                    </td>
                                                    <td class="building-name-item">
                                                        <%#Eval("BuildingName")%>
                                                    </td>
                                                    <td class="lab-name-item">
                                                        <%#Eval("LabName")%>
                                                    </td>
                                                    <td class="process-tech-item">
                                                        <%#Eval("ProcessTechName")%>
                                                    </td>
                                                    <td class="resource-id-item">
                                                        <%#Eval("ResourceID")%>
                                                    </td>
                                                    <td class="resource-name-item">
                                                        <%#Eval("ResourceName")%>
                                                    </td>
                                                    <td class="tool-engineer-item">
                                                        <%#Eval("ToolEngineer")%>
                                                    </td>
                                                    <td class="schedulable-item">
                                                        <%#Eval("Schedulable")%>
                                                    </td>
                                                    <td class="picture-item">
                                                        <%#Eval("Picture")%>
                                                    </td>
                                                </tr>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </tbody> </table>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                        <div style="clear: both;">
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </asp:Panel>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="scripts/jquery.admin.js"></script>
    <script>
        $(".resource-list").adminResourceList();
        $(".resource-edit-form").adminResourceEditForm();
    </script>
</asp:Content>
