Public Class NewReservationProcessInfoRedirect
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ' fetch reservtionid which is just created and create RPI data from JSON and store
        hidProcessInfoData.Value = Session("ReservationProcessInfoJsonData").ToString()
        hidReservationID.Value = Request.QueryString("ReservationID")
        hidRedirectPath.Value = Session("ReturnTo").ToString()
    End Sub

    Private Sub RedirectOnFinish()
        Dim redirectUrl As String = Session("ReturnTo").ToString()
        Response.Redirect(redirectUrl, False)
    End Sub

End Class