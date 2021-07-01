Imports LNF.Feeds
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports LNF.Web.Scheduler.TreeView

Namespace Pages
    Public Class ResourceDayWeek
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim sw As Stopwatch = Stopwatch.StartNew()

            Helper.AppendLog($"ResourceDayWeek.Page_Load: Started...")

            Dim res As IResource = GetCurrentResource()

            Dim resourceId As Integer
            If res IsNot Nothing Then
                resourceId = res.ResourceID
            End If

            Helper.AppendLog($"ResourceDayWeek.Page_Load: res is {If(res Is Nothing, "null", "not null")}, resourceId = {resourceId}")

            If Not IsPostBack Then
                If res IsNot Nothing Then
                    'Initialize the resource object
                    LoadReservationView(res)
                Else
                    Response.Redirect("~")
                End If
            Else
                If res IsNot Nothing Then
                    ReservationView1.Resource = res
                Else
                    Response.Redirect("~")
                End If
            End If

            Helper.AppendLog($"ResourceDayWeek.Page_Load: Completed in {sw.Elapsed.TotalSeconds:0.0000} seconds")
            sw.Stop()
        End Sub

        Private Sub LoadReservationView(res As IResource)
            If res.IsSchedulable Then
                'Initialize the ReservationView UserControl
                Dim index As Integer
                Dim view As ViewType

                If Integer.TryParse(Request.QueryString("TabIndex"), index) Then
                    view = CType(index, ViewType)
                Else
                    view = GetCurrentView()
                End If

                ' view must be either DayView or WeekView on this page
                If view <> ViewType.DayView AndAlso view <> ViewType.WeekView Then
                    'get either the most recently selected DayView/WeekView or the default setting
                    view = GetDayViewOrWeekView()
                    SetCurrentView(view)
                End If

                ' Track the current view
                SetCurrentView(view)

                ' Need to track this separately from CurrentView
                SetDayViewOrWeekView(view)

                ResourceTabMenu1.SelectedIndex = view
                ReservationView1.View = view
                ReservationView1.Resource = res
                txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, "all", res.ResourceID.ToString(), "tool-reservations", Request.Url)

                Dim treeView As SchedulerResourceTreeView = Helper.CurrentResourceTreeView()
                Dim selectedNode As ResourceNode = treeView.FindResourceNode(ContextBase.Request.SelectedPath())
                If selectedNode IsNot Nothing AndAlso selectedNode.State <> ResourceState.Online Then
                    phResourceToolTip.Visible = True
                    panResourceToolTip.CssClass = "resource-tool-tip " + selectedNode.CssClass
                    litResourceToolTip.Text = selectedNode.ToolTip
                End If

            Else
                Redirect("ResourceClients.aspx")
            End If
        End Sub

        Private Sub SetDayViewOrWeekView(view As ViewType)
            If view = ViewType.DayView OrElse view = ViewType.WeekView Then
                Session("DayViewOrWeekView") = view
            Else
                Throw New ArgumentException("The argument value must be either ViewType.DayView or ViewType.WeekView", "view")
            End If
        End Sub

        ''' <summary>
        ''' Tracks which view was last used. So when the current view is ViewType.UserView or ViewType.ProcessTechView we know which view to display when a tool is selected from the tree.
        ''' </summary>
        Private Function GetDayViewOrWeekView() As ViewType
            If Session("DayViewOrWeekView") Is Nothing Then
                Dim defval As ViewType = Helper.GetClientSetting().GetDefaultView()
                If defval = ViewType.DayView OrElse defval = ViewType.WeekView Then
                    Session("DayViewOrWeekView") = defval
                Else
                    ' if all else fails
                    Session("DayViewOrWeekView") = ViewType.DayView
                End If
            End If

            Return CType(Session("DayViewOrWeekView"), ViewType)
        End Function
    End Class
End Namespace