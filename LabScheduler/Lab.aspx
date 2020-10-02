<%@ Page Title="Lab" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="Lab.aspx.vb" Inherits="LabScheduler.Pages.Lab" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .datatable {
            width: 100%;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <h5 style="font-weight: bold;">
        <asp:Literal runat="server" ID="litLabPath"></asp:Literal>
        <span style="color: #cc6633;">
            <asp:Literal runat="server" ID="litLabName" /></span>
    </h5>

    <asp:Label runat="server" ID="lblDescription" Font-Bold="true"></asp:Label>

    <div style="margin-top: 10px;">
        <asp:Image runat="server" ID="imgPicture"></asp:Image>
    </div>

    <div class="table-container" style="margin-top: 10px; visibility: hidden;">
        <asp:PlaceHolder runat="server" ID="phResourcesByProcTech">
            <table class="resources datatable table table-striped">
                <thead>
                    <tr>
                        <th>Process Tech</th>
                        <th>Resource</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="rptResourcesByProcTech">
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
        </asp:PlaceHolder>
        <asp:PlaceHolder runat="server" ID="phResourcesByLocation" Visible="false">
            <table class="resources datatable table table-striped">
                <thead>
                    <tr>
                        <th>Location</th>
                        <th>Resource</th>
                    </tr>
                </thead>
                <tbody>
                    <asp:Repeater runat="server" ID="rptResourcesByLocation">
                        <ItemTemplate>
                            <tr>
                                <td>
                                    <a href='<%#Eval("LocationUrl")%>'><%#Eval("LocationName")%></a>
                                </td>
                                <td>
                                    <a href='<%#Eval("ResourceUrl")%>'><%#String.Format("[{0}] {1}", Convert.ToInt32(Eval("ResourceID")).ToString("000000"), Eval("ResourceName"))%></a>
                                </td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>
                </tbody>
            </table>
        </asp:PlaceHolder>
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
                null
            ]
        });
    </script>
</asp:Content>
