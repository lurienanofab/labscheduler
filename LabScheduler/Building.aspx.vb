Imports LabScheduler.AppCode
Imports LNF.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Building
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            LoadBuilding()
        End Sub

        Private Sub LoadBuilding()
            Dim bldg As IBuilding = Helper.GetCurrentBuilding()
            If bldg IsNot Nothing Then
                litBuildingName.Text = bldg.BuildingName
                lblDescription.Text = bldg.BuildingDescription
                UploadFileUtility.DisplayImage(imgPicture, "Building", bldg.BuildingID.ToString())
            End If

            rptResources.DataSource = Helper.GetResourceTableItemList(bldg.BuildingID)
            rptResources.DataBind()
        End Sub

    End Class
End Namespace