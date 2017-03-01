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

<%@ Page Title="Helpdesk" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ResourceContact.aspx.vb" Inherits="LabScheduler.Pages.ResourceContact" %>

<%@ Register TagPrefix="uc" TagName="ResourceTabMenu" Src="~/UserControls/ResourceTabMenu.ascx" %>
<%@ Register TagPrefix="uc" TagName="Helpdesk" Src="~/UserControls/Helpdesk.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <uc:ResourceTabMenu runat="server" ID="ResourceTabMenu" SelectedIndex="3" />
    <uc:Helpdesk runat="server" ID="Helpdesk1" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
