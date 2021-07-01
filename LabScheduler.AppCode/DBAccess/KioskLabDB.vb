Imports LNF.Repository

Namespace DBAccess
    Public Class KioskLabDB
        ' Returns all kiosks
        Public Function SelectAll() As DataTable
            Dim dt As DataTable = DataCommand.Create() _
                .Param("Action", "SelectAll") _
                .FillDataTable("sselScheduler.dbo.procKioskLabSelect")

            dt.PrimaryKey = New DataColumn() {dt.Columns("KioskLabID")}
            dt.PrimaryKey(0).AutoIncrement = True
            dt.PrimaryKey(0).AutoIncrementSeed = -1
            dt.PrimaryKey(0).AutoIncrementStep = -1

            Return dt
        End Function

        ' Returns all kiosks in the specified laboratory
        Public Function SelectByLab(ByVal LabID As Integer) As DataTable
            Dim dt As DataTable = DataCommand.Create() _
                .Param("Action", "SelectByLab") _
                .Param("LabID", LabID) _
                .FillDataTable("sselScheduler.dbo.procKioskLabSelect")

            dt.PrimaryKey = New DataColumn() {dt.Columns("KioskLabID")}
            dt.PrimaryKey(0).AutoIncrement = True
            dt.PrimaryKey(0).AutoIncrementSeed = -1
            dt.PrimaryKey(0).AutoIncrementStep = -1

            Return dt
        End Function

        ' Insert/Update/Delete Kiosks
        Public Sub Update(ByRef dt As DataTable)
            DataCommand.Create.Update(dt, AddressOf UpdateAction)
        End Sub

        Private Sub UpdateAction(x As UpdateConfiguration)
            x.Insert.SetCommandText("sselScheduler.dbo.procKioskLabInsert")
            x.Insert.AddParameter("KioskID", SqlDbType.Int)
            x.Insert.AddParameter("LabID", SqlDbType.Int)

            x.Delete.SetCommandText("sselScheduler.dbo.procKioskLabDelete")
            x.Delete.AddParameter("KioskID", SqlDbType.Int)
        End Sub
    End Class
End Namespace