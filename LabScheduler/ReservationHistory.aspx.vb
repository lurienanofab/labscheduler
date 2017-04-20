Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Models.Data
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler.Data
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports repo = LNF.Repository.Scheduler

Namespace Pages
    Public Class ReservationHistory
        Inherits SchedulerPage

        Private _EditReservation As repo.Reservation

        Private Function GetClient() As ClientModel
            Dim c As ClientModel = Nothing
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

        Public ReadOnly Property EditReservation As repo.Reservation
            Get
                If _EditReservation Is Nothing OrElse _EditReservation.ReservationID.ToString() <> hidEditReservationID.Value Then
                    Dim reservationId As Integer
                    If Integer.TryParse(hidEditReservationID.Value, reservationId) Then
                        _EditReservation = DA.Scheduler.Reservation.Single(reservationId)
                    End If
                End If
                Return _EditReservation
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                LoadClients()
                Dim reservationId As Integer
                If Not String.IsNullOrEmpty(Request.QueryString("ReservationID")) Then
                    If Not Integer.TryParse(Request.QueryString("ReservationID"), reservationId) Then
                        reservationId = 0
                    End If
                End If
                If reservationId <> 0 Then
                    hidEditReservationID.Value = reservationId.ToString()
                    LoadEditForm()
                Else
                    LoadReservationHistory()
                End If
            End If
        End Sub

        Private Sub LoadClients()
            Dim dtClients As DataTable = ResourceClientData.SelectReservHistoryClient(CurrentUser.ClientID)
            ddlClients.DataSource = dtClients
            ddlClients.DataBind()
            If dtClients.Rows.Count > 0 Then
                trUser.Visible = True
                If EditReservation IsNot Nothing Then
                    Session("SelectedClientID") = EditReservation.Client.ClientID.ToString()
                End If
                ddlClients.SelectedValue = If(Session("SelectedClientID") Is Nothing, CurrentUser.ClientID.ToString(), Session("SelectedClientID").ToString())
                Session("SelectedClientID") = Nothing
            Else
                trUser.Visible = False
            End If
            Select Case ddlRange.SelectedValue
                Case "0" '30 days
                    txtEndDate.Text = Date.Now.Date.ToString("MM/dd/yyyy")
                    txtStartDate.Text = Date.Now.Date.AddDays(-30).ToString("MM/dd/yyyy")
                Case "1" '3 months
                    txtEndDate.Text = Date.Now.Date.ToString("MM/dd/yyyy")
                    txtStartDate.Text = Date.Now.Date.AddMonths(-3).ToString("MM/dd/yyyy")
                Case "2" '1 year
                    txtEndDate.Text = Date.Now.Date.ToString("MM/dd/yyyy")
                    txtStartDate.Text = Date.Now.Date.AddYears(-1).ToString("MM/dd/yyyy")
                Case "3" 'All
                    txtEndDate.Text = String.Empty
                    txtStartDate.Text = String.Empty
            End Select
        End Sub

        Private Sub LoadReservationHistory()
            Dim includeCanceledForModification = CacheManager.Current.ShowCanceledForModification() AndAlso CurrentUser.HasPriv(ClientPrivilege.Staff)

            Dim sd As Date = ReservationHistoryUtility.GetStartDate(txtStartDate.Text)
            Dim ed As Date = ReservationHistoryUtility.GetEndDate(txtEndDate.Text)

            rptHistory.DataSource = ReservationHistoryUtility.GetReservationHistoryData(GetClient(), sd, ed, includeCanceledForModification).OrderByDescending(Function(x) x.BeginDateTime).ToList()
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
            Page.ClientScript.RegisterStartupScript(Me.GetType(), "alert_" + Guid.NewGuid().ToString(), String.Format("alert('{0}');", message), True)
        End Sub

        Private Sub LoadEditForm()
            Dim canForgive As Boolean = ReservationHistoryUtility.ReservationCanBeForgiven(CurrentUser, EditReservation, Date.Now)
            Dim canChangeAcct As Boolean = ReservationHistoryUtility.ReservationAccountCanBeChanged(CurrentUser, EditReservation, Date.Now)
            Dim canChangeNotes As Boolean = ReservationHistoryUtility.ReservationNotesCanBeChanged(CurrentUser, EditReservation)

            Dim sd As DateTime = EditReservation.ChargeBeginDateTime().FirstOfMonth()
            Dim ed As DateTime = EditReservation.ChargeEndDateTime().FirstOfMonth().AddMonths(1)

            hidSelectedClientID.Value = ddlClients.SelectedValue
            hidEditReservationID.Value = EditReservation.ReservationID.ToString()
            hidStartDate.Value = sd.ToString("yyyy-MM-dd")
            hidEndDate.Value = ed.ToString("yyyy-MM-dd")
            litReservationID.Text = EditReservation.ReservationID.ToString()
            litResourceName.Text = EditReservation.Resource.ResourceName
            litActivityName.Text = EditReservation.Activity.ActivityName
            litIsStarted.Text = If(EditReservation.IsStarted, "Yes", "No")
            litIsCanceled.Text = If(IsCanceled(EditReservation.IsActive), "Yes", "No")
            trInvitees.Visible = EditReservation.GetInvitees().Count > 0
            litInvitees.Text = InviteeListHTML()
            litCurrentAccount.Text = EditReservation.Account.Name
            trAccount.Visible = canChangeNotes

            '2012-10-23 It's possible that there are no available accounts. For example
            'a remote processing run where no one was ever invited.
            Dim availAccts As List(Of ClientAccount) = EditReservation.AvailableAccounts().ToList()
            Dim accts As IEnumerable(Of Account) = Nothing
            If availAccts IsNot Nothing Then
                accts = availAccts.Select(Function(ca) ca.Account)
            End If
            If accts IsNot Nothing Then
                ddlEditReservationAccount.DataSource = AccountUtility.ConvertToAccountTable(accts.ToList())
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
            For Each item As repo.ReservationInvitee In EditReservation.GetInvitees()
                sb.AppendLine(String.Format("<div>{0}</div>", item.Invitee.DisplayName))
            Next
            Return sb.ToString()
        End Function

        ' this overload is called from the aspx page
        Protected Overloads Function IsBeforeForgiveCutoff(reservationId As Integer) As Boolean
            Dim rsv As repo.Reservation = DA.Scheduler.Reservation.Single(reservationId)
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