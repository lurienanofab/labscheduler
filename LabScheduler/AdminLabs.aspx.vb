Imports LabScheduler.AppCode
Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Models.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class AdminLabs
        Inherits SortablePage

        Private dbLab As New LabDB
        Private dbKiosk As New KioskDB
        Private dbKioskLab As New KioskLabDB

        Private dtLab, dtKiosk, dtKioskLab As DataTable

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return PageSecurity.AdminAuthTypes
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                SwitchPanels(True)
                LoadBuildings()
                LoadRooms()
                LoadKiosks()
                LoadLabs()
            Else
                dtLab = CType(Session("AdminLab"), DataTable)
                dtKiosk = CType(Session("dtKiosk"), DataTable)
                dtKioskLab = CType(Session("dtKioskLab"), DataTable)
            End If
        End Sub

#Region " Lab Form "
        Private Sub LoadBuildings()
            ddlBuilding.DataSource = BuildingDB.SelectAllDataTable()
            ddlBuilding.DataBind()
        End Sub

        Private Sub LoadRooms()
            Using reader As IDataReader = dbLab.SelectRooms()
                ddlRooms.DataSource = reader
                ddlRooms.DataBind()
                ddlRooms.Items.Insert(0, New ListItem("None", "-1"))
            End Using
        End Sub

        Private Sub LoadKiosks()
            Dim LabID As Integer
            If dgLabs.SelectedItem Is Nothing Then
                LabID = -1
            Else
                LabID = Convert.ToInt32(dgLabs.SelectedItem.Cells(0).Text)
            End If
            dtKiosk = dbKiosk.SelectAll()
            dtKioskLab = dbKioskLab.SelectByLab(LabID)
            dgKioskLab.DataSource = dtKioskLab
            dgKioskLab.DataBind()
            Session("dtKiosk") = dtKiosk
            Session("dtKioskLab") = dtKioskLab
        End Sub

        Private Sub btnAdd_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAdd.Click
            If UpdateLab(True) Then
                SwitchPanels(True)
            End If
        End Sub

        Private Sub btnAddAnother_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnAddAnother.Click
            If UpdateLab(True) Then
                ClearControls()
            End If
        End Sub

        Private Sub btnUpdate_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnUpdate.Click
            If UpdateLab(False) Then
                SwitchPanels(True)
            End If
        End Sub

        Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancel.Click
            SwitchPanels(True)
        End Sub

        Private Function UpdateLab(ByVal Insert As Boolean) As Boolean
            If ddlBuilding.Items.Count = 0 Then
                lblErrMsg.Text = "Please create a building first."
                Return False
            End If

            If String.IsNullOrEmpty(txbLabName.Text.Trim()) Then
                lblErrMsg.Text = "Please enter lab name."
                Return False
            End If

            lblErrMsg.Text = ""
            Try
                Dim drLab As DataRow
                If Insert Then
                    drLab = dtLab.NewRow
                Else
                    drLab = dtLab.Rows.Find(dgLabs.SelectedItem.Cells(0).Text)
                End If
                drLab("BuildingID") = ddlBuilding.SelectedValue
                drLab("BuildingName") = ddlBuilding.SelectedItem.Text
                drLab("LabName") = txbLabName.Text
                drLab("Room") = ddlRooms.SelectedItem.Text
                drLab("RoomID") = If(ddlRooms.SelectedValue = "-1", DBNull.Value, CType(ddlRooms.SelectedItem.Value, Object))
                drLab("Description") = txbDesc.Text
                If Insert Then dtLab.Rows.Add(drLab)
                dbLab.Update(dtLab)
                UploadFileUtility.UploadImage(filePic, "Lab", drLab("LabID").ToString())

                For Each dr As DataRow In dtKioskLab.Rows
                    If dr.RowState = DataRowState.Added Then dr("LabID") = drLab("LabID")
                Next
                dbKioskLab.Update(dtKioskLab)

                LoadLabs()
                'CType(Me.Master, MasterPageScheduler).LoadTreeView()
                Return True

            Catch ex As Exception
                lblErrMsg.Text = ex.Message
                Return False
            End Try
        End Function

#Region " Kiosks "
        Private Sub dgKioskLab_ItemCommand(ByVal source As Object, ByVal e As DataGridCommandEventArgs) Handles dgKioskLab.ItemCommand
            Dim LabID As Integer
            If dgLabs.SelectedItem Is Nothing Then
                LabID = -1
            Else
                LabID = Convert.ToInt32(dgLabs.SelectedItem.Cells(0).Text)
            End If

            Select Case e.CommandName
                Case "AddNewRow"
                    Dim ddlKiosk As DropDownList = CType(e.Item.FindControl("ddlKiosk"), DropDownList)
                    If ddlKiosk.SelectedItem Is Nothing Then
                        ServerJScript.JSAlert(Page, "No more kiosks to select from.")
                    Else
                        Dim dr As DataRow = dtKioskLab.NewRow
                        dr("KioskID") = ddlKiosk.SelectedValue
                        dr("KioskName") = ddlKiosk.SelectedItem.Text
                        dr("LabID") = LabID
                        dtKioskLab.Rows.Add(dr)
                    End If

                Case "Delete"
                    Dim KioskLabID As Integer = Convert.ToInt32(e.Item.Cells(0).Text)
                    Dim dr As DataRow = dtKioskLab.Rows.Find(KioskLabID)
                    If Not dr Is Nothing Then dr.Delete()
            End Select
            dgKioskLab.DataSource = dtKioskLab
            dgKioskLab.DataBind()
        End Sub

        Private Sub dgKioskLab_ItemDataBound(ByVal sender As System.Object, ByVal e As DataGridItemEventArgs) Handles dgKioskLab.ItemDataBound

            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                CType(e.Item.FindControl("lblKioskName"), Label).Text = di("KioskName").AsString
            ElseIf e.Item.ItemType = ListItemType.Footer Then
                Dim ddlKiosk As DropDownList = CType(e.Item.FindControl("ddlKiosk"), DropDownList)
                ddlKiosk.DataSource = dtKiosk.DefaultView
                ddlKiosk.DataBind()

                ' Remove kiosks from ddl that have alrady been selected
                For Each dr As DataRow In dtKioskLab.Rows
                    If dr.RowState <> DataRowState.Deleted Then
                        ddlKiosk.Items.Remove(ddlKiosk.Items.FindByValue(dr("KioskID").ToString()))
                    End If
                Next
                dgKioskLab.ShowFooter = ddlKiosk.Items.Count > 0
            End If
        End Sub
#End Region
#End Region

#Region " Lab List "
        Private Sub LoadLabs()
            ' Load into dataset
            dtLab = CType(Session("AdminLab"), DataTable)
            If dtLab Is Nothing Then
                dtLab = LabDB.SelectAll()
                Session("AdminLab") = dtLab
            End If

            ' Sort dataview
            Dim DBView As New DataView(dtLab)
            DBView.Sort = GetSort()
            dgLabs.DataSource = DBView
            dgLabs.DataBind()
        End Sub

        Private Sub btnNewLab_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnNewLab.Click
            SwitchPanels(False, True)
        End Sub

        Private Sub dgLabs_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgLabs.ItemDataBound
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Or e.Item.ItemType = ListItemType.SelectedItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)

                ' Delete Button
                Dim ibtnDelete As ImageButton = CType(e.Item.Cells(1).FindControl("ibtnDelete"), ImageButton)
                ibtnDelete.Attributes.Add("onclick", "return confirm('Are you sure you want to delete this Laboratory?');")

                ' Picture
                UploadFileUtility.DisplayIcon(CType(e.Item.FindControl("Picture"), Image), "Lab", di("LabID").AsString)
            End If
        End Sub

        Private Sub dgLabs_EditCommand(ByVal source As Object, ByVal e As DataGridCommandEventArgs) Handles dgLabs.EditCommand
            SwitchPanels(False, False)

            Dim drLab As DataRow = dtLab.Rows.Find(e.Item.Cells(0).Text)
            dgLabs.SelectedIndex = e.Item.ItemIndex
            ddlBuilding.ClearSelection()
            ddlBuilding.Items.FindByValue(drLab("BuildingID").ToString()).Selected = True
            txbLabName.Text = drLab("LabName").ToString()
            UploadFileUtility.DisplayIcon(imgPic, "Lab", drLab("LabID").ToString())
            txbDesc.Text = drLab("Description").ToString()
            Dim RoomItem As ListItem = ddlRooms.Items.FindByValue(drLab("RoomID").ToString())
            If Not RoomItem Is Nothing Then
                ddlRooms.ClearSelection()
                RoomItem.Selected = True
            Else
                ddlRooms.SelectedValue = "-1"
            End If
            LoadKiosks()
        End Sub

        Private Sub dgLabs_DeleteCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles dgLabs.DeleteCommand
            Try
                ' Give warning if there are process techs in this Lab
                If dbLab.HasProcessTechs(Convert.ToInt32(e.Item.Cells(0).Text)) Then
                    ServerJScript.JSAlert(Me.Page, "Unable to delete Laboratory.  There are process technologies belonging to this laboratory.")
                    Exit Sub
                End If

                ' Update dataset
                Dim drLab As DataRow = dtLab.Select("LabID = " + e.Item.Cells(0).Text)(0)
                drLab.Delete()
                dbLab.Update(dtLab)
                UploadFileUtility.DeleteImages("Lab", e.Item.Cells(0).Text)
                LoadLabs()
                'CType(Me.Master, MasterPageScheduler).LoadTreeView()

            Catch ex As Exception
                ServerJScript.JSAlert(Me.Page, ex.Message)
                Exit Sub
            End Try
        End Sub

        Private Sub dgLabs_PageIndexChanged(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridPageChangedEventArgs) Handles dgLabs.PageIndexChanged
            dgLabs.CurrentPageIndex = e.NewPageIndex
            LoadLabs()
        End Sub

        Private Sub dgLabs_SortCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridSortCommandEventArgs) Handles dgLabs.SortCommand
            HandleSortCommand(e)
            LoadLabs()
        End Sub
#End Region

#Region " Private Functions "
        Private Sub SwitchPanels(ByVal ShowList As Boolean, Optional ByVal ShowAddForm As Boolean = True)
            pListLab.Visible = ShowList
            pEditLab.Visible = Not ShowList

            If Not ShowList Then
                ClearControls()
                lblAction.Text = If(ShowAddForm, "Add", "Edit")
                btnAdd.Visible = ShowAddForm
                btnAddAnother.Visible = ShowAddForm
                btnUpdate.Visible = Not ShowAddForm
            End If
        End Sub

        Private Sub ClearControls()
            lblErrMsg.Text = ""
            ddlBuilding.ClearSelection()
            txbLabName.Text = ""
            imgPic.Visible = False
            txbDesc.Text = ""
        End Sub
#End Region

    End Class
End Namespace