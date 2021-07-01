Imports LabScheduler.AppCode
Imports LNF.Cache
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ResourceConfig
        Inherits SchedulerPage

        Private Const MAX_OTF_SCHED_TIME As Integer = 60

        Private _resource As IResource
        Private _resourceActivityAuths As IEnumerable(Of IResourceActivityAuth)

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            _resource = GetCurrentResource()

            'If user types url directly, we have to return immediately if resDB is not loaded
            If _resource IsNot Nothing Then
                LoadProcessInfo()
                InitReseourceAuth()
                If Not IsPostBack Then
                    LoadResource()
                End If

                If Session("ErrorMessage") IsNot Nothing Then
                    Dim errmsg = Session("ErrorMessage").ToString()
                    WebUtility.BootstrapAlert(phMessage, "danger", errmsg, True)
                    Session.Remove("ErrorMessage")
                End If
            Else
                'Something wrong - probably user types the url directly
                Response.Redirect("~")
            End If
        End Sub

        Private Function GetResourceActivityAuth(ByVal activityId As Integer) As IResourceActivityAuth
            Return _resourceActivityAuths.FirstOrDefault(Function(x) x.ActivityID = activityId)
        End Function

        Private Function InitCreateActivity(act As IActivity) As IResourceActivityAuth
            Dim rauth As IResourceActivityAuth = GetResourceActivityAuth(act.ActivityID)
            If rauth Is Nothing Then
                rauth = Provider.Scheduler.Resource.AddResourceActivityAuth(_resource.ResourceID, act.ActivityID, act.UserAuth, act.InviteeAuth, act.StartEndAuth, act.NoReservFenceAuth, act.NoMaxSchedAuth)
            End If
            Return rauth
        End Function

        Private Sub InitReseourceAuth()
            hidResourceID.Value = _resource.ResourceID.ToString()

            ' fetch the reseource details from the ResourceActivityAuth table.
            _resourceActivityAuths = Provider.Scheduler.Resource.GetResourceActivityAuths(_resource.ResourceID)

            Dim allActivities As IEnumerable(Of IActivity) = CacheManager.Current.Activities()

            Dim table As Table = New Table()
            Dim columnIndex As Integer = 1
            Dim trow As TableRow = Nothing
            Dim acAll As IEnumerable(Of IAuthLevel) = Provider.Scheduler.Resource.GetAuthLevels()

            For Each act In allActivities
                InitCreateActivity(act)

                Dim lblTitle As Label = New Label()
                lblTitle.Font.Bold = True
                lblTitle.Font.Size = 12
                lblTitle.Font.Underline = True
                lblTitle.Text = act.ActivityName

                If columnIndex = 1 Then
                    trow = New TableRow()
                    table.Rows.Add(trow)
                End If

                columnIndex = columnIndex + 1
                If columnIndex = 4 Then
                    columnIndex = 1
                End If

                Dim tcell As TableCell = New TableCell With {
                    .Width = 200
                }

                tcell.Controls.Add(lblTitle)

                ' Dynamically create the number of checkbox's based on AuthLevel Table and update UI 
                'cbl.CssClass = "cbl_" + act.ActivityID.ToString()
                Dim cbl As CheckBoxList = New CheckBoxList With {
                    .ID = "cbl_" + act.ActivityID.ToString(),
                    .DataTextField = "AuthLevelName",
                    .DataValueField = "AuthLevelValue",
                    .DataSource = acAll
                }

                cbl.DataBind()
                tcell.Controls.Add(cbl)
                trow.Cells.Add(tcell)

                UpdateResourceUIFromDB(cbl, act)
            Next
        End Sub

        Private Sub UpdateResourceUIFromDB(cbl As CheckBoxList, act As IActivity)
            Dim rauth As IResourceActivityAuth = GetResourceActivityAuth(act.ActivityID)
            For i = 0 To cbl.Items.Count - 1
                'txtboxtest.Text = txtboxtest.Text + "   ,  " + cbl.Items(i).Text + ":" + cblInviteeAuths.Items(i).Value

                Dim authLevel As ClientAuthLevel = CType(cbl.Items(i).Value, ClientAuthLevel)

                Dim inviteeAuth As ClientAuthLevel = 0
                If rauth IsNot Nothing Then
                    inviteeAuth = rauth.InviteeAuth
                End If

                If (inviteeAuth And authLevel) > 0 Then
                    cbl.Items(i).Selected = True
                Else
                    cbl.Items(i).Selected = False      ' This Else block is needed as it have to update the default behaviour
                End If
            Next
        End Sub

        'Public Sub btn_Auth_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_Auth.Click
        '    UpdateDBResourceActivytAuth_With_UI_Values()
        'End Sub

        'Private Sub UpdateDBResourceActivytAuth_With_UI_Values()
        '    Dim allActivities As IList(Of Activity) = DA.Current.Query(Of Activity)().ToList()
        '    For Each act In allActivities
        '        Dim rauth As ResourceActivityAuth = GetResourceActivityAuth(act.ActivityID)

        '        Dim content As ContentPlaceHolder = DirectCast(Me.Master.FindControl("cphMain"), ContentPlaceHolder)
        '        Dim cbl As CheckBoxList = CType(panAuths.FindControl("cbl_" + act.ActivityID.ToString()), CheckBoxList)
        '        If rauth IsNot Nothing Then
        '            Dim cumlativeAuthValue As Integer = 0
        '            For i = 0 To cbl.Items.Count - 1
        '                If cbl.Items(i).Selected Then
        '                    cumlativeAuthValue = cumlativeAuthValue + Convert.ToInt32(cbl.Items(i).Value)
        '                End If
        '                'txtboxtest.Text = txtboxtest.Text + " , " + cblInviteeAuths.Items(i).Selected.ToString()
        '            Next
        '            'txtboxtest.Text = txtboxtest.Text + "[" + cumlativeAuthValue.ToString() + "] "

        '            rauth.InviteeAuth = CType(cumlativeAuthValue, ClientAuthLevel)
        '            rauth.Save()

        '            UpdateResourceUIFromDB(cbl, act)
        '        End If
        '    Next
        'End Sub

