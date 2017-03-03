<%@ Page Language="C#" Title="Resource Maintenance" Inherits="LNF.Web.Scheduler.Pages.ResourceMaintenance" MasterPageFile="~/MasterPageScheduler.Master" Async="true" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>

<asp:Content runat="server" ID="content1" ContentPlaceHolderID="head">
</asp:Content>

<asp:Content runat="server" ID="content2" ContentPlaceHolderID="ContentPlaceHolder1">
    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu1" SelectedIndex="5" />
    <div class="lnf panel panel-default repair">
        <div class="panel-heading">
            <h3 class="panel-title">Resource Status</h3>
        </div>
        <div class="panel-body">
            <div runat="server" id="divRepairBeginDateTime">
                <asp:Literal runat="server" ID="litRepairBeginMessage"></asp:Literal>
                <asp:Literal runat="server" ID="litRepairEndMessage"></asp:Literal>
                <hr />
            </div>
            <div class="form-horizontal">
                <div runat="server" id="divStatusOptions" class="form-group">
                    <label class="control-label col-lg-3">Resource Status *</label>
                    <div class="col-lg-3">
                        <label class="radio-inline">
                            <input runat="server" id="rdoStatusOffline" type="radio" name="status" class="repair-status offline" checked />
                            Offline
                        </label>
                        <label class="radio-inline">
                            <input runat="server" id="rdoStatusLimited" type="radio" name="status" class="repair-status limited" />
                            Limited
                        </label>
                    </div>
                </div>
                <div runat="server" id="divStatusText" class="form-group">
                    <label class="control-label col-lg-3">Resource Status *</label>
                    <div class="col-lg-6">
                        <p class="form-control-static">
                            <asp:Literal runat="server" ID="litStatus"></asp:Literal>
                        </p>
                    </div>
                </div>
                <div runat="server" id="divRepairStart" class="form-group repair-duration">
                    <label class="control-label col-lg-3">How long ago the issue occurred *</label>
                    <div class="col-lg-1">
                        <asp:TextBox ID="txtRepairStart" runat="server" MaxLength="3" CssClass="form-control repair-duration-start"></asp:TextBox>
                    </div>
                    <div class="col-lg-8">
                        <label class="radio-inline">
                            <input runat="server" id="rdoRepairStartUnitMinutes" type="radio" name="offsetUnit" class="repair-duration-start-option" value="minutes" />
                            Minutes
                        </label>
                        <label class="radio-inline">
                            <input runat="server" id="rdoRepairStartUnitHours" type="radio" name="offsetUnit" class="repair-duration-start-option" value="hours" checked />
                            Hours
                        </label>
                    </div>
                </div>
                <div runat="server" id="divRepairTime" class="form-group repair-duration">
                    <label class="control-label col-lg-3">Estimated time to repair (starting now) *</label>
                    <div class="col-lg-1">
                        <asp:TextBox ID="txtRepairTime" runat="server" MaxLength="3" CssClass="form-control repair-duration-end"></asp:TextBox>
                    </div>
                    <div class="col-lg-8">
                        <label class="radio-inline">
                            <input runat="server" id="rdoRepairTimeUnitMinutes" type="radio" name="estimateUnit" class="repair-duration-end-option" value="minutes" />
                            Minutes
                        </label>
                        <label class="radio-inline">
                            <input runat="server" id="rdoRepairTimeUnitHours" type="radio" name="estimateUnit" class="repair-duration-end-option" value="hours" checked />
                            Hours
                        </label>
                    </div>
                </div>
                <div runat="server" id="divRepairDuration" class="form-group repair-duration">
                    <label class="control-label col-lg-3">Duration</label>
                    <div class="col-lg-9">
                        <p class="form-control-static repair-duration-message"></p>
                    </div>
                </div>
                <div class="form-group">
                    <label class="control-label col-lg-3">Notes</label>
                    <div class="col-lg-4">
                        <asp:TextBox runat="server" ID="txtNotes" Rows="5" Columns="40" TextMode="MultiLine" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-lg-offset-3 col-lg-9">
                        <asp:Button runat="server" ID="btnStartRepair" Text="Start" CssClass="lnf btn btn-default" OnCommand="ResourceStatus_Command" CommandName="start"></asp:Button>
                        <asp:Button runat="server" ID="btnUpdateRepair" Text="Update" CssClass="lnf btn btn-default" OnCommand="ResourceStatus_Command" CommandName="update"></asp:Button>
                        <asp:Button runat="server" ID="btnEndRepair" Text="End Repair" CssClass="lnf btn btn-default" OnCommand="ResourceStatus_Command" CommandName="end"></asp:Button>
                        <input type="reset" runat="server" id="btnReset" value="Reset" class="lnf btn btn-default" />
                    </div>
                </div>
            </div>
            <asp:Literal runat="server" ID="litErrMsg"></asp:Literal>
        </div>
    </div>

    <div runat="server" id="divInterlockState" class="lnf panel panel-default interlock-state">
        <div class="panel-heading">
            <h3 class="panel-title">Interlock Status</h3>
        </div>
        <div class="panel-body">
            <div class="form-horizontal">
                <div class="form-group" style="min-height: 30px;">
                    <label class="control-label col-lg-3">Interlock Status</label>
                    <div class="col-lg-9">
                        <p class="form-control-static status">&nbsp;</p>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-lg-offset-3 col-lg-9">
                        <button type="button" class="lnf btn btn-default toggle-button">Toggle Interlock</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content runat="server" ID="content3" ContentPlaceHolderID="scripts">
    <script src="scripts/resourceMaintenance.js"></script>
    <script>
        $(".repair").resourceMaintenance();
    </script>
</asp:Content>
