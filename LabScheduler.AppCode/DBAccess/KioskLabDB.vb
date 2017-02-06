Imports LNF.Repository
Imports LNF.CommonTools

Namespace DBAccess
    Public Class KioskLabDB
        ' Returns all kiosks
        Public Function SelectAll() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim dt As DataTable = dba.ApplyParameters(New With {.Action = "SelectAll"}).FillDataTable("procKioskLabSelect")
                dt.PrimaryKey = New DataColumn() {dt.Columns("KioskLabID")}
                dt.PrimaryKey(0).AutoIncrement = True
                dt.PrimaryKey(0).AutoIncrementSeed = -1
                dt.PrimaryKey(0).AutoIncrementStep = -1
                Return dt
            End Using
        End Function

        ' Returns all kiosks in the specified laboratory
        Public Function SelectByLab(ByVal LabID As Integer) As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim dt As DataTable = dba.ApplyParameters(New With {.Action = "SelectByLab", .LabID = LabID}).FillDataTable("procKioskLabSelect")
                dt.PrimaryKey = New DataColumn() {dt.Columns("KioskLabID")}
                dt.PrimaryKey(0).AutoIncrement = True
                dt.PrimaryKey(0).AutoIncrementSeed = -1
                dt.PrimaryKey(0).AutoIncrementStep = -1
                Return dt
            End Using
        End Function

        ' Insert/Update/Delete Kiosks
        Public Sub Update(ByRef dt As DataTable)
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.InsertCommand
                    .AddParameter("@KioskID", SqlDbType.Int)
                    .AddParameter("@LabID", SqlDbType.Int)
                End With

                dba.DeleteCommand.AddParameter("@KioskID", SqlDbType.Int)

                dba.UpdateDataTable(dt, insertSql:="procKioskLabInsert", deleteSql:="procKioskLabDelete")
            End Using
        End Sub
    End Class
End Namespace