Imports LNF.Repository
Imports LNF.Repository.Scheduler
Imports LNF.Scheduler

Namespace DBAccess
    Public Class ResourceDB
        Public Shared Function SelectByLab(labId As Integer?) As IList(Of Resource)
            Return DA.Use(Of IResourceManager)().SelectByLab(labId).ToList()
        End Function
    End Class
End Namespace
