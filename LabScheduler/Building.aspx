<%@ Page Title="Building" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="Building.aspx.vb" Inherits="LabScheduler.Pages.Building" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .datatable {
            width: 100%;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5 style="font-weight: bold; color: #cc6633;">
        <asp:Literal runat="server" ID="litBuildingName"></asp:Literal>
    </h5>

    <asp:Label runat="server" ID="lblDescription" Font-Bold="true"></asp:Label>

    <div style="margin-top: 10px;">
        <asp:Image runat="server" ID="imgPicture"></asp:Image>
    </div>

    <div class="table-container" style="margin-top: 10px; visibility: hidden;">
        <table class="resources datatable table table-striped">
            <thead>
                <tr>
                    <th>Lab</th>
                    <th>Process Tech</th>
                    <th>Resource</th>
                </tr>
            </thead>
            <tbody>
                <asp:Repeater runat="server" ID="rptResources">
                    <ItemTemplate>
                        <tr>
                            <td>
                                <a href='<%#Eval("LabUrl")%>'><%#Eval("LabName")%></a>
                            </td>
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
                { "width": "200px" },
                null
            ]
        });
    </script>
</asp:Content>
