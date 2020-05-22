<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="LabLocation.aspx.vb" Inherits="LabScheduler.LabLocation" %>

<%@ Reference Control="~/UserControls/ReservationView.ascx" %>
<%@ Register TagPrefix="uc" TagName="ReservationView" Src="~/UserControls/ReservationView.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td colspan="2">
                <h5>
                    <asp:Label ID="lblLabLocationPath" Font-Bold="True" runat="server" />
                    <asp:Label ID="lblLabLocationName" Font-Bold="True" ForeColor="#cc6633" runat="server" />
                </h5>
            </td>
        </tr>
        <tr>
            <td>
                <b>
                    <span>Date:</span>
                    <asp:Label runat="server" ID="lblDate"></asp:Label>
                </b>
            </td>
        </tr>
        <tr>
            <td class="view">
                <uc:ReservationView runat="server" ID="rvReserv" View="LocationView"></uc:ReservationView>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
