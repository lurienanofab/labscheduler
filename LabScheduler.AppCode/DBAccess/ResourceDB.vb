Imports LNF
Imports LNF.Repository.Scheduler
Imports LNF.Scheduler

Namespace DBAccess
    Public Class ResourceDB
        Public Shared ReadOnly Property ResourceManager As IResourceManager = ServiceProvider.Current.Use(Of IResourceManager)()

        Public Shared Function SelectByLab(labId As Integer?) As IList(Of Resource)
            Return ResourceManager.SelectByLab(labId).ToList()
        End Function
    End Class
End Namespace
