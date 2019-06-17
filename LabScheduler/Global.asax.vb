Imports System.Security.Principal
Imports LabScheduler.AppCode
Imports LNF.Web

Public Class Global_asax
    Inherits HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
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

        Dim errors As LNF.Repository.Data.ErrorLog()

        Dim lastEx As Exception = Server.GetLastError()
        If lastEx IsNot Nothing Then
            Dim baseEx As Exception = lastEx.GetBaseException()
            Dim util As New ErrorUtility(New HttpContextWrapper(Context))
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
        If IO.Path.GetExtension(Request.FilePath) = ".aspx" AndAlso Request.ContentType = "text/html" Then
            RequestLog.Start()
        End If
    End Sub

    Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)
        If IO.Path.GetExtension(Request.FilePath) = ".aspx" AndAlso Request.ContentType = "text/html" Then
            Response.Write(RequestLog.FlushScript())
        End If
    End Sub
End Class