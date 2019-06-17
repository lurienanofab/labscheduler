Imports LNF
Imports LNF.Models.Scheduler

Namespace DBAccess
    Public Class ResourceDB
        Public Shared Function SelectByLab(labId As Integer?) As IList(Of IResource)
            Return ServiceProvider.Current.Scheduler.Resource.SelectByLab(labId).ToList()
        End Function
    End Class
End Namespace
