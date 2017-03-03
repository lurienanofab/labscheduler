Imports LabScheduler.AppCode.DBAccess
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports repo = LNF.Repository.Scheduler

Namespace Pages
    Public Class ReservationFacilityDownTime
        Inherits SchedulerPage

        Private resFacilityDownTime As FacilityDownTimeRes

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                GridDataBind()
                ClearForm()
            End If
        End Sub

        'Set default time, which is now
        Protected Sub ddlAMPM_DataBound(sender As Object, e As EventArgs)
            Dim tmp As Integer = 11

            Dim ddl As DropDownList = CType(sender, DropDownList)

            Dim tempTime As Date

            If ddl.ID = "ddlAMPM" Or ddl.ID = "ddlAMPMEnd" Then
                tempTime = Date.Now
            ElseIf ddl.ID = "ddlAMPMModify" Then
                tempTime = resFacilityDownTime.BeginDateTime
            ElseIf ddl.ID = "ddlAMPMEndModify" Then
                tempTime = resFacilityDownTime.EndDateTime
            End If

            If tempTime.TimeOfDay.ToString().Substring(0, 2) > tmp.ToString() Then
                ddl.Items(1).Selected = True
            End If
        End Sub

        Protected Sub ddlHour_DataBound(sender As Object, e As EventArgs)
            Dim ddl As DropDownList = CType(sender, DropDownList)

            Dim tempTime As Date

            If ddl.ID = "ddlHour" Or ddl.ID = "ddlHourEnd" Then
                tempTime = Date.Now
            ElseIf ddl.ID = "ddlHourModify" Then
                tempTime = resFacilityDownTime.BeginDateTime
            ElseIf ddl.ID = "ddlHourEndModify" Then
                tempTime = resFacilityDownTime.EndDateTime
            End If

            Dim tmp As Integer = tempTime.Hour
            If tmp > 12 Then
                tmp -= 12
            ElseIf tmp = 0 Then
                tmp = 12
            End If
            For Each item As ListItem In ddl.Items
                If item.Text = tmp.ToString() Then
                    item.Selected = True
                End If
            Next
        End Sub

        Protected Sub ddlMin_DataBound(sender As Object, e As EventArgs)
            Dim ddl As DropDownList = CType(sender, DropDownList)

            Dim tempTime As Date

            If ddl.ID = "ddlMin" Or ddl.ID = "ddlMinEnd" Then
                tempTime = Date.Now
            ElseIf ddl.ID = "ddlMinModify" Then
                tempTime = resFacilityDownTime.BeginDateTime
            ElseIf ddl.ID = "ddlMinEndModify" Then
                tempTime = resFacilityDownTime.EndDateTime
            End If

            For Each item As ListItem In ddl.Items
                If item.Text = tempTime.Minute.ToString() Then
                    item.Selected = True
                End If
            Next
        End Sub

        Protected Sub btnReserve_Click(sender As Object, e As EventArgs)
            ClearAlert()

            'Calculate the correct time format (AM/PM)
            Dim beginDateTime As Date = GetStartDateTime()
            Dim endDateTime As Date = GetEndDateTime()

            If beginDateTime >= endDateTime Then
                ShowAlert("danger", "Error: Please make sure your start time and end time are correct")
                Exit Sub
            End If

            'Get the selected indices and find out their corresponding tool IDs
            Dim toolIdList As List(Of Integer) = New List(Of Integer)()

            Dim indices As Integer() = lboxTools.GetSelectedIndices()

            For Each i As Integer In indices
                toolIdList.Add(Integer.Parse(lboxTools.Items(i).Value))
            Next

            If toolIdList.Count = 0 Then
                ShowAlert("danger", "No tool selected, so there is no reservation made")
                Exit Sub
            End If

            '2009-07-19 make a reservation group
            Dim clientId As Integer = CType(Session("ClientID"), Integer)
            Dim groupId As Integer = FacilityDownTimeDB.CreateNew(clientId, beginDateTime, endDateTime)

            'Loop through each selected tool and make reservation and delete other people's reservation
            For Each resourceId As Integer In toolIdList
                Dim rsv As repo.Reservation = New repo.Reservation()

                rsv.Resource = DA.Current.Single(Of repo.Resource)(resourceId)
                rsv.RecurrenceID = -1 'always -1 for non-recurring reservation
                rsv.GroupID = groupId
                rsv.Client = DA.Current.Single(Of Client)(clientId)
                rsv.BeginDateTime = beginDateTime
                rsv.EndDateTime = endDateTime
                rsv.ActualBeginDateTime = beginDateTime 'is this correct?
                rsv.ActualEndDateTime = endDateTime 'is this correct?
                rsv.LastModifiedOn = Date.Now
                rsv.Account = Properties.Current.LabAccount
                rsv.Activity = Properties.Current.Activities.FacilityDownTime
                rsv.Duration = (beginDateTime - endDateTime).Duration.TotalMinutes
                rsv.Notes = txtNotes.Text
                rsv.AutoEnd = False
                rsv.HasProcessInfo = False
                rsv.HasInvitees = False
                rsv.CreatedOn = Date.Now

                ' Find and Remove any un-started reservations made during time of repair
                Dim query As IList(Of repo.Reservation) = DA.Scheduler.Reservation.SelectByResource(resourceId, beginDateTime, endDateTime, False)
                For Each existing As repo.Reservation In query
                    ' Only if the reservation has not begun
                    If existing.ActualBeginDateTime Is Nothing Then
                        existing.Delete(CurrentUser.ClientID)
                        EmailUtility.EmailOnCanceledByRepair(existing, True, "LNF Facility Down", "Facility is down, thus we have to disable the tool.", endDateTime)
                    Else
                        'We have to disable all those reservations that have been activated by setting isActive to 0.  
                        'The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                        'have to charge the user for that reservation
                    End If
                Next
                rsv.InsertFacilityDownTime(CurrentUser.ClientID)
            Next

            GridDataBind()
            ClearForm()
            hidSelectedTabIndex.Value = "0"

            ShowAlert("success", String.Format("You've created Facility Down Time reservation from {0} to {1} on {2} tools. User's reservations have been deleted as well.", beginDateTime, endDateTime, toolIdList.Count))
        End Sub

        Protected Sub btnBack_Click(sender As Object, e As EventArgs)
            Response.Redirect("~")
        End Sub

        Protected Sub Row_Command(sender As Object, e As CommandEventArgs)
            ClearAlert()
            hidSelectedTabIndex.Value = "1"

            If e.CommandName = "Edit" Then
                Dim groupId As Integer = CType(e.CommandArgument, Integer)

                hidGroupID.Value = groupId.ToString()

                lboxModify.DataSource = ReservationUtility.SelectByGroup(groupId).Select(Function(x) New With {.ResourceID = x.Resource.ResourceID, .ResourceName = x.Resource.ResourceName}).ToList()
                lboxModify.DataBind()

                resFacilityDownTime = FacilityDownTimeDB.GetFacilityDownTimeByGroupID(groupId)

                ddlHourModify.DataSource = DateTimeUtility.GetAllHours()
                ddlMinModify.DataSource = DateTimeUtility.GetAllMinutes()
                ddlAMPMModify.DataSource = DateTimeUtility.GetAllAMPM()

                ddlHourEndModify.DataSource = DateTimeUtility.GetAllHours()
                ddlMinEndModify.DataSource = DateTimeUtility.GetAllMinutes()
                ddlAMPMEndModify.DataSource = DateTimeUtility.GetAllAMPM()

                ddlHourModify.DataBind()
                ddlMinModify.DataBind()
                ddlAMPMModify.DataBind()
                ddlHourEndModify.DataBind()
                ddlMinEndModify.DataBind()
                ddlAMPMEndModify.DataBind()

                'Set the calendar with correct datetime from DB's data.  The drop down lists are set in their own DataBound events
                txtStartDateModify.Text = resFacilityDownTime.BeginDateTime.ToString("MM/dd/yyyy")
                txtEndDateModify.Text = resFacilityDownTime.EndDateTime.ToString("MM/dd/yyyy")

                'Check to see if we should disable some controls based on the time of reservation
                If resFacilityDownTime.BeginDateTime <= Date.Now Then
                    txtStartDateModify.Enabled = False
                    ddlHourModify.Enabled = False
                    ddlMinModify.Enabled = False
                    ddlAMPMModify.Enabled = False
                Else
                    txtStartDateModify.Enabled = True
                    ddlHourModify.Enabled = True
                    ddlMinModify.Enabled = True
                    ddlAMPMModify.Enabled = True
                End If

                If resFacilityDownTime.EndDateTime <= Date.Now Then
                    txtEndDateModify.Enabled = False
                    ddlHourEndModify.Enabled = False
                    ddlMinEndModify.Enabled = False
                    ddlAMPMEndModify.Enabled = False
                Else
                    txtEndDateModify.Enabled = True
                    ddlHourEndModify.Enabled = True
                    ddlMinEndModify.Enabled = True
                    ddlAMPMEndModify.Enabled = True
                End If

                If Not txtEndDateModify.Enabled AndAlso Not txtStartDateModify.Enabled Then
                    btnUpdateModify.Enabled = False
                Else
                    btnUpdateModify.Enabled = True
                End If

                litName.Text = resFacilityDownTime.DisplayName

                phModify.Visible = True
                phGrid.Visible = False
            ElseIf e.CommandName = "Delete" Then
                Dim groupId As Integer = Integer.Parse(hidDeleteGroupID.Value)

                FacilityDownTimeDB.DeleteGroupReservations(groupId)

                'delete all the future reservations and keep the old ones
                DA.Scheduler.Reservation.DeleteByGroup(groupId, CurrentUser.ClientID)

                GridDataBind()

                ShowAlert("success", String.Format("You have successfully deleted a Facility Down Time reservation. [GroupID: {0}]", groupId))
            End If
        End Sub

        Private Sub ClearForm()
            ddlLabs.SelectedIndex = 0
            lboxTools.ClearSelection()
            txtStartDate.Text = Date.Now.ToString("MM/dd/yyyy")
            txtEndDate.Text = Date.Now.AddDays(1).ToString("MM/dd/yyyy")
            txtNotes.Text = String.Empty

            ddlHour.Items.Clear()
            ddlHour.DataSource = DateTimeUtility.GetAllHours()
            ddlHour.DataBind()

            ddlMin.Items.Clear()
            ddlMin.DataSource = DateTimeUtility.GetAllMinutes()
            ddlMin.DataBind()

            ddlAMPM.Items.Clear()
            ddlAMPM.DataSource = DateTimeUtility.GetAllAMPM()
            ddlAMPM.DataBind()

            ddlHourEnd.Items.Clear()
            ddlHourEnd.DataSource = DateTimeUtility.GetAllHours()
            ddlHourEnd.DataBind()

            ddlMinEnd.Items.Clear()
            ddlMinEnd.DataSource = DateTimeUtility.GetAllMinutes()
            ddlMinEnd.DataBind()

            ddlAMPMEnd.Items.Clear()
            ddlAMPMEnd.DataSource = DateTimeUtility.GetAllAMPM()
            ddlAMPMEnd.DataBind()
        End Sub

        Private Sub GridDataBind()
            Dim dt As DataTable = FacilityDownTimeDB.GetFacilityDownTimeRes()
            dt.DefaultView.Sort = "BeginDateTime DESC"
            rptFDT.DataSource = dt.DefaultView
            rptFDT.DataBind()
        End Sub

        Protected Sub rptFDT_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim dataItem As Object = e.Item.DataItem
                Dim drv As DataRowView = CType(dataItem, DataRowView)
                Dim endTime As Date = CType(drv("EndDateTime"), Date)
                If endTime <= Date.Now Then
                    Dim phDeleteGroup As PlaceHolder = CType(e.Item.FindControl("phDeleteGroup"), PlaceHolder)
                    phDeleteGroup.Visible = False
                End If
            End If
        End Sub

        Protected Sub btnUpdateModify_Click(sender As Object, e As EventArgs)
            ClearAlert()

            'Calculate the correct time format (AM/PM)
            Dim beginDateTime As Date = GetModifyStartDateTime()
            Dim endDateTime As Date = GetModifyEndDateTime()

            If beginDateTime >= endDateTime Then
                ShowAlert("danger", "Error: Please make sure your start time and end time are correct")
                Exit Sub
            End If

            'Update the group reservation
            FacilityDownTimeDB.UpdateByGroupID(CType(hidGroupID.Value, Integer), beginDateTime, endDateTime)

            'Update the individual reservations
            ReservationUtility.UpdateByGroup(CType(hidGroupID.Value, Integer), beginDateTime, endDateTime, txtNotesModify.Text, CurrentUser.ClientID)

            'Delete all the reservations in this period
            For Each res As ListItem In lboxModify.Items
                ' Find and Remove any un-started reservations made during time of repair
                Dim query As IList(Of repo.Reservation) = DA.Scheduler.Reservation.SelectByResource(Convert.ToInt32(res.Value), beginDateTime, endDateTime, False)
                For Each existing As repo.Reservation In query
                    ' Only if the reservation has not begun
                    If existing.ActualBeginDateTime Is Nothing Then
                        existing.Delete(CurrentUser.ClientID)
                        EmailUtility.EmailOnCanceledByRepair(existing, True, "LNF Facility Down", "Facility is down, thus we have to disable the tool. Notes:", endDateTime)
                    Else
                        'We have to disable all those reservations that have been activated by setting isActive to 0.  
                        'The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                        'have to charge the user for that reservation
                    End If
                Next
            Next

            ShowAlert("success", "Reservation Updated Successfully")

            CancelModify()
        End Sub

        Protected Sub btnBackModify_Click(sender As Object, e As EventArgs)
            CancelModify()
        End Sub

        Private Sub CancelModify()
            GridDataBind()
            phModify.Visible = False
            phGrid.Visible = True
            hidSelectedTabIndex.Value = "1"
        End Sub

        Private Function GetRealHour(value As String, ampm As String) As Integer
            Dim hh As Integer = Integer.Parse(value)

            If ampm = "PM" Then
                If hh <> 12 Then
                    hh += 12
                End If
            Else 'AM, we have to take care the case when it's 12 something AM
                If hh = 12 Then
                    hh = 0
                End If
            End If

            Return hh
        End Function

        Private Function GetTime(h As Integer, m As Integer, s As Integer) As TimeSpan
            Dim time As String = String.Format("{0:00}:{1:00}:{2:00}", h, m, s)
            Return TimeSpan.Parse(time)
        End Function

        Private Function GetStartDateTime() As Date
            Return GetDateTime(txtStartDate.Text, ddlHour.SelectedValue, ddlMin.SelectedValue, ddlAMPM.SelectedValue)
        End Function

        Private Function GetEndDateTime() As Date
            Return GetDateTime(txtEndDate.Text, ddlHourEnd.SelectedValue, ddlMinEnd.SelectedValue, ddlAMPMEnd.SelectedValue)
        End Function

        Private Function GetModifyStartDateTime() As Date
            Return GetDateTime(txtStartDateModify.Text, ddlHourModify.SelectedValue, ddlMinModify.SelectedValue, ddlAMPMModify.SelectedValue)
        End Function

        Private Function GetModifyEndDateTime() As Date
            Return GetDateTime(txtEndDateModify.Text, ddlHourEndModify.SelectedValue, ddlMinEndModify.SelectedValue, ddlAMPMEndModify.SelectedValue)
        End Function

        Private Function GetDateTime([date] As String, hour As String, minute As String, ampm As String) As Date
            Dim realDate As Date = Date.Parse([date])
            Dim realHour As Integer = GetRealHour(hour, ampm)
            Dim time As TimeSpan = GetTime(realHour, Integer.Parse(minute), 0)
            Dim result As Date = realDate.Add(time)
            Return result
        End Function

        Private Sub ClearAlert()
            litAlert1.Text = String.Empty
            litAlert2.Text = String.Empty
        End Sub

        Private Sub ShowAlert(alertType As String, alertMessage As String)
            Dim html As String = String.Format("<div class=""alert alert-{0}"" role=""alert"">{1}</div>", alertType, alertMessage)
            litAlert1.Text += html
            litAlert2.Text += html
        End Sub
    End Class
End Namespace