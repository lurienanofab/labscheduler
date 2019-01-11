Imports LNF
Imports LNF.Web.Scheduler

Public Class Index
    Implements IHttpHandler, IReadOnlySessionState

    Public ReadOnly Property IsReusable As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property

    Public Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim result As Object = Nothing

        Try
            Dim action As String = GetParam(context, "Action")

            If action = "enablewago" Then

            ElseIf action = "schedule" Then
                'Dim schedule As Object = LNF.Web.Scheduler.AjaxUtility.HandleScheduleRequest()
                'Response.ContentType = "application/json"
                'Response.Write(Providers.Serialization.Json.SerializeObject(schedule))
                'Return
            Else
                result = AjaxUtility.HandleRequest(context)
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

    Private Function GetParam(context As HttpContext, paramName As String) As String
        If context.Request(paramName) Is Nothing OrElse String.IsNullOrEmpty(context.Request(paramName)) Then
            Return String.Empty
        End If

        Return context.Request(paramName)
    End Function

    Private Function JsonResult(obj As Object) As String
        Return ServiceProvider.Current.Serialization.Json.SerializeObject(obj)
    End Function
End Class