#Region " Resource Info Events and Functions "
        Private Sub DdlGranularity_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlGranularity.SelectedIndexChanged
            LoadOffset()
            LoadMinReservTime()
            LoadMaxReservTime()
            LoadGracePeriodHour()
            LoadGracePeriodMin()
            'trIPAddress.Visible = Convert.ToInt32(ddlGranularity.SelectedValue) <= 60
        End Sub

        Private Sub DdlMinReservTime_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlMinReservTime.SelectedIndexChanged
            LoadMaxReservTime()
            LoadGracePeriodHour()
            LoadGracePeriodMin()
        End Sub

        Private Sub DdlGracePeriodHour_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlGracePeriodHour.SelectedIndexChanged
            LoadGracePeriodMin()
        End Sub

        Private Sub BtnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
            Dim res As IResource = GetCurrentResource()

            ' need to check ReservFence, MaxReservTime, and MaxAlloc
            ' these are displayed in hours but saved in the db in minutes

            Dim authDurationMonths As Integer = 0
            Dim minReservationMinutes As Integer = Convert.ToInt32(ddlMinReservTime.SelectedValue)
            Dim maxReservationHours As Integer = Convert.ToInt32(ddlMaxReservation.SelectedValue)
            Dim maxSchedulableHours As Integer = Convert.ToInt32(txtMaxSchedulable.Text)
            Dim fenceHours As Integer = Convert.ToInt32(txtFence.Text)
            Dim granularityMinutes As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            Dim offsetHours = Convert.ToInt32(ddlOffset.SelectedValue)
            Dim minCancelMinutes = Convert.ToInt32(txtMinCancel.Text)
            Dim gracePeriodMinutes = (Convert.ToInt32(ddlGracePeriodHour.SelectedValue) * 60) + Convert.ToInt32(ddlGracePeriodMin.SelectedValue)
            Dim autoEndMinutes = If(String.IsNullOrEmpty(txtAutoEnd.Text), -1, Convert.ToInt32(txtAutoEnd.Text))
            Dim unloadMinutes = If(String.IsNullOrEmpty(txtUnload.Text), -1, Convert.ToInt32(txtUnload.Text))
            'Dim otfSchedTime As Integer = Convert.ToInt32(ddlOTFschedTime.SelectedValue)

            If maxSchedulableHours < maxReservationHours Then
                WebUtility.BootstrapAlert(phMessage, "danger", "Max Schedulable Hours cannot be less than Max Reservable Time.", True)
                Exit Sub
            End If

            If maxSchedulableHours > fenceHours Then
                WebUtility.BootstrapAlert(phMessage, "danger", "Max Schedulable Hours cannot be greater than Reservation Fence.", True)
                Exit Sub
            End If

            If String.IsNullOrEmpty(txtResourceName.Text) Then
                WebUtility.BootstrapAlert(phMessage, "danger", "Resource Name is required.", True)
                Exit Sub
            End If

            If String.IsNullOrEmpty(txtAuthDuration.Text) Then
                WebUtility.BootstrapAlert(phMessage, "danger", "Authorization Duration is required.", True)
                Exit Sub
            ElseIf Not Integer.TryParse(txtAuthDuration.Text, authDurationMonths) Then
                WebUtility.BootstrapAlert(phMessage, "danger", "Authorization Duration must be an integer.", True)
                Exit Sub
            ElseIf authDurationMonths <= 0 Then
                WebUtility.BootstrapAlert(phMessage, "danger", "Authorization Duration must be greater than zero.", True)
                Exit Sub
            End If

            ' Check OTF and IPAddress:
            ' If either OTFSchedTime or IPAddress is entered
            ' Then the other field and the AutoEnd field must be entered as well.
            'If granularity <= MAX_OTF_SCHED_TIME AndAlso (otfSchedTime <> -1 OrElse Not String.IsNullOrEmpty(txtIPAddress.Text.Trim())) AndAlso (otfSchedTime = -1 OrElse String.IsNullOrEmpty(txtIPAddress.Text.Trim()) OrElse String.IsNullOrEmpty(txtAutoEnd.Text.Trim())) Then
            '    ServerJScript.JSAlert(Me.Page, "Error: You must enter OTF Sched Time, IP Address and AutoEnd fields.")
            '    Exit Sub
            'End If

            Try
                Dim oldGranularityMinutes As Double = res.Granularity
                res.ResourceName = txtResourceName.Text
                res.AuthDuration = Convert.ToInt32(txtAuthDuration.Text)
                res.AuthState = chkAuthState.Checked
                res.ReservFence = Convert.ToInt32(TimeSpan.FromHours(fenceHours).TotalMinutes)
                res.Granularity = granularityMinutes
                res.Offset = offsetHours
                res.MinReservTime = minReservationMinutes
                res.MaxReservTime = Convert.ToInt32(TimeSpan.FromHours(maxReservationHours).TotalMinutes)
                res.MinCancelTime = minCancelMinutes
                res.MaxAlloc = Convert.ToInt32(TimeSpan.FromHours(maxSchedulableHours).TotalMinutes)
                res.GracePeriod = gracePeriodMinutes
                res.ResourceAutoEnd = autoEndMinutes
                res.UnloadTime = unloadMinutes
                'res.IPAddress = txtIPAddress.Text
                'res.OTFSchedTime = otfSchedTime
                res.ResourceDescription = txtDesc.Text
                res.WikiPageUrl = txtWikiPageUrl.Text
                If res.GracePeriod = 0 Then ServerJScript.JSAlert(Page, "Warning: You have set grace period to 0 for this resource.")
                UploadFileUtility.UploadImage(fileIcon, "Resource", res.ResourceID.ToString().PadLeft(6, Char.Parse("0")))
                Resources.EngineerUpdate(res)

                ' If update was successful and
                ' If Granularity changes to bigger number then, remove all future reservations
                If granularityMinutes > oldGranularityMinutes Then
                    Dim query As IEnumerable(Of IReservationItem) = SelectByResource(res.ResourceID, Date.Now, Date.Now.AddYears(100), False)
                    For Each rsv As IReservationItem In query
                        CancelReservation(rsv.ReservationID, CurrentUser.ClientID)
                        EmailOnCanceledByResource(rsv.ReservationID, CurrentUser.ClientID)
                    Next
                End If

                WebUtility.BootstrapAlert(phMessage, "success", "Resource has been modified.", True)
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try

            Redirect("ResourceConfig.aspx")
        End Sub

        Private Function SelectByResource(resourceId As Integer, sd As Date, ed As Date, includeDeleted As Boolean) As IEnumerable(Of IReservationItem)
            Return Provider.Scheduler.Reservation.SelectByResource(resourceId, sd, ed, includeDeleted)
        End Function

        Private Sub CancelReservation(reservationId As Integer, clientId As Integer)
            Provider.Scheduler.Reservation.CancelReservation(reservationId, "Cancelled due to tool granularity configuration change.", clientId)
        End Sub

        Private Sub EmailOnCanceledByResource(reservationId As Integer, clientId As Integer)
            Provider.Scheduler.Email.EmailOnCanceledByResource(reservationId, clientId)
        End Sub

        Private Sub LoadMinReservTime()
            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)

            ' Load Hours
            ddlMinReservTime.Items.Clear()
            For i As Integer = 1 To 6
                Dim minReservTime As Integer = i * granularity
                Dim hour As Integer
                Dim minute As Integer
                hour = Convert.ToInt32(Math.Floor(minReservTime / 60))
                minute = Convert.ToInt32(minReservTime Mod 60)

                Dim text As String = String.Empty
                If hour > 0 Then text += hour.ToString() + " hr "
                If minute > 0 Then text += minute.ToString() + " min"
                ddlMinReservTime.Items.Add(New ListItem(text, minReservTime.ToString()))
            Next

            Dim defaultValue As Double = _resource.MinReservTime
            Dim itemMRT As ListItem = ddlMinReservTime.Items.FindByValue(defaultValue.ToString())
            If Not itemMRT Is Nothing Then
                ddlMinReservTime.ClearSelection()
                itemMRT.Selected = True
            End If
        End Sub

        Private Sub LoadMaxReservTime()
            '                                                                             1               2   3    6   12   24   days
            Dim maxReservTimeList As Integer() = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 18, 24, 30, 36, 42, 48, 72, 144, 288, 576} 'hours
            ' the max is 576 because the max granularity is now 1440 (1440 * 24 / 60 = 576)

            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            '[2018-06-08 jg] Any granularity that is 60 minutes or less should have a maxValue of 24 hours. This was done so that some users
            '   of Acid Bench 92 can make 24 reservations, but the tool can still have a 10 minutes granularity.
            Dim maxValue As Integer = Convert.ToInt32(Math.Max(granularity, 60) * 24 / 60)
            Dim minValue As Integer = Convert.ToInt32(Math.Ceiling(Convert.ToInt32(ddlMinReservTime.SelectedValue) / 60))

            ddlMaxReservation.Items.Clear()
            For i As Integer = 0 To maxReservTimeList.Length - 1
                If maxReservTimeList(i) > maxValue Then Exit For
                If maxReservTimeList(i) >= minValue Then
                    ddlMaxReservation.Items.Add(New ListItem(maxReservTimeList(i).ToString(), maxReservTimeList(i).ToString()))
                End If
            Next

            Dim defaultValue As Double = TimeSpan.FromMinutes(_resource.MaxReservTime).TotalHours
            Dim itemMRT As ListItem = ddlMaxReservation.Items.FindByValue(defaultValue.ToString())
            If Not itemMRT Is Nothing Then
                ddlMaxReservation.ClearSelection()
                itemMRT.Selected = True
            End If
        End Sub

        Private Sub LoadGracePeriodHour()
            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            Dim maxHour As Integer = Convert.ToInt32(Math.Floor(Convert.ToInt32(ddlMinReservTime.SelectedValue) / 60))

            ddlGracePeriodHour.Items.Clear()
            Dim stepSize As Integer = Convert.ToInt32(Math.Ceiling(granularity / 60))
            Dim minValue As Integer = 0
            If granularity >= 60 Then minValue = stepSize
            For i As Integer = minValue To maxHour Step stepSize
                ddlGracePeriodHour.Items.Add(New ListItem(i.ToString(), i.ToString()))
            Next

            Dim defaultValue As Integer = TimeSpan.FromMinutes(_resource.GracePeriod).Hours
            Dim item As ListItem = ddlGracePeriodHour.Items.FindByValue(defaultValue.ToString())
            If Not item Is Nothing Then
                ddlGracePeriodHour.ClearSelection()
                item.Selected = True
            End If
        End Sub

        Private Sub LoadGracePeriodMin()
            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            Dim maxHour As Integer = Convert.ToInt32(Math.Floor(Convert.ToInt32(ddlMinReservTime.SelectedValue) / 60))

            ddlGracePeriodMin.Items.Clear()
            If Convert.ToInt32(ddlGracePeriodHour.SelectedValue) = maxHour AndAlso granularity < 60 Then
                Dim maxMin As Integer = Convert.ToInt32(ddlMinReservTime.SelectedValue) Mod 60
                For i As Integer = 0 To maxMin Step granularity
                    ddlGracePeriodMin.Items.Add(New ListItem(i.ToString(), i.ToString()))
                Next
            Else
                Dim count As Integer = Convert.ToInt32(Math.Ceiling(60 / granularity))
                For i As Integer = 0 To count - 1
                    Dim minute As Integer = granularity * i
                    ddlGracePeriodMin.Items.Add(New ListItem(minute.ToString(), minute.ToString()))
                Next
            End If

            Dim defaultValue As Double = _resource.GracePeriod
            Dim itemGPM As ListItem = ddlGracePeriodMin.Items.FindByValue(defaultValue.ToString())
            If Not itemGPM Is Nothing Then
                ddlGracePeriodMin.ClearSelection()
                itemGPM.Selected = True
            End If
        End Sub

        Private Sub LoadOffset()
            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            ddlOffset.Items.Clear()
            ddlOffset.Items.Add(New ListItem("0", "0"))

            If granularity > 60 Then
                ddlOffset.Items.Add(New ListItem("1", "1"))
            End If

            If granularity > 120 Then
                ddlOffset.Items.Add(New ListItem("2", "2"))
            End If

            Dim itemOffset As ListItem = ddlOffset.Items.FindByValue(_resource.Offset.ToString())

            If Not itemOffset Is Nothing Then
                ddlOffset.ClearSelection()
                itemOffset.Selected = True
            End If
        End Sub

        'Private Sub LoadOTFSchedTime(res As Scheduler.Resource)
        '    ddlOTFschedTime.Items.Clear()
        '    For i As Integer = res.Granularity To MAX_OTF_SCHED_TIME Step res.Granularity
        '        Dim item As New ListItem
        '        item.Value = i.ToString()
        '        If i >= 60 Then
        '            item.Text = Math.Floor(i / 60).ToString() + " hour"
        '            If (i Mod 60) > 0 Then item.Text += (i Mod 60).ToString() + " minutes"
        '        Else
        '            item.Text = i.ToString() + " minutes"
        '        End If
        '        ddlOTFschedTime.Items.Add(item)
        '    Next
        '    ddlOTFschedTime.Items.Insert(0, New ListItem("None", "-1"))

        '    Dim itemOTFSched As ListItem = ddlOTFschedTime.Items.FindByValue(res.OTFSchedTime.ToString())
        '    If Not itemOTFSched Is Nothing Then
        '        ddlOTFschedTime.ClearSelection()
        '        itemOTFSched.Selected = True
        '    End If
        '    trIPAddress.Visible = Convert.ToInt32(ddlGranularity.SelectedValue) <= 60
        'End Sub

        Private Sub LoadResource()
            ' need to check ReservFence, MaxReservTime, and MaxAlloc
            ' these are displayed in hours but saved in the db in minutes

            txtResourceName.Text = _resource.ResourceName
            txtAuthDuration.Text = _resource.AuthDuration.ToString()
            chkAuthState.Checked = _resource.AuthState
            txtFence.Text = TimeSpan.FromMinutes(_resource.ReservFence).TotalHours.ToString()
            txtMaxSchedulable.Text = TimeSpan.FromMinutes(_resource.MaxAlloc).TotalHours.ToString()
            txtMinCancel.Text = _resource.MinCancelTime.ToString()
            txtAutoEnd.Text = _resource.ResourceAutoEnd.ToString()
            txtUnload.Text = _resource.UnloadTime.ToString()
            'lblIPPrefix.Text = Properties.Current.ResourceIPPrefix
            'txtIPAddress.Text = res.IPAddress
            ddlGranularity.ClearSelection()
            Dim listItem As ListItem = ddlGranularity.Items.FindByValue(_resource.Granularity.ToString())
            If listItem IsNot Nothing Then
                listItem.Selected = True
            End If
            LoadOffset()
            LoadMinReservTime()
            LoadMaxReservTime()
            LoadGracePeriodHour()
            LoadGracePeriodMin()
            'LoadOTFSchedTime(res)
            txtDesc.Text = _resource.ResourceDescription
            txtWikiPageUrl.Text = _resource.WikiPageUrl
            UploadFileUtility.DisplayIcon(imgIcon, "Resource", _resource.ResourceID.ToString("000000"))
            phIcon.Visible = imgIcon.Visible
        End Sub
#End Region

#Region "ProcessInfo Events and Functions"
        Private Sub LoadProcessInfo()
            divProcessInfo.Attributes.Add("data-resource-id", _resource.ResourceID.ToString())
            divProcessInfo.Attributes.Add("data-ajax-url", VirtualPathUtility.ToAbsolute("~/ajax/processinfo.ashx"))
        End Sub
#End Region

    End Class
End Namespace