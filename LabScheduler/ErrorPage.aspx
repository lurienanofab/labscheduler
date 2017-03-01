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

<%@ Page Title="Error" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ErrorPage.aspx.vb" Inherits="LabScheduler.Pages.ErrorPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .error-page {
        }

        .error-page-detail {
            font-family: 'Courier New';
            white-space: pre;
            color: #ff0000;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="error-page">
        <h2>An error has occurred</h2>
        <hr />
        <asp:Repeater runat="server" ID="rptError">
            <ItemTemplate>
                <h3 style="color: #ff0000;"><%#Eval("Message")%></h3>
                <div class="error-page-detail"><%#Eval("StackTrace")%></div>
                <div class="error-page-detail" style="margin-top: 20px;">ErrorLogID: <%#Eval("ErrorLogID")%></div>
                <hr />
            </ItemTemplate>
        </asp:Repeater>
        <asp:Panel runat="server" ID="panError">
            This error has been logged and an email has been sent to IT support.<br />
            If you have any questions or comments please send an email to <a href="mailto:lnf-it@umich.edu">lnf-it@umich.edu</a> and refer to the ErrorLogID above.
            <asp:Literal runat="server" ID="litMessage"></asp:Literal>
        </asp:Panel>
        <asp:Panel runat="server" ID="panNoError" Visible="false">
            There does not appear to be an error at this time.<br />
            If you would like to report an issue please send an email to <a href="mailto:lnf-it@umich.edu">lnf-it@umich.edu</a>.
        </asp:Panel>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>