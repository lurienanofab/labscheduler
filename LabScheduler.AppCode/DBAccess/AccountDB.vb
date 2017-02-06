Imports LNF.Repository
Imports LNF.CommonTools

Namespace DBAccess
    Public Class AccountDB
        ''' <summary>
        ''' Returns all Accounts
        ''' </summary>
        Public Function SelectAll() As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.ApplyParameters(New With {.Action = "SelectAccounts"}).FillDataTable("sselData_Select")
            End Using
        End Function
    End Class
End Namespace


