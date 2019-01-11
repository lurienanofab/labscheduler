Imports LNF
Imports LNF.Cache
Imports LNF.Email
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports Scheduler = LNF.Repository.Scheduler

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
            Return Request.SelectedPath().ResourceID
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

        Private Function GetEmailFromReservation(rsv As Scheduler.Reservation) As String
            Dim result As String = Page.ClientOrgManager.AccountEmail(rsv.Client, rsv.Account.AccountID)

            If String.IsNullOrEmpty(result) Then
                ' this happens with remote reservations because the user is not associated with the account
                result = Page.ClientManager.PrimaryEmail(rsv.Client)
            End If

            If String.IsNullOrEmpty(result) Then
                Throw New Exception(String.Format("Cannot find an email for {0} [{1}]", rsv.Client.DisplayName, rsv.Client.ClientID))
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
                        Dim client As ClientItem = DA.Current.Single(Of ClientInfo)(clientId).CreateClientItem()
                        If client IsNot Nothing Then
                            SetSendTo(client.DisplayName, client.Email)
                        Else
                            Throw New Exception(String.Format("Cannot find Client with ClientID = {0}", clientId))
                        End If
                    ElseIf reservationId > 0 Then
                        Dim rsv As Scheduler.Reservation = DA.Current.Single(Of Scheduler.Reservation)(reservationId)
                        If rsv IsNot Nothing Then
                            SetSendTo(rsv.Client.DisplayName, GetEmailFromReservation(rsv))
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

        Private Function GetRecentReservations(resourceId As Integer) As IList(Of Scheduler.Reservation)
            If resourceId = 0 Then
                Return DA.SchedulerRepository.SelectRecentReservations(Request.SelectedPath().ResourceID).ToList()
            Else
                Return DA.SchedulerRepository.SelectRecentReservations(resourceId).ToList()
            End If
        End Function

        Private Sub LoadReservations()
            Dim recentRsv As IList(Of Scheduler.Reservation) = GetRecentReservations(GetResourceID())
            ddlReservations.Items.Clear()
            ddlReservations.Items.Add(New ListItem("None"))
            For Each rsv As Scheduler.Reservation In recentRsv
                Dim newItem As New ListItem With {
                    .Value = rsv.ReservationID.ToString(),
                    .Text = rsv.BeginDateTime.ToString() + " - " + rsv.EndDateTime.ToString() + " Reserved by " + rsv.Client.DisplayName
                }

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
                Dim clients As IList(Of Scheduler.ResourceClientInfo) = ResourceClientUtility.SelectByResource(resourceId, authLevel).ToList()
                receiverAddr = String.Join(",", clients.Select(Function(x) x.Email))
            ElseIf clientId > 0 Then
                Dim client As ClientItem = DA.Current.Single(Of ClientInfo)(clientId).CreateClientItem()
                If client IsNot Nothing Then
                    receiverAddr = client.Email
                Else
                    Throw New Exception(String.Format("Cannot find Client with ClientID = {0}", clientId))
                End If
            ElseIf reservationId > 0 Then
                Dim rsv As Scheduler.Reservation = DA.Current.Single(Of Scheduler.Reservation)(reservationId)
                If rsv IsNot Nothing Then
                    receiverAddr = GetEmailFromReservation(rsv)
                Else
                    Throw New Exception(String.Format("Cannot find Reservation with ReservationID = {0}", reservationId))
                End If
            ElseIf GetAdminOnly() Then
                receiverAddr = Properties.Current.SchedulerEmail
            ElseIf String.IsNullOrEmpty(EmailAddr) Then
                If ddlSendTo.SelectedValue = "Administrator" Then
                    receiverAddr = Properties.Current.SchedulerEmail
                ElseIf ddlSendTo.SelectedValue = "Tool Engineers" Then
                    Dim res As ResourceItem = Request.SelectedPath().GetResource()
                    If res IsNot Nothing Then
                        Dim toolEng As IList(Of ResourceClientItem) = CacheManager.Current.ToolEngineers(res.ResourceID).ToList()
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

                Dim res As ResourceItem = Request.SelectedPath().GetResource()

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
                    cc = {Page.CurrentUser.Email}
                End If

                ' Send Email
                Dim args As New SendMessageArgs With {
                    .Caller = "LabScheduler.UserControls.Contact.btnSend_Click(object sender, EventArgs e)",
                    .ClientID = Page.CurrentUser.ClientID,
                    .Subject = txtSubject.Text,
                    .Body = body,
                    .From = Page.CurrentUser.Email,
                    .To = receiverAddr.Split(","c),
                    .Cc = cc,
                    .Bcc = GetContactBccEmails(),
                    .IsHtml = False
                }

                ServiceProvider.Current.Email.SendMessage(args)

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

        Private Function GetContactBccEmails() As String()
            Dim setting As String = ConfigurationManager.AppSettings("ContactBccEmails")
            If String.IsNullOrEmpty(setting) Then Return Nothing
            Return setting.Split(","c).ToArray()
        End Function
    End Class
End Namespace