Imports LNF.Repository
Imports LNF.Scheduler

Namespace DBAccess
    Public Class FacilityDownTimeRes
        Public DisplayName As String
        Public ClientID As Integer
        Public BeginDateTime As Date
        Public EndDateTime As Date
    End Class

    Public Class FacilityDownTimeDB
        Public Shared Function GetFacilityDownTimeRes() As DataTable
            Return DataCommand.Create().Param(New With {.Action = "GetActiveFacilityDownTime"}).FillDataTable("sselScheduler.dbo.procReservationGroupSelect")
        End Function

        Public Shared Function GetFacilityDownTimeByGroupID(GroupID As Integer) As FacilityDownTimeRes
            Dim result As New FacilityDownTimeRes
            Using reader As ExecuteReaderResult = DataCommand.Create().Param(New With {.Action = "ByGroupID", GroupID}).ExecuteReader("sselScheduler.dbo.procReservationGroupSelect")
                If reader.Read() Then
                    result.DisplayName = reader("DisplayName").ToString()
                    result.ClientID = Convert.ToInt32(reader("ClientID"))
                    result.BeginDateTime = Convert.ToDateTime(reader("BeginDateTime"))
                    result.EndDateTime = Convert.ToDateTime(reader("EndDateTime"))
                    reader.Close()
                    Return result
                Else
                    reader.Close()
                    Return Nothing
                End If
            End Using
        End Function

        Public Shared Function CreateNew(clientId As Integer, beginDateTime As Date, endDateTime As Date) As Integer
            Dim groupId As Integer

            groupId = DataCommand.Create() _
                .Param("Action", "InsertNew") _
                .Param("GroupID", groupId, ParameterDirection.Output) _
                .Param("ClientID", clientId) _
                .Param("AccountID", Properties.Current.LabAccount.AccountID) _
                .Param("ActivityID", Properties.Current.Activities.FacilityDownTime.ActivityID) _
                .Param("BeginDateTime", beginDateTime) _
                .Param("EndDateTime", endDateTime) _
                .Param("IsActive", True) _
                .Param("CreatedOn", Date.Now) _
                .ExecuteNonQuery("sselScheduler.dbo.procReservationGroupInsert") _
                .GetParamValue("GroupID", 0)

            Return groupId
        End Function

        ' Deletes a series of reservations starting from the start date
        Public Shared Sub DeleteGroupReservations(GroupID As Integer)
            DataCommand.Create().Param(New With {.Action = "ByGroupID", GroupID}).ExecuteNonQuery("sselScheduler.dbo.procReservationGroupDelete")
        End Sub

        Public Shared Sub UpdateByGroupID(groupId As Integer, beginDateTime As Date, endDateTime As Date)
            DataCommand.Create() _
                .Param("Action", "ByGroupID") _
                .Param("GroupID", groupId) _
                .Param("BeginDateTime", beginDateTime) _
                .Param("EndDateTime", endDateTime) _
                .ExecuteNonQuery("sselScheduler.dbo.procReservationGroupUpdate")
        End Sub
    End Class
End Namespace