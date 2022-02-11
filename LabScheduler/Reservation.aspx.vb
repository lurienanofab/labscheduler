Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Controls
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports LNF.Web.Scheduler.Models

Namespace Pages
    Public Class Reservation
        Inherits SchedulerPage

        Private _resource As IResource
        Private _reservation As IReservation
        Private _selectedTime As TimeSpan?
        Private ReadOnly _overwriteReservations As IEnumerable(Of IReservation)

        Public ReadOnly Property SchedulerUtility As SchedulerUtility

        Public ReadOnly Property ReservationID As Integer
            Get
                Dim result As Integer = 0
                Integer.TryParse(Request.QueryString("ReservationID"), result)
                Return result
            End Get
        End Property

        ''' <summary>
        ''' The current resource. Comes from the query string paramter Path.
        ''' </summary>
        Public ReadOnly Property Resource As IResource
            Get
                If _resource Is Nothing Then
                    _resource = GetCurrentResource()
                    If _resource Is Nothing Then
                        Throw New Exception("A resource must be selected to make a reservation.")
                    End If
                End If
                Return _resource
            End Get
        End Property

        ''' <summary>
        ''' The current selected time. Comes from the query string parameter Time.
        ''' </summary>
        Public ReadOnly Property SelectedTime As TimeSpan
            Get
                If Not _selectedTime.HasValue Then
                    _selectedTime = TimeSpan.FromMinutes(GetTime())
                End If

                Return _selectedTime.Value
            End Get
        End Property

        '''' <summary>
        '''' The current reservation. Comes from the query string paramter ReservationID.
        '''' </summary>
        Public ReadOnly Property Reservation As IReservationItem
            Get
                If ReservationID = 0 Then
                    Return Nothing
                Else
                    If _reservation Is Nothing Then
                        _reservation = Provider.Scheduler.Reservation.GetReservation(ReservationID)
                    End If

                    Return _reservation
                End If
            End Get
        End Property

        Public Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Helper.AppendLog($"Reservation.Page_Load: Started...")
            Helper.AppendLog($"Reservation.Page.IsPostBack = {Page.IsPostBack}, ReservationID = {ReservationID}, QueryString = ""{Request.QueryString}""")
            If Not Page.IsPostBack Then
                ShowLoadError(Nothing)
                ClearSession()
                LoadReservation()
            End If
            Helper.AppendLog($"Reservation.Page_Load: Completed in {sw.Elapsed.TotalSeconds:0.0000} seconds")
            sw.Stop()
        End Sub

        Private Sub ClearSession()
            Session.Remove("IsRecurring")
            Session.Remove($"ReservationProcessInfos#{Resource.ResourceID}")
            Session.Remove($"ReservationInvitees#{Resource.ResourceID}")
            Session.Remove($"AvailableInvitees#{Resource.ResourceID}")
        End Sub

        Private Function GetTime() As Integer
            Dim value As Integer

            If String.IsNullOrEmpty(Request.QueryString("Time")) Then
                Throw New Exception("Missing required querystring parameter: Time")
            End If

            If Not Integer.TryParse(Request.QueryString("Time"), value) Then
                Throw New Exception("An integer value is required for query string parameter: Time")
            End If

            Return value
        End Function

        ''' <summary>
        ''' Gets either Reservation.ClientID or, if there is no reservation, CurrentUser.ClientID.
        ''' </summary>
        Private Function GetCurrentClientID() As Integer
            Dim result As Integer

            If ReservationID = 0 Then
                result = CurrentUser.ClientID
            Else
                result = Reservation.ClientID
            End If

            Return result
        End Function

        Private Function GetCurrentAuthLevel() As ClientAuthLevel
            Return CacheManager.Current.GetAuthLevel(Resource.ResourceID, CurrentUser)
        End Function

        Private Function GetSelectedDateTime() As Date
            Dim result As Date = ContextBase.Request.SelectedDate()

            Dim startTimeHourVal As String = ddlStartTimeHour.SelectedValue
            Dim startTimeMinuteVal As String = ddlStartTimeMin.SelectedValue

            If String.IsNullOrWhiteSpace(startTimeHourVal) Then
                Throw New Exception($"Invalid start time hour value. String cannot be empty. [ddlStartTimeHour.Items.Count={ddlStartTimeHour.Items.Count}, ddlStartTimeHour.SelectedIndex={ddlStartTimeHour.SelectedIndex}]")
            End If

            If String.IsNullOrWhiteSpace(startTimeMinuteVal) Then
                Throw New Exception($"Invalid start time minute value. String cannot be empty. [ddlStartTimeMin.Items.Count={ddlStartTimeMin.Items.Count}, ddlStartTimeMin.SelectedIndex={ddlStartTimeMin.SelectedIndex}]")
            End If

            Dim startTimeHour As Integer
            Dim startTimeMinute As Integer

            If Integer.TryParse(startTimeHourVal, startTimeHour) Then
                result = result.AddHours(startTimeHour)
            Else
                Throw New Exception($"Invalid start time hour value. '{startTimeHourVal}' cannot be converted to integer.")
            End If

            If Integer.TryParse(startTimeMinuteVal, startTimeMinute) Then
                result = result.AddMinutes(startTimeMinute)
            Else
                Throw New Exception($"Invalid start time minute value. '{startTimeMinuteVal}' cannot be converted to integer.")
            End If

            Return result
        End Function

        Private Function GetCurrentActivity() As IActivity
            ' always get from the select - even when modifying
            Dim activityId As Integer = Convert.ToInt32(ddlActivity.SelectedValue)
            Return CacheManager.Current.GetActivity(activityId)
        End Function

        Private Sub LoadReservation()
            SetHeader()
            SetClientName()
            SetStartDate()
            SetIsRecurring()
            SetKeepAlive()
            SetAutoEnd()
            SetNotes()
            SetSubmitButton()

            LoadActivities()
            LoadAccounts()
            LoadRecurring()
            LoadStartTime()
            LoadDuration()
            LoadProcessInfo()
            LoadInvitees()
        End Sub

        Public Overrides Function GetCurrentView() As ViewType
            Dim view As ViewType
            If String.IsNullOrEmpty(Request.QueryString("View")) Then
                view = MyBase.GetCurrentView() 'fall back to session
            Else
                ' This is the oringal view when this page (Reservation.aspx) was loaded.
                ' It's possible the session value was subsequently changed if the user has multiple tabs open.
                view = CType([Enum].Parse(GetType(ViewType), Request.QueryString("View")), ViewType)
            End If
            Return view
        End Function

        Private Sub SetHeader()
            Dim headerText As String

            If ReservationID = 0 Then
                headerText = "Create Reservation for"
            Else
                headerText = "Modify Reservation for"

                ' make sure the current user is the reserver
                If Reservation.ClientID <> CurrentUser.ClientID Then
                    ShowLoadError($"Reservation #{Reservation.ReservationID} was created by {GetDisplayName()} and cannot be modified by someone else. <a href=""{VirtualPathUtility.ToAbsolute(GetReturnUrl())}"">Return</a>")
                End If
            End If

            litHeader.Text = headerText
            litResourceName.Text = Resources.GetResourceDisplayName(Resource.ResourceName, Resource.ResourceID)
        End Sub

        Private Sub SetStartDate()
            litStartDate.Text = ContextBase.Request.SelectedDate().ToLongDateString()
        End Sub

        Private Function GetDisplayName() As String
            Return Clients.GetDisplayName(Reservation.LName, Reservation.FName)
        End Function

        Private Sub SetClientName()
            If ReservationID = 0 Then
                litClientName.Text = CurrentUser.DisplayName
            Else
                litClientName.Text = GetDisplayName()
            End If
        End Sub

        Private Sub SetIsRecurring()
            ' A session variable is set everytime the IsRecurring checkbox is changed.
            ' This way we know what the initial state is when returning from a calendar date change.
            If Session("IsRecurring") IsNot Nothing Then
                chkIsRecurring.Checked = Convert.ToBoolean(Session("IsRecurring"))
            Else
                chkIsRecurring.Checked = False
            End If
        End Sub

        Private Sub SetKeepAlive()
            If ReservationID = 0 Then
                chkKeepAlive.Checked = True
            Else
                chkKeepAlive.Checked = Reservation.KeepAlive
            End If
        End Sub

        Private Sub SetAutoEnd()
            If ReservationID = 0 Then
                chkAutoEnd.Checked = False
            Else
                chkAutoEnd.Checked = Reservation.ReservationAutoEnd
            End If
        End Sub

        Private Sub SetNotes()
            If ReservationID = 0 Then
                txtNotes.Text = String.Empty
            Else
                txtNotes.Text = Reservation.Notes
            End If
        End Sub

        Private Sub SetSubmitButton()
            btnSubmit.Text = If(ReservationID = 0, "Create", "Modify") + " Reservation"
        End Sub

        Private Sub LoadActivities()
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()

            ddlActivity.DataSource = CacheManager.Current.AuthorizedActivities(authLevel)
            ddlActivity.DataBind()

            ' Preselect value
            ddlActivity.ClearSelection()

            If ReservationID > 0 Then
                TrySelectByValue(ddlActivity, Reservation.ActivityID)
                phActivity.Visible = False
                phActivityName.Visible = True
                litActivityName.Text = Reservation.ActivityName
                phActivityMessage.Visible = False
                litActivityMessage.Text = String.Empty
            End If
        End Sub

        Private Sub LoadAccounts()
            Dim act As IActivity
            Dim client As IClient
            Dim selectedAccountId As Integer

            If ReservationID = 0 Then
                act = GetCurrentActivity()
                client = CurrentUser
                ' [2021-12-21 jg] This is needed to make sure the same account is selected after postbacks, for example if an invitee is added.
                If String.IsNullOrEmpty(ddlAccount.SelectedValue) Then
                    selectedAccountId = -1
                Else
                    selectedAccountId = Integer.Parse(ddlAccount.SelectedValue)
                End If
            Else
                act = CacheManager.Current.GetActivity(Reservation.ActivityID)
                client = Provider.Data.Client.GetClient(Reservation.ClientID)
                selectedAccountId = Reservation.AccountID
            End If

            Dim accts As New List(Of IClientAccount)

            Dim model As New ReservationModel(Helper, Date.Now)

            Dim mustAddInvitee As Boolean = SchedulerUtility.Create(Provider).LoadAccounts(accts, act.AccountType, client, model.GetInvitees(), Context.User.Identity.Name)

            ddlAccount.Enabled = True

            If mustAddInvitee Then
                phBillingAccount.Visible = False
                phBillingAccountMessage.Visible = True
                litBillingAccountMessage.Text = "You must add an invitee before selecting an account."
            Else
                phBillingAccount.Visible = True
                phBillingAccountMessage.Visible = False
                litBillingAccountMessage.Text = String.Empty

                ddlAccount.DataSource = accts.Select(Function(x) New With {.Name = Accounts.GetFullAccountName(x.AccountName, x.ShortCode, x.OrgName), .AccountID = x.AccountID.ToString()})
                ddlAccount.DataBind()

                ' check for scheduled maintenance activity
                If IsGeneralLabActivity(act) Then
                    selectedAccountId = Properties.Current.LabAccount.AccountID
                    ddlAccount.Enabled = False
                End If

                TrySelectByValue(ddlAccount, selectedAccountId)
            End If
        End Sub

        Private Function IsGeneralLabActivity(act As IActivity) As Boolean
            Return act.ActivityID = Properties.Current.Activities.ScheduledMaintenance.ActivityID _
                OrElse act.ActivityID = Properties.Current.Activities.FacilityDownTime.ActivityID
        End Function

        Private Sub LoadRecurring()
            ' No matter what, only staff can see is Recurring option
            If Not CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                phRecurring.Visible = False
                chkIsRecurring.Checked = False
                Return
            End If

            If ReservationID = 0 Then
                phRecurring.Visible = True

                ' Store in session so the checkbox can be set accordingly on subsequent
                ' page loads (e.g. after the date is changed by clicking the calendar).
                Session("IsRecurring") = chkIsRecurring.Checked

                If chkIsRecurring.Checked Then
                    phRecurringReservation.Visible = True

                    txtRecurringStartDate.Text = ContextBase.Request.SelectedDate().ToString("MM/dd/yyyy")

                    Dim dow As DayOfWeek = ContextBase.Request.SelectedDate().DayOfWeek
                    SetSelectedDayOfWeek(dow)

                    ' Set the selected activity to ScheduledMaintenance
                    If TrySelectByValue(ddlActivity, Properties.Current.Activities.ScheduledMaintenance.ActivityID) Then
                        ' Set the account to general lab account
                        If TrySelectByValue(ddlAccount, Properties.Current.LabAccount.AccountID) Then
                            phActivity.Visible = False
                            phActivityName.Visible = False
                            phActivityMessage.Visible = True
                            litActivityMessage.Text = "The Scheduled Maintenance Activity is always used for recurring reservations."

                            phBillingAccount.Visible = False
                            phBillingAccountMessage.Visible = True
                            litBillingAccountMessage.Text = "The General Lab Account is always used for recurring reservations."
                        End If
                    End If
                Else
                    phRecurringReservation.Visible = False

                    txtRecurringStartDate.Text = String.Empty

                    phActivity.Visible = True
                    phActivityName.Visible = False ' always false when ReservationID = 0
                    phActivityMessage.Visible = False
                    litActivityMessage.Text = String.Empty

                    phBillingAccount.Visible = True
                    phBillingAccountMessage.Visible = False
                    litBillingAccountMessage.Text = String.Empty
                End If
            Else
                phRecurring.Visible = False
            End If
        End Sub

        Private Sub SetSelectedDayOfWeek(dow As DayOfWeek)
            Select Case dow
                Case DayOfWeek.Sunday
                    rdoRecurringPatternWeeklySunday.Checked = True
                Case DayOfWeek.Monday
                    rdoRecurringPatternWeeklyMonday.Checked = True
                Case DayOfWeek.Tuesday
                    rdoRecurringPatternWeeklyTuesday.Checked = True
                Case DayOfWeek.Wednesday
                    rdoRecurringPatternWeeklyWednesday.Checked = True
                Case DayOfWeek.Thursday
                    rdoRecurringPatternWeeklyThursday.Checked = True
                Case DayOfWeek.Friday
                    rdoRecurringPatternWeeklyFriday.Checked = True
                Case DayOfWeek.Saturday
                    rdoRecurringPatternWeeklySaturday.Checked = True
            End Select
        End Sub

        Private Function GetSelectedDayOfWeek() As DayOfWeek
            If rdoRecurringPatternWeeklySunday.Checked Then
                Return DayOfWeek.Sunday
            ElseIf rdoRecurringPatternWeeklyMonday.Checked Then
                Return DayOfWeek.Monday
            ElseIf rdoRecurringPatternWeeklyTuesday.Checked Then
                Return DayOfWeek.Tuesday
            ElseIf rdoRecurringPatternWeeklyWednesday.Checked Then
                Return DayOfWeek.Wednesday
            ElseIf rdoRecurringPatternWeeklyThursday.Checked Then
                Return DayOfWeek.Thursday
            ElseIf rdoRecurringPatternWeeklyFriday.Checked Then
                Return DayOfWeek.Friday
            ElseIf rdoRecurringPatternWeeklySaturday.Checked Then
                Return DayOfWeek.Saturday
            End If

            Throw New Exception($"No DayOfWeek radio button is selected.")
        End Function

        Private Sub LoadStartTime()
            ShowPastSelectedDateError(Nothing)

            Dim minTime As TimeSpan

            If 0 = Resource.Granularity Then
                ShowPastSelectedDateError("Granularity is zero for this resource.")
                Return
            End If

            ' Check if selectedDate is in the past
            If ContextBase.Request.SelectedDate() < Date.Now.Date Then
                ShowPastSelectedDateError("The selected date cannot be in the past.")
                Return
            End If

            minTime = GetMinimumStartTime()

            ' Restrictions: Start Time either = end of previous reservation or
            ' the gap between reservations has to be multiples of Min Reserv Time
            ' This is checked for when user clicks on Submit button

            '2011-12-28 start time must be less than or equal to chargeable end time and greater or equal to current time.

            ' Determine 24-hour granularities
            Dim stepSize As Integer = Convert.ToInt32(TimeSpan.FromMinutes(Resource.Granularity).TotalHours)
            Dim offsetTotalHours As Integer = Convert.ToInt32(Resource.Offset)

            If stepSize = 0 Then stepSize = 1
            Dim grans As New List(Of Integer)
            For i As Integer = offsetTotalHours To 24 Step stepSize
                grans.Add(i)
            Next

            ' Load Hours
            ddlStartTimeHour.Items.Clear()

            For i As Integer = 0 To grans.Count - 1
                If grans(i) >= minTime.Hours Then
                    Dim hourText As String = If((grans(i) Mod 12) = 0, "12 ", (grans(i) Mod 12).ToString() + " ")
                    hourText += If(grans(i) < 12, "am", "pm")
                    ddlStartTimeHour.Items.Add(New ListItem(hourText, grans(i).ToString()))
                End If
            Next

            Dim startTimeHour As Integer
            Dim startTimeMin As Integer

            ' Select Preselected Time
            If ReservationID = 0 Then
                ' new reservation
                startTimeHour = ContextBase.Request.SelectedDate().Add(SelectedTime).Hour
                startTimeMin = ContextBase.Request.SelectedDate().Add(SelectedTime).Minute
            Else
                ' existing reservation
                startTimeHour = Reservation.BeginDateTime.Hour
                startTimeMin = Reservation.BeginDateTime.Minute
            End If

            ' Must be called before LoadStartTimeMinutes because that method depends on ddlStartTimeHour.SelectedValue
            TrySelectByValue(ddlStartTimeHour, startTimeHour)

            ' Load Minutes
            LoadStartTimeMinutes()

            TrySelectByValue(ddlStartTimeMin, startTimeMin)
        End Sub

        Private Sub LoadStartTimeMinutes()
            ddlStartTimeMin.Items.Clear()

            Dim hours As Integer = Convert.ToInt32(ddlStartTimeHour.SelectedValue)
            Dim start As Date = ContextBase.Request.SelectedDate().AddHours(hours)

            For i As Integer = 0 To 59 Step Resource.Granularity
                If start.AddMinutes(i) >= Date.Now Then
                    ddlStartTimeMin.Items.Add(New ListItem(i.ToString("00"), i.ToString()))
                End If
            Next
        End Sub

        Private Sub LoadDuration()
            btnSubmit.Enabled = True
            If phStartTimeAndDuration.Visible Then
                Select Case GetDurationInputType()
                    Case DurationInputType.DropDown
                        ShowDurationDropDown()
                    Case DurationInputType.TextBox
                        ShowDurationTextBox()
                End Select
            End If
        End Sub

        Private Sub LoadProcessInfo()
            'This method setups up the ProcessInfo UI. The current ReservationProcessInfo data
            'is populated in RptProcessInfo_ItemDataBound (when modifying).

            Dim processInfos As IEnumerable(Of IProcessInfo) = GetProcessInfos()

            If processInfos.Count = 0 Then
                phProcessInfo.Visible = False
            Else
                phProcessInfo.Visible = True
                rptProcessInfo.DataSource = processInfos.OrderBy(Function(x) x.Order).ThenByDescending(Function(x) x.ProcessInfoID)
                rptProcessInfo.DataBind()
            End If
        End Sub

        Private Sub LoadInvitees()
            'This will force a reload of the session data in DgInvitees_ItemCreated
            Session.Remove($"ReservationInvitees#{Resource.ResourceID}")
            Session.Remove($"AvailableInvitees#{Resource.ResourceID}")

            Dim model As ReservationModel = CreateReservationModel(Date.Now)
            dgInvitees.DataSource = model.GetInvitees().Where(Function(x) Not x.Removed)
            dgInvitees.DataBind()
        End Sub

        Private Function ToTimeString(minutes As Integer) As String
            Dim ts As TimeSpan = TimeSpan.FromMinutes(minutes)
            Return Utility.ToHumanReadableTimeString(ts)
        End Function

        Private Sub ShowDurationDropDown()
            Dim showLimitMessage As Boolean = False

            Dim resourceId As Integer = Resource.ResourceID
            Dim clientId As Integer = GetCurrentClientID()
            Dim beginDateTime As Date = GetSelectedDateTime()
            Dim fence As TimeSpan = TimeSpan.FromMinutes(Resource.ReservFence)
            Dim maxalloc As TimeSpan = TimeSpan.FromMinutes(Resource.MaxAlloc)

            Dim availableRsvMin As AvailableReservationMinutesResult = Provider.Scheduler.Reservation.GetAvailableReservationMinutes(Resource, ReservationID, clientId, beginDateTime)

            Dim maxDuration As Double = availableRsvMin.GetMaxDuration().TotalMinutes

            If maxDuration <= 0 Then ' this means that the reservable time is limited by max schedulable
                showLimitMessage = True
                maxDuration = -1 * maxDuration
            End If

            Dim selectedValue As Integer = GetSelectedDuration()

            ' Duration ranges from Min Reserv Time to Max Reserv Time
            ' or until the start of the next reservation
            ddlDuration.Items.Clear()
            Dim curValue As Double = Resource.MinReservTime
            While curValue <= Resource.MaxReservTime AndAlso curValue <= maxDuration
                Dim hour As Integer = Convert.ToInt32(Math.Floor(curValue / 60))
                Dim minute As Integer = Convert.ToInt32(curValue Mod 60)
                Dim text As String = $"{hour} hr {minute} min"

                ddlDuration.Items.Add(New ListItem(text, curValue.ToString()))

                curValue += Resource.Granularity
            End While

            'Duration affect user's eligibility to make reservation.
            If ddlDuration.Items.Count = 0 Then
                Dim alertMsg As String

                'ReasonB and ReasonC should both result in ddlDuration.Items.Count > 0

                Dim availableMsg As String = $"You have {ToTimeString(availableRsvMin.AvailableReservationMinutes)} available because you may reserve up to {ToTimeString(Resource.MaxAlloc)} on this tool, and you currently have {ToTimeString(availableRsvMin.ReservedMinutes)} reserved."

                If availableRsvMin.Reason = AvailableReservationMinutesResult.ReasonA Then
                    alertMsg = $"<strong>Your total reserved time exceeds the limit set for this tool.</strong><br><br>{availableMsg}"
                ElseIf availableRsvMin.Reason = AvailableReservationMinutesResult.ReasonD Then
                    If maxDuration > 0 AndAlso maxDuration < Resource.MinReservTime Then
                        alertMsg = $"<strong>You do not have enough available time to meet the minimum reservation requirement of {ToTimeString(Resource.MinReservTime)}.</strong><br><br>{availableMsg}"
                    Else
                        alertMsg = $"<strong>You do not have enough available time to make another reservation.</strong><br><br>{availableMsg}"
                    End If
                Else
                    Dim nextRsv As IReservation = Provider.Scheduler.Reservation.GetNextReservation(Resource.ResourceID, ReservationID)

                    Dim rsvInfo As String

                    If nextRsv IsNot Nothing Then
                        Dim fullName As String = $"{nextRsv.FName} {nextRsv.LName}"
                        rsvInfo = $"Next reservation: from {nextRsv.BeginDateTime:M/d/yyyy h:mm tt} to {nextRsv.EndDateTime:M/d/yyyy h:mm tt}, created by {fullName}."
                    Else
                        rsvInfo = "Unable to find next reservation."
                    End If

                    alertMsg = $"<strong>Another reservation has already been made for this time. Please select a different start time.</strong><br><br>{rsvInfo}"
                End If

                BootstrapAlert1.Show(alertMsg, AlertType.Danger)

                btnSubmit.Enabled = False
            Else
                If ReservationID > 0 Then
                    selectedValue = Convert.ToInt32(Reservation.Duration)
                End If

                If Not TrySelectByValue(ddlDuration, selectedValue) Then
                    ddlDuration.SelectedIndex = -1
                End If

                btnSubmit.Enabled = True
            End If

            lblMaxSchedLimit.Visible = showLimitMessage

            phDurationSelect.Visible = True
            phDurationText.Visible = False
        End Sub

        Private Sub ShowDurationTextBox()
            Dim selectedValue As Integer = GetSelectedDuration()

            If ReservationID > 0 Then
                selectedValue = Convert.ToInt32(Reservation.Duration)
            End If

            txtDuration.Text = selectedValue.ToString()

            phDurationSelect.Visible = False
            phDurationText.Visible = True
        End Sub

        Private Function GetDurationInputType() As DurationInputType
            Dim act As IActivity = GetCurrentActivity()
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()

            'Right now, to have textbox, the authlevel must be included in NoMaxSchedAuth, and only only sched. maintenance and characterization

            Dim textboxActivities As Integer() = {15, 18, 23}

            If (act.NoMaxSchedAuth And authLevel) > 0 AndAlso textboxActivities.Contains(act.ActivityID) Then
                Return DurationInputType.TextBox
            Else
                Return DurationInputType.DropDown
            End If
        End Function

        Private Function GetSelectedDuration() As Integer
            Dim mrt As Integer = 0
            Dim result As Integer = 0

            Dim val As String

            If phDurationText.Visible Then
                val = txtDuration.Text
            Else
                val = ddlDuration.SelectedValue
            End If

            If Resource IsNot Nothing Then
                mrt = Resource.MinReservTime
            End If

            If String.IsNullOrEmpty(val) Then
                result = mrt
            Else
                If Not Integer.TryParse(val, result) Then
                    result = mrt
                End If
            End If

            If result = 0 Then
                Throw New Exception("Selected Duration cannot be zero.")
            End If

            Return result
        End Function

        ''' <summary>
        ''' Returns the earliest possible reservation time.
        ''' </summary>
        Private Function GetMinimumStartTime() As TimeSpan
            ' First check if the selected date is in the future. If so the result is 00:00:00
            If ContextBase.Request.SelectedDate() > Date.Now.Date Then
                Return TimeSpan.Zero
            End If

            ' Next check if the selected date is the current day. If so the result will is based on the current hour
            If ContextBase.Request.SelectedDate() = Date.Now.Date Then
                Dim minTime As TimeSpan = TimeSpan.FromHours(Date.Now.Hour) ' the earliest possible hour must be the current hour
                Dim addTime As TimeSpan = TimeSpan.Zero

                If Resource.Granularity < 60 Then
                    ' check if remaining time in the current hour is less than or equal to the granularity, if so then add an hour
                    If (60 - Date.Now.Minute) <= Resource.Granularity Then
                        addTime = TimeSpan.FromHours(1)
                    End If
                Else
                    ' add an hour if the granularity is 60 minutes or more
                    addTime = TimeSpan.FromHours(1)
                End If

                minTime = minTime.Add(addTime)

                Return minTime
            End If

            ' If we haven't returned yet then the last possiblility is the selectedDate is in the past so throw exception
            Throw New Exception("The selected date cannot be in the past.")
        End Function

        Private Function GetCurrentDurationMinutes() As Integer
            If GetDurationInputType() = DurationInputType.TextBox Then
                If String.IsNullOrEmpty(txtDuration.Text.Trim()) Then
                    Throw New Exception("Please enter the duration in minutes.")
                End If
                Return Convert.ToInt32(txtDuration.Text.Trim())
            Else
                If ddlDuration.Items.Count = 0 Then
                    Return 0
                Else
                    Return Convert.ToInt32(ddlDuration.SelectedValue)
                End If
            End If
        End Function

        Private Sub ValidateInviteeReservations(invitees As IList(Of Invitee))
            ShowInviteeWarning(Nothing)

            Dim duration As Integer = GetCurrentDurationMinutes()

            Dim startDateTime As Date = GetSelectedDateTime()

            If invitees IsNot Nothing Then
                If invitees.Count > 0 Then
                    Dim names As New List(Of String)

                    For Each inv As Invitee In invitees
                        ' Get the conflicting reservations for this invitee. Exclude the current reservation because
                        ' no warning is needed if a removed invitee is added back to the same reservation.
                        Dim inviteeReservations As List(Of IReservationItem) = GetInviteeReservations(inv.InviteeID, startDateTime, duration).ToList()

                        If inviteeReservations.Count > 0 Then
                            Dim hasConflict As Boolean = False

                            For Each rsv In inviteeReservations
                                If rsv.ReservationID <> ReservationID Then
                                    hasConflict = True
                                End If
                            Next

                            If hasConflict Then
                                names.Add(inv.DisplayName)
                            End If
                        End If
                    Next

                    ShowInviteeWarning(names)
                End If
            End If
        End Sub

        Private Function GetInviteeReservations(inviteeClientId As Integer, startDateTime As Date, duration As Integer) As IEnumerable(Of IReservationItem)
            Dim endDateTime As Date = startDateTime.AddMinutes(duration)
            Dim inviteeReservations As IEnumerable(Of IReservationItem) = Provider.Scheduler.Reservation.SelectByClient(inviteeClientId, ContextBase.Request.SelectedDate().AddHours(0), ContextBase.Request.SelectedDate().AddHours(24), False)
            Dim conflictingReservations As IEnumerable(Of IReservationItem) = Reservations.GetConflictingReservations(inviteeReservations, startDateTime, endDateTime)
            Return conflictingReservations
        End Function

        Private Function GetProcessInfos() As IEnumerable(Of IProcessInfo)
            Dim resourceId As Integer = Resource.ResourceID

            If Items($"ProcessInfos#{resourceId}") Is Nothing Then
                Items($"ProcessInfos#{resourceId}") = Provider.Scheduler.ProcessInfo.GetProcessInfos(resourceId)
            End If

            Dim result As IEnumerable(Of IProcessInfo) = CType(Items($"ProcessInfos#{resourceId}"), IEnumerable(Of IProcessInfo))

            Return result
        End Function

        Private Function GetProcessInfoLines() As IEnumerable(Of IProcessInfoLine)
            Dim resourceId As Integer = Resource.ResourceID

            If Items($"ProcessInfoLines#{resourceId}") Is Nothing Then
                Items($"ProcessInfoLines#{resourceId}") = Provider.Scheduler.ProcessInfo.GetProcessInfoLines(resourceId)
            End If

            Dim result As IEnumerable(Of IProcessInfoLine) = CType(Items($"ProcessInfoLines#{resourceId}"), IEnumerable(Of IProcessInfoLine))

            Return result
        End Function

        Private Function GetReservationDuration() As ReservationDuration
            Dim beginDateTime As Date = ContextBase.Request.SelectedDate().AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue)).AddMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue))
            Dim currentDurationMinutes As Integer = GetCurrentDurationMinutes()
            Dim result As New ReservationDuration(beginDateTime, TimeSpan.FromMinutes(currentDurationMinutes))
            Return result
        End Function

        Private Sub HandleRecurringReservation(duration As ReservationDuration, client As IClient)
            If String.IsNullOrEmpty(txtDuration.Text) Then
                LoadDuration()
                ShowReservationAlert("You must specify the duration.")
                Return
            End If

            'we need:
            '   BeginDate   - the start date of the recurrance
            '   EndDate     - the end date of the recurrance or null if infinite
            '   BeginTime   - the start date plus the start time
            '   Duration    - used to calculate the EndDateTime of reservations

            Dim rrDuration As Double = 0
            Dim rrStartDate As Date
            If Double.TryParse(txtDuration.Text, rrDuration) Then
                If ReservationID = 0 Then
                    ' new recurring reservation
                    If Date.TryParse(txtRecurringStartDate.Text, rrStartDate) Then
                        Dim patId As Integer = If(rdoRecurringPatternWeekly.Checked, 1, 2)

                        Dim rrEndDate As Date? = Nothing
                        If rdoRecurringRangeEndBy.Checked Then
                            Dim val As Date
                            If Date.TryParse(txtEndDate.Value, val) Then
                                rrEndDate = val
                            Else
                                chkIsRecurring.Checked = True
                                LoadDuration()
                                ShowReservationAlert("Invalid end date.")
                                Return
                            End If
                        End If

                        Dim rrPatternParam1 As Integer
                        Dim rrPatternParam2 As Integer?
                        If rdoRecurringPatternWeekly.Checked Then
                            ' Day-of-week is stored in SQL as zero based index (0 = Sunday, 1 = Monday, etc). Note that SQL uses a 1 based index.
                            rrPatternParam1 = Convert.ToInt32(GetSelectedDayOfWeek())
                            rrPatternParam2 = Nothing
                        ElseIf rdoRecurringPatternMonthly.Checked Then
                            rrPatternParam1 = Convert.ToInt32(ddlMonthly1.SelectedValue)
                            rrPatternParam2 = Convert.ToInt32(ddlMonthly2.SelectedValue)
                        End If

                        Dim beginTime As Date = GetSelectedDateTime()

                        Dim recurrenceId As Integer = Provider.Scheduler.Reservation.InsertReservationRecurrence(
                            resourceId:=Resource.ResourceID,
                            clientId:=client.ClientID,
                            accountId:=Properties.Current.LabAccount.AccountID,
                            activityId:=Properties.Current.Activities.ScheduledMaintenance.ActivityID,
                            patternId:=patId,
                            param1:=rrPatternParam1,
                            param2:=rrPatternParam2,
                            beginDate:=rrStartDate,
                            endDate:=rrEndDate,
                            beginTime:=beginTime,
                            duration:=rrDuration,
                            autoEnd:=chkAutoEnd.Checked,
                            keepAlive:=chkKeepAlive.Checked,
                            notes:=txtNotes.Text)

                        Dim model As ReservationModel = CreateReservationModel(Date.Now, recurrenceId)
                        model.CreateOrModifyReservation(duration)
                        ReturnToResourceDayWeek()
                    Else
                        ShowReservationAlert("Error in saving Recurring Reservation: Invalid StartDate.")
                    End If
                End If
            Else
                ShowReservationAlert("Error in saving Recurring Reservation: Invalid Duration.")
            End If
        End Sub

        Private Function StoreReservationProcessInfo(model As ReservationModel) As Boolean
            For i As Integer = 0 To rptProcessInfo.Items.Count - 1
                ' ProcessInfoLine
                Dim ddlParam As DropDownList = CType(rptProcessInfo.Items(i).FindControl("ddlParam"), DropDownList)

                If String.IsNullOrEmpty(ddlParam.SelectedValue) Then
                    ShowReservationAlert("You must select a Process Info Line.")
                    Return False
                End If

                ' ProcessInfo
                Dim processInfoId As Integer = Convert.ToInt32(CType(rptProcessInfo.Items(i).FindControl("hidPIID"), HiddenField).Value)
                Dim processInfoLineId As Integer = Convert.ToInt32(ddlParam.SelectedValue)
                Dim valueText As String = "0"
                Dim special As Boolean = False

                ' check if a ProcessInfoLine was selected
                If processInfoLineId <> 0 Then
                    ' Validate ProcessInfo Value
                    If CType(rptProcessInfo.Items(i).FindControl("txtValue"), TextBox).Visible Then
                        valueText = CType(rptProcessInfo.Items(i).FindControl("txtValue"), TextBox).Text
                    End If

                    Dim value As Integer
                    If Integer.TryParse(valueText, value) Then
                        Dim pil As IProcessInfoLine = GetProcessInfoLines().First(Function(x) x.ProcessInfoLineID = processInfoLineId)
                        Dim pi As IProcessInfo = GetProcessInfos().First(Function(x) x.ProcessInfoID = pil.ProcessInfoID)

                        special = CType(rptProcessInfo.Items(i).FindControl("chkSpecial"), CheckBox).Checked

                        ' get the existing ReservationProcessInfo list
                        Dim infos As List(Of ReservationProcessInfoItem) = model.GetReservationProcessInfos()

                        ' If a reservation is being modified we need to modify an existing ReservationProcessInfo for the current ProcessInfo.
                        ' Note that it's possible the ProcessInfoLineID is getting modified here.
                        Dim rpi As ReservationProcessInfoItem = infos.FirstOrDefault(Function(x) x.ProcessInfoID = processInfoId)

                        If rpi Is Nothing Then
                            ' The ProcessInfoID was not found. Must be a new reservation, or maybe a new ProcessInfo was added since the reservation was first created? (unlikely but possible)
                            infos.Add(CreateReservationProcessInfoItem(processInfoLineId, value, special, pi))
                        Else
                            ' Modify the existing ReservationProcessInfo. At some point the database must be updated.
                            rpi.ProcessInfoLineID = pil.ProcessInfoLineID
                            rpi.Value = value
                            rpi.Special = special
                        End If
                    Else
                        ShowReservationAlert($"Invalid value for process info: {valueText}")
                        Return False
                    End If
                Else
                    Dim infos As List(Of ReservationProcessInfoItem) = model.GetReservationProcessInfos()
                    Dim rpi As ReservationProcessInfoItem = infos.FirstOrDefault(Function(x) x.ProcessInfoID = processInfoId)
                    If rpi IsNot Nothing Then
                        infos.Remove(rpi)
                    End If
                End If
            Next

            Return True
        End Function

        Private Function CreateReservationProcessInfoItem(processInfoLineId As Integer, value As Integer, special As Boolean, pi As IProcessInfo) As ReservationProcessInfoItem
            Return New ReservationProcessInfoItem With {
                .ReservationID = ReservationID,
                .ProcessInfoLineID = processInfoLineId,
                .Value = value,
                .Special = special,
                .Active = True,
                .ChargeMultiplier = 1,
                .ProcessInfoID = pi.ProcessInfoID,
                .RunNumber = 0
            }

            ' .Param = pil.Param,
            '.ProcessInfoName = pi.ProcessInfoName,
        End Function

        Private Sub InviteeModification(action As String, ddlInvitees As DropDownList, lblInviteeID As Label)
            Dim model As ReservationModel = CreateReservationModel(Date.Now)

            Dim invitees As List(Of Invitee) = model.GetInvitees()
            Dim available As List(Of AvailableInvitee) = model.GetAvailableInvitees()

            Dim inv As Invitee

            If action = "Insert" Then
                ' Insert invitee into ReservInvitee list
                If ddlInvitees.Items.Count = 0 Then Return

                Dim clientId As Integer = Integer.Parse(ddlInvitees.SelectedValue)
                Dim avail = available.FirstOrDefault(Function(x) x.ClientID = clientId)

                ' make sure the selected invitee is in the list of available invitees
                If avail IsNot Nothing Then
                    ' check to see if the invitee already included (removed)
                    inv = invitees.FirstOrDefault(Function(x) x.InviteeID = clientId)

                    Dim item As Invitee

                    If inv Is Nothing Then
                        item = New Invitee With {
                            .ReservationID = ReservationID,
                            .InviteeID = avail.ClientID,
                            .LName = avail.LName,
                            .FName = avail.FName,
                            .Removed = False
                        }

                        invitees.Add(item)
                    Else
                        item = inv
                        item.Removed = False
                    End If

                    ' avail will be removed from session in DgInvitees_ItemCreated

                    ' Give Warning if this invitee has made another reservation at the same time
                    ' 2007-03-22 a bug exists here. If modifying the reservation, the the code still mistakenly think the current modfiying reservation as another reservation
                    ValidateInviteeReservations(New List(Of Invitee) From {item})
                End If
            ElseIf action = "Delete" Then
                ' Insert invitee into AvailInvitee list and remove from ReservInvitee list
                inv = invitees.FirstOrDefault(Function(x) x.InviteeID = Integer.Parse(lblInviteeID.Text))

                If inv IsNot Nothing Then
                    inv.Removed = True

                    Dim avail As New AvailableInvitee() With {
                        .ClientID = inv.InviteeID,
                        .LName = inv.LName,
                        .FName = inv.FName
                    }

                    available.Add(avail)
                End If
            End If

            dgInvitees.DataSource = invitees.Where(Function(x) Not x.Removed).OrderBy(Function(x) x.DisplayName)
            dgInvitees.DataBind()
        End Sub

        Private Sub StartReservation(reservationId As Integer)
            Dim rsv As IReservationWithInvitees = Provider.Scheduler.Reservation.GetReservationWithInvitees(reservationId)
            Dim rc As ReservationClient = Helper.GetReservationClient(rsv)
            Reservations.Create(Provider, Date.Now).Start(rsv, rc, CurrentUser.ClientID)
        End Sub

        Private Sub ReturnToResourceDayWeek()
            Dim redirectUrl As String = GetReturnUrl()
            ClearSession()
            Helper.AppendLog($"Reservation.ReturnToResourceDayWeek: QueryString = {Request.QueryString}, redirectUrl = {redirectUrl}")
            Response.Redirect(redirectUrl, False)
        End Sub

        Private Function GetReturnUrl() As String
            Dim result As String

            If Session("ReturnTo") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Session("ReturnTo").ToString()) Then
                result = Session("ReturnTo").ToString()
            Else
                Dim view As ViewType = GetCurrentView()

                ' need to update the session in case the querystring value is now different
                ' e.g. because the user maybe loaded a different view in another tab
                SetCurrentView(view)

                If view = ViewType.UserView Then
                    result = $"~/UserReservations.aspx?Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                ElseIf view = ViewType.ProcessTechView Then
                    ' When we come from ProcessTech.aspx the full path is used (to avoid a null object error). When returning we just want the ProcessTech path.
                    Dim pt As IProcessTech = Helper.GetCurrentProcessTech()
                    Dim path As PathInfo = PathInfo.Create(pt)
                    result = $"~/ProcessTech.aspx?Path={path.UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                ElseIf view = ViewType.LocationView Then
                    Dim locationPath As LocationPathInfo = ContextBase.Request.SelectedLocationPath()
                    If locationPath.IsEmpty() Then
                        ' In this case the querstring view is LocationView but there is no LocationPath querystring var.
                        ' Not sure how this can happen except from messing around with the querystring. In order to avoid
                        ' an error try to get the location from the current resource, send to ResoruceDayWeek.aspx if none,
                        ' otherwise send the user to UserReservations.aspx.

                        Dim res As IResource = Helper.GetCurrentResource()

                        Dim msg As String

                        If res IsNot Nothing Then
                            Dim loc As ILabLocation = Helper.GetLabLocation(res)
                            If loc IsNot Nothing Then
                                result = $"~/LabLocation.aspx?LocationPath={LocationPathInfo.Create(loc).UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                                msg = "got LocationPathInfo from Helper.GetLabLocation"
                            Else
                                result = $"~/ResourceDayWeek.aspx?Path={ContextBase.Request.SelectedPath().UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                                msg = $"no location for resouce {res}, got PathInfo from Request.SelectedPath"
                            End If
                        Else
                            result = $"~/UserReservations.aspx?Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                            msg = "no location or resource (wtf?), falling back to UserReservations.aspx"
                        End If

                        Dim vars As New Dictionary(Of String, Object) From {
                            {"result", $"{result} [{msg}]"}
                        }

                        Helper.SendDebugEmail("Reservation.aspx", $"Redirect to LocationView failed", "Cannot construct return url to LabLocation.aspx because LocationPathInfo is empty.", vars)
                    Else
                        result = $"~/LabLocation.aspx?LocationPath={locationPath.UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                    End If
                Else 'ViewType.DayView OrElse Scheduler.ViewType.WeekView
                    result = $"~/ResourceDayWeek.aspx?Path={ContextBase.Request.SelectedPath().UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                End If
            End If

            Return result
        End Function

        Private Sub ShowLoadError(text As String)
            If String.IsNullOrEmpty(text) Then
                phLoadError.Visible = False
                litLoadError.Text = String.Empty
                phReservation.Visible = True
            Else
                phLoadError.Visible = True
                litLoadError.Text = text
                phReservation.Visible = False
            End If
        End Sub

        Private Sub ShowPastSelectedDateError(text As String)
            If String.IsNullOrEmpty(text) Then
                phPastSelectedDateError.Visible = False
                litPastSelectedDateError.Text = String.Empty
                phStartTimeAndDuration.Visible = True
                btnSubmit.Enabled = True
            Else
                phPastSelectedDateError.Visible = True
                litPastSelectedDateError.Text = text
                phStartTimeAndDuration.Visible = False
                btnSubmit.Enabled = False
            End If
        End Sub

        Private Sub ShowConfirmError(text As String)
            If String.IsNullOrEmpty(text) Then
                phConfirmError.Visible = False
                litConfirmError.Text = String.Empty
            Else
                phConfirmError.Visible = True
                litConfirmError.Text = text
            End If
        End Sub

        Private Sub ShowInviteeWarning(names As List(Of String))
            If names Is Nothing OrElse names.Count = 0 Then
                phInviteeWarning.Visible = False
                litInviteeWarning.Text = String.Empty
            Else
                phInviteeWarning.Visible = True
                If names.Count > 1 Then
                    litInviteeWarning.Text = $"Please be aware that the following invitees have made another reservation at this time: <ul><li>{String.Join("</li><li>", names)}</li></ul>"
                Else
                    litInviteeWarning.Text = $"Please be aware that {names(0)} has made another reservation at this time."
                End If

            End If
        End Sub

        Private Sub ShowReservationAlert(text As String, Optional type As String = "danger", Optional dismissable As Boolean = True)
            If String.IsNullOrEmpty(text) Then
                phReservationAlert.Visible = False
                litReservationAlert.Text = String.Empty
                Return
            End If

            If dismissable Then
                divReservationAlert.Attributes("class") = $"alert alert-{type} alert-dismissible"
                btnReservationAlertClose.Visible = True
            Else
                divReservationAlert.Attributes("class") = $"alert alert-{type}"
                btnReservationAlertClose.Visible = False
            End If

            phReservationAlert.Visible = True
            litReservationAlert.Text = text
        End Sub

        Private Function TrySelectByValue(ddl As DropDownList, value As Object) As Boolean
            If ddl.Items.Count = 0 OrElse value Is Nothing Then
                Return False
            End If

            Dim item As ListItem = ddl.Items.FindByValue(value.ToString())

            If item IsNot Nothing Then
                item.Selected = True
                ddl.SelectedValue = value.ToString()
                Return True
            End If

            Return False
        End Function

        Protected Sub DdlStartTimeHour_SelectedIndexChanged(sender As Object, e As EventArgs)
            LoadDuration()
            LoadStartTimeMinutes()
            ValidateInviteeReservations(CreateReservationModel(Date.Now).GetInvitees())
        End Sub

        Protected Sub DdlStartTimeMin_SelectedIndexChanged(sender As Object, e As EventArgs)
            LoadDuration()
            ValidateInviteeReservations(CreateReservationModel(Date.Now).GetInvitees())
        End Sub

        Protected Sub DdlDuration_SelectedIndexChanged(sender As Object, e As EventArgs)
            ValidateInviteeReservations(CreateReservationModel(Date.Now).GetInvitees())
        End Sub

        Protected Sub ChkIsRecurring_CheckedChanged(sender As Object, e As EventArgs)
            LoadAccounts()
            LoadRecurring()
            LoadDuration()
            LoadInvitees()
        End Sub

        Protected Sub DdlActivity_SelectedIndexChanged(sender As Object, e As EventArgs)
            LoadAccounts()
            LoadDuration()
            LoadInvitees()
        End Sub

        Protected Sub RptProcessInfo_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                ' Populate Process Info
                Dim di As New DataItemHelper(e.Item.DataItem)

                Dim processInfoId As Integer = di("ProcessInfoID").AsInt32

                CType(e.Item.FindControl("hidPIID"), HiddenField).Value = processInfoId.ToString()
                CType(e.Item.FindControl("litPIName"), Literal).Text = di("ProcessInfoName").AsString
                CType(e.Item.FindControl("litParamName"), Literal).Text = di("ParamName").AsString

                If di("RequireValue").AsBoolean Then
                    CType(e.Item.FindControl("litValueName"), Literal).Text = di("ValueName").AsString
                Else
                    CType(e.Item.FindControl("litValueName"), Literal).Visible = False
                    CType(e.Item.FindControl("txtValue"), TextBox).Visible = False
                End If

                If String.IsNullOrEmpty(di("Special").AsString) Then
                    CType(e.Item.FindControl("chkSpecial"), CheckBox).Visible = False
                Else
                    CType(e.Item.FindControl("chkSpecial"), CheckBox).Text = di("Special").AsString
                End If

                ' Process Info Param dropdownlist
                Dim pils As IEnumerable(Of IProcessInfoLine) = GetProcessInfoLines().Where(Function(x) x.ProcessInfoID = processInfoId).OrderBy(Function(x) x.Param).ToList()

                Dim ddlParam As DropDownList = CType(e.Item.FindControl("ddlParam"), DropDownList)

                If di("RequireSelection").AsBoolean Then
                    ddlParam.Items.Add(New ListItem("", ""))
                End If

                If di("AllowNone").AsBoolean Then     ' Allow None
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
                Dim model As ReservationModel = CreateReservationModel(Date.Now)
                Dim infos As List(Of ReservationProcessInfoItem) = model.GetReservationProcessInfos()
                Dim rpi As ReservationProcessInfoItem = infos.FirstOrDefault(Function(x) x.ProcessInfoID = processInfoId)

                If rpi IsNot Nothing Then
                    ddlParam.Items.FindByValue(rpi.ProcessInfoLineID.ToString()).Selected = True
                    CType(e.Item.FindControl("txtValue"), TextBox).Text = rpi.Value.ToString()
                    CType(e.Item.FindControl("chkSpecial"), CheckBox).Checked = rpi.Special
                Else
                    ddlParam.SelectedIndex = 0
                End If
            End If
        End Sub

        Protected Sub DgInvitees_ItemCommand(sender As Object, e As DataGridCommandEventArgs)
            ShowReservationAlert(Nothing) ' clears it
            ShowInviteeWarning(Nothing)
            Dim ddlInvitees As DropDownList = CType(e.Item.FindControl("ddlInvitees"), DropDownList)
            Dim lblInviteeID As Label = CType(e.Item.FindControl("lblInviteeID"), Label)
            Dim lblInviteeName As Label = CType(e.Item.FindControl("lblInviteeName"), Label)
            InviteeModification(e.CommandName, ddlInvitees, lblInviteeID)
            LoadAccounts()
        End Sub

        Protected Sub DgInvitees_ItemCreated(sender As Object, e As DataGridItemEventArgs)
            If e.Item.ItemType = ListItemType.Footer Then
                ' Select Available Invitees and Remove already Selected Invitees

                Dim model As ReservationModel = CreateReservationModel(Date.Now)

                Dim invitees As List(Of Invitee) = model.GetInvitees()
                Dim available As List(Of AvailableInvitee) = model.GetAvailableInvitees()

                For Each inv As Invitee In invitees
                    If Not inv.Removed Then
                        Dim avail As AvailableInvitee = available.FirstOrDefault(Function(x) x.ClientID = inv.InviteeID)
                        available.Remove(avail)
                    End If
                Next

                Dim ddlInvitees As DropDownList = CType(e.Item.FindControl("ddlInvitees"), DropDownList)

                ddlInvitees.DataSource = available.OrderBy(Function(x) x.LName).ThenBy(Function(x) x.FName)
                ddlInvitees.DataBind()
            End If
        End Sub

        Protected Sub DgInvitees_ItemDataBound(sender As Object, e As DataGridItemEventArgs)
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim item As Invitee = CType(e.Item.DataItem, Invitee)
                CType(e.Item.FindControl("lblInviteeID"), Label).Text = item.InviteeID.ToString()
                CType(e.Item.FindControl("lblInviteeName"), Label).Text = item.DisplayName
            End If
        End Sub

        Protected Sub BtnSubmit_Click(sender As Object, e As EventArgs)
            Helper.AppendLog($"Reservation.BtnSubmit_Click: ReservationID = {ReservationID}, QueryString = ""{Request.QueryString}"", CurrentView = {GetCurrentView()}")

            Dim client As IClient
            Dim activity As IActivity

            If ReservationID = 0 Then
                client = CurrentUser
                activity = CacheManager.Current.GetActivity(Integer.Parse(ddlActivity.SelectedValue))
            Else
                client = Provider.Data.Client.GetClient(Reservation.ClientID)
                activity = CacheManager.Current.GetActivity(Reservation.ActivityID) 'remember, the activity cannot be changed once a reservation is made
            End If

            ShowReservationAlert(Nothing) ' clears it

            Dim model As ReservationModel = CreateReservationModel(Date.Now)

            ' Store Reservation Process Info
            If Not StoreReservationProcessInfo(model) Then
                Return
            End If

            ' Validate Reservation
            Dim rd As ReservationDuration = GetReservationDuration()

            If rd.Duration = TimeSpan.Zero Then
                ShowReservationAlert("Duration must be greater than zero.")
                Return
            End If

            '2009-01 After the addition of the recurrence schedule feature, we should also distinguish between regular reservation and recurrence reservation
            If chkIsRecurring.Checked Then
                HandleRecurringReservation(rd, client)
                Return
            End If

            ' Ensure that there are accounts from which to select
            If ddlAccount.Items.Count = 0 Then
                ShowReservationAlert("No selectable billing accounts.")
                Return
            End If

            ' Check Billing Account
            If ddlAccount.SelectedValue = "-1" Then
                ShowReservationAlert("Please select a valid billing account.")
                Return
            End If

            ' Check for Invitee numbers.  Invitee authorizations are filtered in store procedures.
            '2007-03-02 Get the number of added rows
            'I have to get the count of rows that are not deleted, because the Table.Rows.Count return all rows 
            'despite the row state

            'the # of rows that are not "deleted"
            Dim activeRowCount = model.GetInvitees().Where(Function(x) Not x.Removed).ToArray().Length

            ' Proxy activities are activities for reservations that our made by staff on behalf of someone else.
            ' Currently Future Practice (21) and Remote Processing (22) are the only two proxy activities.
            Dim proxyActivities As Integer() = {21, 22}

            '2007-03-22 Check if remote processing / future practice has more than 1 invitee, which is not allowed now
            If activeRowCount > 1 AndAlso proxyActivities.Contains(activity.ActivityID) Then
                ShowReservationAlert("Currently you can have ONLY one invitee for the type of activity you are reserving.")
                Return
            End If

            Select Case activity.InviteeType
                Case ActivityInviteeType.None
                    If activeRowCount > 0 Then
                        ShowReservationAlert("You cannot invite anyone to this reservation.")
                        Return
                    End If
                Case ActivityInviteeType.Required
                    If activeRowCount = 0 Then
                        ShowReservationAlert("You must invite someone to this reservation.")
                        Return
                    End If
                Case ActivityInviteeType.Single
                    If activeRowCount <> 1 Then
                        ShowReservationAlert("You must invite a single person to this reservation.")
                        Return
                    End If
            End Select

            '2007-03-05 if User is making remote processing or future practice, then we have to make sure the invitee's max sched hours
            'do not go over the limit

            '[2020-01-15 jg] If staff needs to exceed the max schedulable hours for the invitee (happens occasionally) they should coordinate with
            '       the tool engineer to extend the limit temporarily. This was the agreed upon solution after discussing the issue in the weekly
            '       staff meeting on 2020-01-14 and per Sandrine. I'm adding the setting check just in case we ever need to disable this rule.
            If Properties.Current.EnforceMaxSchedulableHoursOnRemoteProcessingInvitees Then
                If proxyActivities.Contains(activity.ActivityID) Then
                    For Each ri In model.GetInvitees()
                        Dim availableMinutes As Integer = Provider.Scheduler.Reservation.GetAvailableSchedMin(Resource.ResourceID, ri.InviteeID)

                        'This code is trying to solve the problem about calculating correct available time when current reservation being modified
                        'must be excluded.  So we must added the current reservation's duration back to available time

                        If ReservationID > 0 Then
                            availableMinutes += Convert.ToInt32(Reservation.Duration)
                        End If

                        If rd.Duration.TotalMinutes > availableMinutes Then
                            ShowReservationAlert($"The reservation you are making exceed your invitee's maximum reservable hours. You can only reserve {availableMinutes} minutes for this invitee.")
                            Return
                        End If

                        Exit For
                    Next
                End If
            End If

            ' Check Reservation Fence Constraint for current user's auth level
            ' 2007-03-03 Currently this should be the only place to check the Reserv. Fence Constraint, because
            ' we cannot check it until user specify the Activity Type.
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()
            If (activity.NoReservFenceAuth And Convert.ToInt32(authLevel)) = 0 Then
                If Date.Now.AddMinutes(Resource.ReservFence) <= rd.BeginDateTime Then
                    ShowReservationAlert("You are trying to make a reservation that is too far in the future.")
                    Return
                End If
            End If

            ' ddlDuration prevents exceeding max reservable - the only way to beat this is to be logged onto two system
            '  and to start making two reservations at the same time. To combat this, the final check is made in the SP
            Dim alert As String = String.Empty
            If model.IsThereAlreadyAnotherReservation(rd, alert) Then
                ShowReservationAlert(alert)
                Return
            End If

            ' Restrictions: Start Time either has to equal end of previous reservation or
            ' the gap between reservations has to be at least = Min Reserv Time
            '2007-11-14 Temporarily disable this feature, so users can make reservatin at anytime they want
            'If AuthLevel <> AuthLevels.Engineer Then
            '	Dim TimeSinceLastReservation As Integer = rsvDB.GetTimeSinceLastReservation(ReservationID, ResourceID, BeginDateTime)
            '	If TimeSinceLastReservation <> 0 AndAlso TimeSinceLastReservation < resDB.MinReservTime Then
            '		CommonTools.JSAlert(Page, "Requested reservation too close to previous reservation. Please allow " + resDB.MinReservTime.ToString() + " minutes between two reservations.  Contact tool engineer if you have any questions")
            '		Return
            '	End If
            'End If

            ' All checks complete

            '2009-12-07 Check if the reservation lies within lab clean
            Dim isLabCleanTime As Boolean = SchedulerUtility.Create(Provider).ShowLabCleanWarning(rd.BeginDateTime, rd.EndDateTime)

            ' get actual costs based on the selected account
            'Dim mCompile As New Compile()
            Dim estimator As New CostEstimator(Provider)
            Dim dblCost As Double = -1
            Dim dblCostStr As String
            Dim acct As IAccount = Provider.Data.Account.GetAccount(Convert.ToInt32(ddlAccount.SelectedValue))

            Try
                'dblCost = mCompile.EstimateToolRunCost(Convert.ToInt32(ddlAccount.SelectedValue), Resource.ResourceID, rd.Duration.TotalMinutes)
                dblCost = estimator.EstimateToolRunCost(acct.ChargeTypeID, Resource.ResourceID, rd.Duration.TotalMinutes)
                dblCostStr = dblCost.ToString("$#,##0.00")
            Catch ex As Exception
                dblCostStr = dblCost.ToString("ERR")
            End Try

            ' Display Submit Confirmation
            lblConfirm.Text = $"You are about to reserve resource {Resource.ResourceName}<br/>from {rd.BeginDateTime} to {rd.EndDateTime}."

            If _overwriteReservations IsNot Nothing Then
                lblConfirm.Text += "<br/><br/><b>There are other reservations made during this time.<br/>By accepting this confirmation, you will overwrite the other reservations.</b>"
            End If

            If isLabCleanTime Then
                lblConfirm.Text += "<br/><br/><span style=""color:#ff0000; font-size:larger"">**Warning: Your reservation overlaps with lab clean time (8:30am to 9:30am). You can still continue, but you should not be inside the lab during that time**</span>"
            End If

            lblConfirm.Text += String.Format("<br/><br/>The estimated cost of this activity will be {0}.", dblCostStr)

            Dim processInfoEnum As IEnumerable(Of IProcessInfo) = Provider.Scheduler.ProcessInfo.GetProcessInfos(Resource.ResourceID)

            If processInfoEnum.Count > 0 Then
                lblConfirm.Text += "<br/>Additional precious metal charges may apply."
            End If

            If rd.IsAfterHours() Then
                If Kiosks.Create(Provider.Scheduler.Kiosk).IsKiosk(ContextBase.CurrentIP()) Then
                    ' do not show the link on kiosks
                    lblConfirm.Text += "<br/><br/>This reservation occurs during after-hours.<br/>Please add an event to the After-Hours Buddy Calendar for this reservation."
                Else
                    Dim calendarUrl As String = ConfigurationManager.AppSettings("AfterHoursBuddyCalendarUrl")
                    If String.IsNullOrEmpty(calendarUrl) Then Throw New Exception("Missing appSetting: AfterHoursBuddyCalendarUrl")
                    lblConfirm.Text += String.Format("<br/><br/>This reservation occurs during after-hours.<br/>Please add an event to the <a href=""{0}"" target=""_blank"">After-Hours Buddy Calendar</a> for this reservation.", calendarUrl)
                End If
            End If

            lblConfirm.Text += "<br/><br/>Click 'Yes' to accept reservation or 'No' to cancel scheduling."

            Dim inLab As Boolean = Helper.ClientInLab(Resource.LabID)
            Dim isEngineer As Boolean = (authLevel And ClientAuthLevel.ToolEngineer) > 0
            Dim minCancelTime As Integer = Resource.MinCancelTime
            Dim minReservTime As Integer = Resource.MinReservTime

            Dim args As New ReservationStateArgs(ReservationID, inLab, True, False, True, False, False, minCancelTime, minReservTime, rd.BeginDateTime, rd.EndDateTime, Nothing, Nothing, authLevel)
            Dim reservationState As ReservationState = ReservationStateUtility.Create(Date.Now).GetReservationState(args)
            Dim allowedAuths As ClientAuthLevel = ClientAuthLevel.AuthorizedUser Or ClientAuthLevel.SuperUser Or ClientAuthLevel.ToolEngineer Or ClientAuthLevel.Trainer

            Helper.AppendLog($"Reservation.BtnSubmit_Click: reservationState = {reservationState}, authLevel = {authLevel}")

            phConfirmYesAndStart.Visible = False ' reset to false 
            If ReservationState.StartOnly = reservationState OrElse ReservationState.StartOrDelete = reservationState Then
                If (allowedAuths And authLevel) > 0 Then ' isUserAuthorized Then
                    Dim endableRsvQuery As IEnumerable(Of IReservation) = Provider.Scheduler.Reservation.SelectEndableReservations(Resource.ResourceID)
                    If endableRsvQuery.Count() = 0 Then
                        ' If there are no previous un-ended reservations
                        phConfirmYesAndStart.Visible = True
                    End If
                End If
            End If

            phConfirm.Visible = True
            phReservation.Visible = False
        End Sub

        Protected Sub BtnCancel_Click(sender As Object, e As EventArgs)
            Helper.AppendLog($"Reservation.BtnCancel_Click: QueryString = ""{Request.QueryString}"", CurrentView = {GetCurrentView()}")
            ReturnToResourceDayWeek()
        End Sub

        Protected Sub BtnConfirmYesAndStart_Click(sender As Object, e As EventArgs)
            Dim rsv As IReservationItem = Nothing

            Try
                Helper.AppendLog($"Reservation.BtnConfirmYesAndStart_Click: QueryString = ""{Request.QueryString}"", CurrentView = {GetCurrentView()}")

                ShowConfirmError(Nothing)

                Dim model As ReservationModel = CreateReservationModel(Date.Now)
                rsv = model.CreateOrModifyReservation()
                StartReservation(rsv.ReservationID)

                ' Go back to previous page
                ReturnToResourceDayWeek()
            Catch ex As Exception
                Helper.AppendLog($"Reservation.BtnConfirmYesAndStart_Click: Catch = ""{ex.Message}""")

                Dim vars As New Dictionary(Of String, Object)

                Dim text As String
                Dim body As String

                If rsv Is Nothing Then
                    vars.Add("reservationId", 0)
                    text = $"An error occurred while trying to create your reservation.<br><br>{ex.Message}<br><br>Your reservation was not created."
                    body = "An error occurred in BtnConfirmYesAndStart_Click. Reservation created: false"
                Else
                    phConfirm.Visible = False
                    vars.Add("reservationId", rsv.ReservationID)
                    text = $"Your reservation was created, however an error occurred while trying to start.<br><br>{ex.Message}<br><br><a href=""{VirtualPathUtility.ToAbsolute(GetReturnUrl())}"">Click here to return to the reservations view</a>."
                    body = "An error occurred in BtnConfirmYesAndStart_Click. Reservation created: true"
                End If

                vars.Add("inlab", Helper.IsInLab())
                vars.Add("onkiosk", Helper.IsOnKiosk())

                Helper.SendDebugEmail("Reservation.BtnConfirmYesAndStart_Click", "Create and Start Reservation Error", body, vars)

                ShowConfirmError(text)
            End Try
        End Sub

        Public Sub BtnConfirmYes_Click(sender As Object, e As EventArgs)
            Dim rsv As IReservationItem = Nothing

            Try
                Helper.AppendLog($"Reservation.BtnConfirmYes_Click: QueryString = ""{Request.QueryString}"", CurrentView = {GetCurrentView()}")

                ShowConfirmError(Nothing)

                Dim model As ReservationModel = CreateReservationModel(Date.Now)
                rsv = model.CreateOrModifyReservation()

                ' Go back to previous page
                ReturnToResourceDayWeek()
            Catch ex As Exception
                Helper.AppendLog($"Reservation.BtnConfirmYes_Click: Catch = ""{ex.Message}""")

                Dim vars As New Dictionary(Of String, Object)

                Dim text As String
                Dim body As String

                If rsv Is Nothing Then
                    vars.Add("reservationId", 0)
                    text = $"An error occurred while trying to create your reservation.<br><br>{ex.Message}<br><br>Your reservation was not created."
                    body = "An error occurred in BtnConfirmYes_Click. Reservation created: false"
                Else
                    phConfirm.Visible = False
                    vars.Add("reservationId", rsv.ReservationID)
                    text = $"Your reservation was created, however an error occurred.<br><br>{ex.Message}<br><br><a href=""{VirtualPathUtility.ToAbsolute(GetReturnUrl())}"">Click here to return to the reservations view</a>."
                    body = "An error occurred in BtnConfirmYes_Click. Reservation created: true"
                End If

                vars.Add("inlab", Helper.IsInLab())
                vars.Add("onkiosk", Helper.IsOnKiosk())

                Helper.SendDebugEmail("Reservation.BtnConfirmYesAndStart_Click", "Create and Start Reservation Error", body, vars)

                ShowConfirmError(text)
            End Try
        End Sub

        Protected Sub BtnConfirmNo_Click(sender As Object, e As EventArgs)
            Helper.AppendLog($"Reservation.BtnConfirmNo_Click: QueryString = ""{Request.QueryString}"", CurrentView = {GetCurrentView()}")
            phConfirm.Visible = False
            phReservation.Visible = True
        End Sub

        Private Function CreateReservationModel(now As Date, Optional recurrenceId As Integer = -1) As ReservationModel

            Dim selectedActivityId As Integer

            If Not Integer.TryParse(ddlActivity.SelectedValue, selectedActivityId) Then
                Throw New Exception("Cannot determine selected activity.")
            End If

            Dim selectedAccountId As Integer

            If Not Integer.TryParse(ddlAccount.SelectedValue, selectedAccountId) Then
                Throw New Exception("Cannot determine selected account.")
            End If

            Return New ReservationModel(Helper, now) With {
                .ActivityID = selectedActivityId,
                .AccountID = selectedAccountId,
                .RecurrenceID = recurrenceId,
                .AutoEnd = chkAutoEnd.Checked,
                .KeepAlive = chkKeepAlive.Checked,
                .Notes = txtNotes.Text,
                .ReservationProcessInfoJson = hidProcessInfoData.Value,
                .DurationText = txtDuration.Text,
                .DurationSelectedValue = ddlDuration.SelectedValue,
                .StartTimeHourSelectedValue = ddlStartTimeHour.SelectedValue,
                .StartTimeMinuteSelectedValue = ddlStartTimeMin.SelectedValue
            }
        End Function
    End Class
End Namespace