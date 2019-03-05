Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Repository.Scheduler

Namespace DBAccess
    Public Class ReservationRecurrenceDB
        Public Shared Function SelectRecurringByID(ByVal RecurrenceID As Integer) As ReservationRecurrence
            Dim cmd As DataCommandBase = DA.Command().Param("Action", "ByRecurrenceID").Param("RecurrenceID", RecurrenceID)

            Using dr As IDataReader = cmd.ExecuteReader("sselScheduler.dbo.procReservationRecurrenceSelect")
                Dim rr As New ReservationRecurrence()
                Dim act As New Activity()
                Dim res As New Resource()
                Dim pat As New RecurrencePattern()

                If dr.Read() Then
                    rr.RecurrenceID = Convert.ToInt32(dr("RecurrenceID"))
                    res.ResourceID = Convert.ToInt32(dr("ResourceID"))
                    rr.Client = DA.Current.Single(Of Client)(Convert.ToInt32(dr("ClientID")))
                    rr.Account = DA.Current.Single(Of Account)(Convert.ToInt32(dr("AccountID")))
                    rr.BeginTime = Convert.ToDateTime(dr("BeginTime"))
                    rr.EndTime = Convert.ToDateTime(dr("EndTime"))
                    rr.Duration = Convert.ToDouble(dr("Duration"))
                    rr.CreatedOn = Convert.ToDateTime(dr("CreatedOn"))
                    rr.AutoEnd = Convert.ToBoolean(dr("AutoEnd"))
                    pat.PatternID = Convert.ToInt32(dr("PatternID"))
                    rr.PatternParam1 = Convert.ToInt32(dr("PatternParam1"))
                    rr.PatternParam2 = Convert.ToInt32(dr("PatternParam2"))
                    rr.BeginDate = Convert.ToDateTime(dr("BeginDate"))
                    rr.EndDate = If(dr("EndDate") Is DBNull.Value, Nothing, Convert.ToDateTime(dr("EndDate")))
                    rr.IsActive = Convert.ToBoolean(dr("IsActive"))

                    act.ActivityID = Convert.ToInt32(dr("ActivityID"))
                    rr.Activity = act
                    'rr.RecurrencePattern = pat
                    rr.Pattern = pat

                    rr.Resource = res

                    Return rr
                Else
                    Return Nothing
                End If
            End Using
        End Function

        ' Selects all recurring reservations belonging to this reservation series
        Public Function SelectSeries(ByVal ReservationID As Integer) As DataTable
            Return DA.Command().Param(New With {ReservationID}).FillDataTable("sselScheduler.dbo.procReservationRecurrenceSelect")
        End Function

        ' Inserts a new reservation recurrence
        Public Function Insert(ByVal ReservationID As Integer, ByVal RecurrenceID As Integer) As Integer
            Return DA.Command().Param(New With {ReservationID, RecurrenceID}).ExecuteNonQuery("sselScheduler.dbo.procReservationRecurrenceSelect").Value
        End Function

        ' Deletes a series of reservations starting from the start date
        Public Shared Function DeleteRecurringSeries(ByVal RecurrenceID As Integer) As Integer
            Return DA.Command().Param(New With {.Action = "ByRecurrenceID", RecurrenceID}).ExecuteNonQuery("sselScheduler.dbo.procReservationRecurrenceDelete").Value
        End Function

        ' Delete a specific reservation recurrence
        Public Function Delete(ByVal ReservationID As Integer, ByVal RecurrenceID As Integer) As Integer
            Return DA.Command().Param(New With {.Action = "Delete", ReservationID, RecurrenceID}).ExecuteNonQuery("sselScheduler.dbo.procReservationRecurrenceSelect").Value
        End Function

        Public Shared Function GetRecurringReservationByUserID(ByVal ClientID As Integer) As DataTable
            Return DA.Command().Param(New With {.Action = "ByClientID", ClientID}).FillDataTable("sselScheduler.dbo.procReservationRecurrenceSelect")
        End Function
    End Class
End Namespace