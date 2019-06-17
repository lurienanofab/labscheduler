Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Repository.Scheduler
Imports LNF.Scheduler
Imports LNF.Scheduler.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class Preference
        Inherits SchedulerPage

        Private _workDayCheckboxes As HtmlInputCheckBox()

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load

            _workDayCheckboxes = {
                chkWorkingDaysSun,
                chkWorkingDaysMon,
                chkWorkingDaysTue,
                chkWorkingDaysWed,
                chkWorkingDaysThu,
                chkWorkingDaysFri,
                chkWorkingDaysSat
            }

            If IsPostBack Then
                'setDB = CType(Session("SetDV"), ClientSettingDB)
            Else
                LoadBuildings()
                'LoadLabs()
                LoadResourceClients()
                LoadHours(ddlBeginHour)
                LoadHours(ddlEndHour)
                InitSettings()
            End If
        End Sub

        Private Sub LoadBuildings()
            ddlBuilding.DataValueField = "BuildingID"
            ddlBuilding.DataTextField = "BuildingName"
            ddlBuilding.DataSource = BuildingDB.SelectAllDataTable()
            ddlBuilding.DataBind()
            ddlBuilding.Items.Insert(0, New ListItem("-- Unspecified --", "-1"))
        End Sub

        Private Sub LoadLabs()
            If ddlBuilding.Items.Count = 0 Then Exit Sub
            Dim dbLab As New LabDB
            ddlLab.DataValueField = "LabID"
            ddlLab.DataTextField = "LabName"
            ddlLab.DataSource = dbLab.SelectByBuilding(Convert.ToInt32(ddlBuilding.SelectedValue))
            ddlLab.DataBind()
            ddlLab.Items.Insert(0, New ListItem("-- Unspecified --", "-1"))
        End Sub

        Private Sub LoadResourceClients()
            Dim dtResources = ResourceClientData.SelectByClient(CurrentUser.ClientID)
            dtResources.DefaultView.RowFilter = "ClientID <> -1"
            dgResources.DataSource = dtResources.DefaultView
            dgResources.DataBind()
            If dgResources.Items.Count > 0 Then
                dgResources.Visible = True
                phResourcesNoData.Visible = False
            Else
                dgResources.Visible = False
                phResourcesNoData.Visible = True
            End If

            '2009-09-18 for Practice Reservation
            dtResources.DefaultView.RowFilter = "AuthLevel = 16"
            dgResourcePractice.DataSource = dtResources.DefaultView
            dgResourcePractice.DataBind()
            If dgResourcePractice.Items.Count > 0 Then
                dgResourcePractice.Visible = True
                phResourcePracticeNoData.Visible = False
            Else
                dgResourcePractice.Visible = False
                phResourcePracticeNoData.Visible = True
            End If
        End Sub

        Private Sub LoadHours(ddl As DropDownList)
            ddl.Items.Clear()
            For i As Integer = 0 To 23
                Dim AMPM As String = If(i < 12, "AM", "PM")
                Dim Text As String = String.Format("{0} {1}", If(i = 0 OrElse i = 12, 12, i Mod 12), AMPM)
                ddl.Items.Add(New ListItem(Text, i.ToString()))
            Next
        End Sub

        Private Sub InitSettings()
            Dim clientId As Integer = CurrentUser.ClientID

            Dim cs As ClientSetting = DA.Current.Single(Of ClientSetting)(clientId)

            If cs Is Nothing Then
                cs = New ClientSetting() With {.ClientID = clientId}
                DA.Current.Insert(cs)
            End If

            ddlBuilding.ClearSelection()
            ddlLab.ClearSelection()

            If cs.BuildingID.HasValue Then
                ddlBuilding.SelectedValue = cs.BuildingID.Value.ToString()
            End If

            LoadLabs()

            If cs.LabID.HasValue Then
                ddlLab.SelectedValue = cs.LabID.Value.ToString()
            End If

            rdoDefaultViewDay.Checked = cs.GetDefaultViewOrDefault() = ViewType.DayView
            rdoDefaultViewWeek.Checked = cs.GetDefaultViewOrDefault() = ViewType.WeekView

            ' Default Working Hours
            If cs.GetBeginHourOrDefault() = 0 AndAlso cs.GetEndHourOrDefault() = 24 Then
                rdoHoursAllDay.Checked = True
                rdoHoursRange.Checked = False
            Else
                rdoHoursAllDay.Checked = False
                rdoHoursRange.Checked = True
                ddlBeginHour.SelectedValue = cs.GetBeginHourOrDefault().ToString()
                ddlEndHour.SelectedValue = cs.GetEndHourOrDefault().ToString()
            End If
            ddlBeginHour.Enabled = rdoHoursRange.Checked
            ddlEndHour.Enabled = rdoHoursRange.Checked
            'rblHours_SelectedIndexChanged(Nothing, Nothing)

            ' Default Working Days
            SetWorkDays(cs.WorkDays)

            chkCreateReserv.Checked = cs.GetEmailCreateReservOrDefault()
            chkModifyReserv.Checked = cs.GetEmailModifyReservOrDefault()
            chkDeleteReserv.Checked = cs.GetEmailDeleteReservOrDefault()
            chkInviteReserv.Checked = cs.GetEmailInvitedOrDefault()

            GetAccountOrdering()
            GetShowTreeviewImages()
        End Sub

        Protected Sub GetAccountOrdering()
            'Dim cp As ClientPreference = ClientPreferenceUtility.Find(ClientUtility.CurrentUser, "common")
            'Dim orderedAccounts As IList(Of Account) = DataUtility.OrderAccountsByUserPreference(cp)
            Dim util As New ClientPreferenceUtility(Provider)
            Dim orderedAccounts As IList(Of IAccount) = util.OrderAccountsByUserPreference(CurrentUser)
            Dim resultIdList As New List(Of String)
            Dim resultNamesList As New List(Of String)

            For Each acct As Account In orderedAccounts
                resultIdList.Add(acct.AccountID.ToString())
                resultNamesList.Add(acct.Name)
            Next

            lblAccounts.Text = String.Join(",", resultIdList)
            lblAccountsNames.Text = String.Join(":", resultNamesList)
        End Sub

        Protected Sub SetAccountOrdering()
            'Dim cp As ClientPreference = ClientPreferenceUtility.Find(ClientUtility.CurrentUser, "common")
            'cp.SetPreference("account-order", hidAccountsResult.Value)
            Dim cs As ClientSetting = DA.Current.Single(Of ClientSetting)(CurrentUser.ClientID)
            cs.AccountOrder = hidAccountsResult.Value
        End Sub

        Protected Sub GetShowTreeviewImages()
            Dim cp As ClientPreference = ClientPreferenceUtility.Find(CurrentUser.ClientID, "scheduler")
            chkShowTreeViewImages.Checked = cp.GetPreference("show-treeview-images", False)
        End Sub

        Protected Sub SetShowTreeviewImages()
            Dim cp As ClientPreference = ClientPreferenceUtility.Find(CurrentUser.ClientID, "scheduler")
            cp.SetPreference("show-treeview-images", chkShowTreeViewImages.Checked)
        End Sub

        Private Function GetWorkDays() As IEnumerable(Of Integer)
            Dim result As New List(Of Integer)
            For Each wd In _workDayCheckboxes
                If wd.Checked Then
                    result.Add(1)
                Else
                    result.Add(0)
                End If
            Next
            Return result
        End Function

        Private Sub SetWorkDays(value As String)
            Dim workDays As String() = value.Split(","c)
            For i As Integer = 0 To workDays.Length - 1
                Dim checked As Boolean = Utility.StringToBoolean(workDays(i))
                _workDayCheckboxes(i).Checked = checked
            Next
        End Sub

        Protected Sub DdlBuilding_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlBuilding.SelectedIndexChanged
            LoadLabs()
        End Sub

        Protected Sub DgResources_ItemDataBound(sender As Object, e As DataGridItemEventArgs) Handles dgResources.ItemDataBound
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                If Not di("EmailNotify") Is DBNull.Value Then
                    Dim hidCurrentValue As HiddenField = CType(e.Item.FindControl("hidCurrentValue"), HiddenField)
                    Dim ddlNotify As DropDownList = CType(e.Item.FindControl("ddlNotify"), DropDownList)
                    hidCurrentValue.Value = di("EmailNotify").AsString
                    ddlNotify.SelectedValue = di("EmailNotify").AsString
                End If
            End If
        End Sub

        Private Sub BtnSubmit_Click(sender As Object, e As EventArgs) Handles btnSubmit.Click
            ClearErrorMessage()
            ClearSuccessMessage()

            ' Default Working Hours        
            Dim beginHour As Integer = If(rdoHoursAllDay.Checked, 0, Integer.Parse(ddlBeginHour.SelectedValue))
            Dim endHour As Integer = If(rdoHoursAllDay.Checked, 24, Integer.Parse(ddlEndHour.SelectedValue))

            If endHour <= beginHour Then
                ServerJScript.JSAlert(Me.Page, "Error: End Hour cannot be equal to or earlier than Begin Hour.")
                Exit Sub
            End If

            ' Default Working Days
            Dim workDays As String = String.Join(",", GetWorkDays())

            Try
                ' Insert/Update Setting
                Dim cs As ClientSetting = ContextBase.GetClientSetting()

                cs.BuildingID = Integer.Parse(ddlBuilding.SelectedValue)
                cs.LabID = Integer.Parse(ddlLab.SelectedValue)
                cs.DefaultView = Convert.ToInt32(If(rdoDefaultViewDay.Checked, ViewType.DayView, ViewType.WeekView))
                cs.BeginHour = beginHour
                cs.EndHour = endHour
                cs.WorkDays = workDays
                cs.EmailCreateReserv = chkCreateReserv.Checked
                cs.EmailModifyReserv = chkModifyReserv.Checked
                cs.EmailDeleteReserv = chkDeleteReserv.Checked
                cs.EmailInvited = chkInviteReserv.Checked

                DA.Current.SaveOrUpdate(cs)

                ' Update ResourceClient email notifications if they are different
                Dim hidCurrentValue As HiddenField
                Dim ddlNotify As DropDownList
                For i As Integer = 0 To dgResources.Items.Count - 1
                    hidCurrentValue = CType(dgResources.Items(i).FindControl("hidCurrentValue"), HiddenField)
                    ddlNotify = CType(dgResources.Items(i).FindControl("ddlNotify"), DropDownList)
                    If hidCurrentValue.Value <> ddlNotify.SelectedValue Then
                        ResourceClientData.UpdateEmailNotify(Convert.ToInt32(dgResources.Items(i).Cells(0).Text), Convert.ToInt32(ddlNotify.SelectedValue))
                        hidCurrentValue.Value = ddlNotify.SelectedValue
                    End If
                Next

                '2009-09-16 add Practice Reservation notification
                Dim prefUpdateSuccessfull As Boolean = True
                For i As Integer = 0 To dgResourcePractice.Items.Count - 1
                    hidCurrentValue = CType(dgResourcePractice.Items(i).FindControl("hidCurrentValue"), HiddenField)
                    ddlNotify = CType(dgResourcePractice.Items(i).FindControl("ddlNotify"), DropDownList)
                    Dim hidResourceClientID As HiddenField = CType(dgResourcePractice.Items(i).FindControl("hidResourceClientID"), HiddenField)
                    Dim rawId As String = hidResourceClientID.Value
                    Dim resourceClientId As Integer
                    If Integer.TryParse(rawId, resourceClientId) Then
                        If hidCurrentValue.Value <> ddlNotify.SelectedValue Then
                            ResourceClientData.UpdatePracticeResEmailNotify(resourceClientId, Convert.ToInt32(ddlNotify.SelectedValue))
                            hidCurrentValue.Value = ddlNotify.SelectedValue
                        End If
                    Else
                        SetErrorMessage(String.Format("<div>Unable to get ResourceClientID from Row #{0}: Could not convert [{1}] to integer.</div>", i, rawId))
                        prefUpdateSuccessfull = False
                    End If
                Next

                SetAccountOrdering()
                GetAccountOrdering()

                SetShowTreeviewImages()
                GetShowTreeviewImages()

                If prefUpdateSuccessfull Then
                    SetSuccessMessage("Your preferences have been saved.")
                End If
            Catch ex As Exception
                SetErrorMessage(ex.Message)
            End Try
        End Sub

        Protected Sub GvResourcePractice_ItemDataBound(sender As Object, e As DataGridItemEventArgs) Handles dgResourcePractice.ItemDataBound
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                Dim practiceResEmailNotify As Integer = di("PracticeResEmailNotify").AsInt32
                Dim hidCurrentValue As HiddenField = CType(e.Item.FindControl("hidCurrentValue"), HiddenField)
                Dim ddlNotify As DropDownList = CType(e.Item.FindControl("ddlNotify"), DropDownList)
                Dim hidResourceClientID As HiddenField = CType(e.Item.FindControl("hidResourceClientID"), HiddenField)
                hidCurrentValue.Value = practiceResEmailNotify.ToString()
                ddlNotify.SelectedValue = practiceResEmailNotify.ToString()
                hidResourceClientID.Value = di("ResourceClientID").AsString
            End If
        End Sub

        Private Sub ClearErrorMessage()
            phErrorMessage.Visible = False
            litErrorMessage.Text = String.Empty
        End Sub

        Private Sub ClearSuccessMessage()
            phSuccessMessage.Visible = False
            litSuccessMessage.Text = String.Empty
        End Sub

        Private Sub SetErrorMessage(text As String)
            If Not String.IsNullOrEmpty(text) Then
                phErrorMessage.Visible = True
                litErrorMessage.Text += text
            End If
        End Sub

        Private Sub SetSuccessMessage(text As String)
            If Not String.IsNullOrEmpty(text) Then
                phSuccessMessage.Visible = True
                litSuccessMessage.Text += text
            End If
        End Sub
    End Class
End Namespace