<%@ Page Title="Recurring Reservations" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="UserRecurringReservation.aspx.vb" Inherits="LabScheduler.Pages.UserRecurringReservation" %>

<%@ Import Namespace="LabScheduler.Pages" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <h5 style="margin-bottom: 20px;">My Recurring Reservations</h5>

    <div class="row">
        <div class="col-sm-12">
            <asp:Repeater runat="server" ID="rptRecurringReservations">
                <HeaderTemplate>
                    <table class="table table-hover">
                        <thead>
                            <tr>
                                <th>Resource</th>
                                <th>Begin Date</th>
                                <th>End Date</th>
                                <th>Begin Time</th>
                                <th>End Time</th>
                                <th>Pattern</th>
                                <th style="text-align: center;">Delete</th>
                                <th style="text-align: center;">Edit</th>
                            </tr>
                        </thead>
                        <tbody>
                </HeaderTemplate>
                <ItemTemplate>
                    <tr>
                        <td>
                            <asp:HyperLink runat="server" ID="hypResource" NavigateUrl='<%#CType(Container.DataItem, RecurrenceItem).GetResourceUrl(ContextBase)%>'><%#Eval("ResourceName")%></asp:HyperLink>
                        </td>
                        <td><%#Eval("BeginDate", "{0:MM/dd/yyyy}")%></td>
                        <td><%#CType(Container.DataItem, RecurrenceItem).GetEndDateString()%></td>
                        <td><%#Eval("BeginTime", "{0:hh:mm tt}")%></td>
                        <td><%#Eval("EndTime", "{0:hh:mm tt}")%></td>
                        <td><%#Eval("PatternName")%></td>
                        <td style="text-align: center;">
                            <asp:ImageButton runat="server" ID="imgbtnDel" ImageUrl="~/images/im_delete.gif" AlternateText="Delete" ToolTip="Delete" CommandName="Delete" CommandArgument='<%#Bind("RecurrenceID")%>' OnCommand="RecurringReservations_Command" OnClientClick="return confirm('Are you sure you want to delete this record?');" />
                        </td>
                        <td style="text-align: center;">
                            <asp:HyperLink runat="server" ID="hypEdit" ImageUrl="~/images/im_edit.gif" ToolTip="Edit" NavigateUrl='<%#GetEditUrl(Container.DataItem)%>'></asp:HyperLink>
                        </td>
                    </tr>
                </ItemTemplate>
                <FooterTemplate>
                    </tbody>
                    </table>
                </FooterTemplate>
            </asp:Repeater>
            <asp:PlaceHolder runat="server" ID="phNoData" Visible="false">
                <em class="text-muted">No recurring reservations were found.</em>
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
