Imports LNF.Web.Scheduler.Controls

Namespace UserControls
    Public Class AdminTabMenu
        Inherits ResourceTabMenu

        Protected Overrides Function GetHeader() As String
            Return "Administration"
        End Function

        Protected Overrides Function GetTabs() As IList(Of TabItem)
            Dim tabs As New List(Of TabItem)
            tabs.Add(New TabItem() With {.CssClass = GetTabCssClass(0), .NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminActivities.aspx"), .Text = "Activities", .Visible = True})
            tabs.Add(New TabItem() With {.CssClass = GetTabCssClass(1), .NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminBuildings.aspx"), .Text = "Buildings", .Visible = True})
            tabs.Add(New TabItem() With {.CssClass = GetTabCssClass(2), .NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminLabs.aspx"), .Text = "Labs", .Visible = True})
            tabs.Add(New TabItem() With {.CssClass = GetTabCssClass(3), .NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminProcessTechs.aspx"), .Text = "Process Techs", .Visible = True})
            tabs.Add(New TabItem() With {.CssClass = GetTabCssClass(4), .NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminResources.aspx"), .Text = "Resources", .Visible = True})
            tabs.Add(New TabItem() With {.CssClass = GetTabCssClass(5), .NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminProperties.aspx"), .Text = "Scheduler Properties", .Visible = True})
            Return tabs
        End Function
    End Class
End Namespace