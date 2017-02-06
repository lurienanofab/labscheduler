Imports LabScheduler.AppCode
Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Models.Data
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class AdminBuildings
        Inherits SortablePage

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return PageSecurity.AdminAuthTypes
            End Get
        End Property

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                SwitchPanels(True)
                LoadBuildings(BuildingDB.SelectAllDataTable())
            End If
        End Sub

#Region " Building Form "
        Private Sub btnAdd_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
            If UpdateBuilding(BuildingDB.SelectAllDataTable(), True) Then
                SwitchPanels(True)
            End If
        End Sub

        Private Sub btnAddAnother_Click(sender As Object, e As EventArgs) Handles btnAddAnother.Click
            If UpdateBuilding(BuildingDB.SelectAllDataTable(), True) Then
                ClearControls()
            End If
        End Sub

        Private Sub btnUpdate_Click(sender As Object, e As EventArgs) Handles btnUpdate.Click
            If UpdateBuilding(BuildingDB.SelectAllDataTable(), False) Then
                SwitchPanels(True)
            End If
        End Sub

        Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
            SwitchPanels(True)
        End Sub

        Private Function UpdateBuilding(dt As DataTable, insert As Boolean) As Boolean
            ' Error Checking
            If String.IsNullOrEmpty(txbBuildingName.Text.Trim()) Then
                lblErrMsg.Text = "Error: Please enter building name."
                Return False
            End If

            lblErrMsg.Text = String.Empty

            Try
                Dim drBuilding As DataRow
                If insert Then
                    drBuilding = dt.NewRow()
                Else
                    drBuilding = dt.Rows.Find(dgBuildings.SelectedItem.Cells(0).Text)
                End If
                drBuilding("BuildingName") = txbBuildingName.Text
                drBuilding("Description") = txbDesc.Text
                If insert Then dt.Rows.Add(drBuilding)
                BuildingDB.Update(dt)
                UploadFileUtility.UploadImage(filePic, "Building", drBuilding("BuildingID").ToString())

                LoadBuildings(dt)
                Return True

            Catch ex As Exception
                lblErrMsg.Text = ex.Message
                Return False
            End Try
        End Function
#End Region

#Region " Building List "
        Private Sub LoadBuildings(dt As DataTable)
            ' Sort dataview
            Dim dv As New DataView(dt)
            dv.Sort = GetSort()
            dgBuildings.DataSource = dv
            dgBuildings.DataBind()
        End Sub

        Private Sub btnNewBuilding_Click(sender As Object, e As EventArgs) Handles btnNewBuilding.Click
            SwitchPanels(False, True)
        End Sub

        Private Sub dgBuildings_ItemDataBound(sender As Object, e As DataGridItemEventArgs) Handles dgBuildings.ItemDataBound
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Or e.Item.ItemType = ListItemType.SelectedItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)

                ' Delete Button
                Dim ibtnDelete As ImageButton = CType(e.Item.FindControl("ibtnDelete"), ImageButton)
                ibtnDelete.Attributes.Add("onclick", "return confirm('Are you sure you want to delete this Building?');")

                ' Picture
                UploadFileUtility.DisplayIcon(CType(e.Item.FindControl("Picture"), Image), "Building", di("BuildingID").ToString())
            End If
        End Sub

        Private Sub dgBuildings_EditCommand(source As Object, e As DataGridCommandEventArgs) Handles dgBuildings.EditCommand
            SwitchPanels(False, False)
            Dim dt As DataTable = BuildingDB.SelectAllDataTable()
            Dim drBuilding As DataRow = dt.Rows.Find(e.Item.Cells(0).Text)
            dgBuildings.SelectedIndex = e.Item.ItemIndex
            txbBuildingName.Text = drBuilding("BuildingName").ToString()
            UploadFileUtility.DisplayIcon(imgPic, "Building", drBuilding("BuildingID").ToString())
            txbDesc.Text = drBuilding("Description").ToString()
        End Sub

        Private Sub dgBuildings_DeleteCommand(source As Object, e As DataGridCommandEventArgs) Handles dgBuildings.DeleteCommand
            Try
                ' Give warning if there are labs in this building
                If BuildingDB.HasLabs(Convert.ToInt32(e.Item.Cells(0).Text)) Then
                    ServerJScript.JSAlert(Page, "Unable to delete Building. There are laboratories belonging to this building.")
                    Exit Sub
                End If

                ' Update dataset
                Dim dt As DataTable = BuildingDB.SelectAllDataTable()
                Dim drBuilding As DataRow = dt.Rows.Find(e.Item.Cells(0).Text)
                drBuilding.Delete()
                BuildingDB.Update(dt)
                UploadFileUtility.DeleteImages("Building", e.Item.Cells(0).Text)
                LoadBuildings(dt)
            Catch ex As Exception
                ServerJScript.JSAlert(Page, ex.Message)
            End Try
        End Sub

        Private Sub dgBuildings_PageIndexChanged(source As Object, e As DataGridPageChangedEventArgs) Handles dgBuildings.PageIndexChanged
            dgBuildings.CurrentPageIndex = e.NewPageIndex
            LoadBuildings(BuildingDB.SelectAllDataTable())
        End Sub

        Private Sub dgBuildings_SortCommand(source As Object, e As DataGridSortCommandEventArgs) Handles dgBuildings.SortCommand
            HandleSortCommand(e)
            LoadBuildings(BuildingDB.SelectAllDataTable())
        End Sub
#End Region

#Region " Private Functions "
        Private Sub SwitchPanels(ByVal ShowList As Boolean, Optional ByVal ShowAddForm As Boolean = True)
            pListBuilding.Visible = ShowList
            pEditBuilding.Visible = Not ShowList

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
            txbBuildingName.Text = ""
            imgPic.Visible = False
            txbDesc.Text = ""
        End Sub
#End Region
    End Class
End Namespace