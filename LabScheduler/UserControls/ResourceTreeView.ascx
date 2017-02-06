<%@ Control Language="C#" AutoEventWireup="true" Inherits="LNF.Web.Scheduler.Controls.ResourceTreeView" %>

<%@ Import Namespace="LNF.Web.Scheduler.TreeView" %>

<div class="treeview">
    <input type="hidden" runat="server" id="hidSelectedPath" class="selected-path" />
    <input type="hidden" runat="server" id="hidPathDelimiter" class="path-delimiter" />
    <asp:Repeater runat="server" ID="rptBuilding" OnItemDataBound="rptBuilding_ItemDataBound">
        <HeaderTemplate>
            <ul class="root buildings">
        </HeaderTemplate>
        <ItemTemplate>
            <li data-id='<%#Eval("ID")%>' data-value='<%#Eval("Value")%>' class='<%#GetNodeCssClass((ITreeItem)Container.DataItem, "branch")%>'>
                <div class="node-text" title='<%#Eval("ToolTip")%>'>
                    <table class="node-text-table">
                        <tr>
                            <td class="node-text-clickarea">&nbsp;</td>
                            <td class='<%# Eval("CssClass")%>'>
                                <%#GetImage((ITreeItem)Container.DataItem)%>
                                <a href='<%#GetNodeUrl((ITreeItem)Container.DataItem)%>'><%#Eval("Name")%></a>
                            </td>
                        </tr>
                    </table>
                </div>
                <asp:Repeater runat="server" ID="rptLab" OnItemDataBound="rptLab_ItemDataBound">
                    <HeaderTemplate>
                        <ul class="child labs">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li data-id='<%#Eval("ID")%>' data-value='<%#Eval("Value")%>' class='<%#GetNodeCssClass((ITreeItem)Container.DataItem, "branch")%>'>
                            <div class="node-text" title='<%#Eval("ToolTip")%>'>
                                <table class="node-text-table">
                                    <tr>
                                        <td class="node-text-clickarea">&nbsp;</td>
                                        <td class='<%# Eval("CssClass")%>'>
                                            <%#GetImage((ITreeItem)Container.DataItem)%>
                                            <a href='<%#GetNodeUrl((ITreeItem)Container.DataItem)%>'><%#Eval("Name")%></a>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                            <asp:Repeater runat="server" ID="rptProcessTech" OnItemDataBound="rptProcessTech_ItemDataBound">
                                <HeaderTemplate>
                                    <ul class="child proctechs">
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <li data-id='<%#Eval("ID")%>' data-value='<%#Eval("Value")%>' class='<%#GetNodeCssClass((ITreeItem)Container.DataItem, "branch")%>'>
                                        <div class="node-text" title='<%#Eval("ToolTip")%>'>
                                            <table class="node-text-table">
                                                <tr>
                                                    <td class="node-text-clickarea">&nbsp;</td>
                                                    <td class='<%# Eval("CssClass")%>'>
                                                        <%#GetImage((ITreeItem)Container.DataItem)%>
                                                        <a href='<%#GetNodeUrl((ITreeItem)Container.DataItem)%>'><%#Eval("Name")%></a>
                                                    </td>
                                                </tr>
                                            </table>
                                        </div>
                                        <asp:Repeater runat="server" ID="rptResource">
                                            <HeaderTemplate>
                                                <ul class="child resources">
                                            </HeaderTemplate>
                                            <ItemTemplate>
                                                <li data-id='<%#Eval("ID")%>' data-value='<%#Eval("Value")%>' class='<%#GetNodeCssClass((ITreeItem)Container.DataItem, "leaf")%>'>
                                                    <div class="node-text" title='<%#Eval("ToolTip")%>'>
                                                        <table class="node-text-table">
                                                            <tr>
                                                                <td class="node-text-clickarea">&nbsp;</td>
                                                                <td class='<%# Eval("CssClass")%>'>
                                                                    <%#GetImage((ITreeItem)Container.DataItem)%>
                                                                    <a href='<%#GetNodeUrl((ITreeItem)Container.DataItem)%>'><%#Eval("Name")%></a>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </div>
                                                </li>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                </ul>
                                            </FooterTemplate>
                                        </asp:Repeater>
                                    </li>
                                </ItemTemplate>
                                <FooterTemplate>
                                    </ul>
                                </FooterTemplate>
                            </asp:Repeater>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </li>
        </ItemTemplate>
        <FooterTemplate>
            </ul>
        </FooterTemplate>
    </asp:Repeater>
</div>
