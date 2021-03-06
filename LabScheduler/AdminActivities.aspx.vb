﻿Imports LabScheduler.AppCode
Imports LabScheduler.AppCode.DBAccess
Imports LNF.Data
Imports LNF.DataAccess
Imports LNF.Repository
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content
Imports Scheduler = LNF.Impl.Repository.Scheduler

Namespace Pages
    Public Class AdminActivities
        Inherits SortablePage

        Private dbActivity As New ActivityDB
        Private _AuthLevels As IList(Of Scheduler.AuthLevel)
        Private _Activities As IList(Of Scheduler.Activity)

        Public ReadOnly Property Activities As IEnumerable(Of Scheduler.Activity)
            Get
                If _Activities Is Nothing Then
                    _Activities = DataSession.Query(Of Scheduler.Activity)().ToList()
                End If
                Return _Activities
            End Get
        End Property

        Public ReadOnly Property AuthLevels As IList(Of Scheduler.AuthLevel)
            Get
                If _AuthLevels Is Nothing Then
                    _AuthLevels = DataSession.Query(Of Scheduler.AuthLevel)().ToList()
                End If
                Return _AuthLevels
            End Get
        End Property

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return PageSecurity.AdminAuthTypes
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                SwitchPanels(True, True)
                LoadAuthLevels()
                LoadAccountTypes()
                LoadInviteeTypes()
                LoadActivities()
            End If
        End Sub

        Protected Overrides Function GetDefaultSortExpression() As String
            Return "ListOrder"
        End Function

        Protected Overrides Function GetDefaultSortOrder() As String
            Return "ASC"
        End Function

