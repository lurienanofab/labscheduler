Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports repo = LNF.Repository.Scheduler

Namespace Pages
    Public Class UserRecurringReservationEdit
        Inherits SchedulerPage

        Private Function GetRecurrenceID(ByRef recurrenceId As Integer) As Boolean
            If Integer.TryParse(Request.QueryString("RecurrenceID"), recurrenceId) Then
                Return True
            Else
                DisplayError("Missing paramter: RecurrenceID")
                Return False
            End If
        End Function

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                litMessage.Text = String.Empty

                Dim recurrenceId As Integer = 0
                If GetRecurrenceID(recurrenceId) Then
                    LoadReservationRecurrence(recurrenceId)
                    hypCancel.NavigateUrl = String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
                End If
            End If
        End Sub

        Private Sub LoadReservationRecurrence(recurrenceId As Integer)
            Dim rr As IReservationRecurrence = Provider.Scheduler.Reservation.GetReservationRecurrence(recurrenceId)

            If rr Is Nothing Then
                DisplayError(String.Format("Cannot find recurrence with RecurrenceID = {0}", recurrenceId))
                Exit Sub
            End If

            rptRecurrence.DataSource = {rr}
            rptRecurrence.DataBind()

            Dim existing As IEnumerable(Of IReservation) = Provider.Scheduler.Reservation.GetRecurringReservations(recurrenceId, Date.Now.Date.AddMonths(-6), Nothing)

            rptExistingReservations.DataSource = existing
            rptExistingReservations.DataBind()
        End Sub

        Private Sub DisplayError(msg As String)
            litMessage.Text = String.Format("<div class=""alert alert-danger"" role=""alert"">{0}</div>", msg)
        End Sub

        Protected Sub RptRecurrence_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim rr As IReservationRecurrence = CType(e.Item.DataItem, IReservationRecurrence)

                Dim hypResource As HyperLink = CType(e.Item.FindControl("hypResource"), HyperLink)
                hypResource.Text = String.Format("{0} [{1}]", rr.ResourceName, rr.ResourceID)
                hypResource.NavigateUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(rr.ResourceID).UrlEncode(), ContextBase.Request.SelectedDate())

                Dim ddlStartTimeHour As DropDownList = CType(e.Item.FindControl("ddlStartTimeHour"), DropDownList)
                Dim ddlStartTimeMin As DropDownList = CType(e.Item.FindControl("ddlStartTimeMin"), DropDownList)
                LoadBeginTime(rr, ddlStartTimeHour, ddlStartTimeMin)

                Dim txtDuration As TextBox = CType(e.Item.FindControl("txtDuration"), TextBox)
                txtDuration.Text = rr.Duration.ToString()

                Dim chkKeepAlive As HtmlInputCheckBox = CType(e.Item.FindControl("chkKeepAlive"), HtmlInputCheckBox)
                Dim chkAutoEnd As HtmlInputCheckBox = CType(e.Item.FindControl("chkAutoEnd"), HtmlInputCheckBox)

                chkKeepAlive.Checked = rr.KeepAlive
                chkAutoEnd.Checked = rr.AutoEnd

                Dim txtNotes As TextBox = CType(e.Item.FindControl("txtNotes"), TextBox)
                txtNotes.Text = rr.Notes

                Dim rdoRecurringPatternWeekly As HtmlInputRadioButton = CType(e.Item.FindControl("rdoRecurringPatternWeekly"), HtmlInputRadioButton)
                Dim rdoRecurringPatternMonthly As HtmlInputRadioButton = CType(e.Item.FindControl("rdoRecurringPatternMonthly"), HtmlInputRadioButton)

                rdoRecurringPatternWeekly.Checked = False
                rdoRecurringPatternMonthly.Checked = False

                If rr.PatternID = 1 Then
                    rdoRecurringPatternWeekly.Checked = True
                    Dim dow As DayOfWeek = CType(rr.PatternParam1, DayOfWeek)
                    Dim radio As HtmlInputRadioButton = Nothing
                    Select Case dow
                        Case DayOfWeek.Monday
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklyMonday"), HtmlInputRadioButton)
                        Case DayOfWeek.Tuesday
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklyTuesday"), HtmlInputRadioButton)
                        Case DayOfWeek.Wednesday
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklyWednesday"), HtmlInputRadioButton)
                        Case DayOfWeek.Thursday
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklyThursday"), HtmlInputRadioButton)
                        Case DayOfWeek.Friday
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklyFriday"), HtmlInputRadioButton)
                        Case DayOfWeek.Saturday
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklySaturday"), HtmlInputRadioButton)
                        Case Else
                            radio = CType(e.Item.FindControl("rdoRecurringPatternWeeklySunday"), HtmlInputRadioButton)
                    End Select
                    radio.Checked = True
                Else
                    rdoRecurringPatternMonthly.Checked = True
                    Dim ddlMonthly1 As DropDownList = CType(e.Item.FindControl("ddlMonthly1"), DropDownList)
                    Dim ddlMonthly2 As DropDownList = CType(e.Item.FindControl("ddlMonthly2"), DropDownList)
                    ddlMonthly1.SelectedValue = rr.PatternParam1.ToString()
                    ddlMonthly2.SelectedValue = rr.PatternParam2.ToString()
                End If

                Dim txtStartDate As TextBox = CType(e.Item.FindControl("txtStartDate"), TextBox)
                Dim txtEndDate As HtmlInputText = CType(e.Item.FindControl("txtEndDate"), HtmlInputText)

                txtStartDate.Text = rr.BeginDate.ToString("MM/dd/yyyy")

                Dim rdoRecurringRangeInfinite As HtmlInputRadioButton = CType(e.Item.FindControl("rdoRecurringRangeInfinite"), HtmlInputRadioButton)
                Dim rdoRecurringRangeEndBy As HtmlInputRadioButton = CType(e.Item.FindControl("rdoRecurringRangeEndBy"), HtmlInputRadioButton)

                If rr.EndDate.HasValue Then
                    rdoRecurringRangeInfinite.Checked = False
                    rdoRecurringRangeEndBy.Checked = True
                    txtEndDate.Value = rr.EndDate.Value.ToString("MM/dd/yyyy")
                    txtEndDate.Disabled = False
                Else
                    rdoRecurringRangeInfinite.Checked = True
                    rdoRecurringRangeEndBy.Checked = False
                    txtEndDate.Value = String.Empty
                    txtEndDate.Disabled = True
                End If
            End If
        End Sub

        Private Function SaveReservationRecurrence(recurrenceId As Integer) As Boolean
            Dim item As RepeaterItem = rptRecurrence.Items(0)

            Dim ddlStartTimeHour As DropDownList = CType(item.FindControl("ddlStartTimeHour"), DropDownList)
            Dim ddlStartTimeMin As DropDownList = CType(item.FindControl("ddlStartTimeMin"), DropDownList)
            Dim txtDuration As TextBox = CType(item.FindControl("txtDuration"), TextBox)

            Dim beginTime As TimeSpan = TimeSpan.FromHours(Convert.ToInt32(ddlStartTimeHour.SelectedValue)).Add(TimeSpan.FromMinutes(Convert.ToInt32(ddlStartTimeMin.SelectedValue)))
            Dim duration As Double = Convert.ToDouble(txtDuration.Text)

            Dim chkKeepAlive As HtmlInputCheckBox = CType(item.FindControl("chkKeepAlive"), HtmlInputCheckBox)
            Dim chkAutoEnd As HtmlInputCheckBox = CType(item.FindControl("chkAutoEnd"), HtmlInputCheckBox)

            Dim keepAlive As Boolean = chkKeepAlive.Checked
            Dim autoEnd As Boolean = chkAutoEnd.Checked

            Dim rdoRecurringPatternWeekly As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeekly"), HtmlInputRadioButton)

            Dim patternId As Integer
            Dim param1 As Integer
            Dim param2 As Integer?

            If rdoRecurringPatternWeekly.Checked Then
                Dim rdoRecurringPatternWeeklySunday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklySunday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyMonday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyMonday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyTuesday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyTuesday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyWednesday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyWednesday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyThursday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyThursday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyFriday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyFriday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklySaturday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklySaturday"), HtmlInputRadioButton)

                patternId = 1

                Dim dow As DayOfWeek

                If rdoRecurringPatternWeeklyMonday.Checked Then
                    dow = DayOfWeek.Monday
                ElseIf rdoRecurringPatternWeeklyTuesday.Checked Then
                    dow = DayOfWeek.Tuesday
                ElseIf rdoRecurringPatternWeeklyWednesday.Checked Then
                    dow = DayOfWeek.Wednesday
                ElseIf rdoRecurringPatternWeeklyThursday.Checked Then
                    dow = DayOfWeek.Thursday
                ElseIf rdoRecurringPatternWeeklyFriday.Checked Then
                    dow = DayOfWeek.Friday
                ElseIf rdoRecurringPatternWeeklySaturday.Checked Then
                    dow = DayOfWeek.Saturday
                Else
                    dow = DayOfWeek.Sunday
                End If

                param1 = Convert.ToInt32(dow)
                param2 = Nothing
            Else
                Dim ddlMonthly1 As DropDownList = CType(item.FindControl("ddlMonthly1"), DropDownList)
                Dim ddlMonthly2 As DropDownList = CType(item.FindControl("ddlMonthly2"), DropDownList)

                patternId = 2
                param1 = Convert.ToInt32(ddlMonthly1.SelectedValue)
                param2 = Convert.ToInt32(ddlMonthly2.SelectedValue)
            End If

            Dim txtStartDate As TextBox = CType(item.FindControl("txtStartDate"), TextBox)
            Dim txtEndDate As HtmlInputText = CType(item.FindControl("txtEndDate"), HtmlInputText)

            Dim beginDate As Date = Convert.ToDateTime(txtStartDate.Text).Date
            Dim endDate As Date?

            Dim rdoRecurringRangeInfinite As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringRangeInfinite"), HtmlInputRadioButton)

            If rdoRecurringRangeInfinite.Checked Then
                endDate = Nothing
            Else
                endDate = Date.Parse(txtEndDate.Value).Date
            End If

            Dim txtNotes As TextBox = CType(item.FindControl("txtNotes"), TextBox)
            Dim notes As String = txtNotes.Text

            Return Provider.Scheduler.Reservation.SaveReservationRecurrence(recurrenceId, patternId, param1, param2, beginDate, beginTime, duration, endDate, autoEnd, keepAlive, notes)
        End Function

        Protected Sub BtnSave_Click(sender As Object, e As EventArgs)
            Dim recurrenceId As Integer = 0
            If GetRecurrenceID(recurrenceId) Then
                If SaveReservationRecurrence(recurrenceId) Then
                    Response.Redirect(String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate()))
                Else
                    DisplayError(String.Format("Cannot find recurrence with RecurrenceID = {0}", recurrenceId))
                End If
            End If
        End Sub

        Protected Function GetReservationLink(rsv As IReservation) As String
            If Not rsv.ActualBeginDateTime.HasValue AndAlso rsv.BeginDateTime > Date.Now Then
                Dim url As String = SchedulerUtility.GetReservationReturnUrl(PathInfo.Create(rsv), rsv.ReservationID, rsv.BeginDateTime, rsv.BeginDateTime.TimeOfDay)
                Return String.Format("<a href=""{0}"">{1}</a>", VirtualPathUtility.ToAbsolute(url), rsv.ReservationID)
            Else
                Dim url As String = SchedulerUtility.GetReturnUrl("ReservationHistory.aspx", PathInfo.Create(rsv), rsv.ReservationID, ContextBase.Request.SelectedDate())
                Return String.Format("<a href=""{0}"">{1}</a>", VirtualPathUtility.ToAbsolute(url), rsv.ReservationID)
            End If
        End Function

        Protected Function GetCancelReservationLinkVisible(rsv As IReservation) As Boolean
            Return Not rsv.ActualBeginDateTime.HasValue AndAlso Not rsv.ActualEndDateTime.HasValue AndAlso Not rsv.IsStarted AndAlso rsv.IsActive AndAlso rsv.BeginDateTime > Date.Now
        End Function

        Protected Function GetIsCancelled(rsv As IReservation) As String
            If rsv.IsActive Then
                Return String.Empty
            Else
                Return "&#10003;"
            End If
        End Function

        Private Sub LoadBeginTime(rr As IReservationRecurrence, ddlStartTimeHour As DropDownList, ddlStartTimeMin As DropDownList)
            Dim res As IResource = Provider.Scheduler.Resource.GetResource(rr.ResourceID)

            If res.Granularity = 0 Then
                Throw New Exception(String.Format("Granularity is zero for the resource '{0}' ({1}) : ", res.ResourceName, res.ResourceID.ToString()))
            End If

            ' Restrictions: Start Time either = end of previous reservation or
            ' the gap between reservations has to be multiples of Min Reserv Time
            ' This is checked for when user clicks on Submit button

            '2011-12-28 start time must be less than or equal to chargeable end time and greater or equal to current time.

            ' Determine 24-hour granularities
            Dim gran As TimeSpan = TimeSpan.FromMinutes(res.Granularity)
            Dim stepSize As Integer = Convert.ToInt32(gran.TotalHours)
            If stepSize = 0 Then stepSize = 1
            Dim grans As New List(Of Integer)
            For i As Integer = res.Offset To 24 Step stepSize
                grans.Add(i)
            Next

            Dim selectedDate As Date = rr.BeginDate
            Dim selectedTime As Date = rr.BeginTime

            Dim minTime As TimeSpan = TimeSpan.Zero

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

            For i As Integer = 0 To 59 Step res.Granularity
                ddlStartTimeMin.Items.Add(New ListItem(i.ToString("00"), i.ToString()))
            Next

            ' Select Preselected Time
            Dim item As ListItem
            item = ddlStartTimeHour.Items.FindByValue(rr.BeginTime.Hour.ToString())
            If item IsNot Nothing Then
                item.Selected = True
            End If

            item = ddlStartTimeMin.Items.FindByValue(rr.BeginTime.Minute.ToString())
            If item IsNot Nothing Then
                item.Selected = True
            End If
        End Sub

        Protected Sub BtnCancelReservation_Command(sender As Object, e As CommandEventArgs)
            Dim recurrenceId As Integer = 0
            If GetRecurrenceID(recurrenceId) Then
                Dim existing As IEnumerable(Of IReservation) = Provider.Scheduler.Reservation.GetRecurringReservations(recurrenceId, Date.Now.Date.AddMonths(-6), Nothing)
                Dim reservationId As Integer = Convert.ToInt32(e.CommandArgument)

                Dim rsv As IReservation = existing.FirstOrDefault(Function(x) x.ReservationID = reservationId)

                If rsv IsNot Nothing Then
                    rsv.IsActive = False
                    Provider.Scheduler.Reservation.CancelReservation(rsv.ReservationID, CurrentUser.ClientID)
                    rptExistingReservations.DataSource = existing
                    rptExistingReservations.DataBind()
                End If
            End If
        End Sub
    End Class
End Namespace