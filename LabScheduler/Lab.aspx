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

<%@ Page Title="Lab" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="Lab.aspx.vb" Inherits="LabScheduler.Pages.Lab" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .datatable {
            width: 100%;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td>
                <h5>
                    <asp:Label ID="lblLabPath" Font-Bold="True" runat="server" />
                    <asp:Label ID="lblLabName" Font-Bold="True" ForeColor="#cc6633" runat="server" /></h5>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label ID="lblDescription" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td id="tdImage" runat="server" class="TableCell">
                <asp:Image ID="imgPicture" runat="server"></asp:Image>
            </td>
        </tr>
        <tr>
            <td>
                <div class="table-container" style="margin-top: 5px; visibility: hidden;">
                    <table class="resources datatable">
                        <thead>
                            <tr>
                                <th>Process Tech</th>
                                <th>Resource</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater runat="server" ID="rptResources">
                                <ItemTemplate>
                                    <tr>
                                        <td>
                                            <a href='<%#Eval("ProcessTechUrl")%>'><%#Eval("ProcessTechName")%></a>
                                        </td>
                                        <td>
                                            <a href='<%#Eval("ResourceUrl")%>'><%#String.Format("[{0}] {1}", Convert.ToInt32(Eval("ResourceID")).ToString("000000"), Eval("ResourceName"))%></a>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        $(".datatable").dataTable({
            "autoWidth": false,
            "initComplete": function (settings, json) {
                $(".table-container").css("visibility", "visible");
            },
            "columns": [
                { "width": "200px" },
                null
            ]
        });
    </script>
</asp:Content>
