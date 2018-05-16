Imports System.Text
Imports LNF
Imports LNF.Cache
Imports LNF.Email
Imports LNF.Models.Data
Imports LNF.Repository
Imports LNF.Repository.Data

Public Class ErrorUtility

    Private Shared appName As String = "Scheduler"
    Private Shared fromAddr As String = "system@lnf.umich.edu"
    Private Shared toAddr As String() = {"lnf-it@umich.edu"}

    Public Shared Function GetErrorData(ex As Exception) As ErrorLog()
        Dim list As New List(Of ErrorLog)

        Dim clientId As Integer
        Dim c As ClientItem = Nothing

        Try
            c = CacheManager.Current.CurrentUser
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
            .PageUrl = ServiceProvider.Current.Context.GetRequestUrl().ToString()
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
                .PageUrl = ServiceProvider.Current.Context.GetRequestUrl().ToString()
            })
        End Try

        SendEmail(list, c)

        Return list.ToArray()
    End Function

    Private Shared Sub SendEmail(errors As List(Of ErrorLog), c As ClientItem)
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

            ServiceProvider.Current.Email.SendMessage(args)
        Catch ex As Exception
            errors.Add(New ErrorLog() With {
                .Application = appName,
                .Message = ex.Message,
                .StackTrace = ex.StackTrace,
                .ErrorDateTime = Date.Now,
                .ClientID = clientId,
                .PageUrl = ServiceProvider.Current.Context.GetRequestUrl().ToString()
            })
        End Try
    End Sub
End Class
