Imports LabScheduler.AppCode
Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Scheduler
Imports LNF.Scheduler.Data
Imports LNF.Web
Imports LNF.Web.Scheduler.Content
Imports Scheduler = LNF.Repository.Scheduler

Namespace Pages
    Public Class ResourceConfig
        Inherits SchedulerPage

        Private Const MAX_OTF_SCHED_TIME As Integer = 60

        Public Property ProcessInfoDataTable As DataTable
            Get
                If Session("ProcessInfoDataTable") Is Nothing Then
                    Return Nothing
                Else
                    Return CType(Session("ProcessInfoDataTable"), DataTable)
                End If
            End Get
            Set(value As DataTable)
                Session("ProcessInfoDataTable") = value
            End Set
        End Property

        Public Property ProcessInfoLineDataTable As DataTable
            Get
                If Session("ProcessInfoLineDataTable") Is Nothing Then
                    Return Nothing
                Else
                    Return CType(Session("ProcessInfoLineDataTable"), DataTable)
                End If
            End Get
            Set(value As DataTable)
                Session("ProcessInfoLineDataTable") = value
            End Set
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim res As IResource = GetCurrentResource()

            hidResourceID.Value = res.ResourceID.ToString()

            'If user types url directly, we have to return immediately if resDB is not loaded
            If res Is Nothing Then
                Return
            End If

            If Not res Is Nothing Then
                InitReseourceAuth(res)
                If Not IsPostBack Then
                    LoadResource(res)
                    LoadProcessInfo(res)
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

        Private Function GetResourceActivityAuth(resourceId As Integer, ByVal activityId As Integer) As Scheduler.ResourceActivityAuth
            Return DA.Current.Query(Of Scheduler.ResourceActivityAuth)().FirstOrDefault(Function(x) x.Resource.ResourceID = resourceId AndAlso x.Activity.ActivityID = activityId)
        End Function

        Private Sub InitCreateActivity(res As IResource, act As IActivity)
            Dim rauth As Scheduler.ResourceActivityAuth = GetResourceActivityAuth(res.ResourceID, act.ActivityID)
            If rauth Is Nothing Then
                rauth = New Scheduler.ResourceActivityAuth() With {
                    .Resource = DA.Current.Single(Of Scheduler.Resource)(res.ResourceID),
                    .Activity = DA.Current.Single(Of Scheduler.Activity)(act.ActivityID),
                    .UserAuth = CType(act.UserAuth, ClientAuthLevel),
                    .InviteeAuth = CType(act.InviteeAuth, ClientAuthLevel),
                    .StartEndAuth = CType(act.StartEndAuth, ClientAuthLevel),
                    .NoReservFenceAuth = CType(act.NoReservFenceAuth, ClientAuthLevel),
                    .NoMaxSchedAuth = CType(act.NoMaxSchedAuth, ClientAuthLevel)
                }
                DA.Current.Insert(rauth)
            End If
        End Sub

        Private Sub InitReseourceAuth(res As IResource)
            ' fetch the reseource details from the ResourceActivityAuth table.
            'Dim act As Activity = Activity.DataAccess.Single(20)
            Dim allActivities As IEnumerable(Of IActivity) = CacheManager.Current.Activities()
            Dim table As Table = New Table()
            Dim columnIndex As Integer = 1
            Dim trow As TableRow = Nothing
            Dim acAll As IList(Of Scheduler.AuthLevel) = DA.Current.Query(Of Scheduler.AuthLevel)().ToList() 'Search(Function(x) x.Authorizable = 1)
            For Each act In allActivities
                InitCreateActivity(res, act)
                Dim lblTitle As Label = New Label()
                'lblTitle.ForeColor = System.Drawing.Color.Red
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

                UpdateResourceUIFromDB(cbl, res, act)
            Next
            'PHauths.Controls.Add(table)
            'panAuths.Controls.Add(table)

        End Sub

        Private Sub UpdateResourceUIFromDB(cbl As CheckBoxList, res As IResource, act As IActivity)
            Dim rauth As Scheduler.ResourceActivityAuth = GetResourceActivityAuth(res.ResourceID, act.ActivityID)
            For i = 0 To cbl.Items.Count - 1
                'txtboxtest.Text = txtboxtest.Text + "   ,  " + cbl.Items(i).Text + ":" + cblInviteeAuths.Items(i).Value

                Dim authLevel As ClientAuthLevel = CType(cbl.Items(i).Value, ClientAuthLevel)
                If (rauth.InviteeAuth And authLevel) > 0 Then
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
            Dim res As IResource = GetCurrentResource()

            LoadOffset(res)
            LoadMinReservTime(res)
            LoadMaxReservTime(res)
            LoadGracePeriodHour(res)
            LoadGracePeriodMin(res)
            'trIPAddress.Visible = Convert.ToInt32(ddlGranularity.SelectedValue) <= 60
        End Sub

        Private Sub DdlMinReservTime_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlMinReservTime.SelectedIndexChanged
            Dim res As IResource = GetCurrentResource()

            LoadMaxReservTime(res)
            LoadGracePeriodHour(res)
            LoadGracePeriodMin(res)
        End Sub

        Private Sub DdlGracePeriodHour_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlGracePeriodHour.SelectedIndexChanged
            Dim res As IResource = GetCurrentResource()
            LoadGracePeriodMin(res)
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
                ResourceUtility.EngineerUpdate(res)

                ' If update was successful and
                ' If Granularity changes to bigger number then, remove all future reservations
                If granularityMinutes > oldGranularityMinutes Then
                    Dim query As IEnumerable(Of IReservation) = Provider.Scheduler.Reservation.SelectByResource(res.ResourceID, Date.Now, Date.Now.AddYears(100), False)
                    For Each rsv As IReservation In query
                        Provider.Scheduler.Reservation.CancelReservation(rsv.ReservationID, CurrentUser.ClientID)
                        Provider.EmailManager.EmailOnCanceledByResource(rsv, CurrentUser.ClientID)
                    Next
                End If

                WebUtility.BootstrapAlert(phMessage, "success", "Resource has been modified.", True)
            Catch ex As Exception
                Session("ErrorMessage") = ex.Message
            End Try

            Redirect("ResourceConfig.aspx")
        End Sub

        Private Sub LoadMinReservTime(res As IResource)
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

            Dim defaultValue As Double = res.MinReservTime
            Dim itemMRT As ListItem = ddlMinReservTime.Items.FindByValue(defaultValue.ToString())
            If Not itemMRT Is Nothing Then
                ddlMinReservTime.ClearSelection()
                itemMRT.Selected = True
            End If
        End Sub

        Private Sub LoadMaxReservTime(res As IResource)
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

            Dim defaultValue As Double = TimeSpan.FromMinutes(res.MaxReservTime).TotalHours
            Dim itemMRT As ListItem = ddlMaxReservation.Items.FindByValue(defaultValue.ToString())
            If Not itemMRT Is Nothing Then
                ddlMaxReservation.ClearSelection()
                itemMRT.Selected = True
            End If
        End Sub

        Private Sub LoadGracePeriodHour(res As IResource)
            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            Dim maxHour As Integer = Convert.ToInt32(Math.Floor(Convert.ToInt32(ddlMinReservTime.SelectedValue) / 60))

            ddlGracePeriodHour.Items.Clear()
            Dim stepSize As Integer = Convert.ToInt32(Math.Ceiling(granularity / 60))
            Dim minValue As Integer = 0
            If granularity >= 60 Then minValue = stepSize
            For i As Integer = minValue To maxHour Step stepSize
                ddlGracePeriodHour.Items.Add(New ListItem(i.ToString(), i.ToString()))
            Next

            Dim defaultValue As Integer = TimeSpan.FromMinutes(res.GracePeriod).Hours
            Dim item As ListItem = ddlGracePeriodHour.Items.FindByValue(defaultValue.ToString())
            If Not item Is Nothing Then
                ddlGracePeriodHour.ClearSelection()
                item.Selected = True
            End If
        End Sub

        Private Sub LoadGracePeriodMin(res As IResource)
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

            Dim defaultValue As Double = res.GracePeriod
            Dim itemGPM As ListItem = ddlGracePeriodMin.Items.FindByValue(defaultValue.ToString())
            If Not itemGPM Is Nothing Then
                ddlGracePeriodMin.ClearSelection()
                itemGPM.Selected = True
            End If
        End Sub

        Private Sub LoadOffset(res As IResource)
            Dim granularity As Integer = Convert.ToInt32(ddlGranularity.SelectedValue)
            ddlOffset.Items.Clear()
            ddlOffset.Items.Add(New ListItem("0", "0"))

            If granularity > 60 Then
                ddlOffset.Items.Add(New ListItem("1", "1"))
            End If

            If granularity > 120 Then
                ddlOffset.Items.Add(New ListItem("2", "2"))
            End If

            Dim itemOffset As ListItem = ddlOffset.Items.FindByValue(res.Offset.ToString())

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

        Private Sub LoadResource(res As IResource)
            ' need to check ReservFence, MaxReservTime, and MaxAlloc
            ' these are displayed in hours but saved in the db in minutes

            txtResourceName.Text = res.ResourceName
            txtAuthDuration.Text = res.AuthDuration.ToString()
            chkAuthState.Checked = res.AuthState
            txtFence.Text = TimeSpan.FromMinutes(res.ReservFence).TotalHours.ToString()
            txtMaxSchedulable.Text = TimeSpan.FromMinutes(res.MaxAlloc).TotalHours.ToString()
            txtMinCancel.Text = res.MinCancelTime.ToString()
            txtAutoEnd.Text = res.ResourceAutoEnd.ToString()
            txtUnload.Text = res.UnloadTime.ToString()
            'lblIPPrefix.Text = Properties.Current.ResourceIPPrefix
            'txtIPAddress.Text = res.IPAddress
            ddlGranularity.ClearSelection()
            Dim listItem As ListItem = ddlGranularity.Items.FindByValue(res.Granularity.ToString())
            If listItem IsNot Nothing Then
                listItem.Selected = True
            End If
            LoadOffset(res)
            LoadMinReservTime(res)
            LoadMaxReservTime(res)
            LoadGracePeriodHour(res)
            LoadGracePeriodMin(res)
            'LoadOTFSchedTime(res)
            txtDesc.Text = res.ResourceDescription
            txtWikiPageUrl.Text = res.WikiPageUrl
            UploadFileUtility.DisplayIcon(imgIcon, "Resource", res.ResourceID.ToString("000000"))
            phIcon.Visible = imgIcon.Visible
        End Sub
