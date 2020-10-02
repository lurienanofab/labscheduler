Imports LNF
Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ReservationHistory
        Inherits SchedulerPage

        Private _EditReservation As IReservation

        Private _holidays As IEnumerable(Of IHoliday)
        Private _maxForgivenDay As Integer = Integer.Parse(Utility.GetRequiredAppSetting("MaxForgivenDay"))

        Private Function GetClient() As IClient
            Dim c As IClient = Nothing
            Dim cid As Integer = 0
            Dim trUserVisible = True 'trUser.Visible
            If trUserVisible Then
                If Not Integer.TryParse(ddlClients.SelectedValue, cid) Then
                    c = CurrentUser
                Else
                    c = Provider.Data.Client.GetClient(cid)
                End If
            End If
            Return c
        End Function

        Public ReadOnly Property EditReservationID As Integer
            Get
                Dim reservationId As Integer
                If Integer.TryParse(Request.QueryString("ReservationID"), reservationId) Then
                    Return reservationId
                Else
                    Return 0
                End If
            End Get
        End Property

        Public ReadOnly Property EditReservation As IReservation
            Get
                If _EditReservation Is Nothing OrElse _EditReservation.ReservationID <> EditReservationID Then
                    If EditReservationID > 0 Then
                        _EditReservation = Provider.Scheduler.Reservation.GetReservation(EditReservationID)
                    Else
                        _EditReservation = Nothing
                    End If
                End If

                Return _EditReservation
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                LoadClients()
                If EditReservationID = 0 Then
                    LoadReservationHistory()
                Else
                    LoadEditForm()
                End If
            End If
        End Sub

        Private Sub LoadClients()
            ddlClients.DataSource = SelectReservationHistoryClients()
            ddlClients.DataBind()

            If EditReservation IsNot Nothing Then
                Session("SelectedClientID") = EditReservation.ClientID.ToString()
            End If

            If Session("SelectedClientID") Is Nothing Then
                ddlClients.SelectedValue = CurrentUser.ClientID.ToString()
            Else
                ddlClients.SelectedValue = Session("SelectedClientID").ToString()
            End If

            Dim range As Integer
            Dim sd, ed As Date?

            If Session("SelectedRange") Is Nothing Then
                range = Integer.Parse(ddlRange.SelectedValue)
            Else
                range = Convert.ToInt32(Session("SelectedRange"))
                ddlRange.SelectedValue = range.ToString()
            End If

            Select Case range
                Case 0 '30 days
                    ed = Date.Now.Date
                    sd = Date.Now.Date.AddDays(-30)
                Case 1 '3 months
                    ed = Date.Now.Date
                    sd = Date.Now.Date.AddMonths(-3)
                Case 2 '1 year
                    ed = Date.Now.Date
                    sd = Date.Now.Date.AddYears(-1)
                Case 3 'All
                    ed = Nothing
                    sd = Nothing
            End Select

            If Session("SelectedStartDate") Is Nothing OrElse String.IsNullOrEmpty(Session("SelectedStartDate").ToString()) Then
                If sd.HasValue Then
                    txtStartDate.Text = sd.Value.ToString("MM/dd/yyyy")
                Else
                    txtStartDate.Text = String.Empty
                End If
            Else
                sd = Convert.ToDateTime(Session("SelectedStartDate"))
                txtStartDate.Text = sd.Value.ToString("MM/dd/yyyy")
            End If

            If Session("SelectedEndDate") Is Nothing OrElse String.IsNullOrEmpty(Session("SelectedEndDate").ToString()) Then
                If ed.HasValue Then
                    txtEndDate.Text = ed.Value.ToString("MM/dd/yyyy")
                Else
                    txtEndDate.Text = String.Empty
                End If
            Else
                ed = Convert.ToDateTime(Session("SelectedEndDate"))
                txtEndDate.Text = ed.Value.ToString("MM/dd/yyyy")
            End If

            Session("SelectedRange") = ddlRange.SelectedValue
            Session("SelectedStartDate") = txtStartDate.Text
            Session("SelectedEndDate") = txtEndDate.Text
        End Sub

        Private Sub LoadReservationHistory()
            Dim includeCanceledForModification = CacheManager.Current.ShowCanceledForModification() AndAlso CurrentUser.HasPriv(ClientPrivilege.Staff)

            Dim sd As Date = ReservationHistoryUtility.GetStartDate(txtStartDate.Text)
            Dim ed As Date = ReservationHistoryUtility.GetEndDate(txtEndDate.Text)

            Dim client As IClient = GetClient()

            Session("SelectedClientID") = client.ClientID
            Session("SelectedRange") = Integer.Parse(ddlRange.SelectedValue)
            Session("SelectedStartDate") = If(String.IsNullOrEmpty(txtStartDate.Text), Nothing, Date.Parse(txtStartDate.Text))
            Session("SelectedEndDate") = If(String.IsNullOrEmpty(txtEndDate.Text), Nothing, Date.Parse(txtEndDate.Text))

            rptHistory.DataSource = ReservationHistoryUtility.GetReservationHistoryData(Provider, client, sd, ed, includeCanceledForModification).OrderByDescending(Function(x) x.BeginDateTime).ToList()
            rptHistory.DataBind()

            ' Display datagrid

            Dim trUserVisible As Boolean = True 'trUser.Visible

            If trUserVisible AndAlso rptHistory.Items.Count = 0 Then
                rptHistory.Visible = False
                'lblNoData.Text = "No past reservations were found for " + ddlClients.SelectedItem.Text
                'lblNoData.Visible = True
            ElseIf Not trUserVisible AndAlso rptHistory.Items.Count = 0 Then
                rptHistory.Visible = False
                'lblNoData.Text = ""
                'lblNoData.Visible = True
            Else
                rptHistory.Visible = True
                'lblNoData.Visible = False

                If (includeCanceledForModification) Then
                    litShowCanceledForModificationMessage.Text = "<em class=""canceled-for-modification-message"">Red text indicates the reservation was canceled for modification.</em>"
                Else
                    litShowCanceledForModificationMessage.Text = String.Empty
                End If
            End If

            phSelectHistory.Visible = True
            phEditHistory.Visible = False
        End Sub

        Private Function GetNotes(obj As Object) As String
            Dim defval As String = "None"
            If obj Is DBNull.Value Then Return defval
            If obj Is Nothing Then Return defval
            If String.IsNullOrEmpty(obj.ToString()) Then Return defval
            Return obj.ToString()
        End Function

        Private Sub ShowAlert(message As String)
            Page.ClientScript.RegisterStartupScript([GetType](), "alert_" + Guid.NewGuid().ToString(), $"alert('{message}');", True)
        End Sub

        Private Sub LoadEditForm()
            Dim holidays As IEnumerable(Of IHoliday) = GetHolidays()
            Dim canForgive As Boolean = ReservationHistoryUtility.ReservationCanBeForgiven(CurrentUser, EditReservation, Date.Now, _maxForgivenDay, holidays)
            Dim canChangeAcct As Boolean = ReservationHistoryUtility.ReservationAccountCanBeChanged(CurrentUser, EditReservation, Date.Now, holidays)
            Dim canChangeNotes As Boolean = ReservationHistoryUtility.ReservationNotesCanBeChanged(CurrentUser, EditReservation)

            litReservationID.Text = EditReservation.ReservationID.ToString()
            litResourceName.Text = EditReservation.ResourceDisplayName
            litActivityName.Text = EditReservation.ActivityName
            litIsStarted.Text = If(EditReservation.IsStarted, "Yes", "No")
            litIsCanceled.Text = If(IsCanceled(EditReservation.IsActive), "Yes", "No")
            litInvitees.Text = InviteeListHTML()
            litCurrentAccount.Text = EditReservation.AccountName
            litForgiveChargeNote.Text = ConfigurationManager.AppSettings("ForgiveChargeNote")
            trAccount.Visible = canChangeAcct

            '2012-10-23 It's possible that there are no available accounts. For example
            'a remote processing run where no one was ever invited.
            Dim availAccts As List(Of IClientAccount) = Provider.Scheduler.Reservation.AvailableAccounts(EditReservation).ToList()
            Dim accts As IEnumerable(Of IAccount) = Nothing

            If availAccts IsNot Nothing Then
                accts = availAccts
            End If

            If accts IsNot Nothing Then
                ddlEditReservationAccount.DataSource = Accounts.ConvertToAccountTable(accts.ToList())
                ddlEditReservationAccount.DataBind()
            Else
                ddlEditReservationAccount.Visible = False
                litEditReservationAccountMessage.Text = "<div>No accounts are available for this reservation</div>"
            End If

            Dim item As WebControls.ListItem = ddlEditReservationAccount.Items.FindByValue(EditReservation.AccountID.ToString())

            If item IsNot Nothing Then
                ddlEditReservationAccount.SelectedValue = EditReservation.AccountID.ToString()
            End If

            ddlEditReservationAccount.Enabled = canChangeAcct
            litReservedBeginDateTime.Text = EditReservation.BeginDateTime.ToString("MM/dd/yyyy hh:mm:ss tt")
            litReservedEndDateTime.Text = EditReservation.EndDateTime.ToString("MM/dd/yyyy hh:mm:ss tt")
            litReservedRegularDuration.Text = EditReservation.GetReservedDuration().TotalMinutes.ToString("#0.0")

            If (EditReservation.IsStarted) Then
                litActualBeginDateTime.Text = If(EditReservation.ActualBeginDateTime Is Nothing, "--", EditReservation.ActualBeginDateTime.Value.ToString("MM/dd/yyyy hh:mm:ss tt"))
                litActualEndDateTime.Text = If(EditReservation.ActualEndDateTime Is Nothing, "--", EditReservation.ActualEndDateTime.Value.ToString("MM/dd/yyyy hh:mm:ss tt"))
                litActualRegularDuration.Text = EditReservation.GetActualDuration().TotalMinutes.ToString("#0.0")
                litActualOvertimeDuration.Text = EditReservation.GetOvertimeDuration().TotalMinutes.ToString("#0.0")
            Else
                litActualBeginDateTime.Text = "--"
                litActualEndDateTime.Text = "--"
                litActualRegularDuration.Text = "--"
                litActualOvertimeDuration.Text = "--"
            End If

            litChargeableBeginDateTime.Text = EditReservation.ChargeBeginDateTime.ToString("MM/dd/yyyy hh:mm:ss tt")
            litChargeableEndDateTime.Text = EditReservation.ChargeEndDateTime.ToString("MM/dd/yyyy hh:mm:ss tt")
            litChargeableRegularDuration.Text = EditReservation.GetChargeDuration().TotalMinutes.ToString("#0.0")
            litChargeableOvertimeDuration.Text = If(EditReservation.IsStarted, EditReservation.GetOvertimeDuration().TotalMinutes.ToString("#0.0"), "0.0")
            litForgiveAmount.Text = (1 - EditReservation.ChargeMultiplier).ToString("#0.0%")
            trForgiveForm.Visible = canForgive
            txtForgiveAmount.Text = String.Empty

            txtNotes.Text = EditReservation.Notes
            txtNotes.Enabled = canChangeNotes

            If canForgive Or canChangeAcct OrElse canChangeNotes Then
                chkEmailClient.Visible = CurrentUser.ClientID <> EditReservation.ClientID
                btnEditSave.Visible = True
            Else
                chkEmailClient.Visible = False
                btnEditSave.Visible = False
            End If

            If Request.QueryString("Event") = "EditReservationSave" Then
                SetSaveAlert()
                phUpdateBilling.Visible = UpdateBilling()
                divUpdateBilling.Attributes("data-client-id") = EditReservation.ClientID.ToString()
                divUpdateBilling.Attributes("data-period") = EditReservation.ChargeBeginDateTime.ToString("yyyy-MM-01")
                divUpdateBilling.Attributes("data-ajax-url") = VirtualPathUtility.ToAbsolute("~/ajax/reservation.ashx")
            End If

            phEditHistory.Visible = True
            phSelectHistory.Visible = False
        End Sub

        Private Function InviteeListHTML() As String
            Dim sb As New StringBuilder()
            For Each item As IReservationInvitee In Provider.Scheduler.Reservation.GetInvitees(EditReservationID)
                sb.AppendLine($"<div>{item.DisplayName}</div>")
            Next
            Return sb.ToString()
        End Function

        ' this overload is called from the aspx page
        Protected Overloads Function IsBeforeForgiveCutoff(item As Web.Scheduler.ReservationHistoryItem) As Boolean
            Return ReservationHistoryUtility.IsBeforeForgiveCutoff(item, Date.Now, _maxForgivenDay, GetHolidays())
        End Function

        Protected Sub BtnSearchHistory_Click(sender As Object, e As EventArgs)
            LoadReservationHistory()
        End Sub

        Protected Sub ReservationHistory_Command(sender As Object, e As CommandEventArgs)
            litEditMessage.Text = String.Empty
            Select Case e.CommandName
                Case "cancel"
                    EditReservationCancel()
                Case "save"
                    EditReservationSave()
            End Select
        End Sub

        Private Sub EditReservationCancel()
            phEditHistory.Visible = False
            phSelectHistory.Visible = True
            LoadReservationHistory()
        End Sub

        Private Sub EditReservationSave()
            Dim alertText As String = String.Empty
            Dim alertType As String = "success"
            Dim updateBilling As Boolean = False

            If EditReservationID > 0 Then
                Dim accountId As Integer = Integer.Parse(ddlEditReservationAccount.SelectedValue)
                Dim forgivenPct As Double? = GetForgiveAmount()
                Dim period As Date = EditReservation.ChargeBeginDateTime.FirstOfMonth()
                Dim temp As Boolean = Utility.IsCurrentPeriod(period)
                Dim clientId As Integer = EditReservation.ClientID

                Dim result = Provider.Scheduler.Reservation.SaveReservationHistory(EditReservation, accountId, forgivenPct, txtNotes.Text, chkEmailClient.Checked)

                If result.ReservationUpdated Then
                    alertText += "<strong>Reservation updated OK!</strong>"
                Else
                    alertText += "<strong>Reservation update failed.</strong>"
                    alertType = "danger"
                End If

                updateBilling = result.UpdateBilling AndAlso Not temp 'do not update if period is current month (temp is true)
            Else
                alertText += "<strong>Invalid ReservationID.</strong>"
                alertType = "danger"
            End If

            Session("EditReservationSave.AlertText") = alertText
            Session("EditReservationSave.AlertType") = alertType

            Dim redirectUrl = $"~/ReservationHistory.aspx?Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}&ReservationID={EditReservationID}&Event=EditReservationSave&UpdateBilling={updateBilling}"
            Response.Redirect(redirectUrl, True)
        End Sub

        Protected Function IsCanceled(obj As Object) As Boolean
            If obj Is DBNull.Value Then
                Return True
            Else
                Dim isActive = Convert.ToBoolean(obj)
                Return Not isActive
            End If
        End Function

        Protected Function GetRowCssClass(item As Web.Scheduler.ReservationHistoryItem) As String
            If item.IsCanceledForModification Then
                Return "canceled-for-modification"
            Else
                Return String.Empty
            End If
        End Function

        Protected Function GetEditUrl(item As Web.Scheduler.ReservationHistoryItem) As String
            Return String.Format("~/ReservationHistory.aspx?Date={0:yyyy-MM-dd}&ReservationID={1}", ContextBase.Request.SelectedDate(), item.ReservationID)
        End Function

        Private Function UpdateBilling() As Boolean
            Return Request.QueryString("UpdateBilling") = "True"
        End Function

        Private Sub SetSaveAlert()
            If Session("EditReservationSave.AlertText") IsNot Nothing Then
                Dim alertText As String = Session("EditReservationSave.AlertText").ToString()
                Session.Remove("EditReservationSave.AlertText")
                Dim alertType As String
                If Session("EditReservationSave.AlertType") IsNot Nothing Then
                    alertType = Session("EditReservationSave.AlertType").ToString()
                    Session.Remove("EditReservationSave.AlertType")
                Else
                    alertType = "danger"
                End If
                ShowSaveAlert(alertText, alertType)
            End If
        End Sub

        Private Sub ShowSaveAlert(text As String, Optional type As String = "danger")
            If String.IsNullOrEmpty(text) Then
                phSaveAlert.Visible = False
                litSaveAlertText.Text = String.Empty
            Else
                phSaveAlert.Visible = True
                litSaveAlertText.Text = text
            End If

            divSaveAlert.Attributes("class") = $"alert alert-{type} alert-dismissible"
        End Sub

        Private Function GetForgiveAmount() As Double?
            Dim result As Double? = Nothing

            If Not String.IsNullOrEmpty(txtForgiveAmount.Text) Then
                Dim d As Double
                If Double.TryParse(txtForgiveAmount.Text, d) Then
                    result = d
                Else
                    result = 0
                End If
            End If

            Return result
        End Function

        Private Function GetHolidays() As IEnumerable(Of IHoliday)
            If _holidays Is Nothing Then
                Dim sd As Date
                Dim ed As Date

                If EditReservation Is Nothing Then
                    sd = ReservationHistoryUtility.GetStartDate(txtStartDate.Text)
                    ed = ReservationHistoryUtility.GetEndDate(txtEndDate.Text)
                Else
                    sd = EditReservation.ChargeBeginDateTime().FirstOfMonth()
                    ed = EditReservation.ChargeEndDateTime().FirstOfMonth().AddMonths(1)
                End If

                _holidays = Utility.GetHolidays(sd, ed)
            End If

            Return _holidays
        End Function

        Private Function SelectReservationHistoryClients() As IEnumerable(Of IClient)
            ' Dim canViewEveryone = CurrentUser.HasPriv(ClientPrivilege.Staff Or ClientPrivilege.Administrator Or ClientPrivilege.Developer)

            ' allow everyone to see other users history
            Dim canViewEveryone As Boolean = True

            Dim priv As ClientPrivilege = ClientPrivilege.LabUser Or ClientPrivilege.Staff

            Dim sd As Date = ReservationHistoryUtility.GetStartDate(txtStartDate.Text)
            Dim ed As Date = ReservationHistoryUtility.GetEndDate(txtEndDate.Text)

            Dim result As IEnumerable(Of IClient) = Provider.Data.Client.GetActiveClients(sd, ed, priv:=priv) _
                .Where(Function(x) canViewEveryone OrElse x.ClientID = CurrentUser.ClientID) _
                .OrderBy(Function(x) x.DisplayName) _
                .ToList()

            Return result
        End Function
    End Class
End Namespace