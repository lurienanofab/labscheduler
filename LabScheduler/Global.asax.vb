'Copyright 2017 University of Michigan

'Licensed under the Apache License, Version 2.0 (the "License");
'you may Not use this file except In compliance With the License.
'You may obtain a copy Of the License at

'http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law Or agreed To In writing, software
'distributed under the License Is distributed On an "AS IS" BASIS,
'WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'See the License For the specific language governing permissions And
'limitations under the License.

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
            If baseEx IsNot Nothing Then
                errors = ErrorUtility.GetErrorData(baseEx)
            Else
                errors = ErrorUtility.GetErrorData(lastEx)
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
        If IO.Path.GetExtension(Request.FilePath) = ".aspx" Then
            RequestLog.Start()
        End If
    End Sub

    Sub Application_EndRequest(ByVal sender As Object, ByVal e As EventArgs)
        If IO.Path.GetExtension(Request.FilePath) = ".aspx" Then
            Response.Write(RequestLog.FlushScript())
        End If
    End Sub
End Class