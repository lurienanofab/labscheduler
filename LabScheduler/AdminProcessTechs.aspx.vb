'Copyright 2017 University of Michigan

'Licensed under the Apache License, Version 2.0 (the "License");
'you may Not use this file except In compliance With the License.
'You may obtain a copy Of the License at

'http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law Or agreed To In writing, software
'distributed under the License Is distributed On an "AS IS" BASIS,
'WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'See the License For the specific language governing permissions And
'limitations under the License.

Imports LabScheduler.AppCode
Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Models.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class AdminProcessTechs
        Inherits SortablePage

        Private dbProcessTech As New ProcessTechDB
        Private dtProcessTech As DataTable

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return PageSecurity.AdminAuthTypes
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not IsPostBack Then
                SwitchPanels(True)
                LoadBuildings()
                LoadLabs()
                LoadProcessTechs()
            Else
                dtProcessTech = CType(Session("AdminProcessTech"), DataTable)
            End If
        End Sub

#Region " Process Tech Form "
        Private Sub LoadBuildings()
            ddlBuilding.DataSource = BuildingDB.SelectAllDataTable()
            ddlBuilding.DataBind()
        End Sub

        Private Sub LoadLabs()
            If ddlBuilding.Items.Count = 0 Then Exit Sub
            ddlLab.DataSource = New DataView(LabDB.SelectAll(), String.Format("BuildingID = {0}", ddlBuilding.SelectedValue), "LabName", DataViewRowState.CurrentRows)
            ddlLab.DataBind()
        End Sub

        Private Sub LoadProcessTechList()
            If ddlLab.Items.Count = 0 Then Exit Sub
            ddlProcessTech.DataSource = dbProcessTech.SelectProcessTechNotInLab(Convert.ToInt32(ddlLab.SelectedValue))
            ddlProcessTech.DataBind()
        End Sub

        Private Sub ddlBuilding_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlBuilding.SelectedIndexChanged
            LoadLabs()
        End Sub

        Private Sub ddlLab_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles ddlLab.SelectedIndexChanged
            LoadProcessTechList()
        End Sub

        Private Sub rblProcessTech_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles rblProcessTech.SelectedIndexChanged
            txbProcessTechName.Enabled = rblProcessTech.SelectedIndex = 0
            ddlProcessTech.Enabled = rblProcessTech.SelectedIndex <> 0
            filePic.Disabled = rblProcessTech.SelectedIndex <> 0
            txbDesc.Enabled = rblProcessTech.SelectedIndex = 0
        End Sub

        Private Sub btnAdd_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAdd.Click
            If UpdateProcessTech(True) Then
                SwitchPanels(True)
            End If
        End Sub

        Private Sub btnAddAnother_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnAddAnother.Click
            If UpdateProcessTech(True) Then
                ClearControls()
            End If
        End Sub

        Private Sub btnUpdate_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnUpdate.Click
            If UpdateProcessTech(False) Then
                SwitchPanels(True)
            End If
        End Sub

        Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnCancel.Click
            SwitchPanels(True)
        End Sub

        Private Function UpdateProcessTech(ByVal Insert As Boolean) As Boolean
            If ddlBuilding.Items.Count = 0 Then
                lblErrMsg.Text = "Error: Please create a building first."
                Return False
            End If

            If ddlLab.Items.Count = 0 Then
                lblErrMsg.Text = "Please create a laboratory first."
                Return False
            End If

            lblErrMsg.Text = ""
            Try
                Dim drProcessTech As DataRow
                If Insert Then
                    drProcessTech = dtProcessTech.NewRow
                Else
                    drProcessTech = dtProcessTech.Select("ProcessTechID=" + dgProcessTechs.SelectedItem.Cells(0).Text)(0)
                End If
                drProcessTech("BuildingID") = ddlBuilding.SelectedValue
                drProcessTech("BuildingName") = ddlBuilding.SelectedItem.Text
                drProcessTech("LabID") = ddlLab.SelectedValue
                drProcessTech("LabName") = ddlLab.SelectedItem.Text
                If rblProcessTech.SelectedIndex = 0 Then
                    drProcessTech("ProcessTechID") = 0 ' will be properly assigned by the DB
                    drProcessTech("ProcessTechName") = txbProcessTechName.Text
                Else
                    drProcessTech("ProcessTechID") = ddlProcessTech.SelectedValue
                    drProcessTech("ProcessTechName") = ddlProcessTech.SelectedItem.Text
                End If
                drProcessTech("Description") = txbDesc.Text
                If Insert Then dtProcessTech.Rows.Add(drProcessTech)
                dbProcessTech.Update(dtProcessTech)
                UploadFileUtility.UploadImage(filePic, "ProcessTech", drProcessTech("ProcessTechID").ToString())

                LoadProcessTechs()
                'CType(Me.Master, MasterPageScheduler).LoadTreeView()
                Return True

            Catch ex As Exception
                lblErrMsg.Text = ex.Message
                Return False
            End Try
        End Function
#End Region

