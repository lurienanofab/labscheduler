Imports LNF.Repository
Imports LNF.CommonTools

Namespace DBAccess
    Public Class ResourceDocDB
        ' Returns all docs for the specified resource
        Public Function SelectDocs(ByVal resourceId As Integer) As DataTable
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.ApplyParameters(New With {.Action = "SelectByResource", .ResourceID = resourceId}).FillDataTable("procResourceDocSelect")
            End Using
        End Function

        ' Inserts a resource doc
        Public Function InsertDoc(ByVal ResourceID As Integer, ByVal DocName As String, ByVal FileExtension As String) As Integer
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.ApplyParameters(New With {.ResourceID = "ResourceID", .DocName = DocName, .FileExtension = FileExtension}).ExecuteScalar(Of Integer)("procResourceDocInsert")
            End Using
        End Function

        ' Updates a resource doc
        Public Function UpdateDoc(ByVal DocID As Integer, ByVal DocName As String, ByVal FileName As String) As Integer
            Using dba As New SQLDBAccess("cnSselScheduler")
                Return dba.ApplyParameters(New With {.DocID = "DocID", .DocName = DocName, .FileName = FileName}).ExecuteNonQuery("procResourceDocUpdate")
            End Using
        End Function

        ' Deletes a resource doc
        Public Sub DeleteDoc(ByVal DocID As Integer)
            Using dba As New SQLDBAccess("cnSselScheduler")
                dba.ApplyParameters(New With {.DocID = DocID}).ExecuteNonQuery("procResourceDocDelete")
            End Using
        End Sub
    End Class
End Namespace