Imports System.IO
Imports LNF
Imports LNF.Scheduler
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace Ajax
    Public Class ProcessInfo
        Implements IHttpHandler

        <Inject()>
        Public Property Provider As IProvider

        Sub ProcessRequest(context As HttpContext) Implements IHttpHandler.ProcessRequest

            Dim statusCode As Integer = 200
            Dim result As Object

            Try
                Dim util As New ProcessInfos(Provider)
                Dim command As String

                If context.Request.HttpMethod = "GET" Then
                    command = context.Request.QueryString("Command")

                    Select Case command
                        Case "get-process-info"
                            Dim resourceId As Integer = Integer.Parse(context.Request.QueryString("ResourceID"))
                            result = util.GetProcessInfos(resourceId)
                        Case Else
                            statusCode = 405
                            Throw New Exception($"Method not allowed: {context.Request.HttpMethod}")
                    End Select
                ElseIf context.Request.HttpMethod = "POST" Then
                    Dim json As String

                    Using reader = New StreamReader(context.Request.InputStream)
                        json = reader.ReadToEnd()
                    End Using

                    Dim jobj As JObject = JObject.Parse(json)
                    command = jobj("command").Value(Of String)()

                    Select Case command
                        Case "get-process-info"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            result = util.GetProcessInfos(resourceId)
                        Case "add-process-info"
                            Dim model As IProcessInfo = jobj("model").ToObject(Of ProcessInfoItem)()
                            result = util.AddProcessInfo(model)
                        Case "update-process-info"
                            Dim model As IProcessInfo = jobj("model").ToObject(Of ProcessInfoItem)()
                            result = util.UpdateProcessInfo(model)
                        Case "move-up"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            Dim processInfoId As Integer = jobj("processInfoId").Value(Of Integer)
                            result = util.MoveUp(resourceId, processInfoId)
                        Case "move-down"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            Dim processInfoId As Integer = jobj("processInfoId").Value(Of Integer)
                            result = util.MoveDown(resourceId, processInfoId)
                        Case "delete-process-info"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            Dim processInfoId As Integer = jobj("processInfoId").Value(Of Integer)
                            result = util.DeleteProcessInfo(resourceId, processInfoId)
                        Case "add-process-info-line"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            Dim model As IProcessInfoLine = jobj("model").ToObject(Of ProcessInfoLineItem)()
                            result = util.AddProcessInfoLine(resourceId, model)
                        Case "update-process-info-line"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            Dim model As IProcessInfoLine = jobj("model").ToObject(Of ProcessInfoLineItem)()
                            result = util.UpdateProcessInfoLine(resourceId, model)
                        Case "delete-process-info-line"
                            Dim resourceId As Integer = jobj("resourceId").Value(Of Integer)
                            Dim processInfoId As Integer = jobj("processInfoId").Value(Of Integer)
                            Dim processInfoLineId As Integer = jobj("processInfoLineId").Value(Of Integer)
                            result = util.DeleteProcessInfoLine(resourceId, processInfoId, processInfoLineId)
                        Case Else
                            Throw New NotImplementedException($"Not implemented: {command}")
                    End Select
                Else
                    statusCode = 405
                    Throw New Exception($"Method not allowed: {context.Request.HttpMethod}")
                End If
            Catch ex As Exception
                If statusCode = 200 Then
                    statusCode = 500
                End If
                result = New With {.error = ex.Message}
            End Try

            context.Response.ContentType = "application/json"
            context.Response.StatusCode = statusCode
            context.Response.Write(JsonConvert.SerializeObject(result))
        End Sub

        ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
            Get
                Return False
            End Get
        End Property
    End Class
End Namespace