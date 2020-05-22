Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Index
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then

                If Request.QueryString("ClearSession") = "1" Then
                    Session.Abandon()
                    Response.Redirect("~/index.aspx", True)
                End If

                ' If client logged in from a Kiosk (or is in a lab), then display My Reservations page
                If Helper.ClientInLab() AndAlso Not Request.QueryString.ToString().Contains("Home") Then
                    Response.Redirect("~/UserReservations.aspx", True)
                End If

                litDisplayName.Text = CurrentUser.DisplayName
            End If
        End Sub
    End Class
End Namespace