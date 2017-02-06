Imports LNF.Repository
Imports LNF.CommonTools

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
        Public Sub New(ByVal LID As Integer, ByVal ProcTechID As Integer)  ' non-standard name needed to avoid name clash
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.SelectCommand
                    .AddParameter("@Action", "Select")
                    .AddParameter("@LabID", LID)
                    .AddParameter("@ProcessTechID", ProcTechID)
                End With
                Using reader As IDataReader = dba.ExecuteReader("procProcessTechSelect")
                    If reader.Read() Then
                        IsValid = True
                        BuildingID = Convert.ToInt32(reader("BuildingID"))
                        BuildingName = reader("BuildingName").ToString()
                        LabID = Convert.ToInt32(reader("LabID"))
                        LabName = reader("LabName").ToString()
                        ProcessTechID = Convert.ToInt32(reader("ProcessTechID"))
                        ProcessTechName = reader("ProcessTechName").ToString()
                        Descriptoin = reader("Description").ToString()
                    End If
                    reader.Close()
                End Using
            End Using
        End Sub

        ' Returns ProcessTechs belonging to specified Lab
        Public Function SelectDataReaderByLab(ByVal LabID As Integer) As IDataReader
            Dim dba As New SQLDBAccess("cnSselScheduler")
            Return dba.ApplyParameters(New With {.Action = "SelectByLab", .LabID = LabID}).ExecuteReader("procProcessTechSelect")
        End Function

        Public Function SelectDataTableByLab(ByVal LabID As Integer) As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.ApplyParameters(New With {.Action = "SelectByLab", .LabID = LabID}).FillDataTable("procProcessTechSelect")
            End Using
        End Function

        ' Returns ProcessTechs not belonging to the specified Lab
        Public Function SelectProcessTechNotInLab(ByVal LabID As Integer) As IDataReader
            Dim dba As New SQLDBAccess("cnSselScheduler")
            Return dba.ApplyParameters(New With {.Action = "SelectProcessTechNotInLab", .LabID = LabID}).ExecuteReader("procProcessTechSelect")
        End Function

        ' TODO: Returns all ProcessTechs
        Public Shared Function SelectAll() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.MapSchema().ApplyParameters(New With {.Action = "SelectAll"}).FillDataTable("procProcessTechSelect")
            End Using
        End Function

        Public Function HasResources(ByVal ProcessTechID As Integer) As Boolean
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim count As Integer = dba.ApplyParameters(New With {.Action = "HasLabs", .BuildingID = BuildingID}).ExecuteScalar(Of Integer)("procProcessTechSelect")
                Return count <> 0
            End Using
        End Function

        ' Insert/Update/Delete ProcessTechs
        Public Sub Update(ByRef dt As DataTable)
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.InsertCommand
                    .AddParameter("@ProcessTechIDOut", 0, ParameterDirection.Output)
                    .AddParameter("@ProcessTechID", SqlDbType.Int)
                    .AddParameter("@LabID", SqlDbType.Int)
                    .AddParameter("@ProcessTechName", SqlDbType.NVarChar, 50)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                With dba.UpdateCommand
                    .AddParameter("@ProcessTechID", SqlDbType.Int)
                    .AddParameter("@LabID", SqlDbType.Int)
                    .AddParameter("@ProcessTechName", SqlDbType.NVarChar, 50)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                With dba.DeleteCommand
                    .AddParameter("@ProcessTechID", SqlDbType.Int)
                    .AddParameter("@LabID", SqlDbType.Int, 4)
                End With

                dba.UpdateDataTable(dt, "procProcessTechInsert", "procProcessTechUpdate", "procProcessTechDelete")
            End Using
        End Sub

    End Class
End Namespace