#Region " Process Tech List "
        Private Sub LoadProcessTechs()
            ' Load into dataset
            dtProcessTech = CType(Session("AdminProcessTech"), DataTable)
            If dtProcessTech Is Nothing Then
                dtProcessTech = ProcessTechDB.SelectAll()
                Session("AdminProcessTech") = dtProcessTech
            End If

            ' Sort dataview
            Dim DBView As New DataView(dtProcessTech)
            DBView.Sort = GetSort()
            dgProcessTechs.DataSource = DBView
            dgProcessTechs.DataBind()
        End Sub

        Private Sub btnNewProcessTech_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewProcessTech.Click
            SwitchPanels(False, True)
        End Sub

        Private Sub dgProcessTechs_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgProcessTechs.ItemDataBound
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Or e.Item.ItemType = ListItemType.SelectedItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)

                ' Delete Button
                Dim ibtnDelete As ImageButton = CType(e.Item.Cells(1).FindControl("ibtnDelete"), ImageButton)
                ibtnDelete.Attributes.Add("onclick", "return confirm('Are you sure you want to delete this Process Technology?');")

                ' Picture
                UploadFileUtility.DisplayIcon(CType(e.Item.FindControl("Picture"), System.Web.UI.WebControls.Image), "ProcessTech", di("ProcessTechID").ToString())
            End If
        End Sub

        Private Sub dgProcessTechs_EditCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles dgProcessTechs.EditCommand
            SwitchPanels(False, False)

            Dim drProcessTech As DataRow = dtProcessTech.Select("ProcessTechID=" + e.Item.Cells(0).Text)(0)
            dgProcessTechs.SelectedIndex = e.Item.ItemIndex
            ddlBuilding.ClearSelection()
            ddlBuilding.Items.FindByValue(drProcessTech("BuildingID").ToString()).Selected = True
            LoadLabs()
            ddlLab.ClearSelection()
            ddlLab.Items.FindByValue(drProcessTech("LabID").ToString()).Selected = True
            txbProcessTechName.Text = drProcessTech("ProcessTechName").ToString()
            UploadFileUtility.DisplayIcon(imgPic, "ProcessTech", drProcessTech("ProcessTechID").ToString())
            txbDesc.Text = drProcessTech("Description").ToString()
        End Sub

        Private Sub dgProcessTechs_DeleteCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles dgProcessTechs.DeleteCommand
            Try
                ' Give warning if there are resources in this Process Tech
                If dbProcessTech.HasResources(Convert.ToInt32(e.Item.Cells(0).Text)) Then
                    ServerJScript.JSAlert(Me.Page, "Unable to delete Process Technology.  There are resources belonging to this process technology.")
                    Exit Sub
                End If

                ' Update dataset
                Dim drProcessTech As DataRow = dtProcessTech.Select("ProcessTechID=" + e.Item.Cells(0).Text)(0)
                drProcessTech.Delete()
                dbProcessTech.Update(dtProcessTech)
                UploadFileUtility.DeleteImages("ProcessTech", e.Item.Cells(0).Text)
                LoadProcessTechs()
                'CType(Me.Master, MasterPageScheduler).LoadTreeView()

            Catch ex As Exception
                ServerJScript.JSAlert(Me.Page, ex.Message)
            End Try
        End Sub

        Private Sub dgProcessTechs_PageIndexChanged(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridPageChangedEventArgs) Handles dgProcessTechs.PageIndexChanged
            dgProcessTechs.CurrentPageIndex = e.NewPageIndex
            LoadProcessTechs()
        End Sub

        Private Sub dgProcessTechs_SortCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridSortCommandEventArgs) Handles dgProcessTechs.SortCommand
            HandleSortCommand(e)
            LoadProcessTechs()
        End Sub
#End Region

#Region " Private Functions "
        Private Sub SwitchPanels(ByVal ShowList As Boolean, Optional ByVal ShowAddForm As Boolean = True)
            pListProcessTech.Visible = ShowList
            pEditProcessTech.Visible = Not ShowList

            If Not ShowList Then
                ClearControls()
                lblAction.Text = If(ShowAddForm, "Add", "Edit")
                btnAdd.Visible = ShowAddForm
                btnAddAnother.Visible = ShowAddForm
                btnUpdate.Visible = Not ShowAddForm
                If ShowAddForm Then
                    ddlBuilding.Enabled = True
                    ddlLab.Enabled = True
                    rblProcessTech.Visible = True
                    txbProcessTechName.Visible = True
                    ddlProcessTech.Visible = True
                    LoadProcessTechList()
                Else
                    ddlBuilding.Enabled = False
                    ddlLab.Enabled = False
                    rblProcessTech.Visible = False
                    txbProcessTechName.Visible = True
                    ddlProcessTech.Visible = False
                End If
            End If
        End Sub

        Private Sub ClearControls()
            lblErrMsg.Text = ""
            ddlBuilding.SelectedIndex = 0
            LoadLabs()
            txbProcessTechName.Text = ""
            imgPic.Visible = False
            txbDesc.Text = ""
        End Sub
#End Region
    End Class
End Namespace