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
                    hypCancel.NavigateUrl = String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", Request.SelectedDate())
                End If
            End If
        End Sub

        Private Sub LoadReservationRecurrence(recurrenceId As Integer)
            Dim rr As repo.ReservationRecurrence = DA.Current.Single(Of repo.ReservationRecurrence)(recurrenceId)

            If rr Is Nothing Then
                DisplayError(String.Format("Cannot find recurrence with RecurrenceID = {0}", recurrenceId))
                Exit Sub
            End If

            rptRecurrence.DataSource = {rr}
            rptRecurrence.DataBind()

            Dim existingReservations As IEnumerable(Of repo.Reservation) = From rsv In DA.Current.Query(Of repo.Reservation)()
                                                                           Where rsv.RecurrenceID.HasValue AndAlso rsv.RecurrenceID.Value = recurrenceId AndAlso rsv.BeginDateTime >= Date.Now.Date.AddMonths(-6)
                                                                           Order By rsv.ReservationID Descending

            rptExistingReservations.DataSource = existingReservations
            rptExistingReservations.DataBind()
        End Sub

        Private Sub DisplayError(msg As String)
            litMessage.Text = String.Format("<div class=""alert alert-danger"" role=""alert"">{0}</div>", msg)
        End Sub

        Protected Sub rptRecurrence_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            Dim rr As repo.ReservationRecurrence = CType(e.Item.DataItem, repo.ReservationRecurrence)

            Dim hypResource As HyperLink = CType(e.Item.FindControl("hypResource"), HyperLink)
            hypResource.Text = String.Format("{0} [{1}]", rr.Resource.ResourceName, rr.Resource.ResourceID)
            hypResource.NavigateUrl = String.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(rr.Resource).UrlEncode(), Request.SelectedDate())

            Dim ddlStartTimeHour As DropDownList = CType(e.Item.FindControl("ddlStartTimeHour"), DropDownList)
            Dim ddlStartTimeMin As DropDownList = CType(e.Item.FindControl("ddlStartTimeMin"), DropDownList)
            LoadBeginTime(rr, ddlStartTimeHour, ddlStartTimeMin)

            Dim chkKeepAlive As HtmlInputCheckBox = CType(e.Item.FindControl("chkKeepAlive"), HtmlInputCheckBox)
            Dim chkAutoEnd As HtmlInputCheckBox = CType(e.Item.FindControl("chkAutoEnd"), HtmlInputCheckBox)

            chkKeepAlive.Checked = rr.KeepAlive
            chkAutoEnd.Checked = rr.AutoEnd

            Dim rdoRecurringPatternWeekly As HtmlInputRadioButton = CType(e.Item.FindControl("rdoRecurringPatternWeekly"), HtmlInputRadioButton)
            Dim rdoRecurringPatternMonthly As HtmlInputRadioButton = CType(e.Item.FindControl("rdoRecurringPatternMonthly"), HtmlInputRadioButton)

            rdoRecurringPatternWeekly.Checked = False
            rdoRecurringPatternMonthly.Checked = False

            If rr.Pattern.PatternID = 1 Then
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
        End Sub

        Private Function SaveReservationRecurrence(recurrenceId As Integer) As Boolean
            Dim rr As repo.ReservationRecurrence = DA.Current.Single(Of repo.ReservationRecurrence)(recurrenceId)

            If rr Is Nothing Then
                DisplayError(String.Format("Cannot find recurrence with RecurrenceID = {0}", recurrenceId))
                Return False
            End If

            Dim item As RepeaterItem = rptRecurrence.Items(0)

            Dim ddlStartTimeHour As DropDownList = CType(item.FindControl("ddlStartTimeHour"), DropDownList)
            Dim ddlStartTimeMin As DropDownList = CType(item.FindControl("ddlStartTimeMin"), DropDownList)

            Dim ts As TimeSpan = TimeSpan.FromHours(Integer.Parse(ddlStartTimeHour.SelectedValue)).Add(TimeSpan.FromMinutes(Integer.Parse(ddlStartTimeMin.SelectedValue)))

            Dim chkKeepAlive As HtmlInputCheckBox = CType(item.FindControl("chkKeepAlive"), HtmlInputCheckBox)
            Dim chkAutoEnd As HtmlInputCheckBox = CType(item.FindControl("chkAutoEnd"), HtmlInputCheckBox)

            Dim rdoRecurringPatternWeekly As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeekly"), HtmlInputRadioButton)

            If rdoRecurringPatternWeekly.Checked Then
                Dim rdoRecurringPatternWeeklySunday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklySunday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyMonday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyMonday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyTuesday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyTuesday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyWednesday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyWednesday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyThursday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyThursday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklyFriday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklyFriday"), HtmlInputRadioButton)
                Dim rdoRecurringPatternWeeklySaturday As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringPatternWeeklySaturday"), HtmlInputRadioButton)

                rr.Pattern = DA.Current.Single(Of repo.RecurrencePattern)(1)

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

                rr.PatternParam1 = Convert.ToInt32(dow)
                rr.PatternParam2 = Nothing
            Else
                Dim ddlMonthly1 As DropDownList = CType(item.FindControl("ddlMonthly1"), DropDownList)
                Dim ddlMonthly2 As DropDownList = CType(item.FindControl("ddlMonthly2"), DropDownList)

                rr.Pattern = DA.Current.Single(Of repo.RecurrencePattern)(2)
                rr.PatternParam1 = Integer.Parse(ddlMonthly1.SelectedValue)
                rr.PatternParam2 = Integer.Parse(ddlMonthly2.SelectedValue)
            End If

            Dim txtStartDate As TextBox = CType(item.FindControl("txtStartDate"), TextBox)
            Dim txtEndDate As HtmlInputText = CType(item.FindControl("txtEndDate"), HtmlInputText)

            rr.BeginDate = Date.Parse(txtStartDate.Text).Date
            rr.BeginTime = rr.BeginDate.Add(ts)

            Dim rdoRecurringRangeInfinite As HtmlInputRadioButton = CType(item.FindControl("rdoRecurringRangeInfinite"), HtmlInputRadioButton)

            If rdoRecurringRangeInfinite.Checked Then
                rr.EndDate = Nothing
            Else
                rr.EndDate = Date.Parse(txtEndDate.Value).Date
            End If
            Return True
        End Function

        Protected Sub btnSave_Click(sender As Object, e As EventArgs)
            Dim recurrenceId As Integer = 0
            If GetRecurrenceID(recurrenceId) Then
                If SaveReservationRecurrence(recurrenceId) Then
                    Response.Redirect(String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", Request.SelectedDate()))
                End If
            End If
        End Sub

        Protected Function GetReservationLink(rsv As repo.Reservation) As String
            If Not rsv.ActualBeginDateTime.HasValue AndAlso rsv.BeginDateTime > Date.Now Then
                Dim url As String = SchedulerUtility.GetReservationReturnUrl(PathInfo.Create(rsv.Resource), rsv.ReservationID, rsv.BeginDateTime, rsv.BeginDateTime.TimeOfDay)
                Return String.Format("<a href=""{0}"">{1}</a>", VirtualPathUtility.ToAbsolute(url), rsv.ReservationID)
            Else
                Dim url As String = SchedulerUtility.GetReturnUrl("ReservationHistory.aspx", PathInfo.Create(rsv.Resource), rsv.ReservationID, Request.SelectedDate())
                Return String.Format("<a href=""{0}"">{1}</a>", VirtualPathUtility.ToAbsolute(url), rsv.ReservationID)
            End If
        End Function

        Private Sub LoadBeginTime(rr As repo.ReservationRecurrence, ddlStartTimeHour As DropDownList, ddlStartTimeMin As DropDownList)
            Dim res As repo.Resource = rr.Resource

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
                ddlStartTimeMin.Items.Add(New ListItem(i.ToString(), i.ToString()))
            Next

            ' Select Preselected Time
            Dim item As ListItem
            item = ddlStartTimeHour.Items.FindByValue(rr.BeginTime.Hour.ToString())
            If item IsNot Nothing Then
                item.Selected = True
            End If

            item = ddlStartTimeMin.Items.FindByValue(rr.EndTime.Minute.ToString())
            If item IsNot Nothing Then
                item.Selected = True
            End If
        End Sub
    End Class
End Namespace