Imports LNF.Repository

Namespace DBAccess
    Public Class AccountDB
        ''' <summary>
        ''' Returns all Accounts
        ''' </summary>
        Public Function SelectAll() As DataTable
            Return SessionDataCommand.Create() _
                .Param("Action", "SelectAccounts") _
                .FillDataTable("sselScheduler.dbo.sselData_Select")
        End Function
    End Class
End Namespace


