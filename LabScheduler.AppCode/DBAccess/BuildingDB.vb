Imports LNF.CommonTools
Imports LNF.Repository

Namespace DBAccess
    Public Class BuildingDB
        Public IsValid As Boolean = False
        Public BuildingID As Integer
        Public BuildingName As String
        Public Description As String

        Public Sub New()
            'do nothing
        End Sub

        ' Returns specified Building
        Public Sub New(ByVal id As Integer)
            Using dba As New SQLDBAccess("cnSselScheduler")
                Using reader As IDataReader = dba.ApplyParameters(New With {.Action = "Select", .BuildingID = id}).ExecuteReader("procBuildingSelect")
                    If reader.Read() Then
                        IsValid = True
                        BuildingID = Convert.ToInt32(reader("BuildingID"))
                        BuildingName = reader("BuildingName").ToString()
                        Description = reader("Description").ToString()
                    End If
                    reader.Close()
                End Using
            End Using
        End Sub

        ' Returns all Buildings
        Public Function SelectAllDataReader() As IDataReader
            Dim dba As New SQLDBAccess("cnSselScheduler")
            Return dba.ApplyParameters(New With {.Action = "SelectAll"}).ExecuteReader("procBuildingSelect")
        End Function

        ' Returns all Buildings
        Public Shared Function SelectAllDataTable() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.MapSchema().ApplyParameters(New With {.Action = "SelectAll"}).FillDataTable("procBuildingSelect")
            End Using
        End Function

        Public Shared Function HasLabs(ByVal BuildingID As Integer) As Boolean
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim count As Integer = dba.ApplyParameters(New With {.Action = "HasLabs", .BuildingID = BuildingID}).ExecuteScalar(Of Integer)("procBuildingSelect")
                Return count <> 0
            End Using
        End Function

        ' Insert/Update/Delete Buildings
        Public Shared Sub Update(ByRef dt As DataTable)
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.InsertCommand
                    .AddParameter("@BuildingID", SqlDbType.Int, ParameterDirection.Output)
                    .AddParameter("@BuildingName", SqlDbType.NVarChar, 50)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                With dba.UpdateCommand
                    .AddParameter("@BuildingID", SqlDbType.Int)
                    .AddParameter("@BuildingName", SqlDbType.NVarChar, 50)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                dba.DeleteCommand.AddParameter("@BuildingID", SqlDbType.Int)

                dba.UpdateDataTable(dt, "procBuildingInsert", "procBuildingUpdate", "procBuildingDelete")
            End Using
        End Sub

    End Class
End Namespace