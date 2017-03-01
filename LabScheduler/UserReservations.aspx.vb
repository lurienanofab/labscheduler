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

Imports LNF.Cache
Imports LNF.Feeds
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class UserReservations
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                Dim userState As UserState = CacheManager.Current.CurrentUserState()

                lblDate.Text = userState.Date.ToLongDateString()

                If CurrentUser IsNot Nothing Then
                    litCurrentUser.Text = CurrentUser.DisplayName
                    txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, CurrentUser.UserName, "all", "user-reservations")
                    If CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                        hypRecurringPage.Visible = True
                    End If
                Else
                    litCurrentUser.Text = "[unknown user]"
                End If

                userState.AddAction("Viewing My Reservations page.")
            End If

            SetCurrentView(ViewType.UserView)
        End Sub
    End Class
End Namespace