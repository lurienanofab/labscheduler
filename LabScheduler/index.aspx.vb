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

Imports LabScheduler.AppCode
Imports LNF.Cache
Imports LNF.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Index
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                litDisplayName.Text = CurrentUser.DisplayName

                ' If client logged in from a Kiosk (or is in a lab), then display My Reservations page
                If CacheManager.Current.IsOnKiosk() Then
                    HttpContext.Current.Response.Redirect("~/UserReservations.aspx")
                End If
            End If
        End Sub
    End Class
End Namespace