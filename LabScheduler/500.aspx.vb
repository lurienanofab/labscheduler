Imports LNF
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Web

Public Class _500
    Inherits Page

    <Inject> Public Property Provider As IProvider
    Public Property ContextBase As HttpContextBase

    Private errors As IList(Of ErrorItem) = New List(Of ErrorItem)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        ContextBase = New HttpContextWrapper(Context)

        Dim lastEx As Exception = Server.GetLastError()

        If lastEx IsNot Nothing Then
            Dim innerEx = lastEx.InnerException
            If innerEx IsNot Nothing Then
                HandleError(innerEx)
            Else
                HandleError(lastEx)
            End If
        Else
            errors.Add(New ErrorItem With {.Message = "No error found.", .StackTrace = String.Empty})
        End If

        rptErrors.DataSource = errors
        rptErrors.DataBind()
    End Sub

    Private Sub HandleError(ex As Exception)
        AddError(ex)
        Dim currentUser As IClient = GetCurrentUser()
        SendErrorEmail(ex, currentUser)
    End Sub

    Private Sub AddError(ex As Exception)
        errors.Add(New ErrorItem With {.Message = ex.Message, .StackTrace = ex.StackTrace})
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

    Private Sub SendErrorEmail(err As Exception, currentUser As IClient)
        Try
            Dim app As String = Provider.Log.Name
            SendEmail.SendErrorEmail(err, currentUser, app, ContextBase.CurrentIP(), ContextBase.Request.Url)
        Catch ex As Exception
            AddError(ex)
        End Try
    End Sub
End Class

Public Class ErrorItem
    Public Property Message As String
    Public Property StackTrace As String
End Class