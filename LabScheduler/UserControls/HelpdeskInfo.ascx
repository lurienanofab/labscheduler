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