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
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ResourceDayWeek
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            Dim res As ResourceModel = GetCurrentResource()

            If Not IsPostBack Then
                If res IsNot Nothing Then
                    'Initialize the resource object
                    LoadReservationView(res)
                Else
                    Response.Redirect("~")
                End If

                CacheManager.Current.CurrentUserState().AddAction("Viewing Resource page: {0} [{1}]", res.ResourceName, res.ResourceID)
            Else
                If res IsNot Nothing Then
                    ReservationView1.Resource = res
                Else
                    Response.Redirect("~")
                End If
            End If
        End Sub

        Private Sub LoadReservationView(res As ResourceModel)
            If res.IsSchedulable Then
                'Initialize the ReservationView UserControl
                Dim index As Integer
                Dim view As ViewType

                If Integer.TryParse(Request.QueryString("TabIndex"), index) Then
                    view = CType(index, ViewType)
                Else
                    view = GetCurrentView()
                End If

                ' view must be either DayView or WeekView on this page
                If view <> ViewType.DayView AndAlso view <> ViewType.WeekView Then
                    'get either the most recently selected DayView/WeekView or the default setting
                    view = GetDayViewOrWeekView()
                    SetCurrentView(view)
                End If

                ' Track the current view
                SetCurrentView(view)

                ' Need to track this separately from CurrentView
                SetDayViewOrWeekView(view)

                ResourceTabMenu1.SelectedIndex = view
                ReservationView1.View = view
                ReservationView1.Resource = res
                txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, "all", res.ResourceID.ToString(), "tool-reservations")
            Else
                Response.Redirect(String.Format("~/ResourceClients.aspx?Path={0}", PathInfo.Current))
            End If
        End Sub

        Private Sub SetDayViewOrWeekView(view As ViewType)
            If view = ViewType.DayView OrElse view = ViewType.WeekView Then
                Session("DayViewOrWeekView") = view
            Else
                Throw New ArgumentException("The argument value must be either ViewType.DayView or ViewType.WeekView", "view")
            End If
        End Sub

        ''' <summary>
        ''' Tracks which view was last used. So when the current view is ViewType.UserView or ViewType.ProcessTechView we know which view to display when a tool is selected from the tree.
        ''' </summary>
        Private Function GetDayViewOrWeekView() As ViewType
            If Session("DayViewOrWeekView") Is Nothing Then
                Dim defval As ViewType = CacheManager.Current.GetClientSetting().GetDefaultViewOrDefault()
                If defval = ViewType.DayView OrElse defval = ViewType.WeekView Then
                    Session("DayViewOrWeekView") = defval
                Else
                    ' if all else fails
                    Session("DayViewOrWeekView") = ViewType.DayView
                End If
            End If

            Return CType(Session("DayViewOrWeekView"), ViewType)
        End Function
    End Class
End Namespace