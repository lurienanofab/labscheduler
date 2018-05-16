Imports System.Threading.Tasks
Imports LNF
Imports LNF.Web.Scheduler

Public Class index
    Inherits HttpTaskAsyncHandler
    Implements IReadOnlySessionState

    Public Overrides Async Function ProcessRequestAsync(context As HttpContext) As Task
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
                result = Await AjaxUtility.HandleRequest(context)
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
    End Function

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