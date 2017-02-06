<%@ Control Language="C#" AutoEventWireup="true" Inherits="LNF.Web.Scheduler.Controls.ResourceTabMenu" EnableViewState="true" %>

<div class="resource-tab-menu">
    <div class="tabs-title">
        <h5>
            <asp:Literal runat="server" ID="litHeaderText"></asp:Literal>
        </h5>
    </div>

    <asp:PlaceHolder runat="server" ID="phQuickReservation" Visible="false">
        <div class="ical-container">
            <div class="ical-title">
                <div class="ical-title-text">
                    Quick Reservation
                </div>
                <div class="ical-title-info">
                    <a href="#" class="ical-title-info-link">More Info</a>
                </div>
                <div style="clear: both;">
                </div>
            </div>
            <div class="ical-message" style="display: none;">
                You may either click an open time slot in the grid below or click the "Quick Reservation" button to go directly to the new reservation page and manually choose a start time.
            </div>
            <div class="ical-url">
                <asp:Button runat="server" ID="btnQuickReservation" Text="Create Reservation" OnClick="btnQuickReservation_Click" />
            </div>
        </div>
    </asp:PlaceHolder>

    <asp:Repeater runat="server" ID="rptTabs">
        <HeaderTemplate>
            <ul class="nav nav-tabs" role="tablist">
        </HeaderTemplate>
        <ItemTemplate>
            <li role="presentation" class='<%#Eval("CssClass")%>'><a href='<%#Eval("NavigateUrl")%>' role="tab"><%#Eval("Text")%></a></li>
        </ItemTemplate>
        <FooterTemplate>
            </ul>
        </FooterTemplate>
    </asp:Repeater>
</div>
