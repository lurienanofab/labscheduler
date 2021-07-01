<%@ Page Title="Configuration" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ResourceConfig.aspx.vb" Inherits="LabScheduler.Pages.ResourceConfig" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="lnf" Assembly="LNF.Web.Scheduler" Namespace="LNF.Web.Scheduler.Controls" %>
<%@ Register TagPrefix="uc" TagName="NumericBox" Src="~/UserControls/NumericBox.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" type="text/css" href="scripts/process-info/process-info.css" />
    <style>
        .process-info-table td {
            padding: 5px;
        }

        .tt-menu {
            background-color: #fff;
            width: 100%;
            margin-top: 3px;
            padding: 8px 0;
            background-color: #fff;
            border: 1px solid #ccc;
            border: 1px solid rgba(0, 0, 0, 0.2);
            border-radius: 8px;
            box-shadow: 0 5px 10px rgba(0, 0, 0, 0.2);
        }

            .tt-menu .tt-suggestion {
                text-align: left;
                padding: 3px 20px;
                line-height: 24px;
                cursor: pointer;
            }

                .tt-menu .tt-suggestion:hover {
                    background-color: #eee;
                }

        .alert {
            margin-bottom: 0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <lnf:ResourceTabMenu runat="server" ID="ResourceTabMenu1" SelectedIndex="4" />

    <input type="hidden" runat="server" id="hidResourceID" class="resource-id" />

    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Modify Resource</h3>
        </div>
        <div class="panel-body">
            <div class="form-horizontal">
                <div class="form-group">
                    <label class="col-sm-2 control-label">Resource Name *</label>
                    <div class="col-sm-4">
                        <asp:TextBox runat="server" ID="txtResourceName" MaxLength="50" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Authorization Duration *</label>
                    <div class="col-sm-2">
                        <div class="input-group" style="width: 150px;">
                            <asp:TextBox runat="server" ID="txtAuthDuration" MaxLength="5" CssClass="form-control"></asp:TextBox>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">months</div>
                        </div>
                    </div>
                    <div class="col-sm-4">
                        <div class="checkbox">
                            <asp:CheckBox ID="chkAuthState" runat="server" Text="Rolling Period"></asp:CheckBox>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Reservation Fence *</label>
                    <div class="col-sm-2">
                        <div class="input-group" style="width: 150px;">
                            <asp:TextBox ID="txtFence" runat="server" MaxLength="5" CssClass="form-control fence"></asp:TextBox>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">hours</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Granularity *</label>
                    <div class="col-sm-3">
                        <div class="input-group" style="width: 150px;">
                            <asp:DropDownList runat="server" ID="ddlGranularity" AutoPostBack="True" CssClass="form-control">
                                <asp:ListItem Value="5">5</asp:ListItem>
                                <asp:ListItem Value="10">10</asp:ListItem>
                                <asp:ListItem Value="15">15</asp:ListItem>
                                <asp:ListItem Value="30">30</asp:ListItem>
                                <asp:ListItem Value="60">60</asp:ListItem>
                                <asp:ListItem Value="120">120</asp:ListItem>
                                <asp:ListItem Value="180">180</asp:ListItem>
                                <asp:ListItem Value="240">240</asp:ListItem>
                                <asp:ListItem Value="1440">1440</asp:ListItem>
                            </asp:DropDownList>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">min</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">24 Hour Offset *</label>
                    <div class="col-sm-3">
                        <div class="input-group" style="width: 150px;">
                            <asp:DropDownList runat="server" ID="ddlOffset" CssClass="form-control">
                            </asp:DropDownList>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">hours</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Min Reservation Time *</label>
                    <div class="col-sm-3">
                        <asp:DropDownList runat="server" ID="ddlMinReservTime" AutoPostBack="True" CssClass="form-control" Width="150">
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Max Reservation Time *</label>
                    <div class="col-sm-3">
                        <div class="input-group" style="width: 150px;">
                            <asp:DropDownList runat="server" ID="ddlMaxReservation" CssClass="form-control max-reservation">
                            </asp:DropDownList>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">hours</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Max Schedulable Hours *</label>
                    <div class="col-sm-2">
                        <div class="input-group" style="width: 150px;">
                            <asp:TextBox ID="txtMaxSchedulable" runat="server" MaxLength="5" CssClass="form-control max-schedulable"></asp:TextBox>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">hours</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Minimum Cancel Time *</label>
                    <div class="col-sm-2">
                        <div class="input-group" style="width: 150px;">
                            <asp:TextBox runat="server" ID="txtMinCancel" MaxLength="5" CssClass="form-control"></asp:TextBox>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">min</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Grace Period *</label>
                    <div class="col-sm-1">
                        <div class="input-group" style="width: 105px;">
                            <asp:DropDownList runat="server" ID="ddlGracePeriodHour" AutoPostBack="True" CssClass="form-control">
                            </asp:DropDownList>
                            <div class="input-group-addon" style="width: 40px; text-align: left;">hr</div>
                        </div>
                    </div>
                    <div class="col-sm-1">
                        <div class="input-group" style="width: 105px;">
                            <asp:DropDownList runat="server" ID="ddlGracePeriodMin" CssClass="form-control">
                            </asp:DropDownList>
                            <div class="input-group-addon" style="width: 40px; text-align: left;">min</div>
                        </div>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">AutoEnd *</label>
                    <div class="col-sm-2">
                        <div class="input-group" style="width: 150px;">
                            <asp:TextBox runat="server" ID="txtAutoEnd" MaxLength="5" CssClass="form-control"></asp:TextBox>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">min</div>
                        </div>
                    </div>
                    <div class="col-sm-8">
                        <p class="form-control-static">(set to -1 to disable auto-end action for this tool)</p>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Unload Time *</label>
                    <div class="col-sm-2">
                        <div class="input-group" style="width: 150px;">
                            <asp:TextBox runat="server" ID="txtUnload" MaxLength="5" CssClass="form-control"></asp:TextBox>
                            <div class="input-group-addon" style="width: 60px; text-align: left;">min</div>
                        </div>
                    </div>
                    <div class="col-sm-8">
                        <p class="form-control-static">(set to -1 to disable unload action for this tool)</p>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Image</label>
                    <div class="col-sm-4">
                        <div class="input-group">
                            <span class="input-group-btn">
                                <span class="lnf btn btn-default btn-file">Choose File
                                    <input type="file" runat="server" id="fileIcon" name="fileIcon" class="bootstrap-file" />
                                </span>
                            </span>
                            <input type="text" class="form-control" readonly="readonly" />
                        </div>
                        <asp:PlaceHolder runat="server" ID="phIcon">
                            <div style="margin-top: 10px;">
                                <asp:Image runat="server" ID="imgIcon" BorderStyle="NotSet" />
                            </div>
                        </asp:PlaceHolder>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Description</label>
                    <div class="col-sm-4">
                        <asp:TextBox runat="server" ID="txtDesc" Columns="40" Rows="5" TextMode="MultiLine" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-sm-2 control-label">Wiki Page URL</label>
                    <div class="col-sm-4">
                        <asp:TextBox runat="server" ID="txtWikiPageUrl" CssClass="form-control"></asp:TextBox>
                    </div>
                </div>
                <div class="form-group">
                    <div class="col-sm-offset-2 col-sm-10">
                        <div id="divErrMsg" style="color: #ff0000; font-weight: bold; margin-bottom: 10px;"></div>
                        <asp:Button runat="server" ID="btnSubmit" CssClass="lnf btn btn-default" Text="Modify Resource" OnClientClick="return resValidate()"></asp:Button>
                        <input type="reset" value="Reset" class="lnf btn btn-default" />
                    </div>
                </div>
            </div>
            <asp:PlaceHolder runat="server" ID="phMessage"></asp:PlaceHolder>
        </div>
    </div>

    <asp:PlaceHolder runat="server" ID="phProcessInfoMessage"></asp:PlaceHolder>

    <div class="lnf panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Process Info</h3>
        </div>
        <div class="panel-body">
            <div class="alert alert-warning"><strong>Please Note:</strong> Changes to process info take effect immediately.</div>
            <div runat="server" id="divProcessInfo" class="process-info-config">
                <img src="//ssel-apps.eecs.umich.edu/static/images/ajax-loader-4.gif" /> Loading...
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="//ssel-apps.eecs.umich.edu/static/lib/handlebars/handlebars.runtime.min-v4.7.7.js"></script>
    <script src="scripts/process-info/templates/processinfo.precompiled.js"></script>
    <script src="scripts/process-info/templates/processinfoline.precompiled.js"></script>
    <script src="scripts/process-info/process-info.js"></script>
    <script src="scripts/bootstrap-file.js"></script>
    <script>
        $(".modify-resource-modal").modal("show");
        var pinfoConfig = $(".process-info-config").processInfo();
    </script>
</asp:Content>
