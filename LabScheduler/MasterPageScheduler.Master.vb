Imports LNF.Data
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports LNF.Web.Scheduler.TreeView

Namespace Pages
    Public Class MasterPageScheduler
        Inherits SchedulerMasterPage

        Public Overrides ReadOnly Property ShowMenu As Boolean
            Get
                If Request.QueryString("menu") = "1" Then
                    Return True
                Else
                    Dim result As Boolean
                    If Boolean.TryParse(ConfigurationManager.AppSettings("ShowMenu"), result) Then
                        Return result
                    Else
                        Return False
                    End If
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property AddScripts As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property AddStyles As Boolean
            Get
                Return False
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim sw As Stopwatch = Stopwatch.StartNew()

            CheckSession()

            If Not String.IsNullOrEmpty(Request.QueryString("error")) Then
                Throw New Exception(Request.QueryString("error"))
            End If

            If Not IsPostBack Then
                hypAdmin.NavigateUrl = String.Format("~/AdminActivities.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
                hypMyReservations.NavigateUrl = String.Format("~/UserReservations.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
                hypReservationHistory.NavigateUrl = String.Format("~/ReservationHistory.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
                hypPreference.NavigateUrl = String.Format("~/Preference.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
                hypContact.NavigateUrl = String.Format("~/Contact.aspx?AdminOnly=1&Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
                hypFDT.NavigateUrl = String.Format("~/ReservationFacilityDownTime.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())

                phAdmin.Visible = CurrentUser.HasPriv(ClientPrivilege.Administrator)
                phFDT.Visible = CurrentUser.HasPriv(ClientPrivilege.Staff)
                phUtility.Visible = HasUtilityPriv()
            End If

            ResourceTreeView1.SelectedPath = ContextBase.Request.SelectedPath().ToString()
            ResourceTreeView1.View = Helper.CurrentResourceTreeView()

            phLocations.Visible = False
            If ShowLabLocations() Then
                Dim locationTree As SchedulerResourceTreeView = Helper.CurrentLocationTreeView()
                If locationTree.Root.Count > 0 Then
                    phLocations.Visible = True
                    ResourceTreeView2.SelectedPath = LocationPathInfo.Parse(Request.QueryString("LocationPath")).ToString()
                    ResourceTreeView2.View = Helper.CurrentLocationTreeView()
                End If
            End If

            Dim elapsed As TimeSpan = sw.Elapsed
            sw.Stop()

            RequestLog.Append("MasterPageScheduler.Page_Load: {0}", elapsed)
            'litMasterTimer.Text = $"<div>MasterPageScheduler.Page_Load: {elapsed}</div>"
        End Sub

        Private Function ShowLabLocations() As Boolean
            Return Boolean.Parse(ConfigurationManager.AppSettings("ShowLabLocations"))
        End Function

        Private Function HasUtilityPriv() As Boolean
            If CurrentUser.HasPriv(ClientPrivilege.Developer) Then
                Return True
            End If

            Dim origUser As IClient = Nothing
            If Session("LogInAsOriginalUser") IsNot Nothing Then
                Dim un As String = Session("LogInAsOriginalUser").ToString()
                origUser = Provider.Data.Client.GetClient(un)
                If origUser.HasPriv(ClientPrivilege.Developer) Then
                    Return True
                End If
            End If

            Return False
        End Function

        Private Sub CheckSession()
            If Session("UserName") IsNot Nothing Then
                If Session("UserName").ToString() <> Page.User.Identity.Name Then
                    Session.Clear()
                    Session("UserName") = Page.User.Identity.Name
                End If
            Else
                Session("UserName") = Page.User.Identity.Name
            End If
        End Sub
    End Class
End Namespace