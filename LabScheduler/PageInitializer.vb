Imports LNF.Web

<Assembly: PreApplicationStartMethod(GetType(PageInitializer), "Initialize")>

Public Class PageInitializer
    Public Shared Sub Initialize()
        PageInitializerModule.Initialize()
    End Sub
End Class