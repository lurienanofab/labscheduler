Imports LabScheduler.AppCode.DBAccess
Imports LNF
Imports LNF.Impl.Repository.Scheduler
Imports LNF.Repository
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
                Provider.Scheduler.Reservation.CancelByRecurrence(RecurrenceID, CurrentUser.ClientID)
                'ReservationDB.DeleteByRecurrenceID(RecurrenceID, Client.Current.ClientID)

                GridDataBind()
            End If
        End Sub

        Private Sub GridDataBind()
            Dim clientId As Integer = CurrentUser.ClientID
            Dim rre As IQueryable(Of ReservationRecurrence) = DA.Current.Query(Of ReservationRecurrence)()

            Dim reservations As IList(Of RecurrenceItem) = rre.Where(Function(x) x.IsActive AndAlso x.Client.ClientID = clientId).Select(Function(x) RecurrenceItem.Create(x)).ToList()

            rptRecurringReservations.DataSource = reservations
            rptRecurringReservations.DataBind()

            If reservations.Count > 0 Then
                rptRecurringReservations.Visible = True
                phNoData.Visible = False
            Else
                rptRecurringReservations.Visible = False
                phNoData.Visible = True
            End If

        End Sub

        Protected Function GetEditUrl(item As RecurrenceItem) As String
            Return String.Format("~/UserRecurringReservationEdit.aspx?Date={0:yyyy-MM-dd}&RecurrenceID={1}", ContextBase.Request.SelectedDate(), item.RecurrenceID)
        End Function
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

        Public Function GetResourceUrl(context As HttpContextBase, provider As IProvider) As String
            Dim helper As New ContextHelper(context, provider)
            Dim model As IResourceTree = helper.GetResourceTreeItemCollection().GetResourceTree(ResourceID)
            Return String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(model), context.Request.SelectedDate())
        End Function

        Public Shared Function Create(source As ReservationRecurrence) As RecurrenceItem
            Dim patternName As String

            If source.Pattern.PatternID = 1 Then
                Dim dow As String = [Enum].GetName(GetType(DayOfWeek), CType(source.PatternParam1, DayOfWeek))
                patternName = $"{source.Pattern.PatternName} ({dow})"
            Else
                Dim dom As String

                Select Case source.PatternParam2
                    Case 1
                        dom = "1st"
                    Case 2
                        dom = "2nd"
                    Case 3
                        dom = "3rd"
                    Case 4
                        dom = "4th"
                    Case Else
                        dom = "Last"
                End Select

                Dim dow As String = [Enum].GetName(GetType(DayOfWeek), CType(source.PatternParam2, DayOfWeek))
                patternName = $"{source.Pattern.PatternName} ({dom} {dow})"
            End If

            Dim result As New RecurrenceItem() With {
                .RecurrenceID = source.RecurrenceID,
                .BeginDate = source.BeginDate,
                .EndDate = source.EndDate,
                .BeginTime = source.BeginTime,
                .EndTime = source.EndTime,
                .ActivityID = source.Activity.ActivityID,
                .ActivityName = source.Activity.ActivityName,
                .ResourceID = source.Resource.ResourceID,
                .ResourceName = source.Resource.ResourceName,
                .PatternName = patternName
            }
            Return result
        End Function
    End Class
End Namespace