#End Region

#Region " ProcessInfo Events and Functions "
        ' ProcessInfo OnItemDataBound
        Private Sub DgProcessInfo_ItemDataBound(sender As Object, e As DataGridItemEventArgs) Handles dgProcessInfo.ItemDataBound
            Try

                If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem OrElse e.Item.ItemType = ListItemType.SelectedItem Then
                    ' Item, Alternating Item or SelectedItem
                    Dim di As New DataItemHelper(e.Item.DataItem)

                    ' Display ProcessInfo Info
                    CType(e.Item.FindControl("lblPIName"), Label).Text = di("ProcessInfoName").AsString
                    CType(e.Item.FindControl("lblParamName"), Label).Text = di("ParamName").AsString
                    CType(e.Item.FindControl("lblValueName"), Label).Text = di("ValueName").AsString
                    If di("Special") Is DBNull.Value Then
                        CType(e.Item.FindControl("lblSpecial"), Label).Text = String.Empty
                    Else
                        CType(e.Item.FindControl("lblSpecial"), Label).Text = di("Special").AsString
                    End If
                    CType(e.Item.FindControl("lblAllowNone"), Label).Text = di("AllowNone").AsString
                    CType(e.Item.FindControl("lblRequireValue"), Label).Text = di("RequireValue").AsString
                    CType(e.Item.FindControl("lblRequireSelection"), Label).Text = di("RequireSelection").AsString
                    CType(e.Item.FindControl("ibtnDelete"), ImageButton).Attributes.Add("onclick", "return confirm('Are you sure you want to delete this Process Info?');")

                    ' Display ProcessInfoLine datagrid
                    If di("ProcessInfoID").AsInt32 = -1 AndAlso e.Item.ItemIndex > 0 Then
                        ' Set Previous record button to show collapse
                        Dim ibtnPrevExpand As ImageButton = CType(dgProcessInfo.Items(e.Item.ItemIndex - 1).FindControl("ibtnExpand"), ImageButton)
                        ibtnPrevExpand.ImageUrl = "~/images/collapse.gif"
                        ibtnPrevExpand.CommandArgument = "Collapse"

                        ' Set current record buttons to be invisible
                        CType(e.Item.FindControl("ibtnExpand"), ImageButton).Visible = False
                        CType(e.Item.FindControl("ibtnUp"), ImageButton).Visible = False
                        CType(e.Item.FindControl("ibtnDown"), ImageButton).Visible = False
                        e.Item.BackColor = System.Drawing.Color.White

                        ' Make inner datagrid visible
                        Dim numCells As Integer = e.Item.Cells.Count
                        e.Item.Cells(0).ColumnSpan = numCells - 1
                        For i As Integer = numCells - 1 To 1 Step -1
                            e.Item.Cells(i).Visible = False '.RemoveAt(i)
                        Next

                        ' Add events for ProcessInfoLine datagrid
                        Dim ProcessInfoID As Integer = Convert.ToInt32(dgProcessInfo.Items(e.Item.ItemIndex - 1).Cells(1).Text)
                        Dim dgProcessInfoLine As DataGrid = CType(e.Item.FindControl("dgProcessInfoLine"), DataGrid)
                        LoadProcessInfoLine(ProcessInfoID, dgProcessInfoLine)
                    End If

                ElseIf e.Item.ItemType = ListItemType.EditItem Then
                    ' Edit Item
                    Dim di As New DataItemHelper(e.Item.DataItem)

                    ' Display ProcessInfo for editing
                    CType(e.Item.FindControl("txtPIName"), TextBox).Text = di("ProcessInfoName").AsString
                    CType(e.Item.FindControl("txtParamName"), TextBox).Text = di("ParamName").AsString
                    CType(e.Item.FindControl("txtValueName"), TextBox).Text = di("ValueName").AsString
                    If di("Special") Is DBNull.Value Then
                        CType(e.Item.FindControl("txtSpecial"), TextBox).Text = String.Empty
                    Else
                        CType(e.Item.FindControl("txtSpecial"), TextBox).Text = di("Special").AsString
                    End If
                    CType(e.Item.FindControl("chkAllowNone"), CheckBox).Checked = di("AllowNone").AsBoolean
                    CType(e.Item.FindControl("chkRequireValue"), CheckBox).Checked = di("RequireValue").AsBoolean
                    CType(e.Item.FindControl("chkRequireSelection"), CheckBox).Checked = di("RequireSelection").AsBoolean
                End If
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "ProcessInfo ItemDataBound Error: " + ex.Message, True)
            End Try
        End Sub

        ' ProcessInfo OnItemCommand
        Private Sub DgProcessInfo_ItemCommand(source As Object, e As DataGridCommandEventArgs) Handles dgProcessInfo.ItemCommand
            Try
                Dim res As IResource = GetCurrentResource()

                Select Case e.CommandName
                    Case "Expand"       ' Expand ProcessInfo row to display ProcessInfoLine datagrid
                        Dim bExpand As Boolean = True
                        Dim ibtnExpand As ImageButton = CType(e.Item.FindControl("ibtnExpand"), ImageButton)
                        If ibtnExpand.CommandArgument = "Collapse" Then bExpand = False

                        ' Collapse all rows in the datagrid, Handles collapse
                        Dim dr As DataRow = ProcessInfoDataTable.Rows.Find(-1)
                        Dim HasChild As Boolean = False
                        If Not dr Is Nothing Then
                            HasChild = True
                            ProcessInfoDataTable.Rows.Remove(dr)
                        End If
                        For i As Integer = 0 To dgProcessInfo.Items.Count - 1
                            Dim dgProcessInfoLine As DataGrid = CType(e.Item.FindControl("dgProcessInfoLine"), DataGrid)
                            If dgProcessInfoLine.Visible Then
                                dgProcessInfoLine.Visible = False
                                ibtnExpand = CType(dgProcessInfo.Items(i).FindControl("ibtnExpand"), ImageButton)
                                ibtnExpand.ImageUrl = "~/images/expand.gif"
                                ibtnExpand.CommandArgument = "Expand"
                            End If
                        Next

                        ' Set up row for displaying ProcessInfoLine datagrid
                        If bExpand Then
                            Dim drExpand As DataRow = ProcessInfoDataTable.NewRow()
                            drExpand("ProcessInfoID") = -1
                            drExpand("ResourceID") = -1
                            drExpand("ProcessInfoName") = ""
                            drExpand("ParamName") = ""
                            drExpand("ValueName") = ""
                            drExpand("Special") = 0
                            drExpand("AllowNone") = 0
                            drExpand("RequireValue") = 1
                            drExpand("RequireSelection") = 1
                            drExpand("Order") = e.Item.Cells(2).Text
                            drExpand("MaxAllowed") = 1
                            ProcessInfoDataTable.Rows.Add(drExpand)
                            ProcessInfoDataTable.AcceptChanges()

                            If HasChild AndAlso e.Item.ItemIndex > 0 Then
                                dgProcessInfo.SelectedIndex = e.Item.ItemIndex - 1
                            Else
                                dgProcessInfo.SelectedIndex = e.Item.ItemIndex
                            End If
                        End If

                        ProcessInfoDataTable.DefaultView.Sort = "Order ASC, ProcessInfoID DESC"
                        dgProcessInfo.DataSource = ProcessInfoDataTable
                        dgProcessInfo.DataBind()
                    Case "MoveUp" 'Move ProcessInfo up the list
                        If e.Item.ItemIndex = 0 Then Exit Sub
                        Dim drCurPI As DataRow = ProcessInfoDataTable.Select(String.Format("ProcessInfoID = {0}", e.Item.Cells(1).Text))(0)
                        Dim drPrevPI As DataRow = ProcessInfoDataTable.Select(String.Format("ProcessInfoID = {0}", dgProcessInfo.Items(e.Item.ItemIndex - 1).Cells(1).Text))(0)
                        Dim prevOrder As Integer = Convert.ToInt32(drPrevPI("Order"))
                        drPrevPI("Order") = drCurPI("Order")
                        drCurPI("Order") = prevOrder
                        UpdateProcessInfo()
                    Case "MoveDown" 'Move ProcessInfo down the list
                        If e.Item.ItemIndex = dgProcessInfo.Items.Count - 1 Then Exit Sub
                        Dim drCurPI As DataRow = ProcessInfoDataTable.Select(String.Format("ProcessInfoID = {0}", e.Item.Cells(1).Text))(0)
                        Dim drNextPI As DataRow = ProcessInfoDataTable.Select(String.Format("ProcessInfoID = {0}", dgProcessInfo.Items(e.Item.ItemIndex + 1).Cells(1).Text))(0)
                        Dim nextOrder As Integer = Convert.ToInt32(drNextPI("Order"))
                        drNextPI("Order") = drCurPI("Order")
                        drCurPI("Order") = nextOrder
                        UpdateProcessInfo()
                    Case "Insert" 'Add new ProcessInfo
                        ' Error Checking
                        Dim txtNewPIName As TextBox = CType(e.Item.FindControl("txtNewPIName"), TextBox)
                        Dim txtNewParamName As TextBox = CType(e.Item.FindControl("txtNewParamName"), TextBox)
                        Dim txtNewValueName As TextBox = CType(e.Item.FindControl("txtNewValueName"), TextBox)
                        Dim txtNewSpecial As TextBox = CType(e.Item.FindControl("txtNewSpecial"), TextBox)
                        Dim chkNewAllowNone As CheckBox = CType(e.Item.FindControl("chkNewAllowNone"), CheckBox)
                        Dim chkNewRequireValue As CheckBox = CType(e.Item.FindControl("chkNewRequireValue"), CheckBox)
                        Dim chkNewRequireSelection As CheckBox = CType(e.Item.FindControl("chkNewRequireSelection"), CheckBox)

                        If String.IsNullOrEmpty(txtNewPIName.Text) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Please enter Process Info Name.", True)
                            Exit Select
                        End If

                        If String.IsNullOrEmpty(txtNewParamName.Text) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Please enter Param Name.", True)
                            Exit Select
                        End If

                        If String.IsNullOrEmpty(txtNewValueName.Text) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Please enter Value Name.", True)
                            Exit Select
                        End If

                        ' Update dataset
                        Dim drNewProcessInfo As DataRow = ProcessInfoDataTable.NewRow()
                        drNewProcessInfo("ResourceID") = res.ResourceID
                        drNewProcessInfo("ProcessInfoName") = txtNewPIName.Text
                        drNewProcessInfo("ParamName") = txtNewParamName.Text
                        drNewProcessInfo("ValueName") = txtNewValueName.Text
                        drNewProcessInfo("Special") = If(String.IsNullOrEmpty(txtNewSpecial.Text), String.Empty, txtNewSpecial.Text)
                        drNewProcessInfo("AllowNone") = chkNewAllowNone.Checked
                        drNewProcessInfo("RequireValue") = chkNewRequireValue.Checked
                        drNewProcessInfo("RequireSelection") = chkNewRequireSelection.Checked
                        drNewProcessInfo("Order") = 0
                        If ProcessInfoDataTable.Rows.Count > 0 Then
                            drNewProcessInfo("Order") = Convert.ToInt32(ProcessInfoDataTable.Rows(ProcessInfoDataTable.Rows.Count - 1)("Order")) + 1
                        End If
                        drNewProcessInfo("MaxAllowed") = 1
                        ProcessInfoDataTable.Rows.Add(drNewProcessInfo)
                        UpdateProcessInfo()
                    Case "Edit"         ' Edit ProcessInfo
                        dgProcessInfo.EditItemIndex = e.Item.ItemIndex
                        dgProcessInfo.ShowFooter = False

                        ProcessInfoDataTable.DefaultView.Sort = "Order ASC, ProcessInfoID DESC"
                        dgProcessInfo.DataSource = ProcessInfoDataTable
                        dgProcessInfo.DataBind()
                    Case "Update"       ' Update ProcessInfo
                        ' Error Checking
                        Dim txtPIName As TextBox = CType(e.Item.FindControl("txtPIName"), TextBox)
                        Dim txtParamName As TextBox = CType(e.Item.FindControl("txtParamName"), TextBox)
                        Dim txtValueName As TextBox = CType(e.Item.FindControl("txtValueName"), TextBox)
                        Dim txtSpecial As TextBox = CType(e.Item.FindControl("txtSpecial"), TextBox)
                        Dim chkAllowNone As CheckBox = CType(e.Item.FindControl("chkAllowNone"), CheckBox)
                        Dim chkRequireValue As CheckBox = CType(e.Item.FindControl("chkRequireValue"), CheckBox)
                        Dim chkRequireSelection As CheckBox = CType(e.Item.FindControl("chkRequireSelection"), CheckBox)

                        If String.IsNullOrEmpty(txtPIName.Text) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Please enter Process Info Name.", True)
                            Exit Select
                        End If

                        If String.IsNullOrEmpty(txtParamName.Text) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Please enter Param Name.", True)
                            Exit Select
                        End If

                        If String.IsNullOrEmpty(txtValueName.Text) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Please enter Value Name.", True)
                            Exit Select
                        End If

                        ' Update dataset
                        Dim drProcessInfo As DataRow = ProcessInfoDataTable.Select(String.Format("ProcessInfoID = {0}", e.Item.Cells(1).Text))(0)
                        drProcessInfo("ResourceID") = res.ResourceID
                        drProcessInfo("ProcessInfoName") = txtPIName.Text
                        drProcessInfo("ParamName") = txtParamName.Text
                        drProcessInfo("ValueName") = txtValueName.Text
                        drProcessInfo("Special") = If(String.IsNullOrEmpty(txtSpecial.Text), String.Empty, txtSpecial.Text)
                        drProcessInfo("AllowNone") = chkAllowNone.Checked
                        drProcessInfo("RequireValue") = chkRequireValue.Checked
                        drProcessInfo("RequireSelection") = chkRequireSelection.Checked
                        drProcessInfo("MaxAllowed") = 1
                        dgProcessInfo.EditItemIndex = -1
                        dgProcessInfo.ShowFooter = True
                        UpdateProcessInfo()
                    Case "Cancel"       ' Cancels the action to udpate ProcessInfo
                        dgProcessInfo.EditItemIndex = -1
                        dgProcessInfo.ShowFooter = True

                        ProcessInfoDataTable.DefaultView.Sort = "Order ASC, ProcessInfoID DESC"
                        dgProcessInfo.DataSource = ProcessInfoDataTable
                        dgProcessInfo.DataBind()
                    Case "Delete"       ' Delete ProcessInfo
                        Dim drProcessInfo As DataRow = ProcessInfoDataTable.Select(String.Format("ProcessInfoID = {0}", e.Item.Cells(1).Text))(0)
                        drProcessInfo.Delete()

                        Dim dr As DataRow = ProcessInfoDataTable.Rows.Find(-1)
                        If Not dr Is Nothing Then
                            ProcessInfoDataTable.Rows.Remove(dr)
                        End If

                        UpdateProcessInfo()
                End Select
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("ProcessInfo ItemCommand Error: {0}", ex.Message), True)
            End Try
        End Sub

        ' Loads Process Info
        Private Sub LoadProcessInfo(res As IResource)
            Try
                ProcessInfoDataTable = ProcessInfoData.SelectProcessInfo(res.ResourceID)

                ProcessInfoDataTable.DefaultView.Sort = "Order ASC, ProcessInfoID DESC"
                dgProcessInfo.DataSource = ProcessInfoDataTable
                dgProcessInfo.DataBind()
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("LoadProcessInfo Error: {0}", ex.Message), True)
            End Try
        End Sub

        ' Updates Process Info 
        Private Sub UpdateProcessInfo()
            Try
                ProcessInfoData.Update(ProcessInfoDataTable)

                ProcessInfoDataTable.DefaultView.Sort = "Order ASC, ProcessInfoID DESC"
                dgProcessInfo.DataSource = ProcessInfoDataTable
                dgProcessInfo.DataBind()
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("UpdateProcessInfo Error: {0}", ex.Message), True)
            End Try
        End Sub
