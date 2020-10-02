Imports LNF
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Web
Imports LNF.Web.Models
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Models

Public Class _500
    Inherits Page

    ' For some reason property injection isn't working on this page. Maybe because it's part of the error handling process?
    Public ReadOnly Property Provider As IProvider = WebApp.Current.GetInstance(Of IProvider)()

    Public Property ContextBase As HttpContextBase

    Private errors As IList(Of ErrorItem) = New List(Of ErrorItem)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        ContextBase = New HttpContextWrapper(Context)

        Dim lastEx As Exception = Server.GetLastError()

        Server.ClearError()

        If lastEx IsNot Nothing Then
            Dim err = lastEx
            While err IsNot Nothing
                AddError(err)
                err = err.InnerException
            End While
        Else
            errors.Add(New ErrorItem("No error found."))
        End If

        HandleErrors()

        rptErrors.DataSource = errors
        rptErrors.DataBind()

        rptSessionLog.Visible = False
        'rptSessionLog.DataSource = GetLogData()
        'rptSessionLog.DataBind()
    End Sub

    Private Function GetLogData() As List(Of SessionLogMessage)
        Dim helper As New SchedulerContextHelper(ContextBase, Provider)
        Dim log As List(Of SessionLogMessage) = helper.GetLog().ToList()
        Return log
    End Function

    Private Sub HandleErrors()
        Dim currentUser As IClient = GetCurrentUser()
        Dim exceptions As List(Of Exception) = errors.Where(Function(x) x.HasException()).Select(Function(x) x.GetException()).ToList()
        If exceptions.Count > 0 Then
            SendErrorEmail(exceptions, currentUser)
        End If
    End Sub

    Private Sub AddError(ex As Exception)
        errors.Add(New ErrorItem(ex))
    End Sub

    Private Function GetCurrentUser() As IClient
        Dim currentUser As IClient = Nothing

        Try
            currentUser = ContextBase.CurrentUser(Provider)
        Catch ex As Exception
            AddError(ex)
        End Try

        Return currentUser
    End Function

    Private Sub SendErrorEmail(errs As IEnumerable(Of Exception), currentUser As IClient)
        Try
            Dim log As List(Of SessionLogMessage) = GetLogData()
            Dim msg As String = String.Join(Environment.NewLine, log.Select(Function(x) x.Message).ToArray())
            SendEmail.SendErrorEmail(errs, msg, currentUser, GetApp(), ContextBase.CurrentIP(), ContextBase.Request.Url)
        Catch ex As Exception
            AddError(ex)
        End Try
    End Sub

    Private Function GetApp() As String
        Dim app As String = "unknown"
        If Provider.Log IsNot Nothing Then
            app = Provider.Log.Name
        End If
        Return app
    End Function
End Class

Public Class StackTraceItem
    Public Property Caller As String
    Public Property FileName As String
    Public Property LineNumber As Integer
End Class

Public Class ErrorItem
    Private _ex As Exception

    Public ReadOnly Property Message As String

    Public ReadOnly Property StackTrace As String

    Public Sub New(ex As Exception)
        _ex = ex
        Message = _ex.Message
        StackTrace = _ex.StackTrace
    End Sub

    Public Sub New(message As String)
        _ex = Nothing
        Me.Message = message
        StackTrace = String.Empty
    End Sub

    Public Function GetException() As Exception
        Return _ex
    End Function

    Public Function HasException() As Boolean
        Return _ex IsNot Nothing
    End Function
End Class