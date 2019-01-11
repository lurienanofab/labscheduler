Imports LNF.Repository

Namespace DBAccess
    Public Class ProcessTechDB
        Public IsValid As Boolean = False
        Public BuildingID As Integer
        Public BuildingName As String
        Public LabID As Integer
        Public LabName As String
        Public ProcessTechID As Integer
        Public ProcessTechName As String
        Public Descriptoin As String

        Public Sub New()
            ' do nothing
        End Sub

        ' Returns specified ProcessTech
        Public Sub New(ByVal LabID As Integer, ByVal ProcTechID As Integer)
            Using reader = DA.Command().Param(New With {.Action = "Select", LabID, ProcTechID}).ExecuteReader("sselScheduler.dbo.procProcessTechSelect")
                If reader.Read() Then
                    IsValid = True
                    BuildingID = Convert.ToInt32(reader("BuildingID"))
                    BuildingName = reader("BuildingName").ToString()
                    Me.LabID = Convert.ToInt32(reader("LabID"))
                    LabName = reader("LabName").ToString()
                    ProcessTechID = Convert.ToInt32(reader("ProcessTechID"))
                    ProcessTechName = reader("ProcessTechName").ToString()
                    Descriptoin = reader("Description").ToString()
                End If
                reader.Close()
            End Using
        End Sub

        ' Returns ProcessTechs belonging to specified Lab
        Public Function SelectDataReaderByLab(ByVal LabID As Integer) As IDataReader
            Return DA.Command().Param(New With {.Action = "SelectByLab", LabID}).ExecuteReader("sselScheduler.dbo.procProcessTechSelect")
        End Function

        Public Function SelectDataTableByLab(ByVal LabID As Integer) As DataTable
            Return DA.Command().Param(New With {.Action = "SelectByLab", LabID}).FillDataTable("sselScheduler.dbo.procProcessTechSelect")
        End Function

        ' Returns ProcessTechs not belonging to the specified Lab
        Public Function SelectProcessTechNotInLab(ByVal LabID As Integer) As IDataReader
            Return DA.Command().Param(New With {.Action = "SelectProcessTechNotInLab", LabID}).ExecuteReader("sselScheduler.dbo.procProcessTechSelect")
        End Function

        ' TODO: Returns all ProcessTechs
        Public Shared Function SelectAll() As DataTable
            Return DA.Command().MapSchema().Param(New With {.Action = "SelectAll"}).FillDataTable("sselScheduler.dbo.procProcessTechSelect")
        End Function

        Public Function HasResources(ByVal ProcessTechID As Integer) As Boolean
            Dim count As Integer = DA.Command().Param(New With {.Action = "HasLabs", BuildingID}).ExecuteScalar(Of Integer)("sselScheduler.dbo.procProcessTechSelect")
            Return count > 0
        End Function

        ' Insert/Update/Delete ProcessTechs
        Public Sub Update(ByRef dt As DataTable)
            DA.Command().Update(dt, Sub(x)
                                        x.Insert.SetCommandText("sselScheduler.dbo.procProcessTechInsert")
                                        x.Insert.AddParameter("ProcessTechIDOut", 0, ParameterDirection.Output)
                                        x.Insert.AddParameter("ProcessTechID", SqlDbType.Int)
                                        x.Insert.AddParameter("LabID", SqlDbType.Int)
                                        x.Insert.AddParameter("ProcessTechName", SqlDbType.NVarChar, 50)
                                        x.Insert.AddParameter("Description", SqlDbType.NVarChar, 200)

                                        x.Update.SetCommandText("sselScheduler.dbo.procProcessTechUpdate")
                                        x.Update.AddParameter("ProcessTechID", SqlDbType.Int)
                                        x.Update.AddParameter("LabID", SqlDbType.Int)
                                        x.Update.AddParameter("ProcessTechName", SqlDbType.NVarChar, 50)
                                        x.Update.AddParameter("Description", SqlDbType.NVarChar, 200)

                                        x.Delete.SetCommandText("sselScheduler.dbo.procProcessTechDelete")
                                        x.Delete.AddParameter("ProcessTechID", SqlDbType.Int)
                                        x.Delete.AddParameter("LabID", SqlDbType.Int, 4)
                                    End Sub)
        End Sub

    End Class
End Namespace