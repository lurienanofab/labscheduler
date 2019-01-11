<%@ Page Title="Admin: Properties" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="AdminProperties.aspx.vb" Inherits="LabScheduler.Pages.AdminProperties" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <lnf:AdminTabMenu runat="server" ID="AdminTabMenu1" SelectedIndex="5" />

    <asp:Literal runat="server" ID="litAlertMessage"></asp:Literal>

    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Modifiy Global Properties</h3>
        </div>
        <div class="panel-body">
            <div class="form-horizontal">
                <div class="form-group">
                    <label class="col-sm-3 control-label">Late Charge Penalty Multiplier *</label>
                    <div class="col-sm-2">
                        <asp:TextBox ID="txtLateChargePenalty" MaxLength="5" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Authorization Expiration Warning *</label>
                    <div class="col-sm-2">
                        <asp:TextBox ID="txtAuthExpWarning" MaxLength="5" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Resource IP Prefix *</label>
                    <div class="col-sm-2">
                        <asp:TextBox ID="txtResourceIPPrefix" runat="server" MaxLength="15" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">Scheduler Administrator *</label>
                    <div class="col-sm-4">
                        <asp:DropDownList ID="ddlAdmin" runat="server" DataValueField="ClientID" DataTextField="DisplayName" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-3 control-label">General Lab Account *</label>
                    <div class="col-sm-4">
                        <asp:DropDownList ID="ddlAccount" runat="server" DataValueField="AccountID" DataTextField="Name" CssClass="form-control">
                        </asp:DropDownList>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Kiosks</h3>
        </div>
        <div class="panel-body">
            <asp:DataGrid ID="dgKiosk" runat="server" BorderColor="#4682B4" AllowSorting="True" AutoGenerateColumns="False" ShowFooter="True" CellPadding="3" ShowHeader="True" Width="100%" CssClass="Table" OnItemCommand="DgKiosk_ItemCommand" OnItemDataBound="DgKiosk_ItemDataBound">
                <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                <FooterStyle CssClass="GridText" BackColor="LightGreen" HorizontalAlign="Center"></FooterStyle>
                <Columns>
                    <asp:BoundColumn DataField="KioskID" ReadOnly="True" Visible="false"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Kiosk Name">
                        <ItemTemplate>
                            <asp:Label ID="lblKioskName" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txbKioskName" runat="server" MaxLength="50" CssClass="form-control" Width="100%"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txbNewKioskName" runat="server" MaxLength="50" CssClass="form-control" Width="100%"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Kiosk IP">
                        <HeaderStyle Width="200" />
                        <ItemTemplate>
                            <asp:Label ID="lblKioskIP" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txbKioskIP" runat="server" MaxLength="15" CssClass="form-control" Width="100%"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txbNewKioskIP" runat="server" Columns="15" MaxLength="15" CssClass="form-control" Width="100%"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn>
                        <HeaderStyle Width="100" />
                        <ItemStyle HorizontalAlign="Center" />
                        <ItemTemplate>
                            <asp:ImageButton ID="ibtnEdit" ImageUrl="~/images/edit.gif" CommandName="Edit" ToolTip="Edit" runat="server" CausesValidation="False"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" ToolTip="Delete" CausesValidation="False" runat="server"></asp:ImageButton>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:ImageButton ID="ibtnUpdate" ImageUrl="~/images/update.gif" CommandName="Update" ToolTip="Update" runat="server" CausesValidation="False"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnCancel" ImageUrl="~/images/cancel.gif" CommandName="Cancel" ToolTip="Cancel" CausesValidation="False" runat="server"></asp:ImageButton>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:Button ID="btnAddRow" runat="server" Text="Add" CommandName="AddNewRow" CssClass="lnf btn btn-default"></asp:Button>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </div>
    </div>

    <asp:Button ID="btnSubmit" runat="server" CssClass="lnf btn btn-default" Text="Modify Properties" OnClick="BtnSubmit_Click" />
    <input class="lnf btn btn-default" type="reset" value="Reset" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
