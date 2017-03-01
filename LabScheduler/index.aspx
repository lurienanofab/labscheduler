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

<%@ Page Title="" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="index.aspx.vb" Inherits="LabScheduler.Pages.Index" %>

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