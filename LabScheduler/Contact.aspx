<%@ Page Title="Contact" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="Contact.aspx.vb" Inherits="LabScheduler.Pages.Contact" %>

<%@ Register TagPrefix="uc" TagName="Contact" Src="~/UserControls/Contact.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <uc:Contact ID="contact" runat="server" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
