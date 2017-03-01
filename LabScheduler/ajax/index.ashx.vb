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
            Dim gr As New GenericResult()
            gr.Data = "-Exception-"
            gr.Success = False
            gr.Message = ex.ToString()
            result = gr
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
        Return Providers.Serialization.Json.SerializeObject(obj)
    End Function
End Class