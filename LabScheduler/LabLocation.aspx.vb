Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Public Class LabLocation
    Inherits SchedulerPage

    Private _location As ILabLocation
    Private _lab As ILab

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        GetLabLocation()

        If Not Page.IsPostBack Then
            lblDate.Text = ContextBase.Request.SelectedDate().ToLongDateString()
            LoadLabLocation()
            LoadReservationView()
        Else
            LoadReservationView()
        End If

        SetCurrentView(ViewType.LocationView)
    End Sub

    Private Sub LoadLabLocation()
        lblLabLocationPath.Text = _lab.BuildingName + " > " + _lab.LabDisplayName

        If _location IsNot Nothing Then
            lblLabLocationPath.Text += " > "
            lblLabLocationName.Text = _location.LocationName
        End If
    End Sub

    Private Sub LoadReservationView()
        Dim labLocationId As Integer

        If _location IsNot Nothing Then
            labLocationId = _location.LabLocationID
        Else
            labLocationId = 0
        End If

        rvReserv.LabLocationID = labLocationId
        rvReserv.LabID = _lab.LabID
    End Sub

    Private Sub GetLabLocation()
        'Need to handle the case when there is a Lab but no LabLocation. See LNF.Web.Scheduler.SchedulerUtility.GetLocationPath

        Dim locationPath As LocationPathInfo = GetLocationPathInfo()

        Dim labLocationId As Integer = locationPath.LabLocationID

        If labLocationId > 0 Then
            _location = Provider.Scheduler.LabLocation.GetLabLocation(labLocationId)
        Else
            _location = Nothing
        End If

        _lab = Provider.Scheduler.Resource.GetLab(locationPath.LabID)

        If _lab Is Nothing Then
            Throw New Exception($"Cannot find Lab with LabID {locationPath.LabID}")
        End If
    End Sub

    Private Function GetLocationPathInfo() As LocationPathInfo
        If String.IsNullOrEmpty(Request.QueryString("LocationPath")) Then
            Throw New Exception("Missing required querystring parameter: LocationPath")
        Else
            Return LocationPathInfo.Parse(Request.QueryString("LocationPath"))
        End If
    End Function
End Class