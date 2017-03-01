'Copyright 2017 University of Michigan

'Licensed under the Apache License, Version 2.0 (the "License");
'you may Not use this file except In compliance With the License.
'You may obtain a copy Of the License at

'http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law Or agreed To In writing, software
'distributed under the License Is distributed On an "AS IS" BASIS,
'WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'See the License For the specific language governing permissions And
'limitations under the License.

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