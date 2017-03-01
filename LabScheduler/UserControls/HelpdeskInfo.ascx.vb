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

Imports LNF
Imports LNF.Cache
Imports LNF.Models.Scheduler
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
                    Dim resList As IList(Of ResourceModel) = CacheManager.Current.Resources().Where(Function(x) Resources.Contains(x.ResourceID)).ToList()
                    For Each res As ResourceModel In resList
                        list.Add(New With {.id = res.ResourceID, .name = res.ResourceName})
                    Next
                End If
            Else
                If PathInfo.Current.ResourceID > 0 Then
                    Dim res As ResourceModel = PathInfo.Current.GetResource()
                    If res IsNot Nothing Then
                        list.Add(New With {.id = res.ResourceID, .name = res.ResourceName})
                    End If
                End If
            End If

            Return Providers.Serialization.Json.SerializeObject(list)
        End Function
    End Class
End Namespace