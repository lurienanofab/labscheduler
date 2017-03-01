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
