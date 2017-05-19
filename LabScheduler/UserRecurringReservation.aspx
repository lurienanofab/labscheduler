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
                            <asp:HyperLink runat="server" ID="hypResource" NavigateUrl='<%#CType(Container.DataItem, RecurrenceItem).GetResourceUrl()%>'><%#Eval("ResourceName")%></asp:HyperLink>
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
        </div>
    </div>

    <table class="content-table">
        <tr>
            <td>
                <asp:GridView ID="zgvRecurring" runat="server" AutoGenerateColumns="false">
                    <Columns>
                        <asp:BoundField HeaderText="Resource" DataField="ResourceName" ItemStyle-HorizontalAlign="center" ItemStyle-Width="180" />
                        <asp:BoundField HeaderText="Begin Date" DataField="BeginDate" DataFormatString="{0:MM/dd/yyyy}" HtmlEncode="False" ItemStyle-HorizontalAlign="center" ItemStyle-Width="90" />
                        <asp:BoundField HeaderText="End Date" DataField="EndDate" NullDisplayText="Infinite" DataFormatString="{0:MM/dd/yyyy}" HtmlEncode="False" ItemStyle-HorizontalAlign="center" ItemStyle-Width="90" />
                        <asp:BoundField HeaderText="Begin Time" DataField="BeginTime" DataFormatString="{0:hh:mm tt}" HtmlEncode="False" ItemStyle-HorizontalAlign="center" ItemStyle-Width="90" />
                        <asp:BoundField HeaderText="End Time" DataField="EndTime" DataFormatString="{0:hh:mm tt}" HtmlEncode="False" ItemStyle-HorizontalAlign="center" ItemStyle-Width="90" />
                        <asp:BoundField HeaderText="Pattern Type" DataField="PatternName" ItemStyle-HorizontalAlign="center" ItemStyle-Width="90" />
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:ImageButton ID="imgbtnDel" AlternateText="Delete this order" runat="Server" OnClientClick="return confirm('Are you sure you want to delete this record?');" ImageUrl="~/images/im_delete.gif" CommandName="Delete" CommandArgument='<%# Bind("RecurrenceID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Edit">
                            <ItemTemplate>
                                <asp:ImageButton ID="imgbtnEdit" runat="Server" ImageUrl="~/images/im_edit.gif" AlternateText="Edit" CommandName="EditMe" CommandArgument='<%# Bind("RecurrenceID") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        &nbsp;&nbsp;-- You don't have any recurring reservation --&nbsp;&nbsp;
                    </EmptyDataTemplate>
                </asp:GridView>
                <asp:FormView ID="fvRecurring" runat="server">
                    <ItemTemplate>
                        <div class="divmargin20">
                            <span class="RowItemCaption">Creation Date:</span>
                            <%#Eval("RecurrenceID")%>
                        </div>
                    </ItemTemplate>
                </asp:FormView>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
