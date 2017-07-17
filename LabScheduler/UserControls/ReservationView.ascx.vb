Imports System.Threading.Tasks
Imports LNF.Cache
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Controls
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports repo = LNF.Repository.Scheduler

Namespace UserControls
    Public Class ReservationView
        Inherits SchedulerUserControl

        Public Property View As ViewType
        Public Property Resource As ResourceModel
        Public Property LabID As Integer
        Public Property ProcessTechID As Integer

        Private _minGran As Integer

        ' This is loaded in PopulateRecurringReservations(), unless View = ViewType.UserView, then it is loaded in LoadHeaders()
        Private _reservations As ReservationCollection

        ' needs to be called everytime for event wiring
        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim startTime As Date = Date.Now

            If View = ViewType.UserView Then
                HelpdeskInfo1.MultiTool = True
            Else
                HelpdeskInfo1.MultiTool = False
            End If

            If CacheManager.Current.DisplayDefaultHours() Then
                hypHourRange.Text = "Full<br>Day"
                hypHourRange.NavigateUrl = String.Format("~/ReservationController.ashx?Command=ChangeHourRange&Range=FullDay&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate())
            Else
                hypHourRange.Text = "Default<br>Hours"
                hypHourRange.NavigateUrl = String.Format("~/ReservationController.ashx?Command=ChangeHourRange&Range=DefaultHours&Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate())
            End If

            phErrorMessage.Visible = False
            litErrorMessage.Text = String.Empty

            Try
                LoadScheduleTable()
            Catch ex As Exception
                DisplayError(ex.Message)
            End Try

            HandleError()

            HandleStartConfirmation()

            Session("ReturnFromEmail") = SchedulerUtility.GetReservationViewReturnUrl(View)

            RequestLog.Append("ReservationView.Page_load: {0}", Date.Now - startTime)
        End Sub

        Protected Sub DialogButton_OnCommand(sender As Object, e As CommandEventArgs)
            If e.CommandName = "ok" Then
                Dim reservationId As Integer
                If e.CommandArgument IsNot Nothing AndAlso Integer.TryParse(e.CommandArgument.ToString(), reservationId) Then
                    Dim rsv As repo.Reservation = DA.Current.Single(Of repo.Reservation)(reservationId)
                    If rsv IsNot Nothing Then
                        Page.RegisterAsyncTask(New PageAsyncTask(Function() StartReservationAsync(rsv, CurrentUser.ClientID)))
                    Else
                        Session("ErrorMessage") = String.Format("Cannot find Reservation with ReservationID = {0}", reservationId)
                    End If
                Else
                    Session("ErrorMessage") = "Missing CommandArgument: ReservationID."
                End If
            End If
            Response.Redirect(String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), Request.SelectedDate()), False)
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

        ''' <summary>
        ''' Holds all the reservations in the current view.
        ''' </summary>
        Public Function GetReservations() As ReservationCollection
            If _reservations Is Nothing Then
                _reservations = New ReservationCollection()

                Dim userState As UserState = CacheManager.Current.CurrentUserState()

                Dim sd, ed As Date

                Select Case View
                    Case ViewType.WeekView
                        Dim cols As Integer = tblSchedule.Rows(0).Cells.Count

                        ' The start date is based on the header cell in the 2nd column (the first column is for times).
                        Dim cell As CustomTableCell

                        cell = CType(tblSchedule.Rows(0).Cells(1), CustomTableCell)
                        sd = cell.CellDate

                        cell = CType(tblSchedule.Rows(0).Cells(cols - 1), CustomTableCell)
                        ed = cell.CellDate.AddDays(1)

                        _reservations.SelectByResource(Resource.ResourceID, sd, ed)
                    Case ViewType.DayView
                        sd = Request.SelectedDate()
                        ed = sd.AddDays(1)
                        _reservations.SelectByResource(Resource.ResourceID, sd, ed)
                    Case ViewType.ProcessTechView
                        sd = Request.SelectedDate()
                        ed = sd.AddDays(1)
                        _reservations.SelectByProcessTech(ProcessTechID, sd, ed)
                    Case ViewType.UserView
                        sd = Request.SelectedDate()
                        ed = sd.AddDays(1)
                        _reservations.SelectByClient(CacheManager.Current.CurrentUser.ClientID, sd, ed)
                End Select
            End If

            Return _reservations
        End Function

#Region "Load Table"
        Public Sub LoadScheduleTable()
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
            lblNoData.Visible = False

            Dim userState As UserState = CacheManager.Current.CurrentUserState()

            Select Case View
                Case ViewType.DayView, ViewType.WeekView
                    'Determines start date of the week
                    If Resource Is Nothing Then
                        Response.Redirect("~", False)
                        Return
                    End If
                    Dim weekStartDate As Date = Request.SelectedDate()
                    Dim maxDay As Integer = If(View = ViewType.WeekView, 7, 1)
                    For i As Integer = 1 To maxDay
                        AddHeaderCell(Resource.ResourceID, Resource.ResourceName, weekStartDate.AddDays(i - 1))
                    Next
                    _minGran = Convert.ToInt32(Resource.Granularity.TotalMinutes)
                Case ViewType.ProcessTechView
                    Dim query As IList(Of ResourceModel) = CacheManager.Current.Resources().Where(Function(x) x.ProcessTechID = ProcessTechID AndAlso x.ResourceIsActive).OrderBy(Function(x) x.ResourceName).ToList()

                    Dim d As Date = Request.SelectedDate()

                    For Each r As ResourceModel In query
                        AddHeaderCell(r.ResourceID, r.ResourceName, d)
                    Next

                    _minGran = Convert.ToInt32(query.Min(Function(x) x.Granularity).TotalMinutes)
                    _minGran = If(_minGran > 60, 60, _minGran)
                Case ViewType.UserView
                    HelpdeskInfo1.Resources = New List(Of Integer)()

                    Dim query As IList(Of repo.Reservation) = GetReservations().Find(Request.SelectedDate(), False)
                    Dim prevResourceId As Integer = -1

                    For Each res As repo.Reservation In query.OrderBy(Function(x) x.Resource.ResourceID)
                        If res.Resource.ResourceID <> prevResourceId Then
                            prevResourceId = res.Resource.ResourceID
                            AddHeaderCell(res.Resource.ResourceID, res.Resource.ResourceName, Request.SelectedDate())
                            HelpdeskInfo1.Resources.Add(prevResourceId)
                        End If
                    Next

                    Dim result As Integer = 0

                    If query.Count > 0 Then
                        result = query.Min(Function(x) x.Resource.Granularity)
                    End If

                    _minGran = If(result = 0, 60, result)
                    _minGran = If(_minGran > 60, 60, _minGran)
            End Select
        End Sub

        Private Sub AddHeaderCell(resourceId As Integer, resourceName As String, cellDate As Date)
            Dim headerCell As New CustomTableCell()

            headerCell.ResourceID = resourceId
            headerCell.CellDate = cellDate
            headerCell.CssClass = "ReservTableHeader"

            If View = ViewType.DayView Then
                headerCell.Text = cellDate.ToString("dddd'<br>'MMMM d, yyyy")
            Else
                Dim link As New HyperLink()
                link.CssClass = "ReservLinkHeader"

                If View = ViewType.WeekView Then
                    link.Text = cellDate.ToString("dddd'<br>'MMMM d, yyyy")
                    link.NavigateUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", Request.SelectedPath().UrlEncode(), cellDate)
                Else
                    Dim res As ResourceModel = CacheManager.Current.GetResource(resourceId)
                    link.Text = res.ResourceName
                    link.NavigateUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res).UrlEncode(), cellDate)
                End If

                headerCell.Controls.Add(link)
            End If

            tblSchedule.Rows(0).Cells.Add(headerCell)
        End Sub

        Private Sub LoadEmptyCells()
            If tblSchedule.Rows(0).Cells.Count = 1 Then Exit Sub

            Dim authLevel As ClientAuthLevel

            If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                authLevel = GetAuthorization(Resource.ResourceID)
            End If

            ' Determine start and end times
            Dim columnCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(1), CustomTableCell)
            Dim weekStart As Date = columnCell.CellDate
            Dim startTime As Date = weekStart
            Dim endTime As Date = weekStart
            Dim offset As Integer = 0

            If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                offset = Convert.ToInt32(Resource.Offset.TotalHours)
            End If

            ResourceUtility.GetTimeSlotBoundary(TimeSpan.FromMinutes(_minGran), TimeSpan.FromHours(offset), startTime, endTime)
            Dim currentTime As TimeSpan = startTime - weekStart
            Dim currentTimeEnd As TimeSpan = endTime - weekStart

            ' Create Table Cells
            While currentTime < currentTimeEnd
                ' Time Cell
                Dim timeCell As New TableCell()
                timeCell.CssClass = "TableCell"
                timeCell.Wrap = False
                timeCell.Text = weekStart.Add(currentTime).ToShortTimeString()
                Dim newRow As New TableRow()
                newRow.Cells.Add(timeCell)

                ' Empty Cells
                For i As Integer = 1 To tblSchedule.Rows(0).Cells.Count - 1 'iterate through each column skipping the first (time column)
                    Dim headerCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(i), CustomTableCell)
                    Dim rsvCell As New CustomTableCell()
                    rsvCell.ReservationID = 0
                    rsvCell.MouseOverText = String.Empty
                    rsvCell.Enabled = True
                    rsvCell.AutoPostBack = False

                    If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                        rsvCell.ResourceID = Resource.ResourceID
                        rsvCell.CellDate = weekStart.AddDays(i - 1).Add(currentTime)

                        ' Reservable Cell ToolTip
                        Dim toolTip As String = String.Empty

                        If Resource.IsSchedulable = False Then
                            ' If resource is not schedulable
                            toolTip = "<b>This resource is not schedulable.</b>"
                            rsvCell.CssClass = "TableCell"
                        ElseIf (rsvCell.CellDate > Date.Now AndAlso rsvCell.CellDate < Date.Now.Add(Resource.ReservFence)) OrElse (rsvCell.CellDate > Date.Now AndAlso authLevel >= ClientAuthLevel.SuperUser) Then
                            ' If the cell date is not in the past and before the reservation fence
                            ' Or if the user is the tool engineer and cell date is not in the past
                            toolTip = String.Format("<b>Click to make reservation for {0}<br />on {1}<br />at {2}</b>", Resource.ResourceName, rsvCell.CellDate.ToLongDateString(), rsvCell.CellDate.ToShortTimeString())
                            rsvCell.CssClass = "ReservationCell"
                            SetReservationActionCellAttributes(rsvCell, "NewReservation", ReservationState.Undefined)
                        ElseIf rsvCell.CellDate > Date.Now.Add(Resource.ReservFence) AndAlso authLevel < ClientAuthLevel.SuperUser Then
                            ' If the cell date is after the reservation fence and user is not a tool engineer
                            toolTip = "<b>You cannot make reservations past the reservation fence.</b>"
                            rsvCell.CssClass = "TableCell"
                        ElseIf rsvCell.CellDate < Date.Now Then
                            ' If the cell date is in the past
                            toolTip = "<b>You cannot make reservations in the past.</b>"
                            rsvCell.CssClass = "TableCell"
                        End If

                        rsvCell.Attributes("data-tooltip") = toolTip
                        rsvCell.Attributes("data-caption") = String.Empty
                    ElseIf View = ViewType.ProcessTechView Then
                        Dim resourceId As Integer = headerCell.ResourceID
                        Dim r As repo.Resource = DA.Current.Single(Of repo.Resource)(resourceId)

                        authLevel = GetAuthorization(resourceId)
                        rsvCell.CellDate = weekStart.Add(currentTime)
                        rsvCell.ResourceID = resourceId

                        'in this case, the uniqueness is defined by resource id + current hour and current minutes
                        rsvCell.ID = "id" + resourceId.ToString() + currentTime.Ticks.ToString()

                        ' Reservable Cell ToolTip
                        Dim toolTip As String = String.Empty
                        If r.IsSchedulable = False Then
                            toolTip = "<b>This resource is not schedulable.</b>"
                            rsvCell.CssClass = "TableCell"
                            rsvCell.AutoPostBack = False
                        ElseIf rsvCell.CellDate < Date.Now Then
                            ' If the cell date is in the past
                            toolTip = "<b>You cannot make reservations in the past.</b>"
                            rsvCell.CssClass = "TableCell"
                            rsvCell.AutoPostBack = False
                        ElseIf rsvCell.CellDate < Date.Now.AddHours(r.ReservFence) OrElse authLevel >= ClientAuthLevel.SuperUser Then
                            ' If the cell date is not in the past and before the reservation fence
                            ' Or if the user is the tool engineer and cell date is not in the past
                            toolTip = String.Format("<b>Click to make reservation for {0}<br />on {1}<br />at {2}</b>", r.ResourceName, rsvCell.CellDate.ToLongDateString(), rsvCell.CellDate.ToShortTimeString())
                            SetReservationActionCellAttributes(rsvCell, "NewReservation", ReservationState.Undefined)
                        ElseIf rsvCell.CellDate > Date.Now.AddHours(r.ReservFence) AndAlso authLevel < ClientAuthLevel.SuperUser Then
                            ' If the cell date is after the reservation fence and user is not a tool engineer
                            toolTip = "<b>You cannot make reservations past the reservation fence.</b>"
                            rsvCell.CssClass = "TableCell"
                            rsvCell.AutoPostBack = False
                        End If

                        rsvCell.Attributes("data-tooltip") = toolTip
                        rsvCell.Attributes("data-caption") = String.Empty
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
            Dim recurringRes As IList(Of repo.ReservationRecurrence) = Nothing

            Select Case View
                Case ViewType.DayView, ViewType.WeekView
                    recurringRes = ReservationRecurrenceUtility.SelectByResource(Resource.ResourceID).OrderBy(Function(x) x.Resource.ResourceID).ToList()
                Case ViewType.ProcessTechView
                    Dim lab As repo.Lab = DA.Current.Single(Of repo.Lab)(LabID)
                    recurringRes = ReservationRecurrenceUtility.SelectByProcessTech(ProcessTechID).OrderBy(Function(x) x.Resource.ResourceID).ToList()
                Case ViewType.UserView
                    recurringRes = ReservationRecurrenceUtility.SelectByClient(CacheManager.Current.CurrentUser.ClientID).OrderBy(Function(x) x.Resource.ResourceID).ToList()
                    ' iter = 1 means today there is no reservation on any this tool at this time
                    ' iter is the number of columns, so 1 means there is only one column (index 0) which is the time.
                    ' Therefore when the table was built there were no columns to add for each tool in the My Reservations view. 
                    If iter = 1 Then iter = 2
            End Select

            'Since we want to mimize this code to execute, we first make sure if there is no any recurring for this tool, we quit immediately
            Dim hasData As Boolean = recurringRes.Count > 0

            If Not hasData Then Exit Sub

            Dim dtRegRsvToday As DataTable = Nothing
            Dim listRegRsvToday As IList(Of repo.Reservation) = Nothing

            'The first for loop is for process tech, week or day view.  If it's day view, it will just loop once.
            'If it's user view, then it won't come here at all
            Dim hasNewData As Boolean = False

            'the number of loops here depends number of colums of the html Scheduler table
            For i As Integer = 1 To iter - 1
                Dim curStartTime As Date

                If tblSchedule.Rows(0).Cells.Count > i Then
                    Dim columnCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(i), CustomTableCell)
                    curStartTime = columnCell.CellDate
                Else
                    ' this happens when View = ViewType.UserView and there are no columns besides the time column
                    curStartTime = Request.SelectedDate()
                End If

                ' Select Reservations for this day
                listRegRsvToday = GetReservations().Find(curStartTime, True)

                dtRegRsvToday = DataUtility.ConvertToReservationTable(listRegRsvToday)

                'dtRecurRsv contains recurring reservation, and it's the place holder for the newly generated regular reservation
                'from recurring reservations
                Dim dtRecurRsv As DataTable = dtRegRsvToday.Clone()

                'Populate the temporary regular reservation table from recurring reservation for this period of time
                For Each rr As repo.ReservationRecurrence In recurringRes
                    RecurringReservationTransform.GetRegularFromRecurring(rr, curStartTime, dtRecurRsv)
                Next

                'Check if the recurrence res is already existing in reservation table
                For Each row As DataRow In dtRecurRsv.Rows
                    Dim recurrenceId As Integer = row.Field(Of Integer)("RecurrenceID")
                    Dim rows() As DataRow = dtRegRsvToday.Select(String.Format("RecurrenceID = {0}", recurrenceId))
                    If rows.Length = 0 Then
                        Dim resourceId As Integer = Convert.ToInt32(row("ResourceID"))
                        Dim clientId As Integer = Convert.ToInt32(row("ClientID"))
                        Dim createdOn As Date = Convert.ToDateTime(row("CreatedOn"))
                        Dim beginDateTime As Date = Convert.ToDateTime(row("BeginDateTime"))
                        Dim endDateTime As Date = Convert.ToDateTime(row("EndDateTime"))

                        Dim reservations As ReservationCollection = GetReservations()

                        '[2013-05-16 jg] We need find any existing, unended and uncancelled reservations in the
                        'same time slot. This can happen if there are overlapping recurring reservation patterns.
                        'To determine if two reservations overlap I'm using this logic: (StartA < EndB) and (EndA > StartB)
                        Dim overlappingRsv As IList(Of repo.Reservation) = reservations.Where(Function(x) _
                            x.IsActive AndAlso x.Resource.ResourceID = resourceId _
                            AndAlso (x.BeginDateTime < endDateTime AndAlso x.EndDateTime > beginDateTime) _
                            AndAlso x.ActualEndDateTime Is Nothing).ToList()

                        If overlappingRsv.Count = 0 Then
                            'Add new row to the database as well
                            ' Insert/Update Reservation
                            Dim rsv As repo.Reservation = New repo.Reservation()
                            rsv.Resource = DA.Scheduler.Resource.Single(resourceId)
                            rsv.Client = DA.Current.Single(Of Client)(clientId)
                            rsv.CreatedOn = createdOn
                            rsv.BeginDateTime = beginDateTime
                            rsv.EndDateTime = endDateTime
                            rsv.LastModifiedOn = Date.Now
                            rsv.Account = DA.Current.Single(Of Account)(Convert.ToInt32(row("AccountID")))
                            rsv.Duration = Convert.ToDouble(row("Duration"))
                            rsv.MaxReservedDuration = Convert.ToDouble(row("Duration"))
                            rsv.Notes = row("Notes").ToString()
                            rsv.AutoEnd = Convert.ToBoolean(row("AutoEnd"))
                            rsv.HasProcessInfo = False
                            rsv.HasInvitees = False
                            rsv.RecurrenceID = recurrenceId
                            rsv.Activity = DA.Scheduler.Activity.Single(Convert.ToInt32(row("ActivityID")))
                            rsv.KeepAlive = Convert.ToBoolean(row("KeepAlive"))
                            rsv.Notes = row("Notes").ToString()

                            ' Insert Reservation Info
                            rsv.Insert(CurrentUser.ClientID)
                            reservations.Add(rsv)

                            ' Check for process info
                            RecurringReservationTransform.CopyProcessInfo(recurrenceId, rsv)

                            ' Check for invitees
                            RecurringReservationTransform.CopyInvitees(recurrenceId, rsv)

                            'the strategy to solve the problem of new record of new tool display is solved by
                            ' as long as we have new data added, we already re-draw the whole Scheduler HTML table
                            hasNewData = True
                        End If
                    End If
                Next

                If View = ViewType.ProcessTechView Then
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
            Dim listRsv As IList(Of repo.Reservation) = Nothing

            Dim totalReservationCount As Integer = 0

            For i As Integer = 1 To tblSchedule.Rows(0).Cells.Count - 1
                Dim columnCell As CustomTableCell = CType(tblSchedule.Rows(0).Cells(i), CustomTableCell)
                Dim currentStartTime As Date = columnCell.CellDate
                Dim currentEndTime As Date = columnCell.CellDate 'this doesn't get used
                Dim offset As Integer = 0

                If View = ViewType.DayView OrElse View = ViewType.WeekView Then
                    offset = Convert.ToInt32(Resource.Offset.TotalHours)
                End If

                ResourceUtility.GetTimeSlotBoundary(TimeSpan.FromMinutes(_minGran), TimeSpan.FromHours(offset), currentStartTime, currentEndTime)

                ' Select Reservations for this resource for this day
                listRsv = GetReservations().Find(currentStartTime, False)

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
                    Dim filteredRsv As IList(Of repo.Reservation) = Nothing

                    Select Case View
                        Case ViewType.DayView, ViewType.WeekView
                            filteredRsv = listRsv.Where(Function(x) ReservationFilter(x, beginTime, endTime)).OrderBy(Function(x) x.BeginDateTime).ToList()
                        Case ViewType.ProcessTechView, ViewType.UserView
                            filteredRsv = listRsv.Where(Function(x) x.Resource.ResourceID = rsvCell.ResourceID AndAlso ReservationFilter(x, beginTime, endTime)).OrderBy(Function(x) x.BeginDateTime).ToList()
                    End Select

                    Dim reservationCount As Integer = filteredRsv.Count
                    totalReservationCount += reservationCount

                    If reservationCount = 0 Then
                        lastResourceIds = String.Empty
                        lastReservationCount = 0
                    Else
                        ' the space is used to disambiguate the string (which should never be a problem, but
                        ' for example, without the space 123 + 456 would be the same as 12 + 3456
                        resourceIds = "" 'shouldn't this be called reservationIds?
                        For k As Integer = 0 To reservationCount - 1
                            resourceIds += filteredRsv(k).ReservationID.ToString() + " "
                        Next

                        ' if the number of res in cell changes, or the id's change, need to get cell info
                        If lastReservationCount <> reservationCount OrElse lastResourceIds <> resourceIds Then
                            lastResourceIds = resourceIds
                            lastReservationCount = reservationCount
                            mergeStartCell = j

                            If reservationCount = 1 OrElse Not filteredRsv(reservationCount - 1).ActualEndDateTime.HasValue Then
                                ' Display reservation
                                Dim rsv As repo.Reservation

                                If reservationCount = 1 Then
                                    rsv = filteredRsv.First()
                                Else
                                    lastReservationCount = 1 ' so this can merge with next cell
                                    lastResourceIds = filteredRsv(reservationCount - 1).ReservationID.ToString() + " "
                                    mergeStartCell = j
                                    rsv = filteredRsv(reservationCount - 1)
                                End If

                                rsvCell.ReservationID = rsv.ReservationID

                                ' Reservation Cell Events
                                Dim isInLab As Boolean = CacheManager.Current.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID)
                                Dim state As ReservationState = SchedulerUtility.GetReservationCell(rsvCell, rsv, CurrentUser.ClientID, isInLab)

                                If IsReservationActionState(state) Then
                                    SetReservationActionCellAttributes(rsvCell, "ReservationAction", state)
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
            ElseIf View = ViewType.UserView Then
                If totalReservationCount = 0 Then
                    ShowNoDataMessage("You do not have any reservations made for this date.")
                End If
            End If
        End Sub

        Private Sub ShowNoDataMessage(text As String)
            lblNoData.Text = text
            lblNoData.Visible = True
            tblSchedule.Visible = False
        End Sub

        Private Sub SetReservationActionCellAttributes(cell As CustomTableCell, command As String, state As ReservationState)
            Dim res As ResourceModel = CacheManager.Current.GetResource(cell.ResourceID)
            cell.CssClass = (cell.CssClass + " reservation-action").Trim()
            cell.Attributes.Add("data-command", command)
            cell.Attributes.Add("data-reservation-id", cell.ReservationID.ToString())
            cell.Attributes.Add("data-date", cell.CellDate.ToString("yyyy-MM-dd'T'HH:mm:ss"))
            cell.Attributes.Add("data-state", state.ToString())
            cell.Attributes.Add("data-path", PathInfo.Create(res).ToString())
            cell.AutoPostBack = False
        End Sub
#End Region

#Region "Utility"
        Private Function ReservationFilter(rsv As repo.Reservation, beginTime As Date, endTime As Date) As Boolean
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
            Return CacheManager.Current.GetAuthLevel(resourceId, CacheManager.Current.ClientID)
        End Function

        Public Async Function StartReservationAsync(rsv As repo.Reservation, clientId As Integer) As Task
            Try
                Dim isInLab As Boolean = CacheManager.Current.ClientInLab(rsv.Resource.ProcessTech.Lab.LabID)
                Await ReservationUtility.StartReservation(rsv, clientId, isInLab)
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try
        End Function
#End Region
    End Class
End Namespace