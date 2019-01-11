<%@ Page Title="Reservations" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="UserReservations.aspx.vb" Inherits="LabScheduler.Pages.UserReservations" %>

<%@ Reference Control="~/UserControls/ReservationView.ascx" %>
<%@ Register TagPrefix="uc" TagName="ReservationView" Src="~/UserControls/ReservationView.ascx" %>
<%@ Register TagPrefix="uc" TagName="HelpdeskInfo" Src="~/UserControls/HelpdeskInfo.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .manage-recurring {
            display: block;
            margin-bottom: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="content-table">
        <tr>
            <td>
                <h5>My Reservations</h5>
            </td>
        </tr>
        <tr>
            <td>
                <asp:HyperLink runat="server" ID="hypRecurringPage" Visible="false" Text="Manage My Recurring Reservations" CssClass="manage-recurring"></asp:HyperLink>
            </td>
        </tr>
        <tr>
            <td>
                <b>
                    <span>Current User:</span>
                    <asp:Literal runat="server" ID="litCurrentUser"></asp:Literal>
                </b>
            </td>
        </tr>
        <tr>
            <td>
                <b>
                    <span>Location:</span>
                    <asp:Literal runat="server" ID="litLocation"></asp:Literal>
                </b>
            </td>
        </tr>
        <tr>
            <td>
                <b>
                    <span>Date:</span>
                    <asp:Label runat="server" ID="lblDate"></asp:Label>
                </b>
            </td>
        </tr>
        <tr>
            <td>
                <uc:ReservationView runat="server" ID="rvReserv" View="UserView"></uc:ReservationView>
            </td>
        </tr>
        <tr>
            <td>
                <div class="ical-container">
                    <div class="ical-title">
                        <div class="ical-title-text">
                            Calendar Data Feed URL
                        </div>
                        <div class="ical-title-info">
                            <a href="#" class="ical-title-info-link">More Info</a>
                        </div>
                        <div style="clear: both;">
                        </div>
                    </div>
                    <div class="ical-message" style="display: none;">
                        This URL can be copied and pasted to a calendar application, such as Google Calendar or Outlook, to import all your reservations into your calendar (instructions for <a href="http://support.google.com/calendar/bin/answer.py?hl=en&answer=37100" target="_blank">Google Calendar</a> | <a href="http://office.microsoft.com/en-us/outlook-help/view-and-subscribe-to-internet-calendars-HA010167325.aspx#BM2" target="_blank">Outlook</a>). Once you have linked your reservation data to your calendar all future reservations you make will appear automatically. [Note: This cannot be done from a kiosk becuase there is no connection to the external Internet.]
                    </div>
                    <div class="ical-url">
                        <img src="//ssel-apps.eecs.umich.edu/static/images/ical-icon.png" alt="ical" />
                        <asp:TextBox runat="server" ID="txtCalendarURL" Width="650" CssClass="calendar-feed-url form-control" Style="display: inline-block;"></asp:TextBox>
                    </div>
                </div>
            </td>
        </tr>
    </table>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
