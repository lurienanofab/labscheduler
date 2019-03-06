Imports LNF
Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Scheduler
Imports LNF.Scheduler.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports Data = LNF.Repository.Data
Imports Scheduler = LNF.Repository.Scheduler

Namespace Pages
    Public Enum DurationInputType
        DropDown = 1
        TextBox = 2
    End Enum

    Public Class Reservation
        Inherits SchedulerPage

        Private _resource As ResourceItem
        Private _reservation As Scheduler.Reservation
        Private _selectedDate As DateTime?
        Private _selectedTime As TimeSpan?
        Private _overwriteReservations As IList(Of Scheduler.Reservation)

        Private ReadOnly Property ReservationID As Integer
            Get
                Dim result As Integer = 0
                Integer.TryParse(Request.QueryString("ReservationID"), result)
                Return result
            End Get
        End Property

        Private ReadOnly Property RecurrenceID As Integer
            Get
                Dim result As Integer = 0
                Integer.TryParse(Request.QueryString("RecurrenceID"), result)
                Return result
            End Get
        End Property

        ''' <summary>
        ''' The current resource. Comes from the query string paramter Path.
        ''' </summary>
        Protected ReadOnly Property Resource As ResourceItem
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
        ''' The current selected date. Comes from the query string parameter Date.
        ''' </summary>
        Protected ReadOnly Property SelectedDate As DateTime
            Get
                If Not _selectedDate.HasValue() Then
                    _selectedDate = Request.SelectedDate()
                    If Not _selectedDate.HasValue Then
                        Throw New Exception("Unable to determine the selected date.")
                    End If
                End If
                Return _selectedDate.Value
            End Get
        End Property

        ''' <summary>
        ''' The current selected time. Comes from the query string parameter Time.
        ''' </summary>
        Protected ReadOnly Property SelectedTime As TimeSpan
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
        Protected ReadOnly Property Reservation As Scheduler.Reservation
            Get
                If ReservationID = 0 Then
                    Return Nothing
                Else
                    If _reservation Is Nothing Then
                        _reservation = DA.Current.Single(Of Scheduler.Reservation)(ReservationID)
                    End If

                    Return _reservation
                End If
            End Get
        End Property

        Public Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Try
                If Not Page.IsPostBack Then
                    ShowLoadError(Nothing)
                    LoadReservation()
                End If
            Catch ex As Exception
                ShowLoadError(ex.Message)
            End Try
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

        Private Function GetCurrentUser() As ClientItem
            Dim result As ClientItem

            If ReservationID = 0 Then
                result = CurrentUser
            Else
                result = CacheManager.Current.GetClient(Reservation.Client.ClientID)
            End If

            Return result
        End Function

        Private Function GetCurrentAuthLevel() As ClientAuthLevel
            Dim resourceId As Integer = Resource.ResourceID
            Dim clientId As Integer = GetCurrentUser().ClientID
            Dim result As ClientAuthLevel = CacheManager.Current.GetAuthLevel(resourceId, clientId)
            Return result
        End Function

        Private Function GetSelectedDateTime() As DateTime
            Dim result As DateTime = SelectedDate
            result = result.AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue))
            result = result.AddMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue))
            Return result
        End Function

        Private Function GetCurrentActivity() As ActivityItem
            ' always get from the select - even when modifying
            Dim activityId As Integer = Convert.ToInt32(ddlActivity.SelectedValue)
            Return CacheManager.Current.GetActivity(activityId)
        End Function

        Private Sub LoadReservation()
            Session("ReservationProcessInfos") = Nothing
            Session("ReservationInvitees") = Nothing
            Session("AvailableInvitees") = Nothing

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

        Private Sub SetHeader()
            litHeader.Text = If(ReservationID = 0, "Create", "Modify") + " Reservation for"
            litResourceName.Text = Resource.ToString()
        End Sub

        Private Sub SetStartDate()
            litStartDate.Text = Request.SelectedDate().ToLongDateString()
        End Sub

        Private Sub SetClientName()
            If ReservationID = 0 Then
                litClientName.Text = CurrentUser.DisplayName
            Else
                litClientName.Text = Reservation.Client.DisplayName
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
                chkKeepAlive.Checked = False
            Else
                chkKeepAlive.Checked = Reservation.AutoEnd
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
                TrySelectByValue(ddlActivity, Reservation.Activity.ActivityID)
            End If
        End Sub

        Private Sub LoadAccounts()
            Dim act As ActivityItem
            Dim clientId As Integer
            Dim selectedAccountId As Integer

            If ReservationID = 0 Then
                act = GetCurrentActivity()
                clientId = CurrentUser.ClientID
                selectedAccountId = -1
            Else
                act = CacheManager.Current.GetActivity(Reservation.Activity.ActivityID)
                clientId = Reservation.Client.ClientID
                selectedAccountId = Reservation.Account.AccountID
            End If

            Dim accts As New List(Of ClientAccountItem)

            Dim mustAddInvitee As Boolean = SchedulerUtility.LoadAccounts(accts, act.AccountType, clientId, GetReservationInvitees())

            ddlAccount.Enabled = True

            If mustAddInvitee Then
                phBillingAccount.Visible = False
                phBillingAccountMessage.Visible = True
                litBillingAccountMessage.Text = "You must add an invitee before selecting an account."
            Else
                phBillingAccount.Visible = True
                phBillingAccountMessage.Visible = False
                litBillingAccountMessage.Text = String.Empty

                ddlAccount.DataSource = accts.Select(Function(x) New With {.Name = Data.Account.GetFullAccountName(x.ShortCode, x.AccountName, x.OrgName), .AccountID = x.AccountID.ToString()})
                ddlAccount.DataBind()

                ' check for scheduled maintenance activity
                If act.ActivityID = Properties.Current.Activities.ScheduledMaintenance.ActivityID Then
                    selectedAccountId = Properties.Current.LabAccount.AccountID
                    ddlAccount.Enabled = False
                End If

                TrySelectByValue(ddlAccount, selectedAccountId)
            End If
        End Sub

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

                    txtRecurringStartDate.Text = SelectedDate.ToString("MM/dd/yyyy")

                    Dim dow As DayOfWeek = SelectedDate.DayOfWeek
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

            Try
                If TimeSpan.Zero = Resource.Granularity Then
                    Throw New Exception("Granularity is zero for this resource.")
                End If

                ' Check if selectedDate is in the past
                If SelectedDate < DateTime.Now.Date Then
                    Throw New Exception("The selected date cannot be in the past.")
                End If

                minTime = GetMinimumStartTime()
            Catch ex As Exception
                ShowPastSelectedDateError(ex.Message)
                Return
            End Try

            ' Restrictions: Start Time either = end of previous reservation or
            ' the gap between reservations has to be multiples of Min Reserv Time
            ' This is checked for when user clicks on Submit button

            '2011-12-28 start time must be less than or equal to chargeable end time and greater or equal to current time.

            ' Determine 24-hour granularities
            Dim stepSize As Integer = Convert.ToInt32(Resource.Granularity.TotalHours)
            Dim offsetTotalHours As Integer = Convert.ToInt32(Resource.Offset.TotalHours)

            If stepSize = 0 Then stepSize = 1
            Dim grans As New List(Of Integer)
            For i As Integer = offsetTotalHours To 24 Step stepSize
                grans.Add(i)
            Next

            ' Load Hours
            ddlStartTimeHour.Items.Clear()

            For i As Integer = 0 To grans.Count - 1
                If grans(i) >= minTime.Hours Then
                    Dim hourText As String = String.Empty
                    hourText = If((grans(i) Mod 12) = 0, "12 ", (grans(i) Mod 12).ToString() + " ")
                    hourText += If(grans(i) < 12, "am", "pm")
                    ddlStartTimeHour.Items.Add(New ListItem(hourText, grans(i).ToString()))
                End If
            Next

            ' Load Minutes
            LoadStartTimeMinutes()

            ' Select Preselected Time
            If ReservationID = 0 Then
                ' new reservation
                TrySelectByValue(ddlStartTimeHour, SelectedDate.Add(SelectedTime).Hour)
                TrySelectByValue(ddlStartTimeMin, SelectedDate.Add(SelectedTime).Minute)
            Else
                ' existing reservation
                TrySelectByValue(ddlStartTimeHour, Reservation.BeginDateTime.Hour)
                TrySelectByValue(ddlStartTimeMin, Reservation.BeginDateTime.Minute)
            End If
        End Sub

        Private Sub LoadStartTimeMinutes()
            ddlStartTimeMin.Items.Clear()

            Dim start As DateTime = SelectedDate.AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue))

            For i As Integer = 0 To 59 Step Convert.ToInt32(Resource.Granularity.TotalMinutes)
                If start.AddMinutes(i) >= DateTime.Now Then
                    ddlStartTimeMin.Items.Add(New ListItem(i.ToString("00"), i.ToString()))
                End If
            Next
        End Sub

        Private Sub LoadDuration()
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
            Dim processInfos = ProcessInfoManager.GetProcessInfos(Resource.ResourceID)

            If processInfos.Count = 0 Then
                phProcessInfo.Visible = False
            Else
                phProcessInfo.Visible = True
                rptProcessInfo.DataSource = processInfos.OrderBy(Function(x) x.Order).ThenByDescending(Function(x) x.ProcessInfoID)
                rptProcessInfo.DataBind()
            End If
        End Sub

        Private Sub LoadInvitees()
            dgInvitees.DataSource = GetReservationInvitees().Where(Function(x) Not x.Removed)
            dgInvitees.DataBind()
        End Sub

        Private Sub ShowDurationDropDown()
            Dim showLimitMessage As Boolean = False

            Dim maxDuration As Double = ReservationManager.GetTimeUntilNextReservation(Resource.ResourceID, ReservationID, GetCurrentUser().ClientID, GetSelectedDateTime()).TotalMinutes

            If maxDuration <= 0 Then ' this means that the reservable time is limited by max schedulable
                showLimitMessage = True
                maxDuration = -1 * maxDuration
            End If

            ' Duration ranges from Min Reserv Time to Max Reserv Time
            ' or until the start of the next reservation
            ddlDuration.Items.Clear()
            Dim curValue As Double = Resource.MinReservTime.TotalMinutes
            While curValue <= Resource.MaxReservTime.TotalMinutes AndAlso curValue <= maxDuration
                Dim hour As Integer = Convert.ToInt32(Math.Floor(curValue / 60))
                Dim minute As Integer = Convert.ToInt32(curValue Mod 60)
                Dim text As String = $"{hour} hr {minute} min"

                ddlDuration.Items.Add(New ListItem(text, curValue.ToString()))

                curValue += Resource.Granularity.TotalMinutes
            End While

            'Duration affect user's eligibility to make reservation.
            If ddlDuration.Items.Count = 0 Then
                ServerJScript.JSAlert(Page, "Another reservation already been made for this time. Please select a different start time.")
                btnSubmit.Enabled = False
            Else
                If ReservationID > 0 Then
                    If Not TrySelectByValue(ddlDuration, Reservation.Duration) Then
                        ddlDuration.SelectedIndex = -1
                    End If
                End If

                btnSubmit.Enabled = True
            End If

            lblMaxSchedLimit.Visible = showLimitMessage

            phDurationSelect.Visible = True
            phDurationText.Visible = False
        End Sub

        Private Sub ShowDurationTextBox()
            If ReservationID = 0 Then
                txtDuration.Text = Resource.MinReservTime.TotalMinutes.ToString()
            Else
                txtDuration.Text = Reservation.Duration.ToString()
            End If

            phDurationSelect.Visible = False
            phDurationText.Visible = True
        End Sub

        Private Function GetDurationInputType() As DurationInputType
            Dim act As ActivityItem = GetCurrentActivity()
            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()

            'Right now, to have textbox, the authlevel must be included in NoMaxSchedAuth, and only only sched. maintenance and characterization

            Dim textboxActivities As Integer() = {15, 18, 23}

            If (act.NoMaxSchedAuth And authLevel) > 0 AndAlso textboxActivities.Contains(act.ActivityID) Then
                Return DurationInputType.TextBox
            Else
                Return DurationInputType.DropDown
            End If
        End Function

        ''' <summary>
        ''' Returns the earliest possible reservation time.
        ''' </summary>
        Private Function GetMinimumStartTime() As TimeSpan
            ' First check if the selected date is in the future. If so the result is 00:00:00
            If SelectedDate > DateTime.Now.Date Then
                Return TimeSpan.Zero
            End If

            ' Next check if the selected date is the current day. If so the result will is based on the current hour
            If SelectedDate = DateTime.Now.Date Then
                Dim minTime As TimeSpan = TimeSpan.FromHours(DateTime.Now.Hour) ' the earliest possible hour must be the current hour
                Dim addTime As TimeSpan = TimeSpan.Zero

                If Resource.Granularity.TotalMinutes < 60 Then
                    ' check if remaining time in the current hour is less than or equal to the granularity, if so then add an hour
                    If (60 - DateTime.Now.Minute) <= Resource.Granularity.TotalMinutes Then
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

        Private Sub ValidateInviteeReservations(invitees As IList(Of LNF.Scheduler.ReservationInviteeItem))
            ShowInviteeWarning(String.Empty)

            Dim duration As Integer = GetCurrentDurationMinutes()

            Dim startDateTime As DateTime = GetSelectedDateTime()

            'Dim invitees As IList(Of LNF.Scheduler.ReservationInviteeItem) = GetReservationInvitees()

            If invitees IsNot Nothing Then
                If invitees.Count > 0 Then
                    Dim names As New List(Of String)
                    For Each inv As LNF.Scheduler.ReservationInviteeItem In invitees
                        If GetInviteeReservations(inv.InviteeID, startDateTime, duration).Count() > 0 Then
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

        Private Function GetInviteeReservations(inviteeClientId As Integer, startDateTime As DateTime, duration As Integer) As IList(Of Scheduler.Reservation)
            Dim endDateTime As DateTime = startDateTime.AddMinutes(duration)
            Dim inviteeReservations As IList(Of Scheduler.Reservation) = ReservationManager.SelectByClient(inviteeClientId, Request.SelectedDate().AddHours(0), Request.SelectedDate().AddHours(24), False)
            Dim conflictingReservations As IList(Of Scheduler.Reservation) = ReservationManager.GetConflictingReservations(inviteeReservations, startDateTime, endDateTime)
            Return conflictingReservations
        End Function

        Private Function GetReservationInvitees() As IList(Of LNF.Scheduler.ReservationInviteeItem)
            If Session("ReservationInvitees") Is Nothing Then
                Dim query = DA.Current.Query(Of Scheduler.ReservationInvitee)().Where(Function(x) x.Reservation.ReservationID = ReservationID)
                Session("ReservationInvitees") = LNF.Scheduler.ReservationInviteeItem.Create(query)
            End If

            Dim result As IList(Of LNF.Scheduler.ReservationInviteeItem) = CType(Session("ReservationInvitees"), IList(Of LNF.Scheduler.ReservationInviteeItem))

            Return result
        End Function

        Private Function GetAvailableInvitees() As IList(Of AvailableInviteeItem)
            Dim clientId As Integer
            Dim activityId As Integer

            If ReservationID > 0 Then
                clientId = Reservation.Client.ClientID
                activityId = Reservation.Activity.ActivityID
            Else
                clientId = CurrentUser.ClientID
                activityId = Convert.ToInt32(ddlActivity.SelectedValue)
            End If

            If Session("AvailableInvitees") Is Nothing Then
                Session("AvailableInvitees") = ReservationInviteeUtility.SelectAvailable(ReservationID, Resource.ResourceID, activityId, clientId)
            End If

            Dim result As IList(Of AvailableInviteeItem) = CType(Session("AvailableInvitees"), IList(Of AvailableInviteeItem))

            Return result
        End Function

        Private Function GetReservationProcessInfos() As IList(Of LNF.Models.Scheduler.ReservationProcessInfoItem)
            ' get current reservation items, items will be added/removed from this collection depending on what the user does
            If Session("ReservationProcessInfos") Is Nothing Then
                Session("ReservationProcessInfos") = ProcessInfoManager.GetReservationProcessInfos(ReservationID)
            End If

            Dim result As IList(Of LNF.Models.Scheduler.ReservationProcessInfoItem) = CType(Session("ReservationProcessInfos"), IList(Of LNF.Models.Scheduler.ReservationProcessInfoItem))

            Return result
        End Function

        Private Function GetProcessInfoLines() As IList(Of ProcessInfoLineItem)
            If Items("ProcessInfoLines") Is Nothing Then
                Items("ProcessInfoLines") = ProcessInfoManager.GetProcessInfoLines(Resource.ResourceID)
            End If

            Dim result As IList(Of ProcessInfoLineItem) = CType(Items("ProcessInfoLines"), IList(Of ProcessInfoLineItem))

            Return result
        End Function

        Private Function GetReservationDuration() As ReservationDuration
            Dim beginDateTime As DateTime = SelectedDate.AddHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue)).AddMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue))
            Dim currentDurationMinutes As Integer = GetCurrentDurationMinutes()
            Dim result As ReservationDuration = New ReservationDuration(beginDateTime, TimeSpan.FromMinutes(currentDurationMinutes))
            Return result
        End Function

        Private Function CreateOrModifyReservation(duration As ReservationDuration) As Scheduler.Reservation
            Session("ReservationProcessInfoJsonData") = hidProcessInfoData.Value

            If IsThereAlreadyAnotherReservation(duration) Then
                ServerJScript.JSAlert(Page, "Another reservation has already been made for this time .")
                phConfirm.Visible = False
                phReservation.Visible = True
                Return Reservation 'null when creating a new reservation, current reservation when modifying
            End If

            ' this will be the result reservation - either a true new reservation when creating, a new reservation for modification, or the existing rsv when modifying non-duration data
            Dim result As Scheduler.Reservation = Nothing

            ' Overwrite other reservations
            OverwriteReservations()

            Dim data As SchedulerUtility.ReservationData = GetReservationData(duration)

            If ReservationID = 0 Then
                result = SchedulerUtility.CreateNewReservation(data)
            Else
                result = SchedulerUtility.ModifyExistingReservation(Reservation, data)
            End If

            Return result
        End Function

        Protected Function IsThereAlreadyAnotherReservation(duration As ReservationDuration) As Boolean
            ' This is called twice: once after btnSubmit is clicked (before the confirmation message is shown) and again 
            ' after btnConfirmYes is clicked (immediately before creating the reservation).

            Dim res As ResourceItem = GetCurrentResource()

            ' Check for other reservations made during this time
            ' Select all reservations for this resource during the time of current reservation

            Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()

            Dim activityId As Integer

            If ReservationID = 0 Then
                activityId = Integer.Parse(ddlActivity.SelectedValue)
            Else
                activityId = Reservation.Activity.ActivityID
            End If

            Dim otherReservations As IList(Of Scheduler.Reservation) = ReservationManager.SelectOverwrittable(res.ResourceID, duration.BeginDateTime, duration.EndDateTime)

            If otherReservations.Count > 0 Then
                If Not (otherReservations.Count = 1 AndAlso otherReservations(0).ReservationID = ReservationID) Then
                    If authLevel = ClientAuthLevel.ToolEngineer Then
                        _overwriteReservations = otherReservations
                    Else
                        Return True
                    End If
                End If
            End If

            Return False
        End Function

        ' Overwrite intervening reservations
        Private Sub OverwriteReservations()
            If _overwriteReservations IsNot Nothing Then
                For Each rsv As Scheduler.Reservation In _overwriteReservations
                    ' Delete Reservation
                    ReservationManager.Delete(rsv, CurrentUser.ClientID)

                    ' Send email to reserver informing them that their reservation has been canceled
                    EmailManager.EmailOnToolEngDelete(rsv, CurrentUser.ClientID)
                Next
            End If
        End Sub

        Private Function GetReservationData(duration As ReservationDuration) As SchedulerUtility.ReservationData
            Dim result As New SchedulerUtility.ReservationData(GetReservationInvitees()) With {
                .ClientID = GetCurrentUser().ClientID,
                .ResourceID = Resource.ResourceID,
                .ActivityID = GetCurrentActivity().ActivityID,
                .AccountID = Integer.Parse(ddlAccount.SelectedValue),
                .ReservationDuration = duration,
                .Notes = txtNotes.Text,
                .AutoEnd = chkAutoEnd.Checked,
                .KeepAlive = chkKeepAlive.Checked
            }
            Return result
        End Function

        Private Sub HandleRecurringReservation(duration As ReservationDuration, client As Data.Client)
            If String.IsNullOrEmpty(txtDuration.Text) Then
                LoadDuration()
                ShowReservationAlert("You must specify the duration.")
                Return
            End If

            Dim rrDuration As Double = 0
            Dim rrStartDate As DateTime
            If Double.TryParse(txtDuration.Text, rrDuration) Then
                If ReservationID = 0 Then
                    ' new recurring reservation
                    If DateTime.TryParse(txtRecurringStartDate.Text, rrStartDate) Then
                        Dim rr As New Scheduler.ReservationRecurrence()
                        Dim patId As Integer = If(rdoRecurringPatternWeekly.Checked, 1, 2)
                        rr.Pattern = DA.Current.Single(Of Scheduler.RecurrencePattern)(patId)
                        rr.Resource = DA.Current.Single(Of Scheduler.Resource)(Resource.ResourceID)
                        rr.Client = client
                        rr.Account = DA.Current.Single(Of Data.Account)(Properties.Current.LabAccount.AccountID) 'Currently only supports general lab account
                        rr.BeginTime = duration.BeginDateTime
                        rr.Duration = rrDuration
                        rr.EndTime = duration.EndDateTime
                        rr.BeginDate = rrStartDate
                        rr.CreatedOn = DateTime.Now
                        rr.AutoEnd = chkAutoEnd.Checked
                        rr.IsActive = True
                        rr.Activity = DA.Current.Single(Of Scheduler.Activity)(Properties.Current.Activities.ScheduledMaintenance.ActivityID)
                        rr.AutoEnd = chkAutoEnd.Checked
                        rr.KeepAlive = chkKeepAlive.Checked
                        rr.Notes = txtNotes.Text

                        If rdoRecurringRangeEndBy.Checked Then
                            Dim rrEndDate As DateTime
                            If DateTime.TryParse(txtEndDate.Value, rrEndDate) Then
                                rr.EndDate = rrEndDate
                            Else
                                chkIsRecurring.Checked = True
                                LoadDuration()
                                ShowReservationAlert("Invalid end date.")
                                Return
                            End If
                        End If

                        If rdoRecurringPatternWeekly.Checked Then
                            ' must add 1 here because the key is zero based - so Sunday = 0, but in the database Sunday = 1
                            rr.PatternParam1 = Convert.ToInt32(GetSelectedDayOfWeek()) + 1
                        ElseIf rdoRecurringPatternMonthly.Checked Then
                            rr.PatternParam1 = Convert.ToInt32(ddlMonthly1.SelectedValue)
                            rr.PatternParam2 = Convert.ToInt32(ddlMonthly2.SelectedValue)
                        End If

                        Try
                            DA.Current.Insert(rr)
                            CreateOrModifyReservation(duration)
                            ReturnToResourceDayWeek()
                        Catch ex As Exception
                            Dim errmsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
                            Throw New Exception($"Error in saving Recurring Reservation: {errmsg}")
                        End Try
                    Else
                        ShowReservationAlert("Error in saving Recurring Reservation: Invalid StartDate.")
                    End If
                End If
            Else
                ShowReservationAlert("Error in saving Recurring Reservation: Invalid Duration.")
            End If
        End Sub

        Private Sub StoreReservationProcessInfo()
            For i As Integer = 0 To rptProcessInfo.Items.Count - 1
                ' ProcessInfoLine
                Dim ddlParam As DropDownList = CType(rptProcessInfo.Items(i).FindControl("ddlParam"), DropDownList)
                If String.IsNullOrEmpty(ddlParam.SelectedValue) Then
                    Throw New Exception("You must select a Process Info Line.")
                End If

                ' ProcessInfo
                Dim processInfoId As Integer = Convert.ToInt32(CType(rptProcessInfo.Items(i).FindControl("hidPIID"), HiddenField).Value)
                Dim processInfoLineId As Integer = Convert.ToInt32(ddlParam.SelectedValue)
                Dim valueText As String = "0"
                Dim special As Boolean = False

                If processInfoLineId <> 0 Then
                    ' Validate ProcessInfo Value
                    If CType(rptProcessInfo.Items(i).FindControl("txtValue"), TextBox).Visible Then
                        valueText = CType(rptProcessInfo.Items(i).FindControl("txtValue"), TextBox).Text
                    End If

                    special = CType(rptProcessInfo.Items(i).FindControl("chkSpecial"), CheckBox).Checked

                    GetReservationProcessInfos().Add(New LNF.Models.Scheduler.ReservationProcessInfoItem() With
                                                     {
                                                        .ReservationID = ReservationID,
                                                        .ProcessInfoLineID = processInfoLineId,
                                                        .Value = Convert.ToInt32(valueText),
                                                        .Special = special
                                                     })
                Else
                    Dim rpi As LNF.Models.Scheduler.ReservationProcessInfoItem = GetReservationProcessInfos().FirstOrDefault(Function(x) x.ProcessInfoID = processInfoId)
                    GetReservationProcessInfos().Remove(rpi)
                End If
            Next
        End Sub

        Private Sub InviteeModification(action As String, ddlInvitees As DropDownList, lblInviteeID As Label, lblInviteeName As Label)
            Dim invitees As IList(Of LNF.Scheduler.ReservationInviteeItem) = GetReservationInvitees()
            Dim available As IList(Of AvailableInviteeItem) = GetAvailableInvitees()

            If action = "Insert" Then
                ' Insert invitee into ReservInvitee list
                If ddlInvitees.Items.Count = 0 Then Return

                Dim activityId As Integer = Integer.Parse(ddlActivity.SelectedValue)

                If ReservationID > 0 Then
                    activityId = Reservation.Activity.ActivityID
                End If

                Dim avail = available.FirstOrDefault(Function(x) x.ClientID = Integer.Parse(ddlInvitees.SelectedValue))

                ' make sure the selected invitee is in the list of available invitees
                If avail IsNot Nothing Then
                    Dim item As LNF.Scheduler.ReservationInviteeItem = New LNF.Scheduler.ReservationInviteeItem With {
                        .ReservationID = ReservationID,
                        .InviteeID = avail.ClientID,
                        .DisplayName = avail.DisplayName,
                        .Removed = False
                    }

                    invitees.Add(item)

                    ' avail will be removed from session in DgInvitees_ItemCreated

                    ' Give Warning if this invitee has made another reservation at the same time
                    ' 2007-03-22 a bug exists here. If modifying the reservation, the the code still mistakenly think the current modfiying reservation as another reservation
                    ValidateInviteeReservations(New List(Of LNF.Scheduler.ReservationInviteeItem) From {item})
                End If
            ElseIf action = "Delete" Then
                ' Insert invitee into AvailInvitee list and remove from ReservInvitee list
                Dim ri = invitees.FirstOrDefault(Function(x) x.InviteeID = Integer.Parse(lblInviteeID.Text))

                If ri IsNot Nothing Then
                    ri.Removed = True
                    available.Add(AvailableInviteeItem.Create(ri.InviteeID, ri.DisplayName))
                End If
            End If

            dgInvitees.DataSource = invitees.Where(Function(x) Not x.Removed).OrderBy(Function(x) x.DisplayName)
            dgInvitees.DataBind()
        End Sub

        Private Sub StartReservation(rsv As Scheduler.Reservation, client As Data.Client)
            If rsv IsNot Nothing Then
                ReservationManager.StartReservation(rsv, client, Context.Request.UserHostAddress)
            End If
        End Sub

        Private Sub ReturnToResourceDayWeek()
            Dim redirectUrl As String

            If Session("ReturnTo") IsNot Nothing AndAlso Not String.IsNullOrEmpty(Session("ReturnTo").ToString()) Then
                redirectUrl = Session("ReturnTo").ToString()
            Else
                Dim view As ViewType = GetCurrentView()

                If view = ViewType.UserView Then
                    redirectUrl = $"~/UserReservations.aspx?Date={Request.SelectedDate():yyyy-MM-dd}"
                ElseIf view = ViewType.ProcessTechView Then
                    ' When we come from ProcessTech.aspx the full path is used (to avoid a null object error). When returning we just want the ProcessTech path.
                    Dim pt As ProcessTechItem = Request.SelectedPath().GetProcessTech()
                    Dim path As PathInfo = PathInfo.Create(pt)
                    redirectUrl = $"~/ProcessTech.aspx?Path={path.UrlEncode()}&Date={Request.SelectedDate():yyyy-MM-dd}"
                Else 'ViewType.DayView OrElse Scheduler.ViewType.WeekView
                    redirectUrl = $"~/ResourceDayWeek.aspx?Path={Request.SelectedPath().UrlEncode()}&Date={Request.SelectedDate():yyyy-MM-dd}"
                End If
            End If

            Response.Redirect(redirectUrl, False)
        End Sub

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

        Private Sub ShowInviteeWarning(name As String)
            If String.IsNullOrEmpty(name) Then
                phInviteeWarning.Visible = False
                litInviteeWarning.Text = String.Empty
            Else
                phInviteeWarning.Visible = True
                litInviteeWarning.Text = $"Please be aware that {name} has made another reservation at this time."
            End If
        End Sub

        Private Sub ShowInviteeWarning(names As IEnumerable(Of String))
            If names Is Nothing OrElse names.Count() = 0 Then
                phInviteeWarning.Visible = False
                litInviteeWarning.Text = String.Empty
            Else
                phInviteeWarning.Visible = True
                litInviteeWarning.Text = $"Please be aware that the following invitees have made another reservation at this time: <ul><li>{String.Join("</li><li>", names)}</li></ul>"
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
            ValidateInviteeReservations(GetReservationInvitees())
        End Sub

        Protected Sub DdlStartTimeMin_SelectedIndexChanged(sender As Object, e As EventArgs)
            LoadDuration()
            ValidateInviteeReservations(GetReservationInvitees())
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

                CType(e.Item.FindControl("hidPIID"), HiddenField).Value = di("ProcessInfoID").AsString
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
                Dim pils As IEnumerable(Of ProcessInfoLineItem) = GetProcessInfoLines().Where(Function(x) x.ProcessInfoID = di("ProcessInfoID").AsInt32).OrderBy(Function(x) x.Param).ToList()

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
                Dim rpi As LNF.Models.Scheduler.ReservationProcessInfoItem = GetReservationProcessInfos().FirstOrDefault(Function(x) x.ProcessInfoID = di("ProcessInfoID").AsInt32)

                'CacheManager.Current.ReservationProcessInfos().FirstOrDefault(Function(x) x.ProcessInfoID = di.Value("ProcessInfoID", 0) AndAlso x.ProcessInfoLineID > 0)

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
            ShowInviteeWarning(String.Empty)
            Dim ddlInvitees As DropDownList = CType(e.Item.FindControl("ddlInvitees"), DropDownList)
            Dim lblInviteeID As Label = CType(e.Item.FindControl("lblInviteeID"), Label)
            Dim lblInviteeName As Label = CType(e.Item.FindControl("lblInviteeName"), Label)
            InviteeModification(e.CommandName, ddlInvitees, lblInviteeID, lblInviteeName)
            LoadAccounts()
        End Sub

        Protected Sub DgInvitees_ItemCreated(sender As Object, e As DataGridItemEventArgs)
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

        Protected Sub DgInvitees_ItemDataBound(sender As Object, e As DataGridItemEventArgs)
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim item As LNF.Scheduler.ReservationInviteeItem = CType(e.Item.DataItem, LNF.Scheduler.ReservationInviteeItem)
                CType(e.Item.FindControl("lblInviteeID"), Label).Text = item.InviteeID.ToString()
                CType(e.Item.FindControl("lblInviteeName"), Label).Text = item.DisplayName.ToString()
            End If
        End Sub

        Protected Sub BtnSubmit_Click(sender As Object, e As EventArgs)
            Try
                Dim client As Data.Client
                Dim activity As ActivityItem

                If ReservationID = 0 Then
                    client = DA.Current.Single(Of Data.Client)(Context.CurrentUser().ClientID)
                    activity = CacheManager.Current.GetActivity(Integer.Parse(ddlActivity.SelectedValue))
                Else
                    client = Reservation.Client
                    activity = CacheManager.Current.GetActivity(Reservation.Activity.ActivityID) 'remember, the activity cannot be changed once a reservation is made
                End If

                ShowReservationAlert(Nothing) ' clears it

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
                Dim activeRowCount As Integer = 0
                activeRowCount = GetReservationInvitees().Count

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
                If proxyActivities.Contains(activity.ActivityID) Then
                    For Each ri In GetReservationInvitees()
                        Dim availableMinutes As Integer = ReservationManager.GetAvailableSchedMin(Resource.ResourceID, ri.InviteeID)

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

                ' Check Reservation Fence Constraint for current user's auth level
                ' 2007-03-03 Currently this should be the only place to check the Reserv. Fence Constraint, because
                ' we cannot check it until user specify the Activity Type.
                Dim authLevel As ClientAuthLevel = GetCurrentAuthLevel()
                If (activity.NoReservFenceAuth And Convert.ToInt32(authLevel)) = 0 Then
                    If DateTime.Now.Add(Resource.ReservFence) <= rd.BeginDateTime Then
                        ShowReservationAlert("You are trying to make a reservation that is too far in the future.")
                        Return
                    End If
                End If

                ' ddlDuration prevents exceeding max reservable - the only way to beat this is to be logged onto two system
                '  and to start making two reservations at the same time. To combat this, the final check is made in the SP
                If IsThereAlreadyAnotherReservation(rd) Then
                    ShowReservationAlert("Another reservation has already been made for this time.")
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

                ' Store Reservation Process Info
                StoreReservationProcessInfo()

                '2009-12-07 Check if the reservation lies within lab clean
                Dim currentDate As New DateTime(rd.BeginDateTime.Year, rd.BeginDateTime.Month, rd.BeginDateTime.Day, 0, 0, 0)
                Dim yesterday As DateTime = currentDate.AddDays(-1) 'need this to determine lab clean days that are moved by holidays
                Dim labCleanBegin As DateTime = currentDate.AddMinutes(510) '8:30 am
                Dim labCleanEnd As DateTime = labCleanBegin.AddHours(1).AddMinutes(45) '9:30 am + 45 minutes (Sandrine wants this as of 2018-10-22 - jg)
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
                    dblCost = mCompile.EstimateToolRunCost(Convert.ToInt32(ddlAccount.SelectedValue), Resource.ResourceID, rd.Duration.TotalMinutes)
                    dblCostStr = dblCost.ToString("$#,##0.00")
                Catch ex As Exception
                    dblCostStr = dblCost.ToString("ERR")
                End Try

                ' Display Submit Confirmation
                lblConfirm.Text = $"You are about to reserve resource {Resource.ResourceName}<br/>from {rd.BeginDateTime} to {rd.EndDateTime}."

                If Not _overwriteReservations Is Nothing Then
                    lblConfirm.Text += "<br/><br/><b>There are other reservations made during this time.<br/>By accepting this confirmation, you will overwrite the other reservations.</b>"
                End If

                If isLabCleanTime Then
                    lblConfirm.Text += "<br/><br/><span style=""color:#ff0000; font-size:larger"">**Warning: Your reservation overlaps with lab clean time (8:30am to 9:30am). You can still continue, but you should not be inside the lab during that time**</span>"
                End If

                lblConfirm.Text += String.Format("<br/><br/>The estimated cost of this activity will be {0}.", dblCostStr)

                Dim processInfoEnum As IList(Of Scheduler.ProcessInfo) = DA.Current.Query(Of Scheduler.ProcessInfo)().Where(Function(x) x.Resource.ResourceID = Resource.ResourceID).ToList()

                If processInfoEnum.Count > 0 Then
                    lblConfirm.Text += "<br/>Additional precious metal charges may apply."
                End If

                If rd.IsAfterHours() Then
                    If KioskUtility.IsKiosk(ServiceProvider.Current.Context.UserHostAddress) Then
                        ' do not show the link on kiosks
                        lblConfirm.Text += "<br/><br/>This reservation occurs during after-hours.<br/>Please add an event to the After-Hours Buddy Calendar for this reservation."
                    Else
                        Dim calendarUrl As String = ConfigurationManager.AppSettings("AfterHoursBuddyCalendarUrl")
                        If String.IsNullOrEmpty(calendarUrl) Then Throw New Exception("Missing appSetting: AfterHoursBuddyCalendarUrl")
                        lblConfirm.Text += String.Format("<br/><br/>This reservation occurs during after-hours.<br/>Please add an event to the <a href=""{0}"" target=""_blank"">After-Hours Buddy Calendar</a> for this reservation.", calendarUrl)
                    End If
                End If

                lblConfirm.Text += "<br/><br/>Click 'Yes' to accept reservation or 'No' to cancel scheduling."

                Dim inLab As Boolean = Context.ClientInLab(Resource.LabID)
                Dim isEngineer As Boolean = (authLevel And ClientAuthLevel.ToolEngineer) > 0
                Dim minReservTime As Integer = Convert.ToInt32(Resource.MinReservTime.TotalMinutes)

                Dim args As New ReservationStateArgs(inLab, True, False, True, False, False, True, minReservTime, rd.BeginDateTime, rd.EndDateTime, Nothing, Nothing, authLevel)
                Dim resevationState As ReservationState = ReservationManager.GetReservationState(args)
                Dim allowedAuths As ClientAuthLevel = ClientAuthLevel.AuthorizedUser Or ClientAuthLevel.SuperUser Or ClientAuthLevel.ToolEngineer Or ClientAuthLevel.Trainer

                phConfirmYesAndStart.Visible = False ' reset to false 
                If ReservationState.StartOnly = resevationState OrElse ReservationState.StartOrDelete = resevationState Then
                    If (allowedAuths And authLevel) > 0 Then ' isUserAuthorized Then
                        Dim endableRsvQuery As IList(Of Scheduler.Reservation) = ReservationManager.SelectEndableReservations(Resource.ResourceID)
                        If endableRsvQuery.Count = 0 Then
                            ' If there are no previous un-ended reservations
                            phConfirmYesAndStart.Visible = True
                        End If
                    End If
                End If

                phConfirm.Visible = True
                phReservation.Visible = False
            Catch ex As Exception
                ShowReservationAlert(ex.Message)
            End Try
        End Sub

        Protected Sub BtnCancel_Click(sender As Object, e As EventArgs)
            ReturnToResourceDayWeek()
        End Sub

        Protected Sub BtnConfirmYesAndStart_Click(sender As Object, e As EventArgs)
            Try
                Dim rd As ReservationDuration = GetReservationDuration()
                Dim rsv As Scheduler.Reservation = CreateOrModifyReservation(rd)
                Dim client = DA.Current.Single(Of Data.Client)(CurrentUser.ClientID)
                StartReservation(rsv, client)
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try

            ' Go back to previous page
            ReturnToResourceDayWeek()
        End Sub

        Protected Sub BtnConfirmYes_Click(sender As Object, e As EventArgs)
            Try
                CreateOrModifyReservation(GetReservationDuration())
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try

            ' Go back to previous page
            ReturnToResourceDayWeek()
        End Sub

        Protected Sub BtnConfirmNo_Click(sender As Object, e As EventArgs)
            phConfirm.Visible = False
            phReservation.Visible = True
        End Sub
    End Class
End Namespace