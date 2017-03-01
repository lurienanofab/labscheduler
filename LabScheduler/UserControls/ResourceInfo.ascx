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

<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="ResourceInfo.ascx.vb" Inherits="LabScheduler.UserControls.ResourceInfo" EnableViewState="true" %>

<asp:HiddenField runat="server" ID="hidResourceID" />

<asp:Repeater runat="server" ID="rptResourceInfo" OnItemDataBound="rptResourceInfo_ItemDataBound">
    <HeaderTemplate>
        <table border="0" style="border-spacing: 3px; border-collapse: separate; width: 100%;">
            <tbody>
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td colspan="2" style="white-space: nowrap; font-weight: bold; color: #cc6633;">
                <%#Eval("ResourceName")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap; vertical-align: top;">Tool Engr(s):</td>
            <td>
                <asp:Repeater runat="server" ID="rptToolEngineers">
                    <HeaderTemplate>
                        <ul style="list-style-type: none; padding: 0; margin: 0;">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li>
                            <a href='<%#Eval("Url")%>' title='<%#Eval("Email")%>'><%#Eval("DisplayName")%></a>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Res Fence:</td>
            <td>
                <%#Eval("ReservationFence", "{0} hours")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Min Res:</td>
            <td>
                <%#Eval("MinReservationTime", "{0} minutes")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Max Res:</td>
            <td>
                <%#Eval("MaxReservationTime", "{0} hours")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Max Sched:</td>
            <td>
                <%#Eval("MaxAlloc", "{0} hours")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Min Cancel:</td>
            <td>
                <%#Eval("MinCancelTime", "{0} minutes")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Grace Per:</td>
            <td>
                <%#Eval("GracePeriod", "{0} minutes")%>
            </td>
        </tr>
        <tr>
            <td style="background-color: #dcdcdc; white-space: nowrap;">Cost:</td>
            <td>
                <%#Eval("HourlyCost")%>
            </td>
        </tr>
        <tr id="trAutoEnd" runat="server" visible='<%#Convert.ToInt32(Eval("AutoEnd")) > -1%>'>
            <td style="background-color: #dcdcdc; white-space: nowrap; color: #ff0000;">Auto End:</td>
            <td style="color: #ff0000;">
                <%#Eval("AutoEnd", "{0} minutes")%>
            </td>
        </tr>
    </ItemTemplate>
    <FooterTemplate>
        </tbody>
        </table>
    </FooterTemplate>
</asp:Repeater>
