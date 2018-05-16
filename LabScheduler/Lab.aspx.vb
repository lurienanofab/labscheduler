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
            LoadLab(Request.SelectedPath().LabID)
        End Sub

        Private Sub LoadLab(labId As Integer)
            Dim lab As LabModel = CacheManager.Current.ResourceTree().GetLab(labId)
            If lab IsNot Nothing Then
                lblLabPath.Text = lab.BuildingName + " > "
                lblLabName.Text = lab.LabDisplayName
                lblDescription.Text = lab.LabDescription
                UploadFileUtility.DisplayImage(imgPicture, "Lab", lab.LabID.ToString())
            End If

            rptResources.DataSource = ResourceListItem.List(lab.BuildingID).Where(Function(x) x.LabID = lab.LabID).OrderBy(Function(x) x.ProcessTechName)
            rptResources.DataBind()
        End Sub

    End Class
End Namespace