#End Region

#Region " ProcessInfoLine Events and Functions "
        ' ProcessInfoLine OnItemDataBound
        Protected Sub DgProcessInfoLine_ItemDataBound(sender As Object, e As DataGridItemEventArgs)
            Try
                If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                    Dim di As New DataItemHelper(e.Item.DataItem)
                    CType(e.Item.FindControl("lblParam"), Label).Text = di("Param").AsString
                    CType(e.Item.FindControl("lblMinVal"), Label).Text = di("MinValue").AsString
                    CType(e.Item.FindControl("lblMaxVal"), Label).Text = di("MaxValue").AsString
                    CType(e.Item.FindControl("ibtnPILDelete"), ImageButton).Attributes.Add("onclick", "return confirm('Are you sure you want to delete this Process Info Line?');")
                ElseIf e.Item.ItemType = ListItemType.EditItem Then
                    Dim di As New DataItemHelper(e.Item.DataItem)
                    CType(e.Item.FindControl("txtParam"), TextBox).Text = di("Param").AsString
                    CType(e.Item.FindControl("txtMinVal"), TextBox).Text = di("MinValue").AsString
                    CType(e.Item.FindControl("txtMaxVal"), TextBox).Text = di("MaxValue").AsString
                End If
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("ProcessInfoLine ItemDataBound Error: {0}", ex.Message), True)
            End Try
        End Sub

        ' ProcessInfoLine OnItemCommand
        Protected Sub DgProcessInfoLine_ItemCommand(source As Object, e As DataGridCommandEventArgs)
            Try
                If ProcessInfoLineDataTable Is Nothing Then
                    Redirect("ResourceConfig.aspx", False)
                    Return
                End If

                Dim dgProcessInfoLine As DataGrid = CType(source, DataGrid)
                Select Case e.CommandName
                    Case "Insert" 'Insert new ProcessInfoLine
                        ' Error Checking
                        Dim txtNewParam As TextBox = CType(e.Item.FindControl("txtNewParam"), TextBox)

                        If String.IsNullOrEmpty(txtNewParam.Text.Trim()) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Error: Please enter Parameter name.", True)
                            Exit Select
                        End If

                        Dim minValue, maxValue As Double

                        If Not Double.TryParse(CType(e.Item.FindControl("txtNewMinVal"), TextBox).Text, minValue) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Error: Please enter a floating point number for Min Value.", True)
                            Exit Select
                        End If

                        If Not Double.TryParse(CType(e.Item.FindControl("txtNewMaxVal"), TextBox).Text, maxValue) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Error: Please enter a floating point number for Max Value.", True)
                            Exit Select
                        End If

                        ' Update Dataset
                        Dim drPIL As DataRow = ProcessInfoLineDataTable.NewRow()
                        drPIL("ProcessInfoID") = dgProcessInfo.SelectedItem.Cells(1).Text
                        drPIL("Param") = txtNewParam.Text
                        drPIL("MinValue") = minValue
                        drPIL("MaxValue") = maxValue
                        drPIL("ProcessInfoLineParamID") = 0
                        ProcessInfoLineDataTable.Rows.Add(drPIL)
                        UpdateProcessInfoLine(dgProcessInfoLine)
                    Case "Edit" 'Edit ProcessInfoLine
                        dgProcessInfoLine.EditItemIndex = e.Item.ItemIndex
                        ProcessInfoLineDataTable.DefaultView.Sort = "Param"
                        dgProcessInfoLine.DataSource = ProcessInfoLineDataTable
                        dgProcessInfoLine.DataBind()
                    Case "Update" 'Update ProcessInfoLine
                        ' Error Checking
                        Dim txtParam As TextBox = CType(e.Item.FindControl("txtParam"), TextBox)

                        If String.IsNullOrEmpty(txtParam.Text.Trim()) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Error: Please enter Parameter name.", True)
                            Exit Select
                        End If

                        Dim minValue, maxValue As Double

                        If Not Double.TryParse(CType(e.Item.FindControl("txtMinVal"), TextBox).Text, minValue) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Error: Please enter a floating point number for Min Value.", True)
                            Exit Select
                        End If

                        If Not Double.TryParse(CType(e.Item.FindControl("txtMaxVal"), TextBox).Text, maxValue) Then
                            WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", "Error: Please enter a floating point number for Max Value.", True)
                            Exit Select
                        End If

                        ' Update Dataset
                        Dim drPIL As DataRow = ProcessInfoLineDataTable.Select(String.Format("ProcessInfoLineID = {0}", e.Item.Cells(0).Text))(0)
                        drPIL("Param") = txtParam.Text
                        drPIL("MinValue") = minValue
                        drPIL("MaxValue") = maxValue
                        dgProcessInfoLine.EditItemIndex = -1
                        UpdateProcessInfoLine(dgProcessInfoLine)
                    Case "Cancel"       ' Cancels the action to update ProcessInfoLine
                        dgProcessInfoLine.EditItemIndex = -1
                        ProcessInfoLineDataTable.DefaultView.Sort = "Param"
                        dgProcessInfoLine.DataSource = ProcessInfoLineDataTable
                        dgProcessInfoLine.DataBind()
                    Case "Delete"       ' Delete ProcessInfoLine
                        Dim drPIL As DataRow = ProcessInfoLineDataTable.Select(String.Format("ProcessInfoLineID = {0}", e.Item.Cells(0).Text))(0)
                        drPIL.Delete()
                        UpdateProcessInfoLine(dgProcessInfoLine)
                End Select
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("ProcessInfoLine ItemCommand Error: {0}", ex.Message), True)
            End Try
        End Sub

        ' Loads Process Info Line
        Private Sub LoadProcessInfoLine(ByVal processInfoId As Integer, ByRef dgProcessInfoLine As DataGrid)
            Try
                ProcessInfoLineDataTable = ProcessInfoLineData.SelectByProcessInfo(processInfoId)
                ProcessInfoLineDataTable.DefaultView.Sort = "Param"
                dgProcessInfoLine.DataSource = ProcessInfoLineDataTable
                dgProcessInfoLine.DataBind()
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("LoadProcessInfoLine Error: {0}", ex.Message), True)
            End Try
        End Sub

        ' Updates Process Info Line
        Private Sub UpdateProcessInfoLine(ByRef dgProcessInfoLine As DataGrid)
            Try
                ProcessInfoLineData.Update(ProcessInfoLineDataTable)
                ProcessInfoLineDataTable.DefaultView.Sort = "Param"
                dgProcessInfoLine.DataSource = ProcessInfoLineDataTable
                dgProcessInfoLine.DataBind()
            Catch ex As Exception
                WebUtility.BootstrapAlert(phProcessInfoMessage, "danger", String.Format("UpdateProcessInfoLine Error: {0}", ex.Message), True)
            End Try
        End Sub
#End Region

    End Class
End Namespace