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

Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ResourceContact
        Inherits SchedulerPage

        Public Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If PathInfo.Current.ResourceID = 0 Then
                Response.Redirect("~/")
            End If
            If Not Page.IsPostBack Then
                Helpdesk1.ResourceID = PathInfo.Current.ResourceID
            End If
        End Sub

    End Class
End Namespace