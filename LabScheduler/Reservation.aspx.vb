Imports System.Threading.Tasks
Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler
Imports LNF.Scheduler.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports Scheduler = LNF.Repository.Scheduler

Namespace Pages
    Public Class Reservation
        Inherits SchedulerPage

        Enum WeekDays
            Monday = 1
            Tuesday
            Wednesday
            Thursday
            Friday
            Saturday
            Sunday
        End Enum

        'Private _resourceId, _reservationId As Integer
        'Private _selectedTime As Date
        Private _currentAccount As String

        ' Resource Info
        ' Need to track both because when creating _reservation will be null and _resource will be based on path.
        ' When modifying _reservation will be based on the QuerySting parameter and _resource will be based on _reservation.Resource
        'Private _reservation As Scheduler.Reservation
        'Private _resource As Scheduler.Resource

        'Private _authLevel As ClientAuthLevel

        ' Reserve Info
        Private _dtReservs As DataTable
        'Private _dtActivities As DataTable
        'Private dtAccounts As DataTable

        ' Process Info
        'Private _dbProcessInfo As New ProcessInfoDB
        'Private _dbPIL As New ProcessInfoLineDB
        'Private _dtProcessInfo, _dtPIL, _dtReservPI As DataTable

        ' Invitee
        'Private _dtReservInvitees As DataTable
        'Private _dtAvailInvitees As DataTable

        ' Other
        Private _overwriteReservations As IList(Of Scheduler.Reservation)

        Private _RecurrenceWeekDays As Dictionary(Of DayOfWeek, HtmlInputRadioButton)

        Public ReadOnly Property RecurrenceWeekDays As Dictionary(Of DayOfWeek, HtmlInputRadioButton)
            Get
                If _RecurrenceWeekDays Is Nothing Then
                    _RecurrenceWeekDays = New Dictionary(Of DayOfWeek, HtmlInputRadioButton) From {
                        {DayOfWeek.Sunday, rdoRecurringPatternWeeklySunday},
                        {DayOfWeek.Monday, rdoRecurringPatternWeeklyMonday},
                        {DayOfWeek.Tuesday, rdoRecurringPatternWeeklyTuesday},
                        {DayOfWeek.Wednesday, rdoRecurringPatternWeeklyWednesday},
                        {DayOfWeek.Thursday, rdoRecurringPatternWeeklyThursday},
                        {DayOfWeek.Friday, rdoRecurringPatternWeeklyFriday},
                        {DayOfWeek.Saturday, rdoRecurringPatternWeeklySaturday}
                    }
                End If

                Return _RecurrenceWeekDays
            End Get
        End Property

        Public Function GetReservationInvitees() As IList(Of LNF.Scheduler.ReservationInviteeItem)
            Dim result As IList(Of LNF.Scheduler.ReservationInviteeItem) = CacheManager.Current.ReservationInvitees().ToList()

            If result Is Nothing Then
                RefreshInvitees()
                result = CacheManager.Current.ReservationInvitees().ToList()
            End If

            Return result
        End Function

        Public Function GetAvailableInvitees() As IList(Of AvailableInviteeItem)
            Dim result As IList(Of AvailableInviteeItem) = CacheManager.Current.AvailableInvitees().ToList()

            If result Is Nothing Then
                RefreshInvitees()
                result = CacheManager.Current.AvailableInvitees().ToList()
            End If

            Return result
        End Function

        Public Function GetRemovedInvitees() As IList(Of LNF.Scheduler.ReservationInviteeItem)
            Dim result As IList(Of LNF.Scheduler.ReservationInviteeItem) = CacheManager.Current.RemovedInvitees().ToList()

            If result Is Nothing Then
                RefreshInvitees()
                result = CacheManager.Current.RemovedInvitees().ToList()
            End If

            Return result
        End Function

        Private Sub RefreshInvitees()
            Dim res As ResourceModel = GetCurrentResource()
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()

            Dim reservationId As Integer
            Dim resourceId As Integer
            Dim activityId As Integer
            Dim clientId As Integer

            If rsv Is Nothing Then
                If res Is Nothing Then Response.Redirect("~")
                reservationId = 0
                resourceId = res.ResourceID
                activityId = Integer.Parse(ddlActivity.SelectedValue)
                clientId = CacheManager.Current.ClientID
            Else
                reservationId = rsv.ReservationID
                resourceId = rsv.Resource.ResourceID
                activityId = rsv.Activity.ActivityID
                clientId = rsv.Client.ClientID
            End If

            SchedulerUtility.LoadReservationInvitees(reservationId)
            SchedulerUtility.LoadAvailableInvitees(reservationId, resourceId, activityId, clientId)
            SchedulerUtility.LoadRemovedInvitees()
        End Sub

        ''' <summary>
        ''' The DateTime that was selected by clicking a cell in the resevation calendar table
        ''' </summary>
        ''' <returns>A DateTime value</returns>
        Public Function GetReservationSelectedTime() As Date
            Dim result As Date

            If Session("ReservationSelectedTime") Is Nothing Then
                If Request.SelectedPath().IsEmpty() Then
                    Response.Redirect("~", False)
                Else
                    Response.Redirect(String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate()), False)
                End If
            Else
                result = CType(Session("ReservationSelectedTime"), Date)
            End If

            Return result
        End Function

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim startTime As Date = Date.Now

            RequestLog.Append("Reservation.Page_load started")

            Session("ReturnTo") = Nothing

            'This variables must be set every time
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()
            Dim res As ResourceModel = GetCurrentResource()

            RequestLog.Append("     GetCurrentReservation and GetCurrentResource: {0}", Date.Now - startTime)

            If res Is Nothing Then
                Response.Redirect("~", False)
                Return
            End If

            If Not IsPostBack Then
                CacheManager.Current.ReservationProcessInfos(Nothing)
                CacheManager.Current.ReservationInvitees(Nothing)
                CacheManager.Current.AvailableInvitees(Nothing)
                CacheManager.Current.RemovedInvitees(Nothing)

                RequestLog.Append("     CacheManager.Current.ReservationProcessInfos: {0}", Date.Now - startTime)

                ' Load Reservation Data
                LoadReservationInfo(rsv)
                RequestLog.Append("     LoadReservationInfo: {0}", Date.Now - startTime)
                LoadActivity(rsv)
                RequestLog.Append("     LoadActivity: {0}", Date.Now - startTime)
                LoadBeginTime(rsv)
                RequestLog.Append("     LoadBeginTime: {0}", Date.Now - startTime)

                ' Get Resource and Reservation Info, 1 record each
                ' if reservationId = 0, then we are creating new reservation
                Dim activityId As Integer

                If rsv Is Nothing Then
                    ' new reservation
                    RequestLog.Append("     New Reservation")
                    LoadAccount()
                    RequestLog.Append("     LoadAccount: {0}", Date.Now - startTime)
                    LoadProcessInfo(res)
                    RequestLog.Append("     LoadProcessInfo: {0}", Date.Now - startTime)

                    activityId = Integer.Parse(ddlActivity.SelectedValue)

                    'Recurring reservation setup
                    LoadRecurringReservation()
                Else
                    'modify reservation
                    RequestLog.Append("     Modify Reservation")
                    LoadAccount(rsv)
                    RequestLog.Append("     LoadAccount: {0}", Date.Now - startTime)
                    LoadProcessInfo(rsv)
                    RequestLog.Append("     LoadProcessInfo: {0}", Date.Now - startTime)

                    activityId = rsv.Activity.ActivityID

                    phIsRecurring.Visible = False
                End If

                LoadInvitees()
                RequestLog.Append("     LoadInvitees: {0}", Date.Now - startTime)
            Else
                'PostBack = True, this is an event handler postback
                _currentAccount = ddlAccount.SelectedValue

                If String.IsNullOrEmpty(btnSubmit.Text) Then
                    GetCurrentReservation()
                End If
            End If

            'no matter what, only staff can see is Recurring option
            If Not CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                phIsRecurring.Visible = False
            End If

            RequestLog.Append("Reservation.Page_load complete")

            RequestLog.Append("Reservation.Page_Load: {0}", Date.Now - startTime)
        End Sub

        Private Sub LoadRecurringReservation()

            If chkIsRecurring.Checked Then
                chkIsRecurring.Checked = True

                ShowDurationText()

                phRecurringReservation.Visible = True

                txtStartDate.Text = Request.SelectedDate().ToString("MM/dd/yyyy")

                Dim weekday As DayOfWeek = Request.SelectedDate().DayOfWeek
                RecurrenceWeekDays(weekday).Checked = True

                phActivity.Visible = False
                phActivityName.Visible = False
                phActivityMessage.Visible = True
                litActivityMessage.Text = "The Scheduled Maintenance Activity is always used for recurring reservations"

                phBillingAccount.Visible = False
                phBillingAccountMessage.Visible = True
                litBillingAccountMessage.Text = "The General Lab Account is always used for recurring reservations"

                'txtDuration.Visible = True
                'lblDuration.Visible = True
                'ddlDuration.Visible = False
                'lblMaxSchedLimit.Visible = False
                'ddlAccount.Enabled = False
                'ddlActivity.Enabled = False

            Else
                ShowDurationSelect()
                phRecurringReservation.Visible = False

                phActivity.Visible = True
                phActivityName.Visible = False
                litActivityName.Text = String.Empty
                phActivityMessage.Visible = False
                litActivityMessage.Text = String.Empty

                phBillingAccount.Visible = True
                phBillingAccountMessage.Visible = False
                litBillingAccountMessage.Text = String.Empty

                ToggleMustAddInviteeMessage(Boolean.Parse(hidMustAddInvitee.Value))

                'txtDuration.Visible = False
                'lblDuration.Visible = False
                'ddlDuration.Visible = True
                'lblMaxSchedLimit.Visible = True
                'ddlAccount.Enabled = True
                'ddlActivity.Enabled = True
            End If
        End Sub

        Private Function DateChanged() As Boolean
            Return Request.QueryString("ChangeDate") = "1"
        End Function

        Private Function GetCurrentReservation() As Scheduler.Reservation
            Dim reservationId As Integer = 0

            If Request.QueryString("ReservationID") IsNot Nothing Then
                If Integer.TryParse(Request.QueryString("ReservationID"), reservationId) Then
                    Dim recurrenceId As Integer
                    If Request.QueryString("RecurrenceID") IsNot Nothing Then
                        If Integer.TryParse(Request.QueryString("RecurrenceID"), recurrenceId) Then
                            Session("ReturnTo") = String.Format("~/UserRecurringReservationEdit2.aspx?id={0}", recurrenceId)
                        End If
                    End If
                End If
            End If

            If reservationId = 0 Then
                Return Nothing
            Else
                Return DA.Current.Single(Of Scheduler.Reservation)(reservationId)
            End If
        End Function

        Public Overrides Function GetCurrentResource() As ResourceModel
            Dim resourceId As Integer = 0

            Dim rsv As Scheduler.Reservation = GetCurrentReservation()

            If rsv Is Nothing Then
                resourceId = Request.SelectedPath().ResourceID
            Else
                resourceId = rsv.Resource.ResourceID
            End If

            Return CacheManager.Current.ResourceTree().GetResource(resourceId)
        End Function

        Private Function GetCurrentClient() As ClientItem
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()

            If rsv IsNot Nothing Then
                Return CacheManager.Current.GetClient(rsv.Client.ClientID)
            End If

            Return CacheManager.Current.CurrentUser
        End Function

        Private Function GetCurrentActivity() As ActivityModel
            ' always get from the select - even when modifying
            Dim activityId As Integer = Integer.Parse(ddlActivity.SelectedValue)
            Return CacheManager.Current.GetActivity(activityId)
        End Function

