Imports LNF.Repository

Namespace DBAccess
    Public Class ResourceDocDB
        ' Returns all docs for the specified resource
        Public Function SelectDocs(ByVal ResourceID As Integer) As DataTable
            Return DA.Command().Param(New With {.Action = "SelectByResource", ResourceID}).FillDataTable("sselScheduler.dbo.procResourceDocSelect")
        End Function

        ' Inserts a resource doc
        Public Function InsertDoc(ByVal ResourceID As Integer, ByVal DocName As String, ByVal FileExtension As String) As Integer
            Return DA.Command().Param(New With {.ResourceID = "ResourceID", DocName, FileExtension}).ExecuteScalar(Of Integer)("sselScheduler.dbo.procResourceDocInsert")
        End Function

        ' Updates a resource doc
        Public Function UpdateDoc(ByVal DocID As Integer, ByVal DocName As String, ByVal FileName As String) As Integer
            Return DA.Command().Param(New With {.DocID = "DocID", DocName, FileName}).ExecuteNonQuery("sselScheduler.dbo.procResourceDocUpdate").Value
        End Function

        ' Deletes a resource doc
        Public Sub DeleteDoc(ByVal DocID As Integer)
            DA.Command().Param(New With {DocID}).ExecuteNonQuery("sselScheduler.dbo.procResourceDocDelete")
        End Sub
    End Class
End Namespace