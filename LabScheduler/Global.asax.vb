Imports System.Reflection
Imports System.Security.Principal
Imports System.Web.Compilation
Imports LabScheduler.AppCode
Imports LNF
Imports LNF.DependencyInjection
Imports LNF.Impl
Imports LNF.Impl.DependencyInjection
Imports LNF.Web

Public Class [Global]
    Inherits HttpApplication

    Private Shared webapp As WebApp

    Public Shared ReadOnly Property ContainerContext As IContainerContext
        Get
            Return webapp.Context
        End Get
    End Property

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        Dim assemblies As Assembly() = BuildManager.GetReferencedAssemblies().Cast(Of Assembly)().ToArray()

        webapp = New WebApp()

        ' setup up dependency injection container
        Dim wcc As WebContainerConfiguration = webapp.GetConfiguration()
        wcc.Context.EnablePropertyInjection()
        wcc.RegisterAllTypes()

        ' setup web dependency injection
        webapp.Bootstrap(assemblies)

        Application("AppServer") = ConfigurationManager.AppSettings("AppServer")
        Application("DocStore") = ConfigurationManager.AppSettings("DocStore")
        Application("DocServer") = ConfigurationManager.AppSettings("DocServer")
    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        If Request.IsAuthenticated Then
            Dim ident As FormsIdentity = CType(User.Identity, FormsIdentity)
            Dim roles As String() = ident.Ticket.UserData.Split("|"c)
            Context.User = New GenericPrincipal(ident, roles)
        End If
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        Dim enabled As Boolean = Boolean.Parse(ConfigurationManager.AppSettings("HandleErrors"))
        If Not enabled Then Return

        Dim errors As Repository.Data.ErrorLog()

        Dim lastEx As Exception = Server.GetLastError()
        If lastEx IsNot Nothing Then
            Dim baseEx As Exception = lastEx.GetBaseException()
            Dim util As New ErrorUtility(New HttpContextWrapper(Context), ContainerContext.GetInstance(Of IProvider)())
            If baseEx IsNot Nothing Then
                errors = util.GetErrorData(baseEx)
            Else
                errors = util.GetErrorData(lastEx)
            End If

            'do not use the custom
            If Not Context.Request.FilePath.EndsWith(".ashx") Then
                Context.ClearError()
                Response.Redirect("~/ErrorPage.aspx?err=" + String.Join(",", errors.Select(Function(x) x.ErrorLogID)))
            End If
        End If
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        Dim sessionId As String = Session.SessionID  ' this is needed as a bug fix for the session id flushed error.
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)

    End Sub

    Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)

    End Sub
End Class