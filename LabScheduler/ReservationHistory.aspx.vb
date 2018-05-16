Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Models.Data
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler.Data
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports Scheduler = LNF.Repository.Scheduler

Namespace Pages
    Public Class ReservationHistory
        Inherits SchedulerPage

        Private _EditReservation As Scheduler.Reservation

        Private Function GetClient() As ClientItem
            Dim c As ClientItem = Nothing
            Dim cid As Integer = 0
            If trUser.Visible Then
                If Not Integer.TryParse(ddlClients.SelectedValue, cid) Then
                    c = CurrentUser
                Else
                    c = CacheManager.Current.GetClient(cid)
                End If
            End If
            Return c
        End Function

        Public ReadOnly Property EditReservation As Scheduler.Reservation
            Get
                If _EditReservation Is Nothing OrElse _EditReservation.ReservationID.ToString() <> hidEditReservationID.Value Then
                    Dim reservationId As Integer
                    If Integer.TryParse(hidEditReservationID.Value, reservationId) Then
                        If reservationId > 0 Then
                            _EditReservation = DA.Current.Single(Of Scheduler.Reservation)(reservationId)
                        Else
                            _EditReservation = Nothing
                        End If

                    End If
                End If

                Return _EditReservation
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            divReservationHistory.Attributes("data-client-id") = CacheManager.Current.ClientID.ToString()

            Dim reservationId As Integer

            If Not String.IsNullOrEmpty(Request.QueryString("ReservationID")) Then
                If Not Integer.TryParse(Request.QueryString("ReservationID"), reservationId) Then
                    reservationId = 0
                End If
            End If

            hidEditReservationID.Value = reservationId.ToString()

            If Not Page.IsPostBack Then
                LoadClients()
                If reservationId <> 0 Then
                    LoadEditForm()
                Else
                    LoadReservationHistory()
                End If
            End If
        End Sub

        Private Sub LoadClients()
            Dim dtClients = ResourceClientData.SelectReservHistoryClient(CurrentUser.ClientID)

            ddlClients.DataSource = dtClients
            ddlClients.DataBind()

            If dtClients.Rows.Count > 0 Then
                trUser.Visible = True

                If EditReservation IsNot Nothing Then
                    Session("SelectedClientID") = EditReservation.Client.ClientID.ToString()
                End If

                If Session("SelectedClientID") Is Nothing Then
                    ddlClients.SelectedValue = CurrentUser.ClientID.ToString()
                Else
                    ddlClients.SelectedValue = Session("SelectedClientID").ToString()
                End If
            Else
                trUser.Visible = False
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

            Dim client As ClientItem = GetClient()

            Session("SelectedClientID") = client.ClientID
            Session("SelectedRange") = Integer.Parse(ddlRange.SelectedValue)
            Session("SelectedStartDate") = If(String.IsNullOrEmpty(txtStartDate.Text), Nothing, Date.Parse(txtStartDate.Text))
            Session("SelectedEndDate") = If(String.IsNullOrEmpty(txtEndDate.Text), Nothing, Date.Parse(txtEndDate.Text))

            rptHistory.DataSource = ReservationHistoryUtility.GetReservationHistoryData(client, sd, ed, includeCanceledForModification).OrderByDescending(Function(x) x.BeginDateTime).ToList()
            rptHistory.DataBind()

            ' Display datagrid
            If trUser.Visible AndAlso rptHistory.Items.Count = 0 Then
                rptHistory.Visible = False
                lblNoData.Text = "No past reservations were found for " + ddlClients.SelectedItem.Text
                lblNoData.Visible = True
            ElseIf Not trUser.Visible AndAlso rptHistory.Items.Count = 0 Then
                rptHistory.Visible = False
                lblNoData.Text = "No past reservations were found"
                lblNoData.Visible = True
            Else
                rptHistory.Visible = True
                lblNoData.Visible = False

                If (includeCanceledForModification) Then
                    litShowCanceledForModificationMessage.Text = "<em class=""canceled-for-modification-message"">Red text indicates the reservation was canceled for modification.</em>"
                Else
                    litShowCanceledForModificationMessage.Text = String.Empty
                End If
            End If

            panHistory.Visible = True
            panEditHistory.Visible = False
            hidEditReservationID.Value = String.Empty
            hidSelectedClientID.Value = String.Empty

            panCanForgiveNotice.Visible = Not lblNoData.Visible
        End Sub

        Private Function GetNotes(obj As Object) As String
            Dim defval As String = "None"
            If obj Is DBNull.Value Then Return defval
            If obj Is Nothing Then Return defval
            If String.IsNullOrEmpty(obj.ToString()) Then Return defval
            Return obj.ToString()
        End Function

        Private Sub ShowAlert(message As String)
            Page.ClientScript.RegisterStartupScript([GetType](), "alert_" + Guid.NewGuid().ToString(), String.Format("alert('{0}');", message), True)
        End Sub

        Private Sub LoadEditForm()
            Dim canForgive As Boolean = ReservationHistoryUtility.ReservationCanBeForgiven(CurrentUser, EditReservation, Date.Now)
            Dim canChangeAcct As Boolean = ReservationHistoryUtility.ReservationAccountCanBeChanged(CurrentUser, EditReservation, Date.Now)
            Dim canChangeNotes As Boolean = ReservationHistoryUtility.ReservationNotesCanBeChanged(CurrentUser, EditReservation)

            Dim sd As Date = EditReservation.ChargeBeginDateTime().FirstOfMonth()
            Dim ed As Date = EditReservation.ChargeEndDateTime().FirstOfMonth().AddMonths(1)

            hidSelectedClientID.Value = ddlClients.SelectedValue
            hidEditReservationID.Value = EditReservation.ReservationID.ToString()
            hidStartDate.Value = sd.ToString("yyyy-MM-dd")
            hidEndDate.Value = ed.ToString("yyyy-MM-dd")
            litReservationID.Text = EditReservation.ReservationID.ToString()
            litResourceName.Text = EditReservation.Resource.ResourceName
            litActivityName.Text = EditReservation.Activity.ActivityName
            litIsStarted.Text = If(EditReservation.IsStarted, "Yes", "No")
            litIsCanceled.Text = If(IsCanceled(EditReservation.IsActive), "Yes", "No")
            trInvitees.Visible = ReservationManager.GetInvitees(EditReservation).Count() > 0
            litInvitees.Text = InviteeListHTML()
            litCurrentAccount.Text = EditReservation.Account.Name
            trAccount.Visible = canChangeNotes

            '2012-10-23 It's possible that there are no available accounts. For example
            'a remote processing run where no one was ever invited.
            Dim availAccts As List(Of ClientAccount) = ReservationManager.AvailableAccounts(EditReservation).ToList()
            Dim accts As IEnumerable(Of Account) = Nothing
            If availAccts IsNot Nothing Then
                accts = availAccts.Select(Function(ca) ca.Account)
            End If
            If accts IsNot Nothing Then
                ddlEditReservationAccount.DataSource = AccountManager.ConvertToAccountTable(accts.ToList())
                ddlEditReservationAccount.DataBind()
            Else
                ddlEditReservationAccount.Visible = False
                litEditReservationAccountMessage.Text = "<div>No accounts are available for this reservation</div>"
            End If

            Dim item As ListItem = ddlEditReservationAccount.Items.FindByValue(EditReservation.Account.AccountID.ToString())
            If item IsNot Nothing Then ddlEditReservationAccount.SelectedValue = EditReservation.Account.AccountID.ToString()
            ddlEditReservationAccount.Enabled = canChangeAcct
            litReservedBeginDateTime.Text = EditReservation.BeginDateTime.ToString("MM/dd/yyyy hh:mm:ss tt")
            litReservedEndDateTime.Text = EditReservation.EndDateTime.ToString("MM/dd/yyyy hh:mm:ss tt")
            litReservedRegularDuration.Text = EditReservation.ReservedDuration().ToString("#0.0")
            If (EditReservation.IsStarted) Then
                litActualBeginDateTime.Text = If(EditReservation.ActualBeginDateTime Is Nothing, "--", EditReservation.ActualBeginDateTime.Value.ToString("MM/dd/yyyy hh:mm:ss tt"))
                litActualEndDateTime.Text = If(EditReservation.ActualEndDateTime Is Nothing, "--", EditReservation.ActualEndDateTime.Value.ToString("MM/dd/yyyy hh:mm:ss tt"))
                litActualRegularDuration.Text = EditReservation.ActualDuration().ToString("#0.0")
                litActualOvertimeDuration.Text = EditReservation.Overtime().ToString("#0.0")
            Else
                litActualBeginDateTime.Text = "--"
                litActualEndDateTime.Text = "--"
                litActualRegularDuration.Text = "--"
                litActualOvertimeDuration.Text = "--"
            End If
            litChargeableBeginDateTime.Text = EditReservation.ChargeBeginDateTime().ToString("MM/dd/yyyy hh:mm:ss tt")
            litChargeableEndDateTime.Text = EditReservation.ChargeEndDateTime().ToString("MM/dd/yyyy hh:mm:ss tt")
            litChargeableRegularDuration.Text = EditReservation.ChargeDuration().ToString("#0.0")
            litChargeableOvertimeDuration.Text = If(EditReservation.IsStarted, EditReservation.Overtime().ToString("#0.0"), "0.0")
            litForgiveAmount.Text = (1 - EditReservation.ChargeMultiplier).ToString("#0.0%")
            trForgiveForm.Visible = canForgive
            txtForgiveAmount.Text = String.Empty

            If canChangeNotes Then
                txtNotes.Text = EditReservation.Notes
                panNotes.Visible = True
            Else
                litNotes.Text = If(String.IsNullOrEmpty(EditReservation.Notes), String.Empty, String.Format("<div style=""border: solid 1px #AAAAAA; padding: 5px;"">{0}</div>", EditReservation.Notes))
                panNotes.Visible = False
            End If

            If canForgive Or canChangeAcct OrElse canChangeNotes Then
                chkEmailClient.Visible = CurrentUser.ClientID <> EditReservation.Client.ClientID
                btnEditSave.Visible = True
            Else
                chkEmailClient.Visible = False
                btnEditSave.Visible = False
            End If

            panEditHistory.Visible = True
            panHistory.Visible = False
        End Sub

        Private Function InviteeListHTML() As String
            Dim sb As New StringBuilder()
            For Each item As Scheduler.ReservationInvitee In ReservationManager.GetInvitees(EditReservation)
                sb.AppendLine(String.Format("<div>{0}</div>", item.Invitee.DisplayName))
            Next
            Return sb.ToString()
        End Function

        ' this overload is called from the aspx page
        Protected Overloads Function IsBeforeForgiveCutoff(reservationId As Integer) As Boolean
            Dim rsv As Scheduler.Reservation = DA.Current.Single(Of Scheduler.Reservation)(reservationId)
            Return ReservationHistoryUtility.IsBeforeForgiveCutoff(rsv, Date.Now)
        End Function

        Protected Sub btnSearchHistory_Click(sender As Object, e As EventArgs)
            LoadReservationHistory()
        End Sub

        Protected Sub ReservationHistory_Command(sender As Object, e As CommandEventArgs)
            litEditMessage.Text = String.Empty
            Select Case e.CommandName
                Case "cancel"
                    EditReservationCancel()
                Case "edit"
                    _EditReservation = Nothing
                    hidEditReservationID.Value = e.CommandArgument.ToString()
                    LoadEditForm()
            End Select
        End Sub

        Private Sub EditReservationCancel()
            ddlClients.SelectedValue = hidSelectedClientID.Value
            hidEditReservationID.Value = String.Empty
            hidSelectedClientID.Value = String.Empty
            panEditHistory.Visible = False
            panHistory.Visible = True
            LoadReservationHistory()
        End Sub

        Protected Function IsCanceled(obj As Object) As Boolean
            If obj Is DBNull.Value Then
                Return True
            Else
                Dim isActive = Convert.ToBoolean(obj)
                Return Not isActive
            End If
        End Function

        Protected Function GetRowCssClass(item As ReservationHistoryItem) As String
            If item.IsCanceledForModification Then
                Return "canceled-for-modification"
            Else
                Return String.Empty
            End If
        End Function

        Protected Function GetEditUrl(item As ReservationHistoryItem) As String
            Return String.Format("~/ReservationHistory.aspx?Date={0:yyyy-MM-dd}&ReservationID={1}", Request.SelectedDate(), item.ReservationID)
        End Function
    End Class
End Namespace