#Region " Resource Info Events and Functions "
        Protected Sub ddlStartTimeHour_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlStartTimeHour.SelectedIndexChanged
            GetMaxDuration(True)
        End Sub

        Protected Sub ddlStartTimeMin_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlStartTimeMin.SelectedIndexChanged
            Dim res As Scheduler.Resource = Nothing
            Dim rsv As Scheduler.Reservation = Nothing
            GetMaxDuration(True)
        End Sub

        Private Sub LoadReservationInfo(rsv As Scheduler.Reservation)
            Dim headerText = String.Empty

            If rsv Is Nothing Then
                ' New Reservation
                headerText = "Create Reservation for"
                litClientName.Text = CurrentUser.DisplayName
                litStartDate.Text = Request.SelectedDate().ToLongDateString()
                phActivity.Visible = True
                phActivityName.Visible = False
                btnSubmit.Text = "Create Reservation"
                btnSubmit.CommandName = "Insert"
            Else
                ' Existing Reservation
                headerText = "Modify Reservation for"
                litClientName.Text = rsv.Client.DisplayName
                If Not DateChanged() Then
                    If rsv.BeginDateTime.Date <> Request.SelectedDate() Then
                        'userState.SetDate(rsv.BeginDateTime.Date)
                        'userState.AddAction("Changed date to {0:yyyy-MM-dd} while loading reservation", rsv.BeginDateTime.Date)
                    End If
                End If
                litStartDate.Text = Request.SelectedDate().ToLongDateString()
                phActivity.Visible = False
                phActivityName.Visible = True
                litActivityName.Text = rsv.Activity.ActivityName
                txtNotes.Text = rsv.Notes
                chkAutoEnd.Checked = rsv.AutoEnd
                chkKeepAlive.Checked = rsv.KeepAlive
                btnSubmit.Text = "Modify Reservation"
                btnSubmit.CommandName = "Update"
            End If

            Dim res As ResourceModel = GetCurrentResource()
            litCreateModifyReservation.Text = String.Format("{0} <span class=""resource-name"">{1} [{2}]</span>", headerText, res.ResourceName, res.ResourceID)
        End Sub

        ''' <summary>
        ''' Returns the earliest possible reservation time
        ''' </summary>
        ''' <returns>A DateTime value</returns>
        Private Function GetMinimumStartTime(selectedDate As Date, selectedTime As Date, res As ResourceModel) As TimeSpan
            ' First check if the selected date is in the future. If so the result is 00:00:00
            If selectedDate > Date.Now.Date Then
                Return TimeSpan.Zero
            End If

            ' Next check if the selected date is the current day. If so the result will is based on the current hour
            If selectedDate = Date.Now.Date Then
                Dim minTime As TimeSpan = TimeSpan.FromHours(Date.Now.Hour) ' the earliest possible hour must be the current hour
                Dim addTime As TimeSpan = TimeSpan.Zero

                If res.Granularity.TotalMinutes < 60 Then
                    If (60 - Date.Now.Minute) <= res.Granularity.TotalMinutes Then
                        addTime = TimeSpan.FromHours(1)
                    End If
                Else
                    addTime = TimeSpan.FromHours(1)
                End If

                '2011-04-03 We cannot allow people to modify the begindatetime if the begindatetime is already past

                ' Need to check the if minTime is valid (it should be be after the current hour)
                If minTime.Hours > selectedTime.Hour Then
                    minTime = TimeSpan.FromHours(selectedTime.Hour)
                End If

                minTime = minTime.Add(addTime)

                Return minTime
            End If

            ' If we haven't returned yet then the last possiblility is the selectedDate is in the past so throw exception
            Throw New Exception("The selected date cannot be in the past")
        End Function

        ' Loads Reservation Start Time Dropdownlist
        Private Sub LoadBeginTime(rsv As Scheduler.Reservation)
            Dim res As ResourceModel = GetCurrentResource()

            ' Restrictions: Start Time either = end of previous reservation or
            ' the gap between reservations has to be multiples of Min Reserv Time
            ' This is checked for when user clicks on Submit button

            '2011-12-28 start time must be less than or equal to chargeable end time and greater or equal to current time.

            ' Determine 24-hour granularities
            Dim stepSize As Integer = Convert.ToInt32(res.Granularity.TotalHours)
            If stepSize = 0 Then stepSize = 1
            Dim grans As New List(Of Integer)
            For i As Integer = Convert.ToInt32(res.Offset.TotalHours) To 24 Step stepSize
                grans.Add(i)
            Next

            Dim selectedDate As Date = Request.SelectedDate()
            Dim selectedTime As Date = GetReservationSelectedTime()

            ' Check if selectedDate is in the past
            If selectedDate < Date.Now.Date Then
                ShowPastSelectedDateWarning()
                Exit Sub
            End If

            Dim minTime As TimeSpan = GetMinimumStartTime(selectedDate, selectedTime, res)

            ' Load Hours
            ddlStartTimeHour.Items.Clear()
            For i As Integer = 0 To grans.Count - 1
                If Convert.ToInt32(grans(i)) >= minTime.Hours Then
                    Dim hourText As String = String.Empty
                    hourText = If((Convert.ToInt32(grans(i)) Mod 12) = 0, "12 ", (Convert.ToInt32(grans(i)) Mod 12).ToString() + " ")
                    hourText += If(Convert.ToInt32(grans(i)) < 12, "am", "pm")
                    ddlStartTimeHour.Items.Add(New ListItem(hourText, grans(i).ToString()))
                End If
            Next

            ' Load Minutes
            ddlStartTimeMin.Items.Clear()

            If TimeSpan.Zero = res.Granularity Then
                Throw New Exception(String.Format("Granularity is zero for the resource '{0}' ({1}) : ", res.ResourceName, res.ResourceID.ToString()))
            End If

            For i As Integer = 0 To 59 Step Convert.ToInt32(res.Granularity.TotalMinutes)
                ddlStartTimeMin.Items.Add(New ListItem(i.ToString("00"), i.ToString()))
            Next

            ' Select Preselected Time
            If rsv Is Nothing Then
                ' new reservation
                SetSelectedStartHour(selectedTime.Hour)
                SetSelectedStartMinute(selectedTime.Minute)
            Else
                ' existing reservation
                SetSelectedStartHour(rsv.BeginDateTime.Hour)
                SetSelectedStartMinute(rsv.BeginDateTime.Minute)
            End If

            GetMaxDuration(False)
        End Sub

        Private Sub ShowPastSelectedDateWarning()
            phPastSelectedDateWarning.Visible = True
            phStartTimeAndDuration.Visible = False
            btnSubmit.Enabled = False
        End Sub

        Private Sub SetSelectedStartHour(value As Integer)
            Dim item As ListItem = ddlStartTimeHour.Items.FindByValue(value.ToString())
            If item IsNot Nothing Then
                item.Selected = True
            End If
        End Sub

        Private Sub SetSelectedStartMinute(value As Integer)
            Dim item As ListItem = ddlStartTimeMin.Items.FindByValue(value.ToString())
            If item IsNot Nothing Then
                item.Selected = True
            End If
        End Sub

        Private Sub GetMaxDuration(datetimeModified As Boolean)
            Dim selectedDateTime As Date = Request.SelectedDate()
            selectedDateTime = selectedDateTime.AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue))
            selectedDateTime = selectedDateTime.AddMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue))

            Dim res As ResourceModel = GetCurrentResource()
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()
            Dim client As ClientItem = GetCurrentClient()

            Dim reservationId As Integer = 0

            If rsv IsNot Nothing Then
                reservationId = rsv.ReservationID
                chkIsRecurring.Checked = rsv.IsRecurring()
            End If

            Dim maxDuration As TimeSpan = ReservationManager.GetTimeUntilNextReservation(res.ResourceID, reservationId, client.ClientID, selectedDateTime)

            If maxDuration.TotalMinutes <= 0 Then ' this means that the reservable time is limited by max schedulable
                LoadDuration(-1 * maxDuration.TotalMinutes, True)
            Else
                LoadDuration(maxDuration.TotalMinutes, False)
            End If

            If datetimeModified Then
                If Not String.IsNullOrEmpty(ddlDuration.SelectedValue) Then
                    ValidateAllInviteesReservationsBetween(selectedDateTime, Convert.ToInt32(ddlDuration.SelectedValue))
                End If
            End If
        End Sub

        Private Function GetCurrentDurationMinutes(activityId As Integer) As Integer
            Dim res As ResourceModel = GetCurrentResource()
            Dim act As ActivityModel = CacheManager.Current.GetActivity(activityId)
            If GetDurationType(act.NoMaxSchedAuth, act.ActivityID) Then
                ' allow privileged users to type in reservation duration
                If String.IsNullOrEmpty(txtDuration.Text.Trim()) Then txtDuration.Text = res.MinReservTime.ToString()
                Return Convert.ToInt32(txtDuration.Text.Trim())
            Else
                If ddlDuration.Items.Count = 0 Then
                    Return 0
                Else
                    Return Convert.ToInt32(ddlDuration.SelectedValue)
                End If
            End If
        End Function

        Private Function GetCurrentAuthLevel() As ClientAuthLevel
            Dim res As ResourceModel = GetCurrentResource()
            Dim client As ClientItem = GetCurrentClient()
            Dim result As ClientAuthLevel = CacheManager.Current.GetAuthLevel(res.ResourceID, client.ClientID)
            Return result
        End Function

        Private Function GetBeginDateTime() As Date
            Dim beginDateTime As Date = Request.SelectedDate().AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue))
            beginDateTime = beginDateTime.AddMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue))
            Return beginDateTime
        End Function

        ' Loads Reservation Duration Dropdownlist
        Private Sub LoadDuration(maxDuration As Double, showLimitMsg As Boolean)

            Dim res As ResourceModel = GetCurrentResource()

            ' Duration ranges from Min Reserv Time to Max Reserv Time
            ' or until the start of the next reservation
            ddlDuration.Items.Clear()
            Dim curValue As Double = res.MinReservTime.TotalMinutes
            While curValue <= res.MaxReservTime.TotalMinutes AndAlso curValue <= maxDuration
                Dim hour As Integer = Convert.ToInt32(Math.Floor(curValue / 60))
                Dim minute As Integer = Convert.ToInt32(curValue Mod 60)
                Dim text As String = String.Format("{0} hr {1} min", hour, minute)

                ddlDuration.Items.Add(New ListItem(text, curValue.ToString()))

                curValue += res.Granularity.TotalMinutes
            End While

            'Duration affect user's eligibility to make reservation.
            If ddlDuration.Items.Count = 0 Then
                btnSubmit.Enabled = False
            Else
                btnSubmit.Enabled = True
            End If

            If chkIsRecurring.Checked Then 'If txtDuration.Visible Then
                btnSubmit.Enabled = True
            Else
                If ddlDuration.Items.Count = 0 Then
                    ServerJScript.JSAlert(Page, "Another reservation already been made for this time. Please select different Start-Time")
                End If
            End If

            lblMaxSchedLimit.Visible = showLimitMsg

            Dim act As ActivityModel = GetCurrentActivity()
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()

            If GetDurationType(act.NoMaxSchedAuth, act.ActivityID) Then
                ShowDurationText()
                If rsv IsNot Nothing Then
                    txtDuration.Text = rsv.Duration.ToString()
                End If
            Else
                Try
                    ShowDurationSelect()
                    If rsv IsNot Nothing Then
                        ddlDuration.Items.FindByValue(rsv.Duration.ToString()).Selected = True
                    End If
                Catch
                    ddlDuration.SelectedIndex = -1
                End Try
            End If
        End Sub

        ' Loads Activity Type Dropdownlist
        Private Sub LoadActivity(rsv As Scheduler.Reservation)
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()
            ddlActivity.DataSource = CacheManager.Current.AuthorizedActivities(authLevel)
            ddlActivity.DataBind()

            ' Preselect value
            ddlActivity.ClearSelection()

            Dim item As ListItem = Nothing

            If rsv IsNot Nothing Then
                item = ddlActivity.Items.FindByValue(rsv.Activity.ActivityID.ToString())
                ddlActivity.Enabled = False
            End If

            If item IsNot Nothing Then
                item.Selected = True
                ddlActivity_SelectedIndexChanged(Nothing, Nothing)
            End If
        End Sub

        Private Sub ddlActivity_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlActivity.SelectedIndexChanged
            ' Change Activity DDL Description
            Dim activityId As Integer = Convert.ToInt32(ddlActivity.SelectedValue)
            Dim activity As ActivityModel = CacheManager.Current.GetActivity(activityId)
            ddlActivity.Attributes.Add("title", activity.Description)

            Dim rsv As Scheduler.Reservation = GetCurrentReservation()
            Dim res As ResourceModel = GetCurrentResource()

            If rsv IsNot Nothing Then
                ' Load Billing Accounts
                LoadAccount(rsv)
            Else
                ' Load Billing Accounts
                LoadAccount()
            End If

            ' Clear Invitees
            LoadInvitees()

            ' Show duration textbox for authorized users and activites with no max time (like sched. maintenance and characterization)
            If GetDurationType(activity.NoMaxSchedAuth, activity.ActivityID) Then
                'Come here when user is allowed to set the time without the limit of specified by the tool engineer
                ShowDurationText()
                txtDuration.Text = res.Granularity.TotalMinutes.ToString()
                btnSubmit.Enabled = True
            Else
                'Come here when the user has no privilege on setting random time
                ShowDurationSelect()
                If ddlDuration.Items.Count = 0 Then
                    btnSubmit.Enabled = False
                End If
            End If
        End Sub

        Private Sub LoadAccount()
            Dim act As ActivityModel = CacheManager.Current.GetActivity(Integer.Parse(ddlActivity.SelectedValue))
            Dim accts As New List(Of ClientAccountItem)
            Dim mustAddInvitee As Boolean = SchedulerUtility.LoadAccounts(accts, act.AccountType, CurrentUser.ClientID)
            Dim selectedAccountId As Integer = -1
            LoadAccountScheduledMaintenanceCheck(act.ActivityID, selectedAccountId)
            FillAccountDropDown(accts, selectedAccountId, mustAddInvitee)
        End Sub

        Private Sub LoadAccount(rsv As Scheduler.Reservation)
            If rsv Is Nothing Then Throw New ArgumentNullException("rsv")
            Dim accts As New List(Of ClientAccountItem)
            Dim mustAddInvitee As Boolean = SchedulerUtility.LoadAccounts(accts, rsv.Activity.AccountType, rsv.Client.ClientID)
            Dim selectedAccountId As Integer = rsv.Account.AccountID
            LoadAccountScheduledMaintenanceCheck(rsv.Activity.ActivityID, selectedAccountId)
            FillAccountDropDown(accts, selectedAccountId, mustAddInvitee)
        End Sub

        Private Sub LoadAccountScheduledMaintenanceCheck(activityId As Integer, ByRef selectedAccountId As Integer)
            ddlAccount.Enabled = True
            If activityId = Properties.Current.Activities.ScheduledMaintenance.ActivityID Then
                selectedAccountId = Properties.Current.LabAccount.AccountID
                ddlAccount.Enabled = False
            End If
        End Sub

        Private Sub ToggleMustAddInviteeMessage(state As Boolean)
            If state Then
                phBillingAccount.Visible = False
                phBillingAccountMessage.Visible = True
                litBillingAccountMessage.Text = "You must add an invitee before selecting an account."
            Else
                phBillingAccount.Visible = True
                phBillingAccountMessage.Visible = False
                litBillingAccountMessage.Text = String.Empty
            End If
        End Sub

        Private Sub FillAccountDropDown(accts As IList(Of ClientAccountItem), selectedAccountId As Integer, mustAddInvitee As Boolean)
            If mustAddInvitee Then
                hidMustAddInvitee.Value = "true"
            Else
                hidMustAddInvitee.Value = "false"
                phBillingAccount.Visible = True
                phBillingAccountMessage.Visible = False
                litBillingAccountMessage.Text = String.Empty

                ddlAccount.DataSource = accts.Select(Function(x) New With {.Name = Account.GetFullAccountName(x.ShortCode, x.AccountName, x.OrgName), .AccountID = x.AccountID.ToString()})
                ddlAccount.DataBind()

                If ddlAccount.Items.Count > 0 Then
                    Dim itemAccount As ListItem = ddlAccount.Items.FindByValue(selectedAccountId.ToString())
                    If Not itemAccount Is Nothing Then
                        itemAccount.Selected = True
                    End If
                End If
            End If

            ToggleMustAddInviteeMessage(mustAddInvitee)
        End Sub
