Imports LNF.Impl
Imports SimpleInjector
Imports SimpleInjector.Integration.Web

Public Class LabSchedulerResolver
    Inherits WebResolver

    Protected Overrides ReadOnly Property EnablePropertyInjection As Boolean
        Get
            Return True
        End Get
    End Property

    Protected Overrides Function GetDefaultScopedLifestyle() As ScopedLifestyle
        Return New WebRequestLifestyle()
    End Function
End Class