Imports LNF.Cache
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ProcessTech
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim pt As ProcessTechModel = Request.SelectedPath().GetProcessTech()

            If Not Page.IsPostBack Then
                lblDate.Text = Request.SelectedDate().ToLongDateString()
                LoadProcessTech(pt)
                LoadReservationView(pt)
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