#End Region

#Region " Process Info Events and Functions "

        Private Function GetProcessInfoJson() As String
            'Return ProcessInfo1.GetJson()
            'Return "[{""ProcessInfoID"":678,""Lines"":[{""ProcessInfoLineID"":1,""Param"":{""ProcessInfoLineParamID"":""123"",""Value"":""500""}},{""ProcessInfoLineID"":2,""Param"":{""ProcessInfoLineParamID"":""123"",""Value"":""5""}}]}]"
            Return String.Empty
        End Function

        Protected Sub rptProcessInfo_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptProcessInfo.ItemDataBound
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                ' Populate Process Info
                Dim di As New DataItemHelper(e.Item.DataItem)

                CType(e.Item.FindControl("lblPIID"), Label).Text = di("ProcessInfoID").ToString()
                CType(e.Item.FindControl("lblPIName"), Label).Text = di("ProcessInfoName").ToString()
                CType(e.Item.FindControl("lblParamName"), Label).Text = di("ParamName").ToString()

                If Convert.ToBoolean(di("RequireValue")) Then
                    CType(e.Item.FindControl("lblValueName"), Label).Text = di("ValueName").ToString()
                Else
                    CType(e.Item.FindControl("lblValueName"), Label).Visible = False
                    CType(e.Item.FindControl("txtValue"), TextBox).Visible = False
                End If

                If String.IsNullOrEmpty(di("Special").ToString()) Then
                    CType(e.Item.FindControl("chkSpecial"), CheckBox).Visible = False
                Else
                    CType(e.Item.FindControl("chkSpecial"), CheckBox).Text = di("Special").ToString()
                End If

                ' Process Info Param dropdownlist
                Dim pils As IEnumerable(Of ProcessInfoLineModel) = CacheManager.Current.ProcessInfoLines(di.Value("ProcessInfoID", 0)).OrderBy(Function(x) x.Param)

                Dim ddlParam As DropDownList = CType(e.Item.FindControl("ddlParam"), DropDownList)

                If Convert.ToBoolean(di("RequireSelection")) Then
                    ddlParam.Items.Add(New ListItem("", ""))
                End If

                If Convert.ToBoolean(di("AllowNone")) Then     ' Allow None
                    ddlParam.Items.Add(New ListItem("None", "0"))
                End If

                Dim text As String
                Dim value As String
                For i As Integer = 0 To pils.Count - 1
                    text = String.Format("{0} ({1} - {2})", pils(i).Param, pils(i).MinValue, pils(i).MaxValue)
                    value = pils(i).ProcessInfoLineID.ToString()
                    ddlParam.Items.Add(New ListItem(text, value))
                Next

                ' Reservation Process Info
                ' This set the textbox and "special" checkbox (if available)
                ' There may be items with ProcessInfoLineID = 0 if there were previosly added by have now been changed to "None" (i.e. removed)
                Dim rpi As ReservationProcessInfoItem = CacheManager.Current.ReservationProcessInfos().FirstOrDefault(Function(x) x.ProcessInfoID = di.Value("ProcessInfoID", 0) AndAlso x.ProcessInfoLineID > 0)
                If rpi IsNot Nothing Then
                    ddlParam.Items.FindByValue(rpi.ProcessInfoLineID.ToString()).Selected = True
                    CType(e.Item.FindControl("txtValue"), TextBox).Text = rpi.Value.ToString()
                    CType(e.Item.FindControl("chkSpecial"), CheckBox).Checked = rpi.Special
                Else
                    ddlParam.SelectedIndex = 0
                End If
            End If
        End Sub

        Private Sub LoadProcessInfo(res As ResourceModel)
            If res Is Nothing Then Throw New ArgumentNullException("res")
            SchedulerUtility.LoadProcessInfo(0)
            Dim items As IList(Of ProcessInfoModel) = CacheManager.Current.ProcessInfos(res.ResourceID).ToList()
            FillProcessInfoRepeater(items)
        End Sub

        Private Sub LoadProcessInfo(rsv As Scheduler.Reservation)
            If rsv Is Nothing Then Throw New ArgumentNullException("rsv")
            SchedulerUtility.LoadProcessInfo(rsv.ReservationID)
            Dim items As IList(Of ProcessInfoModel) = CacheManager.Current.ProcessInfos(rsv.Resource.ResourceID).ToList()
            FillProcessInfoRepeater(items)
        End Sub

        Private Sub FillProcessInfoRepeater(items As IList(Of ProcessInfoModel))
            If items.Count = 0 Then
                phProcessInfo.Visible = False
            Else
                rptProcessInfo.DataSource = items.OrderBy(Function(x) x.Order).ThenByDescending(Function(x) x.ProcessInfoID)
                rptProcessInfo.DataBind()
            End If
        End Sub
