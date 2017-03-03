Imports LabScheduler.AppCode
Imports LNF.Models.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Building
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            LoadBuilding()
        End Sub

        Private Sub LoadBuilding()
            Dim bldg As BuildingModel = PathInfo.Current.GetBuilding()
            If bldg IsNot Nothing Then
                lblBuildingName.Text = bldg.BuildingName
                lblDescription.Text = bldg.Description
                UploadFileUtility.DisplayImage(imgPicture, "Building", bldg.BuildingID.ToString())
            End If

            rptResources.DataSource = ResourceListItem.List(bldg.BuildingID)
            rptResources.DataBind()
        End Sub

    End Class
End Namespace