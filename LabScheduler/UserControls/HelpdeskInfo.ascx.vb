Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace UserControls
    Public Class HelpdeskInfo
        Inherits SchedulerUserControl

        Public Property MultiTool As Boolean = False
        Public Property Resources As List(Of Integer)

        Public Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                hidHelpdeskInfoUrl.Value = "/ostclient/ajax.aspx"
                hidHelpdeskInfoMultiTool.Value = MultiTool.ToString().ToLower()
                hidHelpdeskInfoResources.Value = GetResourcesJson()
            End If
        End Sub

        Private Function GetResourcesJson() As String
            Dim list As New ArrayList()
            If MultiTool Then
                If Resources IsNot Nothing Then
                    Dim resList As IEnumerable(Of IResourceTree) = Helper.GetResourceTreeItemCollection().Resources().Where(Function(x) Resources.Contains(x.ResourceID))
                    For Each res As IResource In resList
                        list.Add(New With {.id = res.ResourceID, .name = res.ResourceName})
                    Next
                End If
            Else
                If ContextBase.Request.SelectedPath().ResourceID > 0 Then
                    Dim res As IResource = Helper.GetCurrentResource()
                    If res IsNot Nothing Then
                        list.Add(New With {.id = res.ResourceID, .name = res.ResourceName})
                    End If
                End If
            End If

            Return Provider.Utility.Serialization.Json.SerializeObject(list)
        End Function
    End Class
End Namespace