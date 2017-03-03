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
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ProcessTech
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim pt As ProcessTechModel = PathInfo.Current.GetProcessTech()

            If Not Page.IsPostBack Then
                lblDate.Text = Request.GetCurrentDate().ToLongDateString()
                LoadProcessTech(pt)
                LoadReservationView(pt)
                CacheManager.Current.CurrentUserState.AddAction("Viewing Process Tech page: {0}", pt.ProcessTechName)
            Else
                rvReserv.ProcessTechID = pt.ProcessTechID
                rvReserv.LabID = pt.LabID
            End If

            SetCurrentView(ViewType.ProcessTechView)
        End Sub

        Private Sub LoadProcessTech(pt As ProcessTechModel)
            If pt IsNot Nothing Then
                lblProcessTechPath.Text = pt.BuildingName + " > " + pt.LabDisplayName + " > "
                lblProcessTechName.Text = pt.ProcessTechName
            End If
        End Sub

        Private Sub LoadReservationView(pt As ProcessTechModel)
            rvReserv.ProcessTechID = pt.ProcessTechID
            rvReserv.LabID = pt.LabID
        End Sub
    End Class
End Namespace