<%@ Page Title="Admin: Activities" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="AdminActivities.aspx.vb" Inherits="LabScheduler.Pages.AdminActivities" %>

<%@ Import Namespace="LabScheduler.AppCode.DBAccess"  %>
<%@ Import Namespace="LNF.Scheduler"  %>
<%@ Import Namespace="LNF.Models.Scheduler"  %>


<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .auth-levels {
            border-collapse: separate;
            border-spacing: 3px;
            border: solid 1px #4682B4;
        }

            .auth-levels th,
            .auth-levels td {
                border: solid 1px #DDDDDD;
                padding: 2px;
            }

        .activity-item {
            padding: 5px;
            margin-bottom: 5px;
            border-bottom: solid 1px #4682B4;
        }

            .activity-item table {
                border-collapse: separate;
                border-spacing: 3px;
                width: 100%;
            }

                .activity-item table th,
                .activity-item table td {
                    border: solid 1px #DDDDDD;
                    padding: 2px;
                }

                    .activity-item table td.label {
                        width: 120px;
                        background-color: #E7E7FF;
                        text-align: right;
                    }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td class="tabs">
                <lnf:AdminTabMenu runat="server" ID="AdminTabMenu1" SelectedIndex="0" />
            </td>
        </tr>
        <tr>
            <td class="view">
                <asp:HiddenField runat="server" ID="hidSelectedActivityID" />
                <asp:Panel ID="pEditActivity" runat="server">
                    <table class="Table" border="0">
                        <tr class="TableHeader">
                            <td>
                                <b>
                                    <asp:Label ID="lblAction" runat="server"></asp:Label>&nbsp;Activity Type
                                </b>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <table border="0">
                                    <tr>
                                        <td>Activity Name *</td>
                                        <td>
                                            <asp:TextBox ID="txtActivityName" runat="server" Columns="20" MaxLength="50"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rfvActivityName" runat="server" Display="Dynamic" ErrorMessage="This is a required field." ControlToValidate="txtActivityName"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Display Order *</td>
                                        <td>
                                            <asp:TextBox runat="server" ID="txtListOrder" MaxLength="5" Width="40" />
                                            <asp:RequiredFieldValidator ID="rfvListOrder" runat="server" Display="Dynamic" ErrorMessage="This is a required field." ControlToValidate="txtListOrder"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Chargeable *</td>
                                        <td>
                                            <asp:CheckBox ID="chkChargeable" runat="server" Checked="True"></asp:CheckBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>Editable *</td>
                                        <td>
                                            <asp:CheckBox ID="chkEditable" runat="server" Checked="True"></asp:CheckBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Account Types *</td>
                                        <td>
                                            <asp:DropDownList ID="ddlAccountType" runat="server">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Reserver's Authorization *</td>
                                        <td>
                                            <asp:Repeater runat="server" ID="rptUserAuth">
                                                <HeaderTemplate>
                                                    <table class="auth-levels">
                                                        <tr>
                                                            <th>Authorization</th>
                                                            <th>
                                                                <img src="//ssel-apps.eecs.umich.edu/static/images/locked.png" alt="Locked" title="Locked" />
                                                            </th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:HiddenField runat="server" ID="hidAuthLevelValue" Value='<%#Eval("AuthLevelID")%>' />
                                                            <label>
                                                                <asp:CheckBox runat="server" ID="chkAuthLevel" />
                                                                <%#Eval("AuthLevelName")%>
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox runat="server" ID="chkLocked" />
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Invitee Type *</td>
                                        <td>
                                            <div style="margin-bottom: 10px;">
                                                <asp:DropDownList ID="ddlInviteeType" runat="server" AutoPostBack="True">
                                                </asp:DropDownList>
                                            </div>
                                            <asp:Repeater runat="server" ID="rptInviteeAuth">
                                                <HeaderTemplate>
                                                    <table class="auth-levels">
                                                        <tr>
                                                            <th>Authorization</th>
                                                            <th>
                                                                <img src="//ssel-apps.eecs.umich.edu/static/images/locked.png" alt="lock" />
                                                            </th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:HiddenField runat="server" ID="hidAuthLevelValue" Value='<%#Eval("AuthLevelID")%>' />
                                                            <label>
                                                                <asp:CheckBox runat="server" ID="chkAuthLevel" />
                                                                <%#Eval("AuthLevelName")%>
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox runat="server" ID="chkLocked" />
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Start/End Authorization *</td>
                                        <td>
                                            <asp:Repeater runat="server" ID="rptStartEndAuth">
                                                <HeaderTemplate>
                                                    <table class="auth-levels">
                                                        <tr>
                                                            <th>Authorization</th>
                                                            <th>
                                                                <img src="//ssel-apps.eecs.umich.edu/static/images/locked.png" alt="lock" />
                                                            </th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:HiddenField runat="server" ID="hidAuthLevelValue" Value='<%#Eval("AuthLevelID")%>' />
                                                            <label>
                                                                <asp:CheckBox runat="server" ID="chkAuthLevel" />
                                                                <%#Eval("AuthLevelName")%>
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox runat="server" ID="chkLocked" />
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Authorizations Not Bounded<br />
                                            by Reservation Fence
                                        </td>
                                        <td>
                                            <asp:Repeater runat="server" ID="rptNoReservFenceAuth">
                                                <HeaderTemplate>
                                                    <table class="auth-levels">
                                                        <tr>
                                                            <th>Authorization</th>
                                                            <th>
                                                                <img src="//ssel-apps.eecs.umich.edu/static/images/locked.png" alt="lock" />
                                                            </th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:HiddenField runat="server" ID="hidAuthLevelValue" Value='<%#Eval("AuthLevelID")%>' />
                                                            <label>
                                                                <asp:CheckBox runat="server" ID="chkAuthLevel" />
                                                                <%#Eval("AuthLevelName")%>
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox runat="server" ID="chkLocked" />
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Authorizations Not Bounded<br />
                                            by Maximum Schedulable Hours
                                        </td>
                                        <td>
                                            <asp:Repeater runat="server" ID="rptNoMaxSchedAuth">
                                                <HeaderTemplate>
                                                    <table class="auth-levels">
                                                        <tr>
                                                            <th>Authorization</th>
                                                            <th>
                                                                <img src="//ssel-apps.eecs.umich.edu/static/images/locked.png" alt="lock" />
                                                            </th>
                                                        </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr>
                                                        <td>
                                                            <asp:HiddenField runat="server" ID="hidAuthLevelValue" Value='<%#Eval("AuthLevelID")%>' />
                                                            <label>
                                                                <asp:CheckBox runat="server" ID="chkAuthLevel" />
                                                                <%#Eval("AuthLevelName")%>
                                                            </label>
                                                        </td>
                                                        <td>
                                                            <asp:CheckBox runat="server" ID="chkLocked" />
                                                        </td>
                                                    </tr>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    </table>
                                                </FooterTemplate>
                                            </asp:Repeater>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="vertical-align: top;">Description</td>
                                        <td>
                                            <asp:TextBox runat="server" ID="txtDescription" Columns="40" Rows="5" TextMode="MultiLine"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>&nbsp;</td>
                                        <td>
                                            <asp:Button ID="btnAdd" runat="server" Text="Save" CssClass="Button"></asp:Button>
                                            <asp:Button ID="btnAddAnother" runat="server" Text="Save and Add Another" CssClass="Button"></asp:Button>
                                            <asp:Button ID="btnUpdate" runat="server" Text="Update" CssClass="Button"></asp:Button>
                                            <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="Button" CausesValidation="False"></asp:Button>
                                        </td>
                                    </tr>
                                </table>
                                <asp:Label ID="lblErrMsg" runat="server" ForeColor="Red"></asp:Label>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
                <asp:Panel ID="pListActivity" runat="server">
                    <table class="Table" border="0">
                        <tr class="TableHeader">
                            <td>
                                <b>Activity List</b>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnNewActivity" runat="server" Text="Add Activities" CssClass="Button" OnClick="btnNewActivity_Click" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Repeater runat="server" ID="rptActivities" OnItemDataBound="rptActivities_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="activity-item">
                                            <table>
                                                <tr>
                                                    <td style="text-align: center; width: 50px;">
                                                        <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="~/images/edit.gif" OnCommand="Activity_Command" CommandName="edit" CommandArgument='<%#Eval("ActivityID")%>' />
                                                        <asp:ImageButton runat="server" ID="btnDelete" ImageUrl="~/images/delete.gif" OnCommand="Activity_Command" CommandName="delete" CommandArgument='<%#Eval("ActivityID")%>' />
                                                    </td>
                                                    <td style="text-align: center; width: 20px;">
                                                        <strong><%#Eval("ListOrder")%></strong>
                                                    </td>
                                                    <td>
                                                        <strong><%#Eval("ActivityName")%></strong>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="3">
                                                        <%#Eval("Description")%>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div style="float: left;">
                                                <table>
                                                    <tr>
                                                        <td class="label">Chargeable</td>
                                                        <td><%#Eval("Chargeable")%></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="label">Editable</td>
                                                        <td><%#Eval("Editable")%></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="label">Account Type</td>
                                                        <td><%#GetAccountTypeName(Eval("AccountType"))%></td>
                                                    </tr>
                                                    <tr>
                                                        <td class="label">Invitee Type</td>
                                                        <td><%#GetInviteeTypeName(Eval("InviteeType"))%></td>
                                                    </tr>
                                                </table>
                                            </div>
                                            <div style="float: left;">
                                                <asp:Repeater runat="server" ID="rptActivityAuths">
                                                    <HeaderTemplate>
                                                        <table>
                                                    </HeaderTemplate>
                                                    <ItemTemplate>
                                                        <tr>
                                                            <td class="label"><%#Eval("ActivityAuthTypeName")%></td>
                                                            <td><%#Eval("DefaultAuthText")%></td>
                                                        </tr>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        </table>
                                                    </FooterTemplate>
                                                </asp:Repeater>
                                            </div>
                                            <div style="clear: both;"></div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
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