Imports LNF
Imports LNF.Models.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

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
            Dim startTime As Date = Date.Now

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

            RequestLog.Append("MasterPageScheduler.Page_Load: {0}", Date.Now - startTime)
        End Sub

        Private Function HasUtilityPriv() As Boolean
            If CurrentUser.HasPriv(ClientPrivilege.Developer) Then
                Return True
            End If

            Dim origUser As IClient = Nothing
            If Session("LogInAsOriginalUser") IsNot Nothing Then
                Dim un As String = Session("LogInAsOriginalUser").ToString()
                origUser = ServiceProvider.Current.Data.Client.GetClient(un)
                If origUser.HasPriv(ClientPrivilege.Developer) Then
                    Return True
                End If
            End If

            Return False
        End Function
    End Class
End Namespace