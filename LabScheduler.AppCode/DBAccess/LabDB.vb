Imports LNF.Repository
Imports LNF.CommonTools

Namespace DBAccess
    Public Class LabDB
        Public IsValid As Boolean = False
        Public BuildingID As Integer
        Public BuildingName As String
        Public LabID As Integer
        Public LabName As String
        Public Description As String

        Public Sub New()
            ' do nothing
        End Sub

        ' Returns specified Lab
        Public Sub New(ByVal id As Integer)
            Using dba As New SQLDBAccess("cnSselScheduler")
                Using reader As IDataReader = dba.ApplyParameters(New With {.Action = "Select", .LabID = id}).ExecuteReader("procLabSelect")
                    If reader.Read() Then
                        IsValid = True
                        BuildingID = Convert.ToInt32(reader("BuildingID"))
                        BuildingName = reader("BuildingName").ToString()
                        LabID = Convert.ToInt32(reader("LabID"))
                        LabName = reader("LabName").ToString()
                        Description = reader("Description").ToString()
                    End If
                    reader.Close()
                End Using
            End Using
        End Sub

        ' Returns labs belonging to specified building
        Public Function SelectByBuilding(ByVal BuildingID As Integer) As IDataReader
            Dim dba As New SQLDBAccess("cnSselScheduler")
            Return dba.ApplyParameters(New With {.Action = "SelectByBuilding", .BuildingID = BuildingID}).ExecuteReader("procLabSelect")
        End Function

        Public Function SelectByBuilding(ByVal BuildingID As Integer, ByVal bDataTable As Boolean) As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.ApplyParameters(New With {.Action = "SelectByBuilding", .BuildingID = BuildingID}).FillDataTable("procLabSelect")
            End Using
        End Function

        ' Returns all Labs
        Public Shared Function SelectAll() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim dt As DataTable = dba.MapSchema().ApplyParameters(New With {.Action = "SelectAll"}).FillDataTable("procLabSelect")
                dt.PrimaryKey = New DataColumn() {dt.Columns("LabID")}
                Return dt
            End Using
        End Function

        ' Returns all rooms
        Public Function SelectRooms() As IDataReader
            Dim dba As New SQLDBAccess("cnSselData")
            Return dba.ApplyParameters(New With {.Action = "All"}).ExecuteReader("Room_Select")
        End Function

        Public Function HasProcessTechs(ByVal LabID As Integer) As Boolean
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim count As Integer = dba.ApplyParameters(New With {.Action = "HasProcessTechs", .LabID = LabID}).ExecuteScalar(Of Integer)("procLabSelect")
                Return count <> 0
            End Using
        End Function

        ' Insert/Update/Delete Labs
        Public Sub Update(ByRef dt As DataTable)
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.InsertCommand
                    .AddParameter("@LabID", SqlDbType.Int, ParameterDirection.Output)
                    .AddParameter("@BuildingID", SqlDbType.Int)
                    .AddParameter("@LabName", SqlDbType.NVarChar, 50)
                    .AddParameter("@RoomID", SqlDbType.Int)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                With dba.UpdateCommand
                    .AddParameter("@LabID", SqlDbType.Int)
                    .AddParameter("@BuildingID", SqlDbType.Int)
                    .AddParameter("@LabName", SqlDbType.NVarChar, 50)
                    .AddParameter("@RoomID", SqlDbType.Int)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                dba.DeleteCommand.AddParameter("@LabID", SqlDbType.Int)

                dba.UpdateDataTable(dt, "procLabInsert", "procLabUpdate", "procLabDelete")
            End Using
        End Sub

    End Class
End Namespace