#Region " Activity Form "
        Private Sub LoadAuthLevels()

            rptUserAuth.DataSource = AuthLevels
            rptUserAuth.DataBind()

            rptInviteeAuth.DataSource = AuthLevels
            rptInviteeAuth.DataBind()

            rptStartEndAuth.DataSource = AuthLevels
            rptStartEndAuth.DataBind()

            rptNoReservFenceAuth.DataSource = AuthLevels
            rptNoReservFenceAuth.DataBind()

            rptNoMaxSchedAuth.DataSource = AuthLevels
            rptNoMaxSchedAuth.DataBind()
        End Sub

        Private Sub LoadAccountTypes()
            ddlAccountType.ClearSelection()
            With ddlAccountType.Items
                .Add(New ListItem(GetAccountTypeName(ActivityAccountType.Reserver), Convert.ToInt32(ActivityAccountType.Reserver).ToString()))
                .Add(New ListItem(GetAccountTypeName(ActivityAccountType.Invitee), Convert.ToInt32(ActivityAccountType.Invitee).ToString()))
                .Add(New ListItem(GetAccountTypeName(ActivityAccountType.Both), Convert.ToInt32(ActivityAccountType.Both).ToString()))
            End With
        End Sub

        Private Sub LoadInviteeTypes()
            ddlInviteeType.ClearSelection()
            With ddlInviteeType.Items
                .Add(New ListItem(GetInviteeTypeName(ActivityInviteeType.None), Convert.ToInt32(ActivityInviteeType.None).ToString()))
                .Add(New ListItem(GetInviteeTypeName(ActivityInviteeType.Optional), Convert.ToInt32(ActivityInviteeType.Optional).ToString()))
                .Add(New ListItem(GetInviteeTypeName(ActivityInviteeType.Required), Convert.ToInt32(ActivityInviteeType.Required).ToString()))
                .Add(New ListItem(GetInviteeTypeName(ActivityInviteeType.Single), Convert.ToInt32(ActivityInviteeType.Single).ToString()))
            End With
            DdlInviteeType_SelectedIndexChanged(ddlInviteeType, Nothing)
        End Sub

        Private Sub DdlInviteeType_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlInviteeType.SelectedIndexChanged
            rptInviteeAuth.Visible = CType(ddlInviteeType.SelectedValue, ActivityInviteeType) <> ActivityInviteeType.None
        End Sub

        Private Sub BtnAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAdd.Click
            If UpdateActivity(True) Then
                LoadActivities()
                SwitchPanels(True, True)
            End If
        End Sub

        Private Sub BtnAddAnother_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAnother.Click
            If UpdateActivity(True) Then
                LoadActivities()
                ClearControls()
            End If
        End Sub

        Private Sub BtnUpdate_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUpdate.Click
            If UpdateActivity(False) Then
                LoadActivities()
                SwitchPanels(True, True)
            End If
        End Sub

        Private Sub BtnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancel.Click
            SwitchPanels(True, True)
        End Sub

        Private Function UpdateActivity(ByVal Insert As Boolean) As Boolean
            lblErrMsg.Text = String.Empty
            Try
                Dim activity As Scheduler.Activity
                'Dim auths As IList(Of Scheduler.GlobalActivityAuth)
                If Insert Then
                    activity = New Scheduler.Activity()
                    DataSession.Insert(activity)
                    'auths = CreateGlobalActivityAuths().ToList()
                    'DA.Current.Insert(auths)
                Else
                    activity = Activities.FirstOrDefault(Function(x) x.ActivityID = Convert.ToInt32(hidSelectedActivityID.Value))
                    'auths = GetGlobalActivityAuthsByActivity(activity).ToList()
                End If
                activity.ActivityName = txtActivityName.Text
                activity.ListOrder = Convert.ToInt32(txtListOrder.Text)
                activity.Chargeable = chkChargeable.Checked
                activity.Editable = chkEditable.Checked
                activity.AccountType = CType(ddlAccountType.SelectedValue, ActivityAccountType)
                Dim userAuth As Integer = AuthLevelUtility.GetAuthLevelValue(rptUserAuth.Items, "chkAuthLevel")
                'SetDefaultAuthByName(auths, "UserAuth", userAuth)
                'SetLockedAuthByName(auths, "UserAuth", AuthLevelUtility.GetAuthLevelValue(rptUserAuth.Items, "chkLocked"))
                If userAuth = 0 Then
                    lblErrMsg.Text = "Error: Please enter user authorizations."
                    Return False
                End If
                activity.InviteeType = CType(Convert.ToInt32(ddlInviteeType.SelectedValue), ActivityInviteeType)
                Dim inviteeAuth As Integer = 0
                If CType(activity.InviteeType, ActivityInviteeType) <> ActivityInviteeType.None Then
                    inviteeAuth = AuthLevelUtility.GetAuthLevelValue(rptInviteeAuth.Items, "chkAuthLevel")
                    If CType(activity.InviteeAuth, ActivityInviteeType) = 0 Then
                        lblErrMsg.Text = "Error: Please enter invitee authorizations."
                        Return False
                    End If
                End If
                'SetDefaultAuthByName(auths, "InviteeAuth", inviteeAuth)
                'SetLockedAuthByName(auths, "InviteeAuth", AuthLevelUtility.GetAuthLevelValue(rptInviteeAuth.Items, "chkLocked"))
                'SetDefaultAuthByName(auths, "StartEndAuth", AuthLevelUtility.GetAuthLevelValue(rptStartEndAuth.Items, "chkAuthLevel"))
                'SetLockedAuthByName(auths, "StartEndAuth", AuthLevelUtility.GetAuthLevelValue(rptStartEndAuth.Items, "chkLocked"))
                'SetDefaultAuthByName(auths, "NoFenceAuth", AuthLevelUtility.GetAuthLevelValue(rptNoReservFenceAuth.Items, "chkAuthLevel"))
                'SetLockedAuthByName(auths, "NoFenceAuth", AuthLevelUtility.GetAuthLevelValue(rptNoReservFenceAuth.Items, "chkLocked"))
                'SetDefaultAuthByName(auths, "NoMaxAuth", AuthLevelUtility.GetAuthLevelValue(rptNoMaxSchedAuth.Items, "chkAuthLevel"))
                'SetLockedAuthByName(auths, "NoMaxAuth", AuthLevelUtility.GetAuthLevelValue(rptNoMaxSchedAuth.Items, "chkLocked"))
                activity.Description = txtDescription.Text
                Return True
            Catch ex As Exception
                lblErrMsg.Text = ex.Message
                Return False
            End Try
        End Function
#End Region

