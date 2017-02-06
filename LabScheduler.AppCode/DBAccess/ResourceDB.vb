Imports LNF.Repository
Imports LNF.Repository.Scheduler

Namespace DBAccess
    Public Class ResourceDB
        Public Shared Function SelectByLab(labId As Integer?) As IList(Of Resource)
            Return DA.Scheduler.Resource.SelectByLab(labId)
        End Function
    End Class
End Namespace
