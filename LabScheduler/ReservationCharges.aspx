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

<%@ Page Title="Reservation Charges" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="ReservationCharges.aspx.vb" Inherits="LabScheduler.Pages.ReservationCharges" %>

<%@ Register TagName="NumericBox" TagPrefix="uc" Src="~/UserControls/NumericBox.ascx" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="styles/main.css?v=20161220" />
    <style>
        .tbl td {
            padding: 10px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <table class="tbl" border="0">
        <tr>
            <td style="width: 100px;">Charge Multiplier</td>
            <td style="width: 200px;">
                <uc:NumericBox ID="txtChargeMultiplier" runat="server" PositiveNumber="true"></uc:NumericBox>
            </td>
        </tr>
        <tr id="trLateCharge" runat="server">
            <td>Apply Late Charges</td>
            <td>
                <asp:CheckBox ID="chkApplyLateChargePenalty" runat="server"></asp:CheckBox>
            </td>
        </tr>
        <tr>
            <td>&nbsp;</td>
            <td>
                <asp:Button ID="btnSubmit" Text="Forgive Charges" runat="server"></asp:Button>
            </td>
        </tr>
    </table>
    <asp:Label ID="lblErrMsg" ForeColor="Red" runat="server"></asp:Label>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
</asp:Content>