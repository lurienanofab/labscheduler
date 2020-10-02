Imports LNF.Impl.DataAccess
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(Startup))>

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        Dim sessionManager As ISessionManager = WebApp.Current.GetInstance(Of ISessionManager)()
        app.Use(GetType(NHibernateMiddleware), sessionManager)

        ' setup for viewing NHibernate queries with Glimpse
        app.ConfigureGlimpse(sessionManager)
    End Sub
End Class