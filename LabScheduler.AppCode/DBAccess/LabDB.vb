Imports LNF.Repository

Namespace DBAccess
    Public Class LabDB
        Public IsValid As Boolean = False
        Public BuildingID As Integer
        Public BuildingName As String
        Public LabID As Integer
        Public LabName As String
        Public LabDisplayName As String
        Public Description As String

        Public Sub New()
            ' do nothing
        End Sub

        ' Returns specified Lab
        Public Sub New(ByVal LabID As Integer)
            Using reader As ExecuteReaderResult = DA.Command().Param(New With {.Action = "Select", LabID}).ExecuteReader("sselScheduler.dbo.procLabSelect")
                If reader.Read() Then
                    IsValid = True
                    BuildingID = Convert.ToInt32(reader("BuildingID"))
                    BuildingName = Convert.ToString(reader("BuildingName"))
                    LabID = Convert.ToInt32(reader("LabID"))
                    LabName = Convert.ToString(reader("LabName"))
                    LabDisplayName = Convert.ToString(reader("LabDisplayName"))
                    Description = Convert.ToString(reader("Description"))
                End If
                reader.Close()
            End Using
        End Sub

        ' Returns labs belonging to specified building
        Public Function SelectByBuilding(ByVal BuildingID As Integer) As ExecuteReaderResult
            Return DA.Command().Param(New With {.Action = "SelectByBuilding", BuildingID}).ExecuteReader("sselScheduler.dbo.procLabSelect")
        End Function

        Public Function SelectByBuilding(ByVal BuildingID As Integer, ByVal bDataTable As Boolean) As DataTable
            Return DA.Command().Param(New With {.Action = "SelectByBuilding", BuildingID}).FillDataTable("sselScheduler.dbo.procLabSelect")
        End Function

        ' Returns all Labs
        Public Shared Function SelectAll() As DataTable
            Dim dt As DataTable = DA.Command().MapSchema().Param(New With {.Action = "SelectAll"}).FillDataTable("sselScheduler.dbo.procLabSelect")
            dt.PrimaryKey = New DataColumn() {dt.Columns("LabID")}
            Return dt
        End Function

        ' Returns all rooms
        Public Function SelectRooms() As ExecuteReaderResult
            Return DA.Command().Param(New With {.Action = "All"}).ExecuteReader("dbo.Room_Select")
        End Function

        Public Function HasProcessTechs(ByVal LabID As Integer) As Boolean
            Dim count As Integer = DA.Command().Param(New With {.Action = "HasProcessTechs", LabID}).ExecuteScalar(Of Integer)("sselScheduler.dbo.procLabSelect").Value
            Return count > 0
        End Function

        ' Insert/Update/Delete Labs
        Public Sub Update(ByRef dt As DataTable)
            DA.Command().Update(dt, Sub(x)
                                        x.Insert.SetCommandText("sselScheduler.dbo.procLabInsert")
                                        x.Insert.AddParameter("LabID", SqlDbType.Int, ParameterDirection.Output)
                                        x.Insert.AddParameter("BuildingID", SqlDbType.Int)
                                        x.Insert.AddParameter("LabName", SqlDbType.NVarChar, 50)
                                        x.Insert.AddParameter("RoomID", SqlDbType.Int)
                                        x.Insert.AddParameter("Description", SqlDbType.NVarChar, 200)

                                        x.Update.SetCommandText("sselScheduler.dbo.procLabUpdate")
                                        x.Update.AddParameter("LabID", SqlDbType.Int)
                                        x.Update.AddParameter("BuildingID", SqlDbType.Int)
                                        x.Update.AddParameter("LabName", SqlDbType.NVarChar, 50)
                                        x.Update.AddParameter("RoomID", SqlDbType.Int)
                                        x.Update.AddParameter("Description", SqlDbType.NVarChar, 200)

                                        x.Delete.SetCommandText("sselScheduler.dbo.procLabDelete")
                                        x.Delete.AddParameter("LabID", SqlDbType.Int)
                                    End Sub)
        End Sub

    End Class
End Namespace