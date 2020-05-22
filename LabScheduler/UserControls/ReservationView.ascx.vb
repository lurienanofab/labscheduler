Imports LNF.Cache
Imports LNF.Data
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Controls
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace UserControls
    Public Class ReservationView
        Inherits SchedulerUserControl

        Private _minGran As Integer

        ' This is loaded in PopulateRecurringReservations(), unless View = ViewType.UserView, then it is loaded in LoadHeaders().
        Private _reservations As ReservationCollection

        ' This is an array of ReservationIDs to which the current user was invited.
        Private _invited As Integer()

        Public Property View As ViewType
        Public Property Resource As IResource
        Public Property LabID As Integer
        Public Property ProcessTechID As Integer
        Public Property LabLocationID As Integer

        ''' <summary>
        ''' Holds all the reservations in the current view.
        ''' </summary>
        Public ReadOnly Property Reservations As ReservationCollection
            Get
                If _reservations Is Nothing Then
                    Throw New Exception("Reservations have not been initialized.")
                End If

                Return _reservations
            End Get
        End Property

        ' needs to be called everytime for event wiring
        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim startTime As Date = Date.Now

            If View = ViewType.UserView Then
                HelpdeskInfo1.MultiTool = True
            Else
                HelpdeskInfo1.MultiTool = False
            End If

            Dim linkText As String
            Dim linkUrl As String
            Dim locationPath As LocationPathInfo

            If ContextBase.GetDisplayDefaultHours() Then
                linkText = "Full<br>Day"
                If View = ViewType.LocationView Then
                    locationPath = LocationPathInfo.Parse(ContextBase.Request.QueryString("LocationPath"))
                    linkUrl = $"~/ReservationController.ashx?Command=ChangeHourRange&Range=FullDay&LocationPath={locationPath.UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                Else
                    linkUrl = $"~/ReservationController.ashx?Command=ChangeHourRange&Range=FullDay&Path={ContextBase.Request.SelectedPath().UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                End If
            Else
                linkText = "Default<br>Hours"
                If View = ViewType.LocationView Then
                    locationPath = LocationPathInfo.Parse(ContextBase.Request.QueryString("LocationPath"))
                    linkUrl = $"~/ReservationController.ashx?Command=ChangeHourRange&Range=DefaultHours&LocationPath={locationPath.UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                Else
                    linkUrl = $"~/ReservationController.ashx?Command=ChangeHourRange&Range=DefaultHours&Path={ContextBase.Request.SelectedPath().UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}"
                End If
            End If

            hypHourRange.Text = linkText
            hypHourRange.NavigateUrl = linkUrl

            phErrorMessage.Visible = False
            litErrorMessage.Text = String.Empty

            LoadScheduleTable()

            HandleError()

            HandleStartConfirmation()

            Session("ReturnFromEmail") = SchedulerUtility.GetReservationViewReturnUrl(View)

            RequestLog.Append("ReservationView.Page_load: {0}", Date.Now - startTime)
        End Sub

        Protected Sub DialogButton_OnCommand(sender As Object, e As CommandEventArgs)
            If e.CommandName = "ok" Then
                Dim reservationId As Integer
                If e.CommandArgument IsNot Nothing AndAlso Integer.TryParse(e.CommandArgument.ToString(), reservationId) Then
                    Dim rsv As IReservationWithInvitees = Provider.Scheduler.Reservation.GetReservationWithInvitees(reservationId)
                    If rsv IsNot Nothing Then
                        StartReservation(rsv)
                    Else
                        Session("ErrorMessage") = $"Cannot find Reservation with ReservationID = {reservationId}"
                    End If
                Else
                    Session("ErrorMessage") = "Missing CommandArgument: ReservationID."
                End If
            End If
            Response.Redirect($"~/ResourceDayWeek.aspx?Path={ContextBase.Request.SelectedPath().UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}", False)
        End Sub

        Private Sub HandleError()
            If Session("ErrorMessage") IsNot Nothing Then
                Dim errmsg As String = Session("ErrorMessage").ToString()
                Session.Remove("ErrorMessage")
                If Not String.IsNullOrEmpty(errmsg) Then
                    DisplayError(errmsg)
                End If
            End If
        End Sub

        Private Sub DisplayError(msg As String)
            phErrorMessage.Visible = True
            litErrorMessage.Text += String.Format("<div>{0}</div>", msg)
        End Sub

        Private Sub HandleStartConfirmation()
            Dim showStartConfirmationDialog As Boolean = False

            Dim reservationId As Integer
            If Integer.TryParse(Request.QueryString("ReservationID"), reservationId) Then
                Dim confirm As Integer
                If Integer.TryParse(Request.QueryString("Confirm"), confirm) Then
                    If confirm = 1 Then
                        showStartConfirmationDialog = True
                    End If
                End If
            End If

            If showStartConfirmationDialog Then
                Dim activeReservationMessage As String = String.Empty

                If Session("ActiveReservationMessage") IsNot Nothing Then
                    activeReservationMessage = Session("ActiveReservationMessage").ToString()
                    Session.Remove("ActiveReservationMessage")
                End If

                litActiveReservationMessage.Text = activeReservationMessage
                divStartConfirmationDialog.Visible = True
                btnConfirmOK.CommandArgument = reservationId.ToString()
            Else
                litActiveReservationMessage.Text = String.Empty
                divStartConfirmationDialog.Visible = False
            End If
        End Sub

        Private Sub InitReservations()
            ' This is first called once in LoadScheduleTable

            _reservations = New ReservationCollection(Provider, CurrentUser.ClientID)

            Dim sd, ed As Date

            Select Case View
                Case ViewType.WeekView
                    sd = ContextBase.Request.SelectedDate()
                    ed = sd.AddDays(7)
                    _reservations.SelectByResource(Resource.ResourceID, sd, ed)
                Case ViewType.DayView
                    sd = ContextBase.Request.SelectedDate()
                    ed = sd.AddDays(1)
                    _reservations.SelectByResource(Resource.ResourceID, sd, ed)
                Case ViewType.ProcessTechView
                    sd = ContextBase.Request.SelectedDate()
                    ed = sd.AddDays(1)
                    _reservations.SelectByProcessTech(ProcessTechID, sd, ed)
                Case ViewType.UserView
                    sd = ContextBase.Request.SelectedDate()
                    ed = sd.AddDays(1)
                    ' We need all reservations, not just for current user, to check for conflicts
                    ' when recurring reservations are created. Also any tool is possible.
                    _reservations.SelectByDateRange(sd, ed)
                Case ViewType.LocationView
                    sd = ContextBase.Request.SelectedDate()
                    ed = sd.AddDays(1)
                    _reservations.SelectByLabLocation(LabLocationID, sd, ed)
            End Select
        End Sub

#Region "Load Table"
        Public Sub LoadScheduleTable()
            InitReservations()
            ClearTable()
            LoadHeaders()
            LoadEmptyCells()
            PopulateRecurringReservations()
            LoadReservationCells()
        End Sub

        Private Sub ClearTable()
            For i As Integer = tblSchedule.Rows.Count - 1 To 1 Step -1
                tblSchedule.Rows.RemoveAt(i)
            Next
            For i As Integer = tblSchedule.Rows(0).Cells.Count - 1 To 1 Step -1
                tblSchedule.Rows(0).Cells.RemoveAt(i)
            Next
        End Sub

        Private Sub LoadHeaders()
            tblSchedule.Visible = True
            ShowNoDataMessage(String.Empty)

            Select Case View
                Case ViewType.DayView, ViewType.WeekView
                    'Determines start date of the week
                    If Resource Is Nothing Then
                        Response.Redirect("~", False)
                        Return
                    End If
                    Dim weekStartDate As Date = ContextBase.Request.SelectedDate()
                    Dim maxDay As Integer = If(View = ViewType.WeekView, 7, 1)
                    For i As Integer = 1 To maxDay
                        AddHeaderCell(Resource.ResourceID, Resource.ResourceName, weekStartDate.AddDays(i - 1))
                    Next
                    _minGran = Resource.Granularity
                Case ViewType.ProcessTechView
                    Dim query As IList(Of IResourceTree) = Helper.GetResourceTreeItemCollection().Resources().Where(Function(x) x.ProcessTechID = ProcessTechID AndAlso x.ResourceIsActive).OrderBy(Function(x) x.ResourceName).ToList()

                    Dim d As Date = ContextBase.Request.SelectedDate()

                    For Each r As IResource In query
                        AddHeaderCell(r.ResourceID, r.ResourceName, d)
                    Next

                    If query.Count > 0 Then
                        _minGran = query.Min(Function(x) x.Granularity)
                    Else
                        _minGran = 0
                    End If

                    _minGran = If(_minGran > 60, 60, _minGran)
                Case ViewType.UserView
                    HelpdeskInfo1.Resources = New List(Of Integer)()

                    ' In this case Reservations returns all reservations for all tools and users in the date range. And Find() returns only the
                    ' reservations for the current user. We need all reservations because 1. We don't know for sure which tools will be displayed yet,
                    ' and 2. we must make sure any newly created recurring reservations do not conflict (the result of GetReservations() will be used
                    ' when it comes time to look for conflicts).
                    Dim activeReservationsForCurrentUser As IEnumerable(Of IReservation) = Reservations.Find(ContextBase.Request.SelectedDate(), False, False).ToList() 'False means do not include cancelled
                    Dim prevResourceId As Integer = -1

                    For Each res As IReservation In activeReservationsForCurrentUser.OrderBy(Function(x) x.ResourceID)
                        If res.ResourceID <> prevResourceId Then
                            prevResourceId = res.ResourceID
                            AddHeaderCell(res.ResourceID, res.ResourceName, ContextBase.Request.SelectedDate())
                            HelpdeskInfo1.Resources.Add(prevResourceId)
                        End If
                    Next

                    Dim result As Integer = 0

                    If activeReservationsForCurrentUser.Count > 0 Then
                        result = activeReservationsForCurrentUser.Min(Function(x) x.Granularity)
                    End If

                    _minGran = If(result = 0, 60, result)
                    _minGran = If(_minGran > 60, 60, _minGran)
                Case ViewType.LocationView
                    Dim query As IList(Of IResource) = Provider.Scheduler.LabLocation.GetResources(LabLocationID).OrderBy(Function(x) x.ResourceName).ToList()

                    Dim d As Date = ContextBase.Request.SelectedDate()

                    For Each r As IResource In query
                        AddHeaderCell(r.ResourceID, r.ResourceName, d)
                    Next

                    If query.Count > 0 Then
                        _minGran = query.Min(Function(x) x.Granularity)
                    Else
                        _minGran = 0
                    End If

                    _minGran = If(_minGran > 60, 60, _minGran)
            End Select
        End Sub

        Private Sub AddHeaderCell(resourceId As Integer, resourceName As String, cellDate As Date)
            Dim headerCell As New CustomTableCell With {
                .ResourceID = resourceId,
                .CellDate = cellDate,
                .CssClass = "ReservTableHeader"
            }

            If View = ViewType.DayView Then
                headerCell.Text = cellDate.ToString("dddd'<br>'MMMM d, yyyy")
            Else
                Dim link As New HyperLink With {
                    .CssClass = "ReservLinkHeader"
                }

                If View = ViewType.WeekView Then
                    link.Text = cellDate.ToString("dddd'<br>'MMMM d, yyyy")
                    link.NavigateUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", ContextBase.Request.SelectedPath().UrlEncode(), cellDate)
                Else
                    Dim res As IResource = Helper.GetResourceTreeItemCollection().GetResource(resourceId)
                    link.Text = Resources.CleanResourceName(res.ResourceName)
                    link.NavigateUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res).UrlEncode(), cellDate)
                End If

                headerCell.Controls.Add(link)
            End If

            tblSchedule.Rows(0).Cells.Add(headerCell)
        End Sub

        Private Sub LoadEmptyCells()
            If tblSchedule.Rows(0).Cells.Count = 1 Then Exit Sub

            ' Determine start and end times
            Dim columnCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(1), CustomTableCell)
            Dim weekStart As Date = columnCell.CellDate
            Dim startTime As Date = weekStart
            Dim endTime As Date = weekStart
            Dim offset As Integer = 0

            If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                offset = Resource.Offset
            End If

            Dim cs As IClientSetting = Helper.GetClientSetting()
            Dim beginHour As Double = cs.GetBeginHour()
            Dim endHour As Double = cs.GetEndHour()

            Resources.GetTimeSlotBoundary(TimeSpan.FromMinutes(_minGran), TimeSpan.FromHours(offset), startTime, endTime, ContextBase.GetDisplayDefaultHours(), beginHour, endHour)
            Dim currentTime As TimeSpan = startTime - weekStart
            Dim currentTimeEnd As TimeSpan = endTime - weekStart

            ' Create Table Cells
            While currentTime < currentTimeEnd
                Dim newRow As New TableRow()

                ' Time Cell
                newRow.Cells.Add(New TableCell With {
                    .CssClass = "TableCell",
                    .Text = weekStart.Add(currentTime).ToShortTimeString()
                })

                Dim authLevel As ClientAuthLevel

                If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                    authLevel = GetAuthorization(Resource.ResourceID)
                End If

                ' Empty Cells
                For i As Integer = 1 To tblSchedule.Rows(0).Cells.Count - 1 'iterate through each column skipping the first (time column)
                    Dim headerCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(i), CustomTableCell)

                    Dim rsvCell As New CustomTableCell With {
                        .ReservationID = 0,
                        .MouseOverText = String.Empty,
                        .Enabled = True,
                        .AutoPostBack = False
                    }

                    ' When SchedulerUtility.GetReservationCell is called it will set this attribute.
                    rsvCell.Attributes("data-caption") = String.Empty

                    If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                        rsvCell.ResourceID = Resource.ResourceID
                        rsvCell.CellDate = weekStart.AddDays(i - 1).Add(currentTime)

                        SetReservationCell(rsvCell, Resource, authLevel)
                    ElseIf View = ViewType.ProcessTechView OrElse View = ViewType.LocationView Then
                        rsvCell.ResourceID = headerCell.ResourceID
                        rsvCell.CellDate = weekStart.Add(currentTime)

                        authLevel = GetAuthorization(rsvCell.ResourceID)

                        'in this case, the uniqueness is defined by resource id + current hour and current minutes
                        rsvCell.ID = $"td_{rsvCell.ResourceID}_{currentTime.Ticks}"

                        Dim res As IResource = Helper.GetResourceTreeItemCollection().GetResource(rsvCell.ResourceID)

                        SetReservationCell(rsvCell, res, authLevel)
                    ElseIf View = ViewType.UserView Then
                        rsvCell.CssClass = "TableCell"
                        rsvCell.CellDate = weekStart.Add(currentTime)
                        rsvCell.ResourceID = headerCell.ResourceID
                        rsvCell.AutoPostBack = False
                    End If

                    newRow.Cells.Add(rsvCell)
                Next

                tblSchedule.Rows.Add(newRow)
                currentTime = currentTime.Add(TimeSpan.FromMinutes(_minGran))
            End While
        End Sub

        Private Sub SetReservationCell(cell As CustomTableCell, res As IResource, authLevel As ClientAuthLevel)
            If res.IsSchedulable = False Then
                ' If resource is not schedulable
                SetNotSchedulableCell(cell)
            ElseIf cell.CellDate < DateTime.Now Then
                ' If the cell date is in the past
                SetInPastCell(cell)
            ElseIf cell.CellDate < DateTime.Now.AddMinutes(res.ReservFence) OrElse authLevel >= ClientAuthLevel.SuperUser Then
                ' If the cell date is not in the past and before the reservation fence
                ' Or if the user is the tool engineer and cell date is not in the past
                SetReservableCell(cell, res)
            ElseIf cell.CellDate > DateTime.Now.AddMinutes(res.ReservFence) AndAlso authLevel < ClientAuthLevel.SuperUser Then
                ' If the cell date is after the reservation fence and user is not a tool engineer
                SetPastFenceCell(cell)
            End If
        End Sub

        Private Sub SetReservableCell(cell As CustomTableCell, res As IResource)
            cell.CssClass = "ReservationCell"
            cell.Attributes("data-tooltip") = $"<b>Click to make reservation for {res.ResourceName}<br />on {cell.CellDate.ToLongDateString()}<br />at {cell.CellDate.ToShortTimeString()}</b>"
            cell.AutoPostBack = False

            SetReservationCellAttributes(cell, ReservationState.Undefined, PathInfo.Create(res))

            SetActionCellAttributes(cell, "NewReservation")
        End Sub

        Private Sub SetReservationCellAttributes(cell As CustomTableCell, state As ReservationState, pathInfo As PathInfo)
            cell.Attributes("data-command") = String.Empty
            cell.Attributes("data-reservation-id") = cell.ReservationID.ToString()
            cell.Attributes("data-date") = cell.CellDate.ToString("yyyy-MM-dd")
            cell.Attributes("data-time") = cell.CellDate.TimeOfDay.TotalMinutes.ToString()
            cell.Attributes("data-state") = state.ToString()
            cell.Attributes("data-path") = pathInfo.ToString()
        End Sub

        Private Sub SetPastFenceCell(cell As CustomTableCell)
            cell.CssClass = "TableCell"
            cell.Attributes("data-tooltip") = "<b>You cannot make reservations past the reservation fence.</b>"
            cell.AutoPostBack = False
        End Sub

        Private Sub SetInPastCell(cell As CustomTableCell)
            cell.CssClass = "TableCell"
            cell.Attributes("data-tooltip") = "<b>You cannot make reservations in the past.</b>"
            cell.AutoPostBack = False
        End Sub

        Private Sub SetNotSchedulableCell(cell As CustomTableCell)
            cell.CssClass = "TableCell"
            cell.Attributes("data-tooltip") = "<b>This resource is not schedulable.</b>"
            cell.AutoPostBack = False
        End Sub

        Private Sub SetActionCellAttributes(cell As CustomTableCell, command As String) ', state As ReservationState, Optional res As IResource = Nothing
            'If res Is Nothing Then
            '    res = CacheManager.Current.ResourceTree().GetResource(cell.ResourceID)
            'End If

            cell.CssClass = (cell.CssClass + " reservation-action").Trim()
            cell.Attributes("data-command") = command

            'cell.Attributes.Add("data-reservation-id", cell.ReservationID.ToString())
            'cell.Attributes.Add("data-date", cell.CellDate.ToString("yyyy-MM-dd"))
            'cell.Attributes.Add("data-time", cell.CellDate.TimeOfDay.TotalMinutes.ToString())
            'cell.Attributes.Add("data-state", state.ToString())
            'cell.Attributes.Add("data-path", PathInfo.Create(res).ToString())
        End Sub

        ''' <summary>
        ''' Whenever this page is loaded, we always check the recurrence table and populate real reservation into reservation table for today/or this week depending on the view
        ''' </summary>
        Private Sub PopulateRecurringReservations()
            'Get all reservations under current time context

            'iter is created because of the complexities caused by UserView.
            'If there is a recurring reservation today and there is not yet a record in reservation table, the 
            'html Schedule table would have only one column at this point of code execution.
            'but we must have at least 2 columns (1 header and 1 regular data column) in order to process propery and display properly
            Dim iter As Integer = tblSchedule.Rows(0).Cells.Count

            'Read from ReservationRecurrence table 
            Dim recurringRes As IList(Of IReservationRecurrence) = Nothing

            Select Case View
                Case ViewType.DayView, ViewType.WeekView
                    recurringRes = Provider.Scheduler.Reservation.GetReservationRecurrencesByResource(Resource.ResourceID).Where(Function(x) x.IsActive).OrderBy(Function(x) x.ResourceID).ToList()
                Case ViewType.ProcessTechView
                    recurringRes = Provider.Scheduler.Reservation.GetReservationRecurrencesByProcessTech(ProcessTechID).Where(Function(x) x.IsActive).OrderBy(Function(x) x.ResourceID).ToList()
                Case ViewType.UserView
                    recurringRes = Provider.Scheduler.Reservation.GetReservationRecurrencesByClient(CurrentUser.ClientID).Where(Function(x) x.IsActive).OrderBy(Function(x) x.ResourceID).ToList()
                    ' iter = 1 means today there is no reservation on any this tool at this time
                    ' iter is the number of columns, so 1 means there is only one column (index 0) which is the time.
                    ' Therefore when the table was built there were no columns to add for each tool in the My Reservations view. 
                    If iter = 1 Then iter = 2
                Case ViewType.LocationView
                    recurringRes = Provider.Scheduler.Reservation.GetReservationRecurrencesByLabLocation(LabLocationID).Where(Function(x) x.IsActive).OrderBy(Function(x) x.ResourceID).ToList()
            End Select

            'Since we want to mimize this code to execute, we first make sure if there is no any recurring for this tool, we quit immediately
            Dim hasData As Boolean = recurringRes.Count() > 0

            If Not hasData Then Exit Sub

            'The first for loop is for process tech, week or day view.  If it's day view, it will just loop once.
            'If it's user view, then it won't come here at all
            Dim hasNewData As Boolean = False

            'the number of loops here depends number of colums of the html Scheduler table
            For i As Integer = 1 To iter - 1
                Dim startTime As Date

                If tblSchedule.Rows(0).Cells.Count > i Then
                    Dim columnCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(i), CustomTableCell)
                    startTime = columnCell.CellDate
                Else
                    ' this happens when View = ViewType.UserView and there are no columns besides the time column
                    startTime = ContextBase.Request.SelectedDate()
                End If

                'Populate the temporary regular reservation table from recurring reservation for this period of time
                'Check if the recurrence res is already existing in reservation table
                For Each rr As IReservationRecurrence In recurringRes
                    hasNewData = RecurringReservationTransform.AddRegularFromRecurring(Reservations, rr, startTime) OrElse hasNewData
                Next

                If View = ViewType.ProcessTechView OrElse View = ViewType.LocationView Then
                    Exit Sub
                ElseIf View = ViewType.UserView Then
                    Exit For
                End If
            Next

            'Re-draw the whole table if we have to
            If hasNewData AndAlso View = ViewType.UserView Then
                ClearTable()
                LoadHeaders()
                LoadEmptyCells()
            End If
        End Sub

        Private Sub LoadReservationCells()
            Dim totalReservationCount As Integer = 0

            Dim clientAccounts As IEnumerable(Of IClientAccount) = Provider.Data.Client.GetActiveClientAccounts()

            Dim displayDefaultHours As Boolean = ContextBase.GetDisplayDefaultHours()
            Dim beginHour As Integer = Helper.GetClientSetting().GetBeginHour()
            Dim endHour As Integer = Helper.GetClientSetting().GetEndHour()

            For i As Integer = 1 To tblSchedule.Rows(0).Cells.Count - 1
                Dim columnCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(i), CustomTableCell)
                Dim currentStartTime As Date = columnCell.CellDate
                Dim currentEndTime As Date = columnCell.CellDate 'this doesn't get used
                Dim offset As Integer = 0

                If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                    offset = Resource.Offset
                End If

                Resources.GetTimeSlotBoundary(TimeSpan.FromMinutes(_minGran), TimeSpan.FromHours(offset), currentStartTime, currentEndTime, displayDefaultHours, beginHour, endHour)

                Dim mergeStartCell As Integer = 0
                Dim lastReservationCount As Integer = 0
                Dim resourceIds As String = String.Empty
                Dim lastResourceIds As String = String.Empty

                ' For each cell in column, create Reservation Cells
                For j As Integer = 1 To tblSchedule.Rows.Count - 1
                    Dim rsvCell As CustomTableCell = CType(tblSchedule.Rows(j).Cells(i), CustomTableCell)
                    rsvCell.RowSpan = 1

                    ' Get number of reservation cells that lie in that cell
                    Dim beginTime As Date = rsvCell.CellDate
                    Dim endTime As Date = beginTime.AddMinutes(_minGran)
                    Dim filteredRsv As IEnumerable(Of IReservation) = Nothing

                    Select Case View
                        Case ViewType.DayView, ViewType.WeekView
                            filteredRsv = FilterReservations(beginTime, endTime, True, False)
                        Case ViewType.ProcessTechView, ViewType.LocationView
                            filteredRsv = FilterReservations(rsvCell.ResourceID, beginTime, endTime, True, False)
                        Case ViewType.UserView
                            filteredRsv = FilterReservations(rsvCell.ResourceID, beginTime, endTime, False, False)
                    End Select

                    Dim reservationCount As Integer = filteredRsv.Count
                    totalReservationCount += reservationCount

                    If reservationCount = 0 Then
                        lastResourceIds = String.Empty
                        lastReservationCount = 0
                    Else
                        ' the space is used to disambiguate the string (which should never be a problem, but
                        ' for example, without the space 123 + 456 would be the same as 12 + 3456
                        resourceIds = String.Empty 'shouldn't this be called reservationIds?
                        For k As Integer = 0 To reservationCount - 1
                            resourceIds += filteredRsv(k).ReservationID.ToString() + " "
                        Next

                        ' if the number of res in cell changes, or the id's change, need to get cell info
                        If lastReservationCount <> reservationCount OrElse lastResourceIds <> resourceIds Then
                            lastResourceIds = resourceIds
                            lastReservationCount = reservationCount
                            mergeStartCell = j

                            Dim singleReservationCell As Boolean = reservationCount = 1 OrElse (filteredRsv(reservationCount - 1).ActualEndDateTime.HasValue = False AndAlso filteredRsv(reservationCount - 1).IsRepair = False)

                            If reservationCount = 1 OrElse IsUnendedReservation(filteredRsv(reservationCount - 1)) Then
                                ' Display reservation
                                Dim rsv As IReservation

                                If reservationCount = 1 Then
                                    rsv = filteredRsv.First()
                                Else
                                    'lastReservationCount = 1 ' so this can merge with next cell
                                    'lastResourceIds = filteredRsv(reservationCount - 1).ReservationID.ToString() + " "
                                    mergeStartCell = j
                                    rsv = filteredRsv(reservationCount - 1)
                                End If

                                rsvCell.ReservationID = rsv.ReservationID

                                ' Reservation Cell Events

                                ' Delete/modify buttons are added here if needed.
                                Dim rci As ReservationClient = Helper.GetReservationClientItem(rsv)
                                Dim state As ReservationState = SchedulerUtility.GetReservationCell(rsvCell, rsv, rci, Date.Now)

                                SetReservationCellAttributes(rsvCell, state, PathInfo.Create(rsv))

                                If IsReservationActionState(state) Then
                                    SetActionCellAttributes(rsvCell, "ReservationAction")
                                End If
                            Else
                                rsvCell.ReservationID = -1
                                SchedulerUtility.GetMultipleReservationCell(rsvCell, filteredRsv)
                            End If
                        Else ' cells are the same, so merge them
                            If j > 1 Then
                                Dim prevCell As CustomTableCell = CType(tblSchedule.Rows(mergeStartCell).Cells(i), CustomTableCell)
                                prevCell.RowSpan += 1
                                rsvCell.Visible = False
                            End If
                        End If
                    End If
                Next
            Next

            If View = ViewType.ProcessTechView Then
                If tblSchedule.Rows(0).Cells.Count = 1 Then
                    ShowNoDataMessage("There are no resources in this Process Technology.")
                End If
            ElseIf View = ViewType.LocationView Then
                If tblSchedule.Rows(0).Cells.Count = 1 Then
                    ShowNoDataMessage("There are no resources in this Location.")
                End If
            ElseIf View = ViewType.UserView Then
                If totalReservationCount = 0 Then
                    ShowNoDataMessage("You do not have any reservations made for this date.")
                End If
            End If
        End Sub

        Private Function IsUnendedReservation(rsv As IReservation) As Boolean
            Return rsv.IsRepair = False AndAlso rsv.ActualEndDateTime.HasValue = False
        End Function

        Private Sub ShowNoDataMessage(text As String)
            Dim showNoData = Not String.IsNullOrEmpty(text)
            phNoData.Visible = showNoData
            litNoData.Text = text
            tblSchedule.Visible = Not showNoData
        End Sub
