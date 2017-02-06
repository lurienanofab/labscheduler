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
            Using dba As UnitOfWorkAdapter = DA.Current.GetAdapter()
                Dim dt As DataTable = dba.ApplyParameters(New With {.Action = "GetActiveFacilityDownTime"}).FillDataTable("sselScheduler.dbo.procReservationGroupSelect")
                Return dt
            End Using
        End Function

        Public Shared Function GetFacilityDownTimeByGroupID(groupId As Integer) As FacilityDownTimeRes
            Using dba As UnitOfWorkAdapter = DA.Current.GetAdapter()
                Dim result As New FacilityDownTimeRes
                Using reader As IDataReader = dba.ApplyParameters(New With {.Action = "ByGroupID", .GroupID = groupId}).ExecuteReader("sselScheduler.dbo.procReservationGroupSelect")
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
            End Using
        End Function

        Public Shared Function CreateNew(clientId As Integer, beginDateTime As Date, endDateTime As Date) As Integer
            Dim groupId As Integer
            Using dba As UnitOfWorkAdapter = DA.Current.GetAdapter()
                With dba.SelectCommand
                    .AddParameter("@Action", "InsertNew")
                    .AddParameter("@GroupID", groupId, ParameterDirection.Output)
                    .AddParameter("@ClientID", clientId)
                    .AddParameter("@AccountID", Properties.Current.LabAccount.AccountID)
                    .AddParameter("@ActivityID", Properties.Current.Activities.FacilityDownTime.ActivityID)
                    .AddParameter("@BeginDateTime", beginDateTime)
                    .AddParameter("@EndDateTime", endDateTime)
                    .AddParameter("@IsActive", True)
                    .AddParameter("@CreatedOn", Date.Now)
                End With
                dba.ExecuteNonQuery("sselScheduler.dbo.procReservationGroupInsert")
                groupId = dba.GetParameterValue(Of Integer)("@GroupID")
                Return groupId
            End Using
        End Function

        ' Deletes a series of reservations starting from the start date
        Public Shared Sub DeleteGroupReservations(groupId As Integer)
            Using dba As UnitOfWorkAdapter = DA.Current.GetAdapter()
                dba.ApplyParameters(New With {.Action = "ByGroupID", .GroupID = groupId}).ExecuteNonQuery("sselScheduler.dbo.procReservationGroupDelete")
            End Using
        End Sub

        Public Shared Sub UpdateByGroupID(groupId As Integer, beginDateTime As Date, endDateTime As Date)
            Using dba As UnitOfWorkAdapter = DA.Current.GetAdapter()
                With dba.SelectCommand
                    .AddParameter("@Action", "ByGroupID")
                    .AddParameter("@GroupID", groupId)
                    .AddParameter("@BeginDateTime", beginDateTime)
                    .AddParameter("@EndDateTime", endDateTime)
                End With
                dba.ExecuteNonQuery("sselScheduler.dbo.procReservationGroupUpdate")
            End Using
        End Sub
    End Class
End Namespace