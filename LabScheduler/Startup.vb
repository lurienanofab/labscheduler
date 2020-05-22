Imports LNF
Imports LNF.Impl
Imports LNF.Impl.DataAccess
Imports LNF.Web.Scheduler
Imports Microsoft.Owin
Imports Owin

<Assembly: OwinStartup(GetType(Startup))>

Public Class Startup
    Public Sub Configuration(app As IAppBuilder)
        Dim resolver As DependencyResolver = Global_asax.Resolver

        Dim sessionManager As ISessionManager = resolver.GetInstance(Of ISessionManager)()

        app.Use(GetType(NHibernateMiddleware), sessionManager)

        ServiceProvider.Setup(resolver.GetInstance(Of IProvider)())

        ' setup for viewing NHibernate queries with Glimpse
        app.ConfigureGlimpse(sessionManager)
    End Sub
End Class