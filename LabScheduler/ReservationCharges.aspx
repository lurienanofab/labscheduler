<%@ Page Title="Reservation Charges" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ReservationCharges.aspx.vb" Inherits="LabScheduler.Pages.ReservationCharges" %>

<%@ Register TagName="NumericBox" TagPrefix="uc" Src="~/UserControls/NumericBox.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/main.css?v=20161220" />
    <style>
        .tbl td {
            padding: 10px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="tbl" border="0">
        <tr>
            <td style="width: 100px;">Charge Multiplier</td>
            <td style="width: 200px;">
                <uc:NumericBox ID="txtChargeMultiplier" runat="server" PositiveNumber="true"></uc:NumericBox>
            </td>
        </tr>
        <tr id="trLateCharge" runat="server">
            <td>Apply Late Charges</td>
            <td>
                <asp:CheckBox ID="chkApplyLateChargePenalty" runat="server"></asp:CheckBox>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                <asp:Button ID="btnSubmit" Text="Forgive Charges" runat="server"></asp:Button>
            </td>
        </tr>
    </table>
    <asp:Label ID="lblErrMsg" ForeColor="Red" runat="server"></asp:Label>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>