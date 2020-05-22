Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ProcessTech
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim pt As IProcessTech = Helper.GetCurrentProcessTech()

            If Not Page.IsPostBack Then
                lblDate.Text = ContextBase.Request.SelectedDate().ToLongDateString()
                LoadProcessTech(pt)
                LoadReservationView(pt)
            Else
                rvReserv.ProcessTechID = pt.ProcessTechID
                rvReserv.LabID = pt.LabID
            End If

            SetCurrentView(ViewType.ProcessTechView)
        End Sub

        Private Sub LoadProcessTech(pt As IProcessTech)
            If pt IsNot Nothing Then
                lblProcessTechPath.Text = pt.BuildingName + " > " + pt.LabDisplayName + " > "
                lblProcessTechName.Text = pt.ProcessTechName
            End If
        End Sub

        Private Sub LoadReservationView(pt As IProcessTech)
            rvReserv.ProcessTechID = pt.ProcessTechID
            rvReserv.LabID = pt.LabID
        End Sub
    End Class
End Namespace