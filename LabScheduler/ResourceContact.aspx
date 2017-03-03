<%@ Page Title="Helpdesk" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ResourceContact.aspx.vb" Inherits="LabScheduler.Pages.ResourceContact" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>
<%@ Register TagPrefix="uc" TagName="Helpdesk" Src="~/UserControls/Helpdesk.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu" SelectedIndex="3" />
    <uc:Helpdesk runat="server" ID="Helpdesk1" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>