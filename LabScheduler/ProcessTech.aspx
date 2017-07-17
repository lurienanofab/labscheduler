<%@ Page Title="Process Tech" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ProcessTech.aspx.vb" Inherits="LabScheduler.Pages.ProcessTech" Async="true" %>

<%@ Reference Control="~/UserControls/ReservationView.ascx" %>
<%@ Register TagPrefix="uc" TagName="ReservationView" Src="~/UserControls/ReservationView.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td colspan="2">
                <h5>
                    <asp:Label ID="lblProcessTechPath" Font-Bold="True" runat="server" />
                    <asp:Label ID="lblProcessTechName" Font-Bold="True" ForeColor="#cc6633" runat="server" />
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
                <uc:ReservationView runat="server" ID="rvReserv" View="ProcessTechView"></uc:ReservationView>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        
    </script>
</asp:Content>
