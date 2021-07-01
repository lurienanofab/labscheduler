Imports System.IO
Imports LNF
Imports LNF.Data
Imports LNF.Scheduler
Imports LNF.Web
Imports Newtonsoft.Json

Public Class ReservationStateTester
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Request.HttpMethod = "POST" Then
            Dim json As String

            Try
                Dim command As String = Request.QueryString("command")

                Dim resp As ReservationStateResponse

                If command = "calc-state" Then
                    resp = CalculateReservationState()
                ElseIf command = "get-state" Then
                    resp = GetReservationState()
                Else
                    Throw New NotSupportedException($"Command not supported: {command}")
                End If

                json = JsonConvert.SerializeObject(resp)
            Catch ex As Exception
                Response.StatusCode = 500
                json = JsonConvert.SerializeObject(New With {ex.Message, ex.StackTrace})
            End Try

            Response.Clear()
            Response.ContentType = "application/json"
            Response.Write(json)
            Response.End()
        End If
    End Sub

    Private Function CalculateReservationState() As ReservationStateResponse
        Using reader As New StreamReader(Request.InputStream)
            Dim body As String = reader.ReadToEnd()
            Dim request As CalculateReservationStateRequest = JsonConvert.DeserializeObject(Of CalculateReservationStateRequest)(body)
            Dim util As ReservationStateUtility = ReservationStateUtility.Create(Date.Now)
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
                .StateText = state.ToString(),
                .ToolEngineer = request.ToolEngineer,
                .InLab = request.InLab,
                .Reserver = request.Reserver,
                .Invited = request.Invited,
                .Authorized = request.Authorized,
                .BeforeMinCancelTime = request.BeforeMinCancelTime,
                .Startable = request.Startable,
                .Started = False
            }

            Return resp
        End Using
    End Function

    Private Function GetReservationState() As ReservationStateResponse
        Using reader As New StreamReader(Request.InputStream)
            Dim body As String = reader.ReadToEnd()
            Dim request As GetReservationStateRequest = JsonConvert.DeserializeObject(Of GetReservationStateRequest)(body)
            Dim util As ReservationStateUtility = ReservationStateUtility.Create(request.Now)

            Dim provider As IProvider = [Global].Container.GetInstance(Of IProvider)()

            Dim rsv As IReservationWithInvitees = provider.Scheduler.Reservation.GetReservationWithInvitees(request.ReservationID)

            If rsv Is Nothing Then
                Throw New Exception($"Cannot find reservation with ReservationID = {request.ReservationID}")
            End If

            Dim client As IClient = provider.Data.Client.GetClient(request.ClientID)

            If client Is Nothing Then
                Throw New Exception($"Cannot find client with ClientID = {request.ClientID}")
            End If

            Dim res As IResource = rsv
            Dim resourceClients As IEnumerable(Of IResourceClient) = provider.Scheduler.Resource.GetResourceClients(res.ResourceID)

            Dim reservationClient As ReservationClient = ReservationClient.Create(rsv, client, resourceClients, request.InLab)

            Dim authLevel As ClientAuthLevel

            Dim everyone As IResourceClient = resourceClients.FirstOrDefault(Function(x) x.ClientID = -1)

            If everyone IsNot Nothing Then
                authLevel = everyone.AuthLevel
            Else
                Dim rc As IResourceClient = resourceClients.FirstOrDefault(Function(x) x.ClientID = request.ClientID)
                If rc IsNot Nothing Then
                    authLevel = rc.AuthLevel
                Else
                    authLevel = ClientAuthLevel.UnauthorizedUser
                End If
            End If

            Dim authorized As Boolean = reservationClient.IsAuthorized(rsv)

            Dim actualBeginDateTime As Date? = Nothing
            Dim actualEndDateTime As Date? = Nothing

            If request.UseActual Then
                actualBeginDateTime = rsv.ActualBeginDateTime
                actualEndDateTime = rsv.ActualEndDateTime
            End If

            Dim args As New ReservationStateArgs(request.ReservationID, request.InLab, reservationClient.IsReserver, reservationClient.IsInvited, authorized, rsv.IsRepair, rsv.IsFacilityDownTime, res.MinCancelTime, res.MinReservTime, rsv.BeginDateTime, rsv.EndDateTime, actualBeginDateTime, actualEndDateTime, authLevel)
            Dim state As ReservationState = util.GetReservationState(args)

            Dim resp As New ReservationStateResponse With {
                .State = state,
                .StateText = state.ToString(),
                .ToolEngineer = args.IsToolEngineer,
                .InLab = args.IsInLab,
                .Reserver = args.IsReserver,
                .Invited = args.IsInvited,
                .Authorized = args.IsAuthorized(),
                .BeforeMinCancelTime = args.IsBeforeMinCancelTime(request.Now),
                .Startable = args.IsStartable(request.Now),
                .Started = actualBeginDateTime.HasValue
            }

            Return resp
        End Using
    End Function
End Class

Public Class ReservationStateResponse
    Inherits CalculateReservationStateRequest
    <JsonProperty("state")> Public Property State As ReservationState
    <JsonProperty("stateText")> Public Property StateText As String
    <JsonProperty("started")> Public Property Started As Boolean
End Class

Public Class CalculateReservationStateRequest
    <JsonProperty("tool_engineer")> Public Property ToolEngineer As Boolean
    <JsonProperty("inlab")> Public Property InLab As Boolean
    <JsonProperty("reserver")> Public Property Reserver As Boolean
    <JsonProperty("invited")> Public Property Invited As Boolean
    <JsonProperty("authorized")> Public Property Authorized As Boolean
    <JsonProperty("before_mct")> Public Property BeforeMinCancelTime As Boolean
    <JsonProperty("startable")> Public Property Startable As Boolean
End Class

Public Class GetReservationStateRequest
    <JsonProperty("now")> Public Property Now As Date
    <JsonProperty("clientId")> Public Property ClientID As Integer
    <JsonProperty("reservationId")> Public Property ReservationID As Integer
    <JsonProperty("inlab")> Public Property InLab As Boolean
    <JsonProperty("useActual")> Public Property UseActual As Boolean
End Class