#End Region

#Region " Invitation Events and Functions "
        Protected Sub dgInvitees_ItemCommand(source As Object, e As DataGridCommandEventArgs) Handles dgInvitees.ItemCommand
            Dim ddlInvitees As DropDownList = CType(e.Item.FindControl("ddlInvitees"), DropDownList)
            Dim lblInviteeID As Label = CType(e.Item.FindControl("lblInviteeID"), Label)
            Dim lblInviteeName As Label = CType(e.Item.FindControl("lblInviteeName"), Label)
            InviteeModification(GetCurrentResource(), GetCurrentReservation(), e.CommandName, ddlInvitees, lblInviteeID, lblInviteeName)
        End Sub

        Private Sub ValidateAllInviteesReservationsBetween(beginDateTime As Date, duration As Integer)
            HideInviteeWarning()

            Dim invitees As IList(Of LNF.Scheduler.ReservationInviteeItem) = GetReservationInvitees()

            If invitees IsNot Nothing Then
                If invitees.Count > 0 Then
                    Dim names As New List(Of String)
                    For Each inv As LNF.Scheduler.ReservationInviteeItem In invitees
                        If InviteeReservationsBetween(inv.InviteeID, beginDateTime, duration).Count() > 0 Then
                            Dim inviteeClient = CacheManager.Current.GetClient(inv.InviteeID)
                            names.Add(inviteeClient.DisplayName)
                        End If
                    Next

                    If names.Count > 1 Then
                        ShowInviteeWarning(names)
                    ElseIf names.Count = 1 Then
                        ShowInviteeWarning(names(0))
                    End If
                End If
            End If
        End Sub

        Private Function InviteeReservationsBetween(inviteeClientId As Integer, startDateTime As Date, duration As Integer) As IList(Of Scheduler.Reservation)
            Dim endDateTime As Date = startDateTime.AddMinutes(duration)

            Dim inviteeRsv As IList(Of Scheduler.Reservation) = ReservationManager.SelectByClient(inviteeClientId, Request.SelectedDate().AddHours(0), Request.SelectedDate().AddHours(24), False)

            Dim conflictingRsv As IList(Of Scheduler.Reservation) = ReservationManager.GetConflictingReservations(inviteeRsv, startDateTime, endDateTime)

            Return conflictingRsv
        End Function

        Private Sub HideInviteeWarning()
            phInviteeWarning.Visible = False
            litInviteeWarning.Text = String.Empty
        End Sub

        Private Sub ShowInviteeWarning(name As String)
            phInviteeWarning.Visible = True
            litInviteeWarning.Text = String.Format("Please be aware that {0} has made another reservation at this time.", name)
        End Sub

        Private Sub ShowInviteeWarning(names As IEnumerable(Of String))
            phInviteeWarning.Visible = True
            litInviteeWarning.Text = String.Format("Please be aware that the following invitees have made another reservation at this time: <ul>{0}</ul>", String.Join(Environment.NewLine, names.Select(Function(x) String.Format("<li>{0}</li>", x))))
        End Sub

        Private Sub InviteeModification(res As ResourceModel, rsv As Scheduler.Reservation, actionType As String, ddlInvitees As DropDownList, lblInviteeID As Label, lblInviteeName As Label)
            Dim invitees As IList(Of LNF.Scheduler.ReservationInviteeItem) = GetReservationInvitees()
            Dim available As IList(Of AvailableInviteeItem) = GetAvailableInvitees()
            Dim removed As IList(Of LNF.Scheduler.ReservationInviteeItem) = GetRemovedInvitees()

            If actionType = "Insert" Then
                ' Insert invitee into ReservInvitee list
                If ddlInvitees.Items.Count = 0 Then Exit Sub

                Dim reservationId As Integer = 0
                Dim activityId As Integer = Integer.Parse(ddlActivity.SelectedValue)
                If rsv IsNot Nothing Then
                    reservationId = rsv.ReservationID
                    activityId = rsv.Activity.ActivityID
                End If

                Dim avail = available.FirstOrDefault(Function(x) x.ClientID = Integer.Parse(ddlInvitees.SelectedValue))

                ' make sure the selected invitee is in the list of available invitees
                If avail IsNot Nothing Then
                    Dim item As New LNF.Scheduler.ReservationInviteeItem With {
                        .ReservationID = reservationId,
                        .InviteeID = avail.ClientID,
                        .DisplayName = avail.DisplayName
                    }

                    invitees.Add(item)
                    ' avail will be removed from session in dgInvitees_ItemCreated
                End If

                ' Give Warning if this invitee has made another reservation at the same time
                '200703-22 a bug exists here.  if modifying the reservation, the the code still mistakenly think the curretn modfiying 
                'reservation as another reservation
                Dim duration As Integer = GetCurrentDurationMinutes(activityId)

                Dim startDateTime As Date = Request.SelectedDate()
                startDateTime = startDateTime.AddHours(Integer.Parse(ddlStartTimeHour.SelectedValue))
                startDateTime = startDateTime.AddMinutes(Integer.Parse(ddlStartTimeMin.SelectedValue))

                If InviteeReservationsBetween(Integer.Parse(ddlInvitees.SelectedValue), startDateTime, duration).Count() > 0 Then
                    ShowInviteeWarning(ddlInvitees.SelectedItem.Text)
                Else
                    HideInviteeWarning()
                End If

            ElseIf actionType = "Delete" Then
                ' Insert invitee into AvailInvitee list and remove from ReservInvitee list
                Dim ri = invitees.FirstOrDefault(Function(x) x.InviteeID = Integer.Parse(lblInviteeID.Text))

                If ri IsNot Nothing Then
                    invitees.Remove(ri)

                    available.Add(AvailableInviteeItem.Create(ri.InviteeID, ri.DisplayName))

                    removed.Add(ri)
                End If
            End If

            If rsv IsNot Nothing Then
                LoadAccount(rsv)
            Else
                LoadAccount()
            End If

            dgInvitees.DataSource = invitees.OrderBy(Function(x) x.DisplayName)
            dgInvitees.DataBind()
        End Sub

        Protected Sub dgInvitees_ItemCreated(sender As Object, e As DataGridItemEventArgs) Handles dgInvitees.ItemCreated
            If e.Item.ItemType = ListItemType.Footer Then
                ' Select Available Invitees and Remove already Selected Invitees

                Dim invitees As IList(Of LNF.Scheduler.ReservationInviteeItem) = GetReservationInvitees()
                Dim available As IList(Of AvailableInviteeItem) = GetAvailableInvitees()

                For Each inv As LNF.Scheduler.ReservationInviteeItem In invitees
                    Dim avail As AvailableInviteeItem = available.FirstOrDefault(Function(x) x.ClientID = inv.InviteeID)
                    available.Remove(avail)
                Next

                Dim ddlInvitees As DropDownList = CType(e.Item.FindControl("ddlInvitees"), DropDownList)

                ddlInvitees.DataSource = available.OrderBy(Function(x) x.DisplayName)
                ddlInvitees.DataBind()
            End If
        End Sub

        Protected Sub dgInvitees_ItemDataBound(sender As Object, e As DataGridItemEventArgs) Handles dgInvitees.ItemDataBound
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim item As LNF.Scheduler.ReservationInviteeItem = CType(e.Item.DataItem, LNF.Scheduler.ReservationInviteeItem)
                CType(e.Item.FindControl("lblInviteeID"), Label).Text = item.InviteeID.ToString()
                CType(e.Item.FindControl("lblInviteeName"), Label).Text = item.DisplayName.ToString()
            End If
        End Sub

        Private Sub LoadInvitees()
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()
            Dim res As ResourceModel = GetCurrentResource()
            Dim client As ClientItem = GetCurrentClient()
            Dim act As ActivityModel = GetCurrentActivity()

            Dim reservationId As Integer = If(rsv Is Nothing, 0, rsv.ReservationID)

            SchedulerUtility.LoadAvailableInvitees(reservationId, res.ResourceID, act.ActivityID, client.ClientID)

            FillInvitees()
        End Sub

        Private Sub FillInvitees()
            dgInvitees.DataSource = GetReservationInvitees()
            dgInvitees.DataBind()
        End Sub
