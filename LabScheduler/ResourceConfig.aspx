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

<%@ Page Title="Configuration" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ResourceConfig.aspx.vb" Inherits="LabScheduler.Pages.ResourceConfig" MaintainScrollPositionOnPostback="true" %>

<%@ Register TagPrefix="uc" TagName="ResourceTabMenu" Src="~/UserControls/ResourceTabMenu.ascx" %>
<%@ Register TagPrefix="uc" TagName="NumericBox" Src="~/UserControls/NumericBox.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
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
    <uc:ResourceTabMenu runat="server" ID="ResourceTabMenu1" SelectedIndex="4" />

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
            <asp:DataGrid runat="server" ID="dgProcessInfo" BorderColor="#4682B4" CellPadding="3" AutoGenerateColumns="False" ShowFooter="True" CssClass="process-info-table">
                <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                <FooterStyle BackColor="MistyRose" HorizontalAlign="Center"></FooterStyle>
                <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                <EditItemStyle HorizontalAlign="Center"></EditItemStyle>
                <Columns>
                    <asp:TemplateColumn ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="ibtnExpand" ImageUrl="~/images/expand.gif" CommandName="Expand" CommandArgument="Expand" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnUp" ImageUrl="~/images/MoveUp.gif" CommandName="MoveUp" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnDown" ImageUrl="~/images/MoveDown.gif" CommandName="MoveDown" runat="server"></asp:ImageButton>
                            <asp:DataGrid ID="dgProcessInfoLine" AutoGenerateColumns="False" runat="server" BorderColor="#4682B4" ShowFooter="True" CellPadding="3" DataKeyField="ProcessInfoLineID" OnItemDataBound="dgProcessInfoLine_ItemDataBound" OnItemCommand="dgProcessInfoLine_ItemCommand">
                                <HeaderStyle Font-Bold="True" HorizontalAlign="Center" ForeColor="White" BackColor="#336699"></HeaderStyle>
                                <FooterStyle HorizontalAlign="Center" BackColor="MistyRose"></FooterStyle>
                                <AlternatingItemStyle BackColor="AliceBlue"></AlternatingItemStyle>
                                <EditItemStyle HorizontalAlign="Center"></EditItemStyle>
                                <Columns>
                                    <asp:BoundColumn DataField="ProcessInfoLineID" ReadOnly="True" Visible="False"></asp:BoundColumn>
                                    <asp:TemplateColumn HeaderText="Param">
                                        <ItemTemplate>
                                            <asp:Label ID="lblParam" runat="server"></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtParam" Columns="20" MaxLength="50" runat="server" CssClass="parameter-name"></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="txtNewParam" Columns="20" MaxLength="50" runat="server" CssClass="parameter-name"></asp:TextBox>
                                        </FooterTemplate>
                                    </asp:TemplateColumn>
                                    <asp:TemplateColumn HeaderText="Min Value">
                                        <ItemTemplate>
                                            <asp:Label ID="lblMinVal" runat="server"></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtMinVal" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="txtNewMinVal" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                                        </FooterTemplate>
                                    </asp:TemplateColumn>
                                    <asp:TemplateColumn HeaderText="Max Value">
                                        <ItemTemplate>
                                            <asp:Label ID="lblMaxVal" runat="server"></asp:Label>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:TextBox ID="txtMaxVal" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:TextBox ID="txtNewMaxVal" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                                        </FooterTemplate>
                                    </asp:TemplateColumn>
                                    <asp:TemplateColumn>
                                        <ItemTemplate>
                                            <asp:ImageButton ID="ibtnPILEdit" ImageUrl="~/images/edit.gif" CommandName="Edit" runat="server"></asp:ImageButton>
                                            <asp:ImageButton ID="ibtnPILDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" runat="server"></asp:ImageButton>
                                        </ItemTemplate>
                                        <EditItemTemplate>
                                            <asp:ImageButton ID="ibtnPILUpdate" CommandName="Update" ImageUrl="~/images/update.gif" runat="server"></asp:ImageButton>
                                            <asp:ImageButton ID="ibtnPILCancel" CommandName="Cancel" ImageUrl="~/images/cancel.gif" runat="server"></asp:ImageButton>
                                        </EditItemTemplate>
                                        <FooterTemplate>
                                            <asp:Button ID="btnPILInsert" Text="Add" CommandName="Insert" runat="server"></asp:Button>
                                        </FooterTemplate>
                                    </asp:TemplateColumn>
                                </Columns>
                            </asp:DataGrid>
                        </ItemTemplate>
                    </asp:TemplateColumn>
                    <asp:BoundColumn DataField="ProcessInfoID" Visible="False" ReadOnly="True"></asp:BoundColumn>
                    <asp:BoundColumn DataField="Order" Visible="False" ReadOnly="True"></asp:BoundColumn>
                    <asp:TemplateColumn HeaderText="Process Info Name">
                        <ItemTemplate>
                            <asp:Label ID="lblPIName" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txtPIName" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txtNewPIName" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Param Name">
                        <ItemTemplate>
                            <asp:Label ID="lblParamName" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txtParamName" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txtNewParamName" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Value Name">
                        <ItemTemplate>
                            <asp:Label ID="lblValueName" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txtValueName" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txtNewValueName" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Special">
                        <ItemTemplate>
                            <asp:Label ID="lblSpecial" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="txtSpecial" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:TextBox ID="txtNewSpecial" Columns="10" MaxLength="50" runat="server"></asp:TextBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Allow None">
                        <ItemTemplate>
                            <asp:Label ID="lblAllowNone" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:CheckBox ID="chkAllowNone" runat="server"></asp:CheckBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:CheckBox ID="chkNewAllowNone" runat="server"></asp:CheckBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Require Value">
                        <ItemTemplate>
                            <asp:Label ID="lblRequireValue" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:CheckBox ID="chkRequireValue" runat="server"></asp:CheckBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:CheckBox ID="chkNewRequireValue" runat="server"></asp:CheckBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn HeaderText="Require Selection">
                        <ItemTemplate>
                            <asp:Label ID="lblRequireSelection" runat="server"></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:CheckBox ID="chkRequireSelection" runat="server"></asp:CheckBox>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:CheckBox ID="chkNewRequireSelection" runat="server"></asp:CheckBox>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                    <asp:TemplateColumn ItemStyle-HorizontalAlign="Center">
                        <ItemTemplate>
                            <asp:ImageButton ID="ibtnEdit" ImageUrl="~/images/edit.gif" CommandName="Edit" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnDelete" ImageUrl="~/images/delete.gif" CommandName="Delete" runat="server"></asp:ImageButton>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:ImageButton ID="ibtnUpdate" ImageUrl="~/images/update.gif" Height="15" Width="15" CommandName="Update" runat="server"></asp:ImageButton>
                            <asp:ImageButton ID="ibtnCancel" ImageUrl="~/images/cancel.gif" CommandName="Cancel" runat="server"></asp:ImageButton>
                        </EditItemTemplate>
                        <FooterTemplate>
                            <asp:Button ID="btnInsert" Text="Add" CommandName="Insert" runat="server"></asp:Button>
                        </FooterTemplate>
                    </asp:TemplateColumn>
                </Columns>
            </asp:DataGrid>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script src="scripts/typeahead.bundle.min.js"></script>
    <script src="scripts/bootstrap-file.js"></script>
    <script>
        $(".modify-resource-modal").modal("show");

        var resourceId = $(".resource-id").val();

        var pilps = new Bloodhound({
            datumTokenizer: Bloodhound.tokenizers.whitespace,
            queryTokenizer: Bloodhound.tokenizers.whitespace,
            prefetch: "ajax/resource.ashx?Command=GetProcessInfoLineParams&ResourceID=" + resourceId
        });

        $(".parameter-name").typeahead({
            "minLength": 1,
            "highlight": true
        }, {
            "name": "pilp-dataset",
            "source": pilps
        });
    </script>
</asp:Content>