#End Region

#Region "Utility"
        Private Function FilterReservations(sd As Date, ed As Date, includeAllClients As Boolean, includeCancelled As Boolean) As IList(Of IReservation)
            Return Reservations.Find(sd, ed, includeAllClients, includeCancelled).Where(Function(x) ReservationFilter(x, sd, ed)).OrderBy(Function(x) x.BeginDateTime).ToList()
        End Function

        Private Function FilterReservations(resourceId As Integer, sd As Date, ed As Date, includeAllClients As Boolean, includeCancelled As Boolean) As IList(Of IReservation)
            Return Reservations.Find(sd, ed, includeAllClients, includeCancelled).Where(Function(x) x.ResourceID = resourceId AndAlso ReservationFilter(x, sd, ed)).OrderBy(Function(x) x.BeginDateTime).ToList()
        End Function

        Private Function ReservationFilter(rsv As IReservation, beginTime As Date, endTime As Date) As Boolean
            If Not rsv.ActualEndDateTime.HasValue Then
                Return rsv.BeginDateTime < endTime AndAlso rsv.EndDateTime > beginTime
            Else
                Return rsv.ActualBeginDateTime.Value < endTime AndAlso rsv.ActualEndDateTime.Value > beginTime
            End If
        End Function

        Private Function IsReservationActionState(state As ReservationState) As Boolean
            Select Case state
                Case ReservationState.StartOnly, ReservationState.StartOrDelete, ReservationState.Endable, ReservationState.PastSelf, ReservationState.Other, ReservationState.Invited, ReservationState.PastOther
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Private Function GetAuthorization(resourceId As Integer) As ClientAuthLevel
            Return CacheManager.Current.GetAuthLevel(resourceId, CurrentUser.ClientID)
        End Function

        Public Sub StartReservation(rsv As IReservationWithInvitees)
            Try
                Dim rci As ReservationClient = Helper.GetReservationClientItem(rsv)
                LNF.Scheduler.Reservations.Create(Provider, Date.Now).Start(rsv, rci, CurrentUser.ClientID)
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try
        End Sub
#End Region
    End Class
End Namespace