#End Region

#Region " Reservation Events "
        ' When submit button is clicked, validate reservation and ask for confirmation
        Protected Sub btnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
            Try
                Dim rsv As Scheduler.Reservation = GetCurrentReservation()
                Dim res As ResourceModel = GetCurrentResource()

                Dim reservationId As Integer
                Dim client As Client
                Dim activity As ActivityModel

                If rsv IsNot Nothing Then
                    reservationId = rsv.ReservationID
                    client = rsv.Client
                    activity = CacheManager.Current.GetActivity(rsv.Activity.ActivityID) 'remember, the activity cannot be changed once a reservation is made
                Else
                    reservationId = 0
                    client = DA.Current.Single(Of Client)(CacheManager.Current.ClientID)
                    activity = CacheManager.Current.GetActivity(Integer.Parse(ddlActivity.SelectedValue))
                End If

                ' Validate Reservation
                Dim rd As ReservationDuration = GetReservationDuration(activity.ActivityID)

                ShowReservationAlert(Nothing)

                If rd.Duration = TimeSpan.Zero Then
                    ShowReservationAlert("Duration must be greater than zero.")
                    Exit Sub
                End If

                '2009-01 After the addition of the recurrence schedule feature, we should also distinguish between regular reservation and recurrence reservation
                If chkIsRecurring.Checked Then
                    If String.IsNullOrEmpty(txtDuration.Text) Then
                        chkIsRecurring.Checked = True
                        ShowDurationText()
                        ShowReservationAlert("You must specify the duration.")
                        Exit Sub
                    End If

                    Dim rrDuration As Double = 0
                    Dim rrStartDate As Date
                    If Double.TryParse(txtDuration.Text, rrDuration) Then
                        If reservationId = 0 Then
                            ' new recurring reservation
                            If Date.TryParse(txtStartDate.Text, rrStartDate) Then
                                Dim rr As New Scheduler.ReservationRecurrence()
                                Dim patId As Integer = If(rdoRecurringPatternWeekly.Checked, 1, 2)
                                rr.Pattern = DA.Current.Single(Of Scheduler.RecurrencePattern)(patId)
                                rr.Resource = DA.Current.Single(Of Scheduler.Resource)(res.ResourceID)
                                rr.Client = client
                                rr.Account = DA.Current.Single(Of Account)(Properties.Current.LabAccount.AccountID) 'Currently only supports general lab account
                                rr.BeginTime = rd.BeginDateTime
                                rr.Duration = rrDuration
                                rr.EndTime = rd.EndDateTime
                                rr.BeginDate = rrStartDate
                                rr.CreatedOn = Date.Now
                                rr.AutoEnd = chkAutoEnd.Checked
                                rr.IsActive = True
                                rr.Activity = DA.Current.Single(Of Scheduler.Activity)(Properties.Current.Activities.ScheduledMaintenance.ActivityID)
                                rr.AutoEnd = chkAutoEnd.Checked
                                rr.KeepAlive = chkKeepAlive.Checked
                                rr.Notes = txtNotes.Text

                                If rdoRecurringRangeEndBy.Checked Then
                                    Dim rrEndDate As Date
                                    If Date.TryParse(txtEndDate.Value, rrEndDate) Then
                                        rr.EndDate = rrEndDate
                                    Else
                                        chkIsRecurring.Checked = True
                                        ShowDurationText()
                                        ShowReservationAlert("Invalid end date.")
                                        Exit Sub
                                    End If
                                End If

                                If rdoRecurringPatternWeekly.Checked Then
                                    rr.PatternParam1 = RecurrenceWeekDays.First(Function(x) x.Value.Checked).Key
                                ElseIf rdoRecurringPatternMonthly.Checked Then
                                    rr.PatternParam1 = Convert.ToInt32(ddlMonthly1.SelectedValue)
                                    rr.PatternParam2 = Convert.ToInt32(ddlMonthly2.SelectedValue)
                                End If

                                Try
                                    DA.Current.Insert(rr)
                                    Response.Redirect("~/UserReservations.aspx", False)
                                    Exit Sub
                                Catch ex As Exception
                                    Dim errmsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
                                    ShowReservationAlert(String.Format("Error in saving Recurring Reservation: {0}", errmsg))
                                    Exit Sub
                                End Try
                            Else
                                ShowReservationAlert("Error in saving Recurring Reservation: Invalid StartDate.")
                                Exit Sub
                            End If
                        End If
                        ' if reservationId != 0 then continue modifying existing reservation
                    Else
                        ShowReservationAlert("Error in saving Recurring Reservation: Invalid Duration.")
                        Exit Sub
                    End If
                End If

                ' Ensure that there are accounts from which to select
                If ddlAccount.Items.Count = 0 Then
                    ShowReservationAlert("No selectable billing accounts.")
                    Exit Sub
                End If

                ' Check Billing Account
                If ddlAccount.SelectedValue = "-1" Then
                    ShowReservationAlert("Please select a valid billing account.")
                    Exit Sub
                End If

                ' Check for Invitee numbers.  Invitee authorizations are filtered in store procedures.
                '2007-03-02 Get the number of added rows
                'I have to get the count of rows that are not deleted, because the Table.Rows.Count return all rows 
                'despite the row state

                'the # of rows that are not "deleted"
                Dim activeRowCount As Integer = 0
                activeRowCount = GetReservationInvitees().Count

                '2007-03-22 Check if remote processing / future practice has more than 1 invitee, which is not allowed now
                If activeRowCount > 1 AndAlso (activity.ActivityID = 21 OrElse activity.ActivityID = 22) Then
                    ShowReservationAlert("Currently you can have ONLY one invitee for the type of activity you are reserving.")
                    Exit Sub
                End If

                Dim invType As ActivityInviteeType = CType(activity.InviteeType, ActivityInviteeType)
                Select Case invType
                    Case ActivityInviteeType.None
                        If activeRowCount > 0 Then
                            ShowReservationAlert("You cannot invite anyone to this reservation.")
                            Exit Sub
                        End If
                    Case ActivityInviteeType.Required
                        If activeRowCount = 0 Then
                            ShowReservationAlert("You must invite someone to this reservation.")
                            Exit Sub
                        End If
                    Case ActivityInviteeType.Single
                        If activeRowCount <> 1 Then
                            ShowReservationAlert("You must invite a single person to this reservation.")
                            Exit Sub
                        End If
                End Select

                ' Store Reservation Process Info

                For i As Integer = 0 To rptProcessInfo.Items.Count - 1
                    ' ProcessInfoLine
                    Dim ddlParam As DropDownList = CType(rptProcessInfo.Items(i).FindControl("ddlParam"), DropDownList)
                    If String.IsNullOrEmpty(ddlParam.SelectedValue) Then
                        ShowReservationAlert("You must select a Process Info Line.")
                        Exit Sub
                    End If

                    ' ProcessInfo
                    Dim processInfoId As Integer = Convert.ToInt32(CType(rptProcessInfo.Items(i).FindControl("lblPIID"), Label).Text)
                    Dim processInfoLineId As Integer = Convert.ToInt32(ddlParam.SelectedValue)
                    Dim valueText As String = "0"
                    Dim special As Boolean = False

                    If processInfoLineId <> 0 Then
                        ' Validate ProcessInfo Value
                        If CType(rptProcessInfo.Items(i).FindControl("txtValue"), TextBox).Visible Then
                            valueText = CType(rptProcessInfo.Items(i).FindControl("txtValue"), TextBox).Text
                        End If

                        special = CType(rptProcessInfo.Items(i).FindControl("chkSpecial"), CheckBox).Checked

                        SchedulerUtility.AddReservationProcessInfo(res.ResourceID, processInfoId, processInfoLineId, reservationId, valueText, special)
                    Else
                        SchedulerUtility.RemoveReservationProcessInfo(processInfoId)
                    End If
                Next

                ' why?
                'For Each dr As DataRow In CacheManager.Current.ReservationProcessInfos().Rows
                '    If dr.RowState = DataRowState.Unchanged Then
                '        dr.Delete()
                '    End If
                'Next

                '2007-03-05 if User is making remote processing or future practice, then we have to make sure the invitee's max sched hours
                'do not go over the limit
                If activity.ActivityID = 21 OrElse activity.ActivityID = 22 Then
                    For Each ri In GetReservationInvitees()
                        Dim availableMinutes As Integer = ReservationManager.GetAvailableSchedMin(res.ResourceID, ri.InviteeID)

                        'This code is trying to solve the problem about calculating correct available time when current reservation being modified
                        'must be excluded.  So we must added the current reservation's duration back to available time

                        If rsv IsNot Nothing Then
                            availableMinutes += Convert.ToInt32(rsv.Duration)
                        End If

                        If rd.Duration.TotalMinutes > availableMinutes Then
                            ShowReservationAlert(String.Format("The reservation you are making exceed your invitee's maximum reservable hours. You can only reserve {0} minutes for this invitee", availableMinutes))
                            Exit Sub
                        End If

                        Exit For
                    Next
                End If

                ' Check Reservation Fence Constraint for current user's auth level
                ' 2007-03-03 Currently this should be the only place to check the Reserv. Fence Constraint, because
                ' we cannot check it until user specify the Activity Type.
                Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()
                If (activity.NoReservFenceAuth And Convert.ToInt32(authLevel)) = 0 Then
                    If Date.Now.Add(res.ReservFence) <= rd.BeginDateTime Then
                        ShowReservationAlert("You're trying to make a reservation that's too far in the future.")
                        Exit Sub
                    End If
                End If

                ' ddlDuration prevents exceeding max reservable - the only way to beat this is to be logged onto two system
                '  and to start making two reservations at the same time. To combat this, the final check is made in the SP
                If IsThereAlreadyAnotherReservation(rsv) Then
                    ShowReservationAlert("Another reservation has already been made for this time.")
                    Exit Sub
                End If

                ' Restrictions: Start Time either has to equal end of previous reservation or
                ' the gap between reservations has to be at least = Min Reserv Time
                '2007-11-14 Temporarily disable this feature, so users can make reservatin at anytime they want
                'If AuthLevel <> AuthLevels.Engineer Then
                '	Dim TimeSinceLastReservation As Integer = rsvDB.GetTimeSinceLastReservation(ReservationID, ResourceID, BeginDateTime)
                '	If TimeSinceLastReservation <> 0 AndAlso TimeSinceLastReservation < resDB.MinReservTime Then
                '		CommonTools.JSAlert(Page, "Requested reservation too close to previous reservation. Please allow " + resDB.MinReservTime.ToString() + " minutes between two reservations.  Contact tool engineer if you have any questions")
                '		Exit Sub
                '	End If
                'End If

                '2009-12-07 Check if the reservation lies within lab clean
                'Dim EightThirty As New DateTime(BeginDateTime.Year, BeginDateTime.Month, BeginDateTime.Day, 
                Dim currentDate As New Date(rd.BeginDateTime.Year, rd.BeginDateTime.Month, rd.BeginDateTime.Day, 0, 0, 0) '
                Dim yesterday As Date = currentDate.AddDays(-1) 'need this to determin lab clean days that are moved by holidays
                Dim labCleanBegin As Date = currentDate.AddMinutes(510) '8:30 am
                Dim labCleanEnd As Date = labCleanBegin.AddHours(1) '9:30 am
                Dim isLabCleanTime As Boolean = False

                If rd.BeginDateTime < labCleanEnd AndAlso rd.EndDateTime > labCleanBegin Then
                    If (rd.BeginDateTime.DayOfWeek = DayOfWeek.Monday OrElse rd.BeginDateTime.DayOfWeek = DayOfWeek.Thursday) AndAlso Not HolidayData.IsHoliday(currentDate) Then
                        isLabCleanTime = True
                    ElseIf (yesterday.DayOfWeek = DayOfWeek.Monday OrElse yesterday.DayOfWeek = DayOfWeek.Thursday) AndAlso HolidayData.IsHoliday(yesterday) Then
                        'in here, we have a non standard lab clean day due to holiday
                        isLabCleanTime = True
                    End If
                End If

                ' get actual costs based on the selected account
                Dim mCompile As New Compile()
                Dim dblCost As Double = -1
                Dim dblCostStr As String = String.Empty

                Try
                    dblCost = mCompile.EstimateToolRunCost(Convert.ToInt32(ddlAccount.SelectedValue), res.ResourceID, rd.Duration.TotalMinutes)
                    dblCostStr = dblCost.ToString("$#,##0.00")
                Catch ex As Exception
                    dblCostStr = dblCost.ToString("ERR")
                End Try

                ' Display Submit Confirmation
                lblConfirm.Text = String.Format("You are about to reserve resource {0}<br/>from {1} to {2}.", res.ResourceName, rd.BeginDateTime, rd.EndDateTime)

                If Not _overwriteReservations Is Nothing Then
                    lblConfirm.Text += "<br/><br/><b>There are other reservations made during this time.<br/>By accepting this confirmation, you will overwrite the other reservations.</b>"
                End If

                If isLabCleanTime Then
                    lblConfirm.Text += "<br/><br/><span style=""color:#ff0000; font-size:larger"">**Warning: Your reservation overlaps with lab clean time (8:30am to 9:30am). You can still continue, but you should not be inside the lab during that time**</span>"
                End If

                lblConfirm.Text += String.Format("<br/><br/>The estimated cost of this activity will be {0}.", dblCostStr)

                Dim processInfoEnum As IList(Of Scheduler.ProcessInfo) = DA.Current.Query(Of Scheduler.ProcessInfo)().Where(Function(x) x.Resource.ResourceID = res.ResourceID).ToList()

                If processInfoEnum.Count > 0 Then
                    lblConfirm.Text += "<br/>Additional precious metal charges may apply."
                End If

                If rd.IsAfterHours() Then
                    If KioskUtility.IsKiosk() Then
                        ' do not show the link on kiosks
                        lblConfirm.Text += "<br/><br/>This reservation occurs during after-hours.<br/>Please add an event to the After-Hours Buddy Calendar for this reservation."
                    Else
                        Dim calendarUrl As String = ConfigurationManager.AppSettings("AfterHoursBuddyCalendarUrl")
                        If String.IsNullOrEmpty(calendarUrl) Then Throw New Exception("Missing appSetting: AfterHoursBuddyCalendarUrl")
                        lblConfirm.Text += String.Format("<br/><br/>This reservation occurs during after-hours.<br/>Please add an event to the <a href=""{0}"" target=""_blank"">After-Hours Buddy Calendar</a> for this reservation.", calendarUrl)
                    End If
                End If

                lblConfirm.Text += "<br/><br/>Click 'Yes' to accept reservation or 'No' to cancel scheduling."

                Dim isInLab As Boolean = CacheManager.Current.IsOnKiosk()
                Dim isEngineer As Boolean = (authLevel And ClientAuthLevel.ToolEngineer) > 0

                'Dim isStartable As Boolean = (DateTime.Now > BeginDateTime.AddMinutes(-1 * resDB.MinReservTime))
                Dim resevationState As ReservationState = ReservationManager.GetUnstartedReservationState(rd.BeginDateTime, Convert.ToInt32(res.MinReservTime.TotalMinutes), isInLab, isEngineer, True, False, True, True)
                Dim allowedAuths As ClientAuthLevel = ClientAuthLevel.AuthorizedUser Or ClientAuthLevel.SuperUser Or ClientAuthLevel.ToolEngineer Or ClientAuthLevel.Trainer

                phConfirmYesAndStart.Visible = False ' reset to false 
                If ReservationState.StartOnly = resevationState OrElse ReservationState.StartOrDelete = resevationState Then
                    If (allowedAuths And authLevel) > 0 Then ' isUserAuthorized Then
                        Dim endableRsvQuery As IList(Of Scheduler.Reservation) = ReservationManager.SelectEndableReservations(res.ResourceID)
                        If endableRsvQuery.Count = 0 Then
                            ' If there are no previous un-ended reservations
                            phConfirmYesAndStart.Visible = True
                        End If
                    End If
                End If

                phConfirm.Visible = True
                phReserve.Visible = False
                ResetScrollPosition()
            Catch ex As Exception
                ShowReservationAlert(ex.Message)
            End Try
        End Sub

        Private Sub ResetScrollPosition()
            If Not ClientScript.IsStartupScriptRegistered([GetType](), "CallResetScrollPosition") Then
                'Add the call to the ResetScrollPosition() function
                ClientScript.RegisterStartupScript([GetType](), "CallResetScrollPosition", "ResetScrollPosition();", True)
            End If
        End Sub

        Private Sub ShowReservationAlert(text As String, Optional alertType As String = "danger", Optional dismissable As Boolean = True)

            If String.IsNullOrEmpty(text) Then
                litReservationAlert.Text = String.Empty
                Return
            End If

            Dim formatString As String

            If dismissable Then
                formatString = "<div class=""alert alert-{0} alert-dismissible"" role=""alert"" style=""margin-top: 10px;""><button type=""button"" class=""close"" data-dismiss=""alert"" aria-label=""Close""><span aria-hidden=""true"">&times;</span></button>{1}</div>"
            Else
                formatString = "<div class=""alert alert-{0}"" role=""alert"" style=""margin-top: 10px;"">{1}</div>"
            End If

            litReservationAlert.Text = String.Format(formatString, alertType, text)
        End Sub

        Protected Function IsThereAlreadyAnotherReservation(rsv As Scheduler.Reservation) As Boolean
            Dim res As ResourceModel = GetCurrentResource()

            ' Check for other reservations made during this time
            ' Select all reservations for this resource during the time of current reservation

            Dim reservationId As Integer = 0
            Dim activityId As Integer = Integer.Parse(ddlActivity.SelectedValue)
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()

            If rsv IsNot Nothing Then
                reservationId = rsv.ReservationID
                activityId = rsv.Activity.ActivityID
            End If

            Dim rd As ReservationDuration = GetReservationDuration(activityId)

            Dim otherRsv As IList(Of Scheduler.Reservation) = ReservationManager.SelectOverwrittable(res.ResourceID, rd.BeginDateTime, rd.EndDateTime)

            If otherRsv.Count > 0 Then
                If Not (otherRsv.Count = 1 AndAlso otherRsv(0).ReservationID = reservationId) Then
                    If authLevel = ClientAuthLevel.ToolEngineer Then
                        _overwriteReservations = otherRsv
                    Else
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        ' Go back to previous page
        Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
            ReturnToResourceDayWeek()
        End Sub

        Private Sub ReturnToResourceDayWeek()
            Dim redirectUrl As String

            If Session("ReturnTo") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Session("ReturnTo").ToString()) Then
                redirectUrl = Session("ReturnTo").ToString()
            Else
                Dim view As ViewType = GetCurrentView()

                If view = ViewType.UserView Then
                    redirectUrl = String.Format("~/UserReservations.aspx?Date={0:yyyy-MM-dd}", Request.SelectedDate())
                ElseIf view = ViewType.ProcessTechView Then
                    ' When we come from ProcessTech.aspx the full path is used (to avoid a null object error). When returning we just want the ProcessTech path.
                    Dim pt As ProcessTechModel = Request.SelectedPath().GetProcessTech()
                    Dim path As PathInfo = PathInfo.Create(pt)
                    redirectUrl = String.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", path.UrlEncode(), Request.SelectedDate())
                Else 'ViewType.DayView OrElse Scheduler.ViewType.WeekView
                    redirectUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate())
                End If
            End If

            Response.Redirect(redirectUrl, False)
        End Sub

        ' Store Reservation info in database
        Protected Sub btnConfirmYes_Click(sender As Object, e As EventArgs)
            Try
                CreateOrModifyReservation()
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try

            ' Go back to previous page
            ReturnToResourceDayWeek()
        End Sub

        ' Show Reservation Page again and hide confirmation
        Protected Sub btnConfirmNo_Click(sender As Object, e As EventArgs)
            phConfirm.Visible = False
            phReserve.Visible = True
        End Sub

        Protected Sub btnConfirmYesAndStart_Click(sender As Object, e As EventArgs)
            Try
                Dim rsv As Scheduler.Reservation = CreateOrModifyReservation()

                Dim clientId As Integer = CurrentUser.ClientID
                Dim isInLab As Boolean = CacheManager.Current.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID)

                RegisterAsyncTask(New PageAsyncTask(Function() StartReservationAsync(rsv, clientId)))
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try

            ' Go back to previous page
            ReturnToResourceDayWeek()
        End Sub

        Private Async Function StartReservationAsync(rsv As Scheduler.Reservation, clientId As Integer) As Task
            If rsv IsNot Nothing Then
                Dim isInLab As Boolean = CacheManager.Current.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID)
                Await ReservationManager.StartReservation(rsv, clientId, isInLab)
            End If
        End Function

        Private Function CreateOrModifyReservation() As Scheduler.Reservation
            Dim rsv As Scheduler.Reservation = GetCurrentReservation()

            Session("ReservationProcessInfoJsonData") = hidProcessInfoData.Value

            If IsThereAlreadyAnotherReservation(rsv) Then
                ServerJScript.JSAlert(Page, "Another reservation has already been made for this time .")
                phConfirm.Visible = False
                phReserve.Visible = True
                Return rsv 'null when creating a new reservation, current reservation when modifying
            End If

            ' this will be the result reservation - either a true new reservation when creating, a new reservation for modification, or the existing rsv when modifying non-duration data
            Dim result As Scheduler.Reservation = Nothing

            ' Overwrite other reservations
            If _overwriteReservations IsNot Nothing Then
                OverwriteReservations()
            End If

            If rsv Is Nothing Then
                result = SchedulerUtility.CreateNewReservation(GetReservationData())
            Else
                result = SchedulerUtility.ModifyExistingReservation(rsv, GetReservationData())
            End If

            Return result
        End Function

        Public Function GetReservationData() As SchedulerUtility.ReservationData
            Dim result As New SchedulerUtility.ReservationData With {
                .ClientID = GetCurrentClient().ClientID,
                .AccountID = Integer.Parse(ddlAccount.SelectedValue),
                .ActivityID = GetCurrentActivity().ActivityID,
                .AutoEnd = chkAutoEnd.Checked,
                .KeepAlive = chkKeepAlive.Checked,
                .Notes = txtNotes.Text
            }
            result.ReservationDuration = GetReservationDuration(result.ActivityID)
            result.ResourceID = GetCurrentResource().ResourceID
            Return result
        End Function

        Private Function GetReservationDuration(activityId As Integer) As ReservationDuration
            Dim beginDateTime As Date = Request.SelectedDate().AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue)).AddMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue))
            Dim currentDurationMinutes As Integer = GetCurrentDurationMinutes(activityId)
            Return New ReservationDuration(beginDateTime, TimeSpan.FromMinutes(currentDurationMinutes))
        End Function

        ' Overwrite intervening reservations
        Private Sub OverwriteReservations()
            For Each rsv As Scheduler.Reservation In _overwriteReservations
                ' Delete Reservation
                ReservationManager.Delete(rsv, CurrentUser.ClientID)

                ' Send email to reserver informing them that their reservation has been canceled
                EmailManager.EmailOnToolEngDelete(rsv, CurrentUser.ClientID)
            Next
        End Sub
