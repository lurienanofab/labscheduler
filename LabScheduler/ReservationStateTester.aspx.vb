Imports System.IO
Imports LNF
Imports LNF.Scheduler
Imports Newtonsoft.Json

Public Class ReservationStateTester
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Request.HttpMethod = "POST" Then
            Using reader As New StreamReader(Request.InputStream)
                Dim body As String = reader.ReadToEnd()
                Dim request As ReservationStateRequest = JsonConvert.DeserializeObject(Of ReservationStateRequest)(body)
                Dim util As ReservationStateUtility = Scheduler.ReservationStateUtility.Create(Date.Now)
                Dim val As Integer
                Dim truthTable As ReservationState()

                If request.ToolEngineer Then
                    val = util.GetSubStateVal(False, False, False, False, request.BeforeMinCancelTime, request.Startable)
                    truthTable = ReservationStateUtility.TruthTableTE
                Else
                    val = util.GetSubStateVal(request.InLab, request.Reserver, request.Invited, request.Authorized, request.BeforeMinCancelTime, request.Startable)
                    truthTable = ReservationStateUtility.TruthTable
                End If

                Dim state As ReservationState = truthTable(val)

                Dim resp As New ReservationStateResponse With {
                    .State = state,
                    .StateText = state.ToString()
                }

                Response.Clear()
                Response.ContentType = "application/json"
                Response.Write(JsonConvert.SerializeObject(resp))
                Response.End()
            End Using
        End If
    End Sub

End Class

Public Class ReservationStateResponse
    <JsonProperty("state")> Public Property State As ReservationState
    <JsonProperty("stateText")> Public Property StateText As String
End Class

Public Class ReservationStateRequest
    <JsonProperty("tool_engineer")> Public Property ToolEngineer As Boolean
    <JsonProperty("inlab")> Public Property InLab As Boolean
    <JsonProperty("reserver")> Public Property Reserver As Boolean
    <JsonProperty("invited")> Public Property Invited As Boolean
    <JsonProperty("authorized")> Public Property Authorized As Boolean
    <JsonProperty("before_mct")> Public Property BeforeMinCancelTime As Boolean
    <JsonProperty("startable")> Public Property Startable As Boolean
End Class