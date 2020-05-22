Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ReservationCharges
        Inherits SchedulerPage

        Private Function GetReservationID() As Integer
            Dim result As Integer = 0
            If Integer.TryParse(Request.QueryString("ReservationID"), result) Then
                Return result
            Else
                Return 0
            End If
        End Function

        Private Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
            If Not IsPostBack Then
                Dim reservationId As Integer = GetReservationID()

                If reservationId = 0 Then
                    lblErrMsg.Text = "Error: Invalid ReservationID"
                    btnSubmit.Enabled = False
                    Exit Sub
                End If

                LoadReservation(reservationId)

                btnSubmit.CommandArgument = reservationId.ToString()
                btnSubmit.Attributes.Add("onclick", "return confirm('Are you sure you want to forgive charges to this reservation?');")
            End If
        End Sub

        Private Sub BtnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
            Dim dblChargeMultiplier As Double = Double.Parse(txtChargeMultiplier.Text)

            If dblChargeMultiplier > 1 Then
                ServerJScript.JSAlert(Page, "Please enter a value less than or equal to 1 for Charge Multiplier.")
                Exit Sub
            End If

            Dim reservationId As Integer = Convert.ToInt32(btnSubmit.CommandArgument)
            Dim rsv As IReservation = Provider.Scheduler.Reservation.GetReservation(reservationId)
            Provider.Scheduler.Reservation.UpdateCharges(rsv.ReservationID, dblChargeMultiplier, chkApplyLateChargePenalty.Checked, CurrentUser.ClientID)

            Response.Write("<script language='javascript'> { self.close() }</script>")
        End Sub

        Private Sub LoadReservation(reservationId As Integer)
            ' Get Reservation Charges
            Dim rsv As IReservation = Provider.Scheduler.Reservation.GetReservation(reservationId)

            If rsv Is Nothing Then
                lblErrMsg.Text = "Error: Invalid ReservationID"
                btnSubmit.Enabled = False
            Else
                txtChargeMultiplier.Text = rsv.ChargeMultiplier.ToString()
                chkApplyLateChargePenalty.Checked = rsv.ApplyLateChargePenalty
                If rsv.ActualEndDateTime > rsv.EndDateTime Then
                    trLateCharge.Visible = True
                Else
                    trLateCharge.Visible = False
                End If
            End If
        End Sub
    End Class
End Namespace