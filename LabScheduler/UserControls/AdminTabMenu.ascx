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

<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="AdminTabMenu.ascx.vb" Inherits="LabScheduler.UserControls.AdminTabMenu" %>

<div class="resource-tab-menu">
    <div class="tabs-title">
        <h5>
            <asp:Literal runat="server" ID="litHeaderText"></asp:Literal>
        </h5>
    </div>

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

<%--<div>
    <h5>Administration
    </h5>
</div>
<div>
    <table class="tab-strip">
        <tr>
            <td style="border-bottom: solid 1px #000000; width: 10px;">&nbsp;
            </td>
            <td id="tdActivities" onclick="location.href='AdminActivities.aspx?tabindex=0'" onmouseover="this.style.border='solid 1px #000000'; this.style.backgroundColor='#FFFFFF';" onmouseout="this.style.border='solid 1px #000000'; this.style.backgroundColor='#B0C4DE';" style="color: #000080; font-family: Tahoma,Sans-Serif; font-weight: bold; font-size: 10pt; background-color: #B0C4DE; cursor: pointer; border: solid 1px #000000;" runat="server">Activities
            </td>
            <td style="border-bottom: solid 1px #000000; width: 10px;">&nbsp;
            </td>
            <td id="tdBuildings" onclick="location.href='AdminBuildings.aspx?tabindex=1'" onmouseover="this.style.border='solid 1px #000000'; this.style.backgroundColor='#FFFFFF';" onmouseout="this.style.border='solid 1px #000000'; this.style.backgroundColor='#B0C4DE';" style="color: #000080; font-family: Tahoma,Sans-Serif; font-weight: bold; font-size: 10pt; background-color: #B0C4DE; cursor: pointer; border: solid 1px #000000;" runat="server">Buildings
            </td>
            <td style="border-bottom: solid 1px #000000; width: 10px;">&nbsp;
            </td>
            <td id="tdLabs" onclick="location.href='AdminLabs.aspx'" onmouseover="this.style.border='solid 1px #000000'; this.style.backgroundColor='#FFFFFF';" onmouseout="this.style.border='solid 1px #000000'; this.style.backgroundColor='#B0C4DE';" style="color: #000080; font-family: Tahoma,Sans-Serif; font-weight: bold; font-size: 10pt; background-color: #B0C4DE; cursor: pointer; border: solid 1px #000000;" runat="server">Laboratories
            </td>
            <td style="border-bottom: solid 1px #000000; width: 10px;">&nbsp;
            </td>
            <td id="tdProcessTechs" onclick="location.href='AdminProcessTechs.aspx'" onmouseover="this.style.border='solid 1px #000000'; this.style.backgroundColor='#FFFFFF';" onmouseout="this.style.border='solid 1px #000000'; this.style.backgroundColor='#B0C4DE';" style="color: #000080; font-family: Tahoma,Sans-Serif; font-weight: bold; font-size: 10pt; background-color: #B0C4DE; cursor: pointer; border: solid 1px #000000; white-space: nowrap;" runat="server">Process Technologies
            </td>
            <td style="border-bottom: solid 1px #000000; width: 10px;">&nbsp;
            </td>
            <td id="tdResources" onclick="location.href='AdminResources.aspx'" onmouseover="this.style.border='solid 1px #000000'; this.style.backgroundColor='#FFFFFF';" onmouseout="this.style.border='solid 1px #000000'; this.style.backgroundColor='#B0C4DE';" style="color: #000080; font-family: Tahoma,Sans-Serif; font-weight: bold; font-size: 10pt; background-color: #B0C4DE; cursor: pointer; border: solid 1px #000000;" runat="server">Resources
            </td>
            <td style="border-bottom: solid 1px #000000; width: 10px;">&nbsp;
            </td>
            <td id="tdProperties" onclick="location.href='AdminProperties.aspx'" onmouseover="this.style.border='solid 1px #000000'; this.style.backgroundColor='#FFFFFF';" onmouseout="this.style.border='solid 1px #000000'; this.style.backgroundColor='#B0C4DE';" style="color: #000080; font-family: Tahoma,Sans-Serif; font-weight: bold; font-size: 10pt; background-color: #B0C4DE; cursor: pointer; border: solid 1px #000000; white-space: nowrap" runat="server">Scheduler Properties
            </td>
            <td style="border-bottom: solid 1px #000000; width: 100%;">&nbsp;
            </td>
        </tr>
    </table>
</div>--%>