#End Region

        Protected Sub ddlAccount_DataBound(sender As Object, e As EventArgs) Handles ddlAccount.DataBound
            If Not _currentAccount Is Nothing Then
                For Each item As ListItem In ddlAccount.Items
                    If item.Value = _currentAccount Then
                        item.Selected = True
                    End If
                Next
            End If
        End Sub

        '2007-03-22
        'Try to refactor the code so we can have consistent way of determinig the Duration control (either textbox or dropdownlist)
        Private Function GetDurationType(noMaxSchedAuth As Integer, activityId As Integer) As Boolean
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()
            'Right now, to have textbox, the authlevel must be included in NoMaxSchedAuth, and only only sched. maintenance and characterization
            If (noMaxSchedAuth And authLevel) > 0 AndAlso (activityId = 15 OrElse activityId = 18 OrElse activityId = 23) Then
                Return True
            Else
                Return False
            End If
        End Function

        Protected Sub chkIsRecurring_CheckedChanged(sender As Object, e As EventArgs)
            LoadRecurringReservation()
        End Sub

        Protected Function ReservationLog(reservation As Scheduler.Reservation, reservationParams As Dictionary(Of String, String)) As Boolean
            For Each kvp As KeyValuePair(Of String, String) In reservationParams
                Dim resLog As Scheduler.ReservationLog = New Scheduler.ReservationLog With {
                    .Reservation = reservation
                }
                'resLog.ParamName = kvp.Key
                'resLog.ParamValue = kvp.Value.ToString()
            Next

            Return True
        End Function

        Private Sub ShowDurationText()
            phDurationSelect.Visible = False
            phDurationText.Visible = True
        End Sub

        Private Sub ShowDurationSelect()
            phDurationSelect.Visible = True
            phDurationText.Visible = False
        End Sub
    End Class
End Namespace