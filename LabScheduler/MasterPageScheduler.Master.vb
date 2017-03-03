Imports LNF.Cache
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
                hypAdmin.NavigateUrl = String.Format("~/AdminActivities.aspx?Date={0:yyyy-MM-dd}", Request.GetCurrentDate())
                hypMyReservations.NavigateUrl = String.Format("~/UserReservations.aspx?Date={0:yyyy-MM-dd}", Request.GetCurrentDate())
                hypReservationHistory.NavigateUrl = String.Format("~/ReservationHistory.aspx?Date={0:yyyy-MM-dd}", Request.GetCurrentDate())
                hypPreference.NavigateUrl = String.Format("~/Preference.aspx?Date={0:yyyy-MM-dd}", Request.GetCurrentDate())
                hypContact.NavigateUrl = String.Format("~/Contact.aspx?AdminOnly=1&Date={0:yyyy-MM-dd}", Request.GetCurrentDate())
                hypFDT.NavigateUrl = String.Format("~/ReservationFacilityDownTime.aspx&Date={0:yyyy-MM-dd}", Request.GetCurrentDate())

                phAdmin.Visible = CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Administrator)
                phFDT.Visible = CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Staff)
            End If

            RequestLog.Append("MasterPageScheduler.Page_Load: {0}", Date.Now - startTime)
        End Sub
    End Class
End Namespace