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

<%@ Page Title="Process Tech" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ProcessTech.aspx.vb" Inherits="LabScheduler.Pages.ProcessTech" %>

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
                <b>Date:
                    <asp:Label runat="server" ID="lblDate"></asp:Label></b>
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
