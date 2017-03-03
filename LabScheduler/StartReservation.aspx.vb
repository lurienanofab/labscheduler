Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class StartReservation
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            Dim reservationId As Integer
            If Integer.TryParse(Request.QueryString("ReservationID"), reservationId) Then
                StartReservation(reservationId)
            Else
                litMessage.Text = "<div class=""error"">Missing ReservationID</div>"
            End If
        End Sub

        Private Sub StartReservation(reservationId As Integer)
            litMessage.Text = String.Empty
            rptStartReservation.DataSource = New Object() {New With {.ReservationID = reservationId}}
            rptStartReservation.DataBind()
        End Sub
    End Class

End Namespace