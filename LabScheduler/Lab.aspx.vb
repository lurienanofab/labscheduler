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
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Lab
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            LoadLab(PathInfo.Current.LabID)
        End Sub

        Private Sub LoadLab(labId As Integer)
            Dim lab As LabModel = CacheManager.Current.GetLab(labId)
            If lab IsNot Nothing Then
                lblLabPath.Text = lab.BuildingName + " > "
                lblLabName.Text = lab.LabDisplayName
                lblDescription.Text = lab.Description
                UploadFileUtility.DisplayImage(imgPicture, "Lab", lab.LabID.ToString())
            End If

            rptResources.DataSource = ResourceListItem.List(lab.BuildingID).Where(Function(x) x.LabID = lab.LabID).OrderBy(Function(x) x.ProcessTechName)
            rptResources.DataBind()
        End Sub

    End Class
End Namespace