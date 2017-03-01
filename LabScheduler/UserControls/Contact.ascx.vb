﻿'Copyright 2017 University of Michigan

'Licensed under the Apache License, Version 2.0 (the "License");
'you may Not use this file except In compliance With the License.
'You may obtain a copy Of the License at

'http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law Or agreed To In writing, software
'distributed under the License Is distributed On an "AS IS" BASIS,
'WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'See the License For the specific language governing permissions And
'limitations under the License.

Imports LNF
Imports LNF.Cache
Imports LNF.Data
Imports LNF.Email
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports repo = LNF.Repository.Scheduler

Namespace UserControls
    Public Class Contact
        Inherits SchedulerUserControl

        Public Function ReturnFromEmail() As String
            If Session("ReturnFromEmail") Is Nothing Then
                Return String.Empty
            Else
                Return Session("ReturnFromEmail").ToString()
            End If
        End Function

        Public ReadOnly Property EmailAddr As String
            Get
                Return Request.QueryString("EmailAddr")
            End Get
        End Property

        Public ReadOnly Property DisplayName As String
            Get
                Return Request.QueryString("DisplayName")
            End Get
        End Property

        Public Function GetResourceID() As Integer
            Return PathInfo.Current.ResourceID
        End Function

        Public Function GetReservationID() As Integer
            Dim result As Integer
            If Integer.TryParse(Request.QueryString("ReservationID"), result) Then
                Return result
            Else
                Return 0
            End If
        End Function

        Public Function GetClientID() As Integer
            Dim result As Integer
            If Integer.TryParse(Request.QueryString("ClientID"), result) Then
                Return result
            Else
                Return 0
            End If
        End Function

        Public Function GetAuthLevel() As ClientAuthLevel
            ' The paramter is named Privs but it's really a ClientAuthLevel
            Dim result As Integer
            If Integer.TryParse(Request.QueryString("Privs"), result) Then
                Return CType(result, ClientAuthLevel)
            Else
                Return 0
            End If
        End Function

        Public Function GetAdminOnly() As Boolean
            Dim result As Boolean
            Dim value As Integer
            If Integer.TryParse(Request.QueryString("AdminOnly"), value) Then
                If value = 1 Then
                    result = True
                Else
                    result = False
                End If
            Else
                result = False
            End If
            Return result
        End Function

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                Dim showCancel = True
                Dim showSendToSelect = False
                Dim clientId As Integer = GetClientID()
                Dim reservationId As Integer = GetReservationID()
                Dim privs As Integer = GetAuthLevel()
                phReservation.Visible = False

                Try
                    If GetAdminOnly() Then ' send to admin only - system contact page
                        SetSendTo("Administrator")
                        showCancel = False
                    ElseIf clientId > 0 Then
                        Dim client As ClientModel = CacheManager.Current.GetClient(clientId)
                        If client IsNot Nothing Then
                            SetSendTo(client.DisplayName, client.Email)
                        Else
                            Throw New Exception(String.Format("Cannot find Client with ClientID = {0}", clientId))
                        End If
                    ElseIf reservationId > 0 Then
                        Dim rsv As repo.Reservation = DA.Current.Single(Of repo.Reservation)(reservationId)
                        If rsv IsNot Nothing Then
                            SetSendTo(rsv.Client.DisplayName, rsv.Client.AccountEmail(rsv.Account.AccountID))
                        Else
                            Throw New Exception(String.Format("Cannot find Reservation with ReservationID = {0}", reservationId))
                        End If
                    ElseIf privs > 0 Then
                        Dim authLevel As ClientAuthLevel = CType(privs, ClientAuthLevel)
                        SetSendTo(GetAuthLevelDisplayName(authLevel))
                    ElseIf Not String.IsNullOrEmpty(EmailAddr) Then ' sending from grid, ResourceInfo or ResourceClients
                        If Not String.IsNullOrEmpty(DisplayName) Then
                            SetSendTo(DisplayName, EmailAddr)
                        Else
                            SetSendTo(EmailAddr)
                        End If
                    Else ' sending from res contact
                        showSendToSelect = True
                    End If
                Catch ex As Exception
                    phErrorMessage.Visible = True
                    litErrorMessage.Text = ex.Message
                    showSendToSelect = True
                End Try

                If showSendToSelect Then
                    phSendTo.Visible = True
                    phSendToText.Visible = False
                    litSendTo.Text = String.Empty
                    ddlSendTo.Items.Clear()
                    ddlSendTo.Items.Add(New ListItem("Tool Engineers"))
                    ddlSendTo.Items.Add(New ListItem("Administrator"))
                    phReservation.Visible = True
                    LoadReservations()
                End If

                If Not String.IsNullOrEmpty(ReturnFromEmail) AndAlso showCancel Then
                    hypCancel.NavigateUrl = ReturnFromEmail()
                    hypCancel.Visible = True
                Else
                    hypCancel.Visible = False
                End If
            End If
        End Sub

        Private Function GetAuthLevelDisplayName(authLevel As ClientAuthLevel) As String
            Select Case authLevel
                Case ClientAuthLevel.ToolEngineer
                    Return "Tool Engineers"
                Case ClientAuthLevel.SuperUser
                    Return "Super Users"
                Case ClientAuthLevel.Trainer
                    Return "Trainers"
                Case ClientAuthLevel.AuthorizedUser
                    Return "Users"
                Case ClientAuthLevel.AuthorizedUser Or ClientAuthLevel.SuperUser Or ClientAuthLevel.Trainer Or ClientAuthLevel.ToolEngineer Or ClientAuthLevel.RemoteUser
                    Return "All Users"
                Case 0
                    Return "[undefined]"
                Case Else
                    Return authLevel.ToString()
            End Select
        End Function

        Private Sub SetSendTo(displayName As String, Optional email As String = Nothing)
            phSendTo.Visible = False
            phSendToText.Visible = True
            If String.IsNullOrEmpty(email) Then
                litSendTo.Text = String.Format("{0}", displayName)
            Else
                litSendTo.Text = String.Format("{0} ({1})", displayName, email)
            End If
        End Sub

        Private Function GetRecentReservations(resourceId As Integer) As IList(Of repo.Reservation)
            If resourceId = 0 Then
                Return DA.Scheduler.Reservation.SelectRecent(PathInfo.Current.ResourceID)
            Else
                Return DA.Scheduler.Reservation.SelectRecent(resourceId)
            End If
        End Function

        Private Sub LoadReservations()
            Dim recentRsv As IList(Of repo.Reservation) = GetRecentReservations(GetResourceID())
            ddlReservations.Items.Clear()
            ddlReservations.Items.Add(New ListItem("None"))
            For Each rsv As repo.Reservation In recentRsv
                Dim newItem As New ListItem
                newItem.Value = rsv.ReservationID.ToString()
                newItem.Text = rsv.BeginDateTime.ToString() + " - " + rsv.EndDateTime.ToString() + " Reserved by " + rsv.Client.DisplayName
                ddlReservations.Items.Add(newItem)
            Next
        End Sub

        Protected Function GetReceiverAddress() As String
            ' Get Receiver Address
            Dim receiverAddr As String = String.Empty
            Dim clientId As Integer = GetClientID()
            Dim reservationId As Integer = GetReservationID()
            Dim resourceId As Integer = GetResourceID()
            Dim authLevel As ClientAuthLevel = GetAuthLevel()

            If resourceId > 0 AndAlso authLevel > 0 Then
                Dim clients As IList(Of repo.ResourceClientInfo) = ResourceClientUtility.SelectByResource(resourceId, authLevel).ToList()
                receiverAddr = String.Join(",", clients.Select(Function(x) x.Email))
            ElseIf clientId > 0 Then
                Dim client As ClientModel = CacheManager.Current.GetClient(clientId)
                If client IsNot Nothing Then
                    receiverAddr = client.Email
                Else
                    Throw New Exception(String.Format("Cannot find Client with ClientID = {0}", clientId))
                End If
            ElseIf reservationId > 0 Then
                Dim rsv As repo.Reservation = DA.Current.Single(Of repo.Reservation)(reservationId)
                If rsv IsNot Nothing Then
                    receiverAddr = rsv.Client.AccountEmail(rsv.Account.AccountID)
                Else
                    Throw New Exception(String.Format("Cannot find Reservation with ReservationID = {0}", reservationId))
                End If
            ElseIf GetAdminOnly() Then
                receiverAddr = Properties.Current.SchedulerEmail
            ElseIf String.IsNullOrEmpty(EmailAddr) Then
                If ddlSendTo.SelectedValue = "Administrator" Then
                    receiverAddr = Properties.Current.SchedulerEmail
                ElseIf ddlSendTo.SelectedValue = "Tool Engineers" Then
                    Dim res As ResourceModel = PathInfo.Current.GetResource()
                    If res IsNot Nothing Then
                        Dim toolEng As IList(Of ResourceClientModel) = CacheManager.Current.ToolEngineers(res.ResourceID)
                        If toolEng.Count > 0 Then
                            receiverAddr = String.Join(",", toolEng.Select(Function(x) x.Email))
                        End If
                    End If
                End If
            Else
                receiverAddr = EmailAddr
            End If

            If String.IsNullOrEmpty(receiverAddr) Then
                Throw New Exception("There are no recipients specified. Email not sent.")
            End If

            '2007-11-14 Sandrine wants to get all emails sent out to group from Scheduler
            '2012-07-09 Sandrine no longer wants this (I just left the setting blank in web.config)
            Dim emailContentAuditAdmin As String = ConfigurationManager.AppSettings("EmailContentAuditAdmin")
            If Not String.IsNullOrEmpty(emailContentAuditAdmin) Then
                receiverAddr += If(String.IsNullOrEmpty(receiverAddr), emailContentAuditAdmin, "," + emailContentAuditAdmin)
            End If

            Return receiverAddr
        End Function

        Protected Sub btnSend_Click(sender As Object, e As EventArgs)
            phErrorMessage.Visible = False
            litErrorMessage.Text = String.Empty

            phSuccessMessage.Visible = False
            litSuccessMessage.Text = String.Empty

            If String.IsNullOrEmpty(txtSubject.Text) Then
                phErrorMessage.Visible = True
                litErrorMessage.Text = "Please enter a subject."
                Return
            End If

            Try
                Dim receiverAddr As String = GetReceiverAddress()

                ' Get Reference Reservation
                Dim body As String = String.Empty
                Dim sb As New StringBuilder()

                Dim res As ResourceModel = PathInfo.Current.GetResource()

                If res IsNot Nothing Then
                    sb.AppendLine("Resource: " + res.ResourceName)
                Else
                    sb.AppendLine("Resource: unspecified")
                End If

                If ddlReservations.Items.Count > 0 Then
                    If ddlReservations.SelectedValue <> "None" Then
                        sb.AppendLine("Referenced Reservation: " + ddlReservations.SelectedItem.Text)
                    End If
                End If

                sb.AppendLine(Environment.NewLine + "--------------------------------------------------" + Environment.NewLine)
                sb.AppendLine(txtBody.Text)

                'Print tool name on email so the recipients know which tool this email refers to
                If res IsNot Nothing Then
                    sb.AppendLine(Environment.NewLine + "** This email is sent via Scheduler, the tool is " + res.ResourceName + " **")
                End If

                body = sb.ToString()

                Dim cc As String() = Nothing
                If chkCC.Checked Then
                    cc = {CacheManager.Current.CurrentUser.Email}
                End If

                ' Send Email
                Dim args As New SendMessageArgs()
                args.Caller = "LabScheduler.UserControls.Contact.btnSend_Click(object sender, EventArgs e)"
                args.ClientID = CacheManager.Current.ClientID
                args.Subject = txtSubject.Text
                args.Body = body
                args.From = CacheManager.Current.Email
                args.To = receiverAddr.Split(","c)
                args.Cc = cc
                args.IsHtml = False

                Dim result As SendMessageResult = Providers.Email.SendMessage(args)

                If result.Exception IsNot Nothing Then
                    Throw result.Exception
                End If

                phSuccessMessage.Visible = True
                litSuccessMessage.Text = "Your email has been sent successfully."

                If Not String.IsNullOrEmpty(ReturnFromEmail) Then
                    Response.Redirect(ReturnFromEmail)
                End If
            Catch ex As Exception
                phErrorMessage.Visible = True
                litErrorMessage.Text = String.Format("<strong>Error in sending email:</strong><p>{0}</p>", ex.Message)
            End Try
        End Sub
    End Class
End Namespace