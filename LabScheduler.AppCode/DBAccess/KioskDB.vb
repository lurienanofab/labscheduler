Imports LNF.Repository

Namespace DBAccess
    Public Class KioskDB
        ' Returns all kiosks
        Public Function SelectAll() As DataTable
            Dim dt As DataTable = DA.Command() _
                .MapSchema() _
                .Param("Action", "SelectAll") _
                .FillDataTable("sselScheduler.dbo.procKioskSelect")
            Return dt
        End Function

        ' Returns labs belonging to specified building
        Public Function IpCheck(ByVal ClientID As Integer, ByVal KioskIP As String) As DataTable
            Return DA.Command().MapSchema().Param(New With {.Action = "IpCheck", ClientID, KioskIP}).FillDataTable("sselScheduler.dbo.procKioskSelect")
        End Function

        ' Insert/Update/Delete Kiosks
        Public Sub Update(ByRef dt As DataTable)
            DA.Command().Update(dt, Sub(x)
                                        x.Insert.SetCommandText("sselScheduler.dbo.procKioskInsert")
                                        x.Insert.AddParameter("KioskName", SqlDbType.NVarChar, 50)
                                        x.Insert.AddParameter("KioskIP", SqlDbType.NVarChar, 15)

                                        x.Update.SetCommandText("sselScheduler.dbo.procKioskUpdate")
                                        x.Update.AddParameter("KioskID", SqlDbType.Int)
                                        x.Update.AddParameter("KioskName", SqlDbType.NVarChar, 50)
                                        x.Update.AddParameter("KioskIP", SqlDbType.NVarChar, 15)

                                        x.Delete.SetCommandText("sselScheduler.dbo.procKioskDelete")
                                        x.Delete.AddParameter("KioskID", SqlDbType.Int)
                                    End Sub)
        End Sub
    End Class
End Namespace