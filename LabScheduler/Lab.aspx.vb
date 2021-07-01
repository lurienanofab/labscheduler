Imports LabScheduler.AppCode
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports LNF.Web.Scheduler.TreeView

Namespace Pages
    Public Class Lab
        Inherits SchedulerPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim locations As Boolean = Request.QueryString("View") = "locations"

            Dim labId As Integer

            If locations Then
                Dim locationPath As LocationPathInfo = ContextBase.Request.SelectedLocationPath()
                labId = locationPath.LabID
            Else
                labId = ContextBase.Request.SelectedPath().LabID
            End If

            LoadLab(labId, locations)
        End Sub

        Private Sub LoadLab(labId As Integer, locations As Boolean)
            Dim lab As ILab = Helper.GetResourceTreeItemCollection().GetLab(labId)

            If lab IsNot Nothing Then
                litLabPath.Text = lab.BuildingName + " &gt; "
                litLabName.Text = lab.LabDisplayName
                lblDescription.Text = lab.LabDescription
                UploadFileUtility.DisplayImage(imgPicture, "Lab", lab.LabID.ToString())

                If locations Then
                    phResourcesByProcTech.Visible = False
                    phResourcesByLocation.Visible = True
                    rptResourcesByLocation.DataSource = GetResourcesByLocation(lab)
                    rptResourcesByLocation.DataBind()
                Else
                    phResourcesByProcTech.Visible = True
                    phResourcesByLocation.Visible = False
                    rptResourcesByProcTech.DataSource = Helper.GetResourceTableItemList(lab.BuildingID).Where(Function(x) x.LabID = lab.LabID).OrderBy(Function(x) x.ProcessTechName)
                    rptResourcesByProcTech.DataBind()
                End If
            End If
        End Sub

        Private Function GetResourcesByLocation(lab As ILab) As IEnumerable(Of ResourceLocationTableItem)
            Dim locationTree As LocationTreeItemCollection = Helper.GetLocationTreeItemCollection()
            Dim resources As IEnumerable(Of IResourceTree) = locationTree.Resources()
            Dim resourceLabLocations As IEnumerable(Of IResourceLabLocation) = locationTree.GetResourceLabLocations()
            Dim locations As IEnumerable(Of ILabLocation) = locationTree.GetLabLocations(lab.LabID)
            Dim result As New List(Of ResourceLocationTableItem)
            For Each loc As ILabLocation In locations
                Dim list As List(Of IResourceLabLocation) = resourceLabLocations.Where(Function(x) x.LabLocationID = loc.LabLocationID).ToList()
                Dim items As New List(Of ResourceLocationTableItem)
                For Each rll As IResourceLabLocation In list
                    Dim res As IResource = locationTree.GetResource(rll.ResourceID)
                    items.Add(CreateResourceLocationTableItem(res, loc))
                Next
                result.AddRange(items)
            Next
            Return result.OrderBy(Function(x) x.LocationName).ThenBy(Function(x) x.ResourceName).ToList()
        End Function

        Private Function CreateResourceLocationTableItem(res As IResource, loc As ILabLocation) As ResourceLocationTableItem
            Dim result As New ResourceLocationTableItem With {
                .LocationName = loc.LocationName,
                .LocationUrl = VirtualPathUtility.ToAbsolute(LocationNodeUtility.GetLocationNodeUrl(loc.LabID, loc.LabLocationID, ContextBase.Request.SelectedDate())),
                .ResourceID = res.ResourceID,
                .ResourceName = res.ResourceName,
                .ResourceUrl = VirtualPathUtility.ToAbsolute(String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), ContextBase.Request.SelectedDate()))
            }
            Return result
        End Function
    End Class

    Public Class ResourceLocationTableItem
        Public Property LocationUrl As String
        Public Property LocationName As String
        Public Property ResourceUrl As String
        Public Property ResourceID As Integer
        Public Property ResourceName As String
    End Class
End Namespace