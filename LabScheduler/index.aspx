<%@ Page Title="Welcome to Lab Scheduler" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="index.aspx.vb" Inherits="LabScheduler.Pages.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td>
                <h5>Welcome to the Lab Scheduler Version 3.0</h5>
                Current user:
                    <asp:Literal runat="server" ID="litDisplayName"></asp:Literal>
            </td>
        </tr>
        <tr>
            <td style="padding-top: 20px;">
                <b>For best results:</b>
                <ul>
                    <li>Please use Internet Explorer 8 or higher, or the latest version of Chrome or FireFox.</li>
                    <li>Older browser versions may cause slower performance and/or display issues.</li>
                    <li>Pop-up blocking software must be turned off.</li>
                    <li>We recommend minimum resolution of 1280 by 1024 pixels.</li>
                </ul>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>