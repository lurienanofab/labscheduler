Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Public Class LabLocation
    Inherits SchedulerPage
    '
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
        lblLabLocationPath.Text = _lab.BuildingName + " > " + _lab.LabDisplayName + " > "
        lblLabLocationName.Text = _location.LocationName
    End Sub

    Private Sub LoadReservationView()
        rvReserv.LabLocationID = _location.LabLocationID
        rvReserv.LabID = _location.LabID
    End Sub

    Private Sub GetLabLocation()
        _location = Provider.Scheduler.LabLocation.GetLabLocation(GetLabLocationID())

        If _location Is Nothing Then
            Throw New Exception($"Cannot find LabLocation with LabLocationID {GetLabLocationID()}")
        End If

        _lab = Provider.Scheduler.Resource.GetLab(_location.LabID)

        If _lab Is Nothing Then
            Throw New Exception($"Cannot find Lab with LabID {_location.LabID}")
        End If
    End Sub

    Private Function GetLabLocationID() As Integer
        If String.IsNullOrEmpty(Request.QueryString("LocationPath")) Then
            Throw New Exception("Missing required querystring parameter: LocationPath")
        Else
            Return LocationPathInfo.Parse(Request.QueryString("LocationPath")).LabLocationID
        End If
    End Function
End Class