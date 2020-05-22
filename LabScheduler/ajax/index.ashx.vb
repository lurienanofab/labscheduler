Imports LNF
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports Newtonsoft.Json

Public Class Index
    Implements IHttpHandler, IReadOnlySessionState

    <Inject> Public Property Provider As IProvider

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim result As Object = Nothing

        Try
            Dim ctx As HttpContextBase = New HttpContextWrapper(context)

            Dim action As String = GetParam(ctx, "Action")

            If action = "enablewago" Then

            ElseIf action = "schedule" Then
                'Dim schedule As Object = LNF.Web.Scheduler.AjaxUtility.HandleScheduleRequest()
                'Response.ContentType = "application/json"
                'Response.Write(Providers.Serialization.Json.SerializeObject(schedule))
                'Return
            Else
                result = AjaxUtility.HandleRequest(ctx, Provider)
            End If
        Catch ex As Exception
            result = New GenericResult With {
                .Data = "-Exception-",
                .Success = False,
                .Message = ex.ToString()
            }
        End Try

        context.Response.ContentType = "application/json"
        context.Response.Write(JsonResult(result))
    End Sub

    Private Function GetParam(context As HttpContextBase, paramName As String) As String
        If context.Request(paramName) Is Nothing OrElse String.IsNullOrEmpty(context.Request(paramName)) Then
            Return String.Empty
        End If

        Return context.Request(paramName)
    End Function

    Private Function JsonResult(obj As Object) As String
        Return JsonConvert.SerializeObject(obj)
    End Function
End Class