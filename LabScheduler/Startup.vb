Imports LNF
Imports LNF.Web
Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(Startup))>

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        app.UseDataAccess([Global].Container)
    End Sub
End Class