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

<%@ Control Language="vb" AutoEventWireup="false" CodeBehind="HelpdeskInfo.ascx.vb" Inherits="LabScheduler.UserControls.HelpdeskInfo" %>

<div class="helpdesk-info">
    <input type="hidden" runat="server" id="hidHelpdeskInfoUrl" class="helpdesk-info-url" />
    <input type="hidden" runat="server" id="hidHelpdeskInfoMultiTool" class="helpdesk-info-multitool" />
    <input type="hidden" runat="server" id="hidHelpdeskInfoResources" class="helpdesk-info-resources" />
    <div class="helpdesk-info-output">
        <div class="helpdesk-info-message">
            Retrieving helpdesk data...
        </div>
    </div>
</div>