Imports LNF
Imports LNF.Data
Imports LNF.Web.Content
Imports LNF.Web.Scheduler

Public Class MasterPageBootstrap
    Inherits OnlineServicesMasterPage

    'Private _menuItems As IList(Of repo.Menu)
    Private _menu As SiteMenu

    Public Overrides ReadOnly Property ShowMenu As Boolean
        Get
            Return True
        End Get
    End Property

    Public Property UseJavascriptNavigation As Boolean

    Public Sub New()
        UseJavascriptNavigation = False
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not Page.IsPostBack Then
            If ShowMenu Then
                ' load the page menu
                _menu = New SiteMenu(CurrentUser, Nothing, Provider.LoginUrl(), Request.IsSecureConnection, Nothing)
                Dim parents As IList(Of IMenu) = _menu.Where(Function(x) x.MenuParentID = 0).ToList()
                rptMenu.DataSource = parents
                rptMenu.DataBind()
            End If

            ' handle the current date
            Dim selectedDate As Date = ContextBase.Request.SelectedDate()
            txtCurrentDate.Value = selectedDate.ToString("MM/dd/yyyy")
            hidSelectedDate.Value = selectedDate.ToString("yyyy-MM-dd")

            ' show/hide navigation links based on user privilege
            liAdminMenu.Visible = False

            If Not CurrentUser.HasPriv(ClientPrivilege.Administrator) Then
                liAdministration.Visible = False
            Else
                liAdminMenu.Visible = True
            End If

            If Not CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                liFacilityDownTime.Visible = False
            Else
                liAdminMenu.Visible = True
            End If
        End If
    End Sub

    Protected Sub RptMenu_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim liParentDropdown As UI.Control = e.Item.FindControl("liParentDropdown")
            Dim liParentLink As UI.Control = e.Item.FindControl("liParentLink")
            Dim parent As MenuItem = CType(e.Item.DataItem, MenuItem)
            Dim children As IList(Of IMenu) = _menu.Where(Function(x) x.MenuParentID = parent.MenuID).ToList()
            If children.Count > 0 Then
                liParentDropdown.Visible = True
                liParentLink.Visible = False
                Dim rptChildren As Repeater = CType(e.Item.FindControl("rptChildren"), Repeater)
                rptChildren.DataSource = children
                rptChildren.DataBind()
            Else
                liParentDropdown.Visible = False
                liParentLink.Visible = True
            End If
        ElseIf e.Item.ItemType = ListItemType.Header OrElse e.Item.ItemType = ListItemType.Footer Then
            Dim litDisplayName As Literal = CType(e.Item.FindControl("litDisplayName"), Literal)
            litDisplayName.Text = CurrentUser.DisplayName
        End If
    End Sub

    Protected Function GetMenuUrl(item As MenuItem) As String
        If String.IsNullOrEmpty(item.MenuURL) Then
            Return "#"
        End If

        Dim appServer As String

        If Request.IsSecureConnection Then
            appServer = "//" + ConfigurationManager.AppSettings("AppServer")
        Else
            appServer = "//" + ConfigurationManager.AppSettings("AppServer")
        End If

        Dim url As String = item.MenuURL.Replace("{AppServer}/", appServer)

        If (UseJavascriptNavigation) Then
            Return String.Format("javascript: menuNav('{0}');", url)
        Else
            Return url
        End If
    End Function

    Protected Function GetMenuTarget(item As MenuItem) As String
        If item.NewWindow Then
            Return "_blank"
        Else
            Return "_self"
        End If
    End Function
End Class