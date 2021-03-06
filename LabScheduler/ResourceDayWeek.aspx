﻿<%@ Page Title="Scheduler" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ResourceDayWeek.aspx.vb" Inherits="LabScheduler.Pages.ResourceDayWeek" EnableViewState="true" %>

<%@ Register TagPrefix="uc" TagName="ReservationView" Src="~/UserControls/ReservationView.ascx" %>
<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .resource-tool-tip {
            margin-bottom: 10px;
            font-weight: bold;
            padding: 5px;
            font-weight: bold;
            display: inline-block;
        }

            .resource-tool-tip.offline {
                background-color: #ff0000;
                color: #EECCCC;
            }

            .resource-tool-tip.limited{
                background-color: #E5E505;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu1" />

    <div class="row" style="overflow-x: hidden;">
        <div class="col-lg-12">
            <div class="view" style="overflow-x: auto;">

                <asp:PlaceHolder runat="server" ID="phResourceToolTip" Visible="false">
                    <asp:Panel runat="server" ID="panResourceToolTip">
                        <asp:Literal runat="server" ID="litResourceToolTip"></asp:Literal>
                    </asp:Panel>
                </asp:PlaceHolder>

                <uc:ReservationView runat="server" ID="ReservationView1"></uc:ReservationView>
                <div class="ical-container">
                    <div class="ical-title">
                        <div class="ical-title-text">
                            Calendar Data Feed URL
                        </div>
                        <div class="ical-title-info">
                            <a href="#" class="ical-title-info-link">More Info</a>
                        </div>
                        <div style="clear: both;"></div>
                    </div>
                    <div class="ical-message" style="display: none;">
                        This URL can be copied and pasted to a calendar application, such as Google Calendar or Outlook, to import all reservations for this tool into your calendar (instructions for <a href="http://support.google.com/calendar/bin/answer.py?hl=en&answer=37100" target="_blank">Google Calendar</a> | <a href="http://office.microsoft.com/en-us/outlook-help/view-and-subscribe-to-internet-calendars-HA010167325.aspx#BM2" target="_blank">Outlook</a>). Once you have linked the reservation data to your calendar all future reservations made by any user will appear automatically. [Note: This cannot be done from a kiosk becuase there is no connection to the external Internet.]
                    </div>
                    <div class="ical-url">
                        <img src="//ssel-apps.eecs.umich.edu/static/images/ical-icon.png" alt="ical" />
                        <asp:TextBox runat="server" ID="txtCalendarURL" Width="650" CssClass="calendar-feed-url form-control" Style="display: inline-block;"></asp:TextBox>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>
