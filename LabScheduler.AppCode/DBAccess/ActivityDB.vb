Imports LNF.Repository
Imports LNF.CommonTools

Namespace DBAccess
    Public Class ActivityDB
        ' Returns all Activities
        Public Function SelectAll() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.MapSchema().ApplyParameters(New With {.Action = "SelectAll"}).FillDataTable("procActivitySelect")
            End Using
        End Function

        ' Returns all Activities authorized for the specified user
        Public Shared Function SelectAuthorizedActivities(ByVal UserAuth As Integer) As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Dim dt As DataTable = dba.ApplyParameters(New With {.Action = "SelectAuthorizedActivities", .UserAuth = UserAuth}).FillDataTable("procActivitySelect")
                dt.PrimaryKey = New DataColumn() {dt.Columns("ActivityID")}
                Return dt
            End Using
        End Function

        ' Insert/Update/Delete Activities
        Public Sub Update(ByRef dt As DataTable)
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.InsertCommand
                    .AddParameter("@ActivityName", SqlDbType.NVarChar, 50)
                    .AddParameter("@ListOrder", SqlDbType.Int)
                    .AddParameter("@Chargeable", SqlDbType.Bit)
                    .AddParameter("@Editable", SqlDbType.Bit)
                    .AddParameter("@AccountType", SqlDbType.Int)
                    .AddParameter("@UserAuth", SqlDbType.Int)
                    .AddParameter("@InviteeType", SqlDbType.Int)
                    .AddParameter("@InviteeAuth", SqlDbType.Int)
                    .AddParameter("@StartEndAuth", SqlDbType.Int)
                    .AddParameter("@NoReservFenceAuth", SqlDbType.Int)
                    .AddParameter("@NoMaxSchedAuth", SqlDbType.Int)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                With dba.UpdateCommand
                    .AddParameter("@ActivityID", SqlDbType.Int)
                    .AddParameter("@ActivityName", SqlDbType.NVarChar, 50)
                    .AddParameter("@ListOrder", SqlDbType.Int)
                    .AddParameter("@Chargeable", SqlDbType.Bit)
                    .AddParameter("@Editable", SqlDbType.Bit)
                    .AddParameter("@AccountType", SqlDbType.Int)
                    .AddParameter("@UserAuth", SqlDbType.Int)
                    .AddParameter("@InviteeType", SqlDbType.Int)
                    .AddParameter("@InviteeAuth", SqlDbType.Int)
                    .AddParameter("@StartEndAuth", SqlDbType.Int)
                    .AddParameter("@NoReservFenceAuth", SqlDbType.Int)
                    .AddParameter("@NoMaxSchedAuth", SqlDbType.Int)
                    .AddParameter("@Description", SqlDbType.NVarChar, 200)
                End With

                dba.DeleteCommand.AddParameter("@ActivityID", SqlDbType.Int)

                dba.UpdateDataTable(dt, "procActivityInsert", "procActivityUpdate", "procActivityDelete")
            End Using
        End Sub
    End Class
End Namespace