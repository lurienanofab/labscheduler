Imports LNF.Web
Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(Startup))>

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        app.UseDataAccess([Global].ContainerContext)
    End Sub
End Class