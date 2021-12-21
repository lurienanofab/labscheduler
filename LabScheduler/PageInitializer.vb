Imports LNF.Web

<Assembly: PreApplicationStartMethod(GetType(PageInitializer), "Initialize")>

Public Class PageInitializer
    Inherits PageInitializerModule

    Public Shared Sub Initialize()
        RegisterModule(GetType(PageInitializer))
    End Sub
End Class