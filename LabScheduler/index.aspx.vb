Imports LabScheduler.AppCode
Imports LNF.Cache
Imports LNF.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Index
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                litDisplayName.Text = CurrentUser.DisplayName

                ' If client logged in from a Kiosk (or is in a lab), then display My Reservations page
                If CacheManager.Current.IsOnKiosk() Then
                    HttpContext.Current.Response.Redirect("~/UserReservations.aspx")
                End If
            End If
        End Sub
    End Class
End Namespace