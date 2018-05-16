Imports LNF.Scheduler.Data
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ResourceClientList
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not IsPostBack Then
                LoadResourceClients()
            End If
        End Sub

        Private Sub LoadResourceClients()
            If Request.QueryString("ResourceID") Is Nothing Then Exit Sub

            Dim resourceId As Integer = Convert.ToInt32(Request.QueryString("ResourceID"))

            Dim clientList = ResourceClientData.SelectClientList(resourceId)
            dgRC.DataSource = clientList
            dgRC.DataBind()
        End Sub

    End Class
End Namespace