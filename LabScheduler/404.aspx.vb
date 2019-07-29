Public Class _404
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        litMessage.Text = $"<strong>{Request.Url}</strong><br>The requested page was not found."
    End Sub

End Class