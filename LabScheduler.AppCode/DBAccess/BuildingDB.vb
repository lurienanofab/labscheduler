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
            Using reader As IDataReader = DA.Command().Param(New With {.Action = "Select", .BuildingID = id}).ExecuteReader("sselScheduler.dbo.procBuildingSelect")
                If reader.Read() Then
                    IsValid = True
                    BuildingID = Convert.ToInt32(reader("BuildingID"))
                    BuildingName = reader("BuildingName").ToString()
                    Description = reader("Description").ToString()
                End If
                reader.Close()
            End Using
        End Sub

        ' Returns all Buildings
        Public Function SelectAllDataReader() As IDataReader
            Return DA.Command().Param(New With {.Action = "SelectAll"}).ExecuteReader("sselScheduler.dbo.procBuildingSelect")
        End Function

        ' Returns all Buildings
        Public Shared Function SelectAllDataTable() As DataTable
            Return DA.Command().MapSchema().Param(New With {.Action = "SelectAll"}).FillDataTable("sselScheduler.dbo.procBuildingSelect")
        End Function

        Public Shared Function HasLabs(ByVal BuildingID As Integer) As Boolean
            Dim count As Integer = DA.Command().Param(New With {.Action = "HasLabs", BuildingID}).ExecuteScalar(Of Integer)("sselScheduler.dbo.procBuildingSelect")
            Return count > 0
        End Function

        ' Insert/Update/Delete Buildings
        Public Shared Sub Update(ByRef dt As DataTable)
            DA.Command.Update(dt, Sub(x)
                                      x.Insert.SetCommandText("sselScheduler.dbo.procBuildingInsert")
                                      x.Insert.AddParameter("BuildingID", SqlDbType.Int, ParameterDirection.Output)
                                      x.Insert.AddParameter("BuildingName", SqlDbType.NVarChar, 50)
                                      x.Insert.AddParameter("Description", SqlDbType.NVarChar, 200)

                                      x.Update.SetCommandText("sselScheduler.dbo.procBuildingUpdate")
                                      x.Update.AddParameter("BuildingID", SqlDbType.Int)
                                      x.Update.AddParameter("BuildingName", SqlDbType.NVarChar, 50)
                                      x.Update.AddParameter("Description", SqlDbType.NVarChar, 200)

                                      x.Delete.SetCommandText("sselScheduler.dbo.procBuildingDelete")
                                      x.Delete.AddParameter("BuildingID", SqlDbType.Int)
                                  End Sub)
        End Sub
    End Class
End Namespace