Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports repo = LNF.Repository.Scheduler

Namespace Pages
    Public Class ReservationRunNotes
        Inherits SchedulerPage

        Private Function GetReservationID() As Integer
            If String.IsNullOrEmpty(Request.QueryString("ReservationID")) Then
                Throw New Exception("QueryString variable ReservationID is missing.")
            Else
                Return Convert.ToInt32(Request.QueryString("ReservationID"))
            End If
        End Function

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not IsPostBack Then
                ' Get Reservation + Resource
                Dim reservationId As Integer = GetReservationID()
                Dim rsv As repo.Reservation = DA.Scheduler.Reservation.Single(reservationId)

                If rsv Is Nothing Then
                    Throw New Exception(String.Format("Cannot find a Reservation with ReservationID = {0}", reservationId))
                End If

                ' Initialize controls
                litResourceName.Text = rsv.Resource.ResourceName
                litBeginTime.Text = rsv.BeginDateTime.ToString()
                litEndTime.Text = rsv.EndDateTime.ToString()
                txtNotes.Text = rsv.Notes
                hypCancel.NavigateUrl = GetReturnFromNotes()
            End If
        End Sub

        Protected Sub btnSubmit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSubmit.Click
            Dim notes As String = Server.HtmlDecode(txtNotes.Text)
            Dim rsv As repo.Reservation = DA.Scheduler.Reservation.Single(GetReservationID())
            rsv.UpdateNotes(notes)
            ReturnFromNotes()
        End Sub

        Private Sub ReturnFromNotes()
            Response.Redirect(GetReturnFromNotes())
        End Sub

        Private Function GetReturnFromNotes() As String
            Dim view As ViewType = GetCurrentView()
            Return SchedulerUtility.GetReservationViewReturnUrl(view)
        End Function
    End Class
End Namespace