#Region " Activity List "
        Private Sub LoadActivities()
            rptActivities.DataSource = Activities
            rptActivities.DataBind()
        End Sub

        Protected Sub BtnNewActivity_Click(sender As Object, e As EventArgs)
            SwitchPanels(False, True)
        End Sub

        Protected Sub Activity_Command(sender As Object, e As CommandEventArgs)
            Select Case e.CommandName
                Case "edit"
                    SwitchPanels(False, False)
                    ' Get Activity Info
                    Dim activity As Scheduler.Activity = Activities.First(Function(x) x.ActivityID = Convert.ToInt32(e.CommandArgument))
                    'Dim auths As IList(Of Scheduler.GlobalActivityAuth) = GetGlobalActivityAuthsByActivity(activity)
                    hidSelectedActivityID.Value = e.CommandArgument.ToString()
                    txtActivityName.Text = activity.ActivityName
                    txtListOrder.Text = activity.ListOrder.ToString()
                    chkChargeable.Checked = activity.Chargeable
                    chkEditable.Checked = activity.Editable
                    ddlAccountType.SelectedValue = Convert.ToInt32(activity.AccountType).ToString()
                    ddlInviteeType.SelectedValue = Convert.ToInt32(activity.InviteeType).ToString()
                    DdlInviteeType_SelectedIndexChanged(ddlInviteeType, Nothing)
                    'AuthLevelUtility.SetAuthLevel(rptUserAuth.Items, GetDefaultAuthByName(auths, "UserAuth"), "chkAuthLevel")
                    'AuthLevelUtility.SetAuthLevel(rptUserAuth.Items, GetLockedAuthByName(auths, "UserAuth"), "chkLocked")
                    'AuthLevelUtility.SetAuthLevel(rptInviteeAuth.Items, GetDefaultAuthByName(auths, "InviteeAuth"), "chkAuthLevel")
                    'AuthLevelUtility.SetAuthLevel(rptInviteeAuth.Items, GetLockedAuthByName(auths, "InviteeAuth"), "chkLocked")
                    'AuthLevelUtility.SetAuthLevel(rptStartEndAuth.Items, GetDefaultAuthByName(auths, "StartEndAuth"), "chkAuthLevel")
                    'AuthLevelUtility.SetAuthLevel(rptStartEndAuth.Items, GetLockedAuthByName(auths, "StartEndAuth"), "chkLocked")
                    'AuthLevelUtility.SetAuthLevel(rptNoReservFenceAuth.Items, GetDefaultAuthByName(auths, "NoFenceAuth"), "chkAuthLevel")
                    'AuthLevelUtility.SetAuthLevel(rptNoReservFenceAuth.Items, GetLockedAuthByName(auths, "NoFenceAuth"), "chkLocked")
                    'AuthLevelUtility.SetAuthLevel(rptNoMaxSchedAuth.Items, GetDefaultAuthByName(auths, "NoMaxAuth"), "chkAuthLevel")
                    'AuthLevelUtility.SetAuthLevel(rptNoMaxSchedAuth.Items, GetLockedAuthByName(auths, "NoMaxAuth"), "chkLocked")
                    txtDescription.Text = activity.Description
                Case "delete"
                    Dim activity As Scheduler.Activity = Activities.First(Function(x) x.ActivityID = Convert.ToInt32(e.CommandArgument))
                    DataSession.Delete(activity)
                    LoadActivities()
            End Select
        End Sub

        Protected Function GetAccountTypeName(acctType As ActivityAccountType) As String
            Select Case acctType
                Case ActivityAccountType.Reserver
                    Return "Reserver's Accounts"
                Case ActivityAccountType.Invitee
                    Return "Invitee's Accounts"
                Case ActivityAccountType.Both
                    Return "Both"
                Case Else
                    Throw New Exception("Invalid AccountType")
            End Select
        End Function

        Protected Function GetInviteeTypeName(invType As ActivityInviteeType) As String
            Select Case invType
                Case ActivityInviteeType.Required
                    Return "Requires Invitees"
                Case ActivityInviteeType.Optional
                    Return "Optional Invitees"
                Case ActivityInviteeType.Single
                    Return "Single Invitee"
                Case ActivityInviteeType.None
                    Return "No Invitees"
                Case Else
                    Throw New Exception("Invalid InviteeType")
            End Select
        End Function
#End Region

#Region " Private Functions "
        Private Sub SwitchPanels(showList As Boolean, showAddForm As Boolean)
            pListActivity.Visible = showList
            pEditActivity.Visible = Not showList

            If Not showList Then
                ClearControls()
                lblAction.Text = If(showAddForm, "Add", "Edit")
                btnAdd.Visible = showAddForm
                btnAddAnother.Visible = showAddForm
                btnUpdate.Visible = Not showAddForm
            End If
        End Sub

        Private Sub ClearControls()
            lblErrMsg.Text = String.Empty
            txtActivityName.Text = String.Empty
            txtListOrder.Text = String.Empty
            txtDescription.Text = String.Empty
            ddlAccountType.ClearSelection()
            ClearSelection(rptUserAuth, "chkAuthLevel")
            ddlInviteeType.ClearSelection()
            ClearSelection(rptInviteeAuth, "chkAuthLevel")
            ClearSelection(rptStartEndAuth, "chkAuthLevel")
            ClearSelection(rptNoReservFenceAuth, "chkAuthLevel")
            ClearSelection(rptNoMaxSchedAuth, "chkAuthLevel")
        End Sub

        Public Sub ClearSelection(rpt As Repeater, id As String)
            For Each item As RepeaterItem In rpt.Items
                Dim chk As CheckBox = CType(item.FindControl(id), CheckBox)
                If chk IsNot Nothing Then
                    chk.Checked = False
                End If
            Next
        End Sub
#End Region

        Protected Sub RptActivities_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            Dim rpt As Repeater = CType(e.Item.FindControl("rptActivityAuths"), Repeater)
            Dim activity As Scheduler.Activity = CType(e.Item.DataItem, Scheduler.Activity)
            rpt.DataSource = Nothing 'GetGlobalActivityAuthsByActivity(activity)
            rpt.DataBind()
        End Sub

    End Class

    Public Class ActivityAuth
        Public Property ActivityID As Integer
        Public Property AuthName As String
        Public Property UnauthorizedUser As Boolean
        Public Property AuthorizedUser As Boolean
        Public Property SuperUser As Boolean
    End Class
End Namespace