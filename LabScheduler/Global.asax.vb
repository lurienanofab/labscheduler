Imports System.Reflection
Imports System.Security.Principal
Imports System.Web.Compilation
Imports LabScheduler.AppCode
Imports LNF
Imports LNF.Impl
Imports LNF.Web
Imports LNF.Web.Content
Imports SimpleInjector
Imports SimpleInjector.Diagnostics

Public Class Global_asax
    Inherits HttpApplication

    Private Shared _resolver As DependencyResolver

    Public Shared ReadOnly Property Resolver As DependencyResolver
        Get
            Return _resolver
        End Get
    End Property

    Public Shared Sub InitializeHandler(handler As IHttpHandler)
        Dim handlerType As Type = If(handler.GetType() Is GetType(Page), handler.GetType().BaseType, handler.GetType())
        Dim container As Container = _resolver.GetContainer()
        container.GetRegistration(handlerType, True).Registration.InitializeInstance(handler)
    End Sub

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        Bootstrap()
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
            Dim util As New ErrorUtility(New HttpContextWrapper(Context), Resolver.GetInstance(Of IProvider)())
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
        If IO.Path.GetExtension(Request.FilePath) = ".aspx" AndAlso Response.ContentType = "text/html" Then
            RequestLog.Start()
        End If
    End Sub

    Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)
        If IO.Path.GetExtension(Request.FilePath) = ".aspx" AndAlso Response.ContentType = "text/html" Then
            Response.Write(RequestLog.FlushScript())
        End If
    End Sub

    Private Shared Sub Bootstrap()
        _resolver = New LabSchedulerResolver()

        Dim container = _resolver.GetContainer()

        ' Register your Page classes to allow them to be verified And diagnosed.
        RegisterWebPages(container)

        container.Verify()
    End Sub

    Private Shared Sub RegisterWebPages(container As Container)
        Dim pageTypes As Type() = LNF.CommonTools.Utility.GetAssignableFromType(Of Page)({Assembly.GetExecutingAssembly()})

        For Each t As Type In pageTypes
            Dim reg = Lifestyle.Transient.CreateRegistration(t, container)
            reg.SuppressDiagnosticWarning(DiagnosticType.DisposableTransientComponent, "ASP.NET creates and disposes page classes for us.")
            container.AddRegistration(t, reg)
        Next
    End Sub
End Class