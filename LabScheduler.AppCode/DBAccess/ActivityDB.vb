Imports LNF.Repository

Namespace DBAccess
    Public Class ActivityDB
        ' Returns all Activities
        Public Function SelectAll() As DataTable
            Return DataCommand.Create().MapSchema().Param("Action", "SelectAll").FillDataTable("sselScheduler.dbo.procActivitySelect")
        End Function

        ' Returns all Activities authorized for the specified user
        Public Shared Function SelectAuthorizedActivities(ByVal UserAuth As Integer) As DataTable
            Dim dt As DataTable = DataCommand.Create() _
                .Param("Action", "SelectAuthorizedActivities") _
                .Param("UserAuth", UserAuth) _
                .FillDataTable("sselScheduler.dbo.procActivitySelect")

            dt.PrimaryKey = New DataColumn() {dt.Columns("ActivityID")}

            Return dt
        End Function

        ' Insert/Update/Delete Activities
        Public Sub Update(ByRef dt As DataTable)
            DataCommand.Create().Update(dt, AddressOf UpdateAction)
        End Sub

        Private Sub UpdateAction(x As UpdateConfiguration)
            x.Insert.SetCommandText("sselScheduler.dbo.procActivityInsert")
            x.Insert.AddParameter("ActivityName", SqlDbType.NVarChar, 50)
            x.Insert.AddParameter("ListOrder", SqlDbType.Int)
            x.Insert.AddParameter("Chargeable", SqlDbType.Bit)
            x.Insert.AddParameter("Editable", SqlDbType.Bit)
            x.Insert.AddParameter("AccountType", SqlDbType.Int)
            x.Insert.AddParameter("UserAuth", SqlDbType.Int)
            x.Insert.AddParameter("InviteeType", SqlDbType.Int)
            x.Insert.AddParameter("InviteeAuth", SqlDbType.Int)
            x.Insert.AddParameter("StartEndAuth", SqlDbType.Int)
            x.Insert.AddParameter("NoReservFenceAuth", SqlDbType.Int)
            x.Insert.AddParameter("NoMaxSchedAuth", SqlDbType.Int)
            x.Insert.AddParameter("Description", SqlDbType.NVarChar, 200)

            x.Update.SetCommandText("sselScheduler.dbo.procActivityUpdate")
            x.Update.AddParameter("ActivityID", SqlDbType.Int)
            x.Update.AddParameter("ActivityName", SqlDbType.NVarChar, 50)
            x.Update.AddParameter("ListOrder", SqlDbType.Int)
            x.Update.AddParameter("Chargeable", SqlDbType.Bit)
            x.Update.AddParameter("Editable", SqlDbType.Bit)
            x.Update.AddParameter("AccountType", SqlDbType.Int)
            x.Update.AddParameter("UserAuth", SqlDbType.Int)
            x.Update.AddParameter("InviteeType", SqlDbType.Int)
            x.Update.AddParameter("InviteeAuth", SqlDbType.Int)
            x.Update.AddParameter("StartEndAuth", SqlDbType.Int)
            x.Update.AddParameter("NoReservFenceAuth", SqlDbType.Int)
            x.Update.AddParameter("NoMaxSchedAuth", SqlDbType.Int)
            x.Update.AddParameter("Description", SqlDbType.NVarChar, 200)

            x.Delete.SetCommandText("sselScheduler.dbo.procActivityDelete")
            x.Delete.AddParameter("ActivityID", SqlDbType.Int)
        End Sub
    End Class
End Namespace