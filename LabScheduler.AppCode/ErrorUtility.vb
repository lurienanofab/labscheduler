Imports System.Text
Imports System.Web
Imports LNF
Imports LNF.Models.Data
Imports LNF.Models.Mail
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Web

Public Class ErrorUtility

    Private appName As String = "Scheduler"
    Private fromAddr As String = "system@lnf.umich.edu"
    Private toAddr As String() = {"lnf-it@umich.edu"}

    Public ReadOnly Property ContextBase As HttpContextBase

    Public ReadOnly Property CurrentUser As IClient
        Get
            Return ContextBase.CurrentUser()
        End Get
    End Property

    Public Sub New(context As HttpContextBase)
        ContextBase = context
    End Sub

    Public Function GetErrorData(ex As Exception) As ErrorLog()
        Dim list As New List(Of ErrorLog)

        Dim clientId As Integer
        Dim c As IClient = Nothing

        Try
            c = CurrentUser
            clientId = c.ClientID
        Catch
            clientId = 0
        End Try

        Dim result As ErrorLog = New ErrorLog With {
            .Application = appName,
            .Message = ex.Message,
            .StackTrace = ex.StackTrace,
            .ErrorDateTime = Date.Now,
            .ClientID = clientId,
            .PageUrl = ContextBase.Request.Url.ToString()
        }

        list.Add(result)

        Try
            DA.Current.Insert(result)
        Catch ex2 As Exception
            list.Add(New ErrorLog() With {
                .Application = appName,
                .Message = ex2.Message,
                .StackTrace = ex2.StackTrace,
                .ErrorDateTime = Date.Now,
                .ClientID = clientId,
                .PageUrl = ContextBase.Request.Url.ToString()
            })
        End Try

        SendEmail(list, c)

        Return list.ToArray()
    End Function

    Private Sub SendEmail(errors As List(Of ErrorLog), c As IClient)
        Dim clientId As Integer = 0
        Dim displayName As String = "unknown"

        Try
            If c IsNot Nothing Then
                clientId = c.ClientID
                displayName = c.DisplayName
            End If

            Dim body As New StringBuilder()
            For Each err As ErrorLog In errors
                body.AppendLine(String.Format("ErrorLogID: {0}", err.ErrorLogID))
                body.AppendLine(String.Format("Application: {0}", err.Application))
                body.AppendLine(String.Format("Message: {0}", err.Message))
                body.AppendLine(String.Format("StackTrace: {0}", err.StackTrace))
                body.AppendLine(String.Format("ErrorDateTime: {0:yyyy-MM-dd HH:mm:ss}", err.ErrorDateTime))
                body.AppendLine(String.Format("Client: {0} ({1})", displayName, clientId))
                body.AppendLine(String.Format("PageUrl: {0}", err.PageUrl))
                body.AppendLine("-------------------------------------------------")
            Next

            Dim args = New SendMessageArgs With {
                .ClientID = 0,
                .Subject = appName + " Error",
                .Body = body.ToString(),
                .From = fromAddr,
                .To = toAddr
            }

            ServiceProvider.Current.Mail.SendMessage(args)
        Catch ex As Exception
            errors.Add(New ErrorLog() With {
                .Application = appName,
                .Message = ex.Message,
                .StackTrace = ex.StackTrace,
                .ErrorDateTime = Date.Now,
                .ClientID = clientId,
                .PageUrl = ContextBase.Request.Url.ToString()
            })
        End Try
    End Sub
End Class
