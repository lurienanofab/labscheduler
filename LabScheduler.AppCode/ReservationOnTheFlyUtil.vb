Imports System.Configuration
Imports System.Web
Imports LNF.DataAccess
Imports LNF.Impl.Repository.Scheduler
Imports LNF.Repository

Public Class ReservationOnTheFlyUtil

    Public Shared Function IsValidIP(ByVal context As HttpContext) As Boolean
        Dim ips = ValidateIPAddress(context)
        If Nothing = ips Then
            Return False
        End If
        Return True
    End Function

    Public Shared Function IsValidIP(ipaddr As String) As Boolean
        Dim ips = ValidateIPAddress(ipaddr)
        If Nothing = ips Then
            Return False
        End If
        Return True
    End Function

    Public Shared Function ValidateIPAddress(ByVal context As HttpContext) As String
        Dim UserIPAddress As String
        UserIPAddress = context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If UserIPAddress = "" Then
            UserIPAddress = context.Request.ServerVariables("REMOTE_ADDR")
        End If

        Return ValidateIPAddress(UserIPAddress)
    End Function

    Public Shared Function ValidateIPAddress(ipaddr As String) As String
        If InIPWhitelist(ipaddr) Then
            Return ipaddr
        Else
            Return Nothing
        End If
    End Function

    Private Shared Function GetWhitelist() As String()
        Dim result As New List(Of String)()

        Dim raw As String = ConfigurationManager.AppSettings("IPWhitelist")

        If String.IsNullOrEmpty(raw) Then
            raw = "192.168.1.*,141.213.6.*,127.0.0.1"
        End If

        If Not String.IsNullOrEmpty(raw) Then
            result.AddRange(raw.Split(Convert.ToChar(",")))
        End If

        Return result.ToArray()
    End Function

    Private Shared Function InIPWhitelist(requestIp As String) As Boolean
        Dim list As String() = GetWhitelist()

        ' if no list provided then we must want everyone to access because when would you ever want no one at all?
        If list.Length = 0 Then
            Return True
        End If

        If String.IsNullOrEmpty(requestIp) Then
            Return False
        End If

        ' 1st check for exact match
        If list.Contains(requestIp) Then
            Return True
        End If

        ' 2nd check for StartsWith
        Dim matches As String() = list.Where(Function(x) x.EndsWith("*") AndAlso requestIp.StartsWith(x.TrimEnd(Convert.ToChar("*")))).ToArray()
        If matches.Length > 0 Then
            Return True
        End If

        Return False
    End Function

    Public Shared Function GetOnTheFlyResource(session As ISession, cardswipedata As String, buttonIndex As Integer) As OnTheFlyResource
        Dim readerName As String = GetReaderName(cardswipedata)
        Return session.Query(Of OnTheFlyResource).FirstOrDefault(Function(x) x.CardReaderName = readerName AndAlso x.ButtonIndex = buttonIndex)
    End Function

    Public Shared Function GetCardNumber(cardswipedata As String) As String
        Return cardswipedata.Substring(3)
    End Function

    Private Shared Function GetReaderName(cardswipedata As String) As String
        Return cardswipedata.Substring(0, 3)
    End Function
End Class

Public Class ResRequest
    Public Status As Integer
    Public ReservationID As Integer
    Public ReservationTime As Double
    Public Details As String
    Public ResourceID As Integer
    Public CardNum As String
    Public IPAddress As String
    Public OnTheFlyName As String

    Public Function GetDataObject() As Object
        Return New With {
            Status,
            Details,
            ReservationTime,
            ReservationID
        }
    End Function
End Class


