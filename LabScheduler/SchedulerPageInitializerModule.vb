Imports LNF.Web
Imports Microsoft.Web.Infrastructure.DynamicModuleHelper

<Assembly: PreApplicationStartMethod(GetType(SchedulerPageInitializerModule), "Initialize")>

Public Class SchedulerPageInitializerModule
    Inherits PageInitializerModule

    Public Shared Sub Initialize()
        DynamicModuleUtility.RegisterModule(GetType(SchedulerPageInitializerModule))
    End Sub

    Protected Overrides Sub InitializeHandler(handler As IHttpHandler)
        [Global].InitializeHandler(handler)
    End Sub
End Class