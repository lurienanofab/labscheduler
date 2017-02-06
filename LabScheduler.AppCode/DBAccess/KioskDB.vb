Imports LNF.Repository
Imports LNF.CommonTools

Namespace DBAccess
    Public Class KioskDB
        ' Returns all kiosks
        Public Function SelectAll() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.MapSchema().ApplyParameters(New With {.Action = "SelectAll"}).FillDataTable("procKioskSelect")
            End Using
        End Function

        ' Returns labs belonging to specified building
        Public Function IpCheck(ByVal ClientID As Integer, ByVal KioskIP As String) As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.SelectCommand
                    .AddParameter("@Action", "IpCheck")
                    .AddParameter("@ClientID", ClientID)
                    .AddParameter("@KioskIP", KioskIP)
                End With

                Return dba.MapSchema().FillDataTable("procKioskSelect")
            End Using
        End Function

        ' Insert/Update/Delete Kiosks
        Public Sub Update(ByRef dt As DataTable)
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.InsertCommand
                    .AddParameter("@KioskName", SqlDbType.NVarChar, 50)
                    .AddParameter("@KioskIP", SqlDbType.NVarChar, 15)
                End With

                With dba.UpdateCommand
                    .AddParameter("@KioskID", SqlDbType.Int)
                    .AddParameter("@KioskName", SqlDbType.NVarChar, 50)
                    .AddParameter("@KioskIP", SqlDbType.NVarChar, 15)
                End With

                dba.DeleteCommand.AddParameter("@KioskID", SqlDbType.Int)

                dba.UpdateDataTable(dt, "procKioskInsert", "procKioskUpdate", "procKioskDelete")
            End Using
        End Sub
    End Class
End Namespace