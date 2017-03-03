Imports LabScheduler.AppCode.DBAccess
Imports LNF.Cache
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class UserRecurringReservation
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, ByVal e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                GridDataBind()
            End If
        End Sub

        Protected Sub RecurringReservations_Command(sender As Object, e As CommandEventArgs)
            If e.CommandName = "EditMe" Then
                Dim RecurrenceID As Integer = CType(e.CommandArgument, Integer)

                Response.Redirect("~/UserRecurringReservationEdit.aspx?id=" + RecurrenceID.ToString())
            ElseIf e.CommandName = "Delete" Then
                Dim RecurrenceID As Integer = CType(e.CommandArgument, Integer)

                ReservationRecurrenceDB.DeleteRecurringSeries(RecurrenceID)

                'delete all the future reservations and keep the old ones
                DA.Scheduler.Reservation.DeleteByRecurrence(RecurrenceID, CurrentUser.ClientID)
                'ReservationDB.DeleteByRecurrenceID(RecurrenceID, Client.Current.ClientID)

                GridDataBind()
            End If
        End Sub

        Private Sub GridDataBind()
            Dim ClientID As Integer = CType(Session("ClientID"), Integer)
            Dim rre As IQueryable(Of ReservationRecurrence) = DA.Current.Query(Of ReservationRecurrence)()

            Dim reservations As IList(Of RecurrenceItem) = rre.Where(Function(x) x.IsActive AndAlso x.Client.ClientID = ClientID).Select(Function(x) RecurrenceItem.Create(x)).ToList()

            'gvRecurring.DataSource = reservations
            'gvRecurring.DataBind()
            rptRecurringReservations.DataSource = reservations
            rptRecurringReservations.DataBind()
        End Sub
    End Class

    Public Class RecurrenceItem
        Public Property RecurrenceID As Integer
        Public Property BeginDate As Date
        Public Property EndDate As Date?
        Public Property BeginTime As Date
        Public Property EndTime As Date
        Public Property ActivityID As Integer
        Public Property ActivityName As String
        Public Property ResourceID As Integer
        Public Property ResourceName As String
        Public Property PatternName As String

        Public Function GetEndDateString() As String
            If EndDate.HasValue Then
                Return EndDate.Value.ToString("MM/dd/yyyy")
            Else
                Return "Infinite"
            End If
        End Function

        Public Function GetResourceUrl() As String
            Dim model As ResourceModel = CacheManager.Current.GetResource(ResourceID)
            Return String.Format("~/ResourceDayWeek.aspx?Path={0}", PathInfo.Create(model))
        End Function

        Public Shared Function Create(source As ReservationRecurrence) As RecurrenceItem
            Dim result As New RecurrenceItem()
            result.RecurrenceID = source.RecurrenceID
            result.BeginDate = source.BeginDate
            result.EndDate = source.EndDate
            result.BeginTime = source.BeginTime
            result.EndTime = source.EndTime
            result.ActivityID = source.Activity.ActivityID
            result.ActivityName = source.Activity.ActivityName
            result.ResourceID = source.Resource.ResourceID
            result.ResourceName = source.Resource.ResourceName
            result.PatternName = source.Pattern.PatternName
            Return result
        End Function
    End Class
End Namespace