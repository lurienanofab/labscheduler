Imports LabScheduler.AppCode.DBAccess
Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class AdminProperties
        Inherits SortablePage

        Private kioskDB As New KioskDB
        Private dtKiosk As DataTable

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return PageSecurity.AdminAuthTypes
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Not IsPostBack Then
                LoadAdmin()
                LoadAccounts()
                LoadKiosks()
                LoadProperties()
                LoadTools()
            Else
                dtKiosk = CType(Session("dtKiosk"), DataTable)
            End If
        End Sub

#Region " Loading Functions "
        Private Sub LoadAdmin()
            Dim query As IList(Of Client) = DA.Current.Query(Of Client)().Where(Function(x) x.Active).ToList()
            ddlAdmin.DataSource = query.Where(Function(x) x.HasPriv(ClientPrivilege.Administrator)).ToList()
            ddlAdmin.DataBind()
        End Sub

        Private Sub LoadAccounts()
            ddlAccount.DataSource = (New AccountDB).SelectAll
            ddlAccount.DataBind()
        End Sub

        Private Sub LoadKiosks()
            If dtKiosk Is Nothing Then
                dtKiosk = kioskDB.SelectAll()
                Session("dtKiosk") = dtKiosk
            End If

            dgKiosk.DataSource = dtKiosk
            dgKiosk.DataBind()
        End Sub

        Private Sub LoadProperties()
            txtLateChargePenalty.Text = Properties.Current.LateChargePenaltyMultiplier.ToString()
            txtAuthExpWarning.Text = Properties.Current.AuthExpWarning.ToString()
            txtResourceIPPrefix.Text = Properties.Current.ResourceIPPrefix

            ' System Administrator
            ddlAdmin.SelectedIndex = -1
            Dim itemAdmin As ListItem = ddlAdmin.Items.FindByValue(Properties.Current.Admin.ClientID.ToString())
            If Not itemAdmin Is Nothing Then itemAdmin.Selected = True

            ' General Lab Account
            ddlAccount.SelectedIndex = -1
            Dim itemAccount As ListItem = ddlAccount.Items.FindByValue(Properties.Current.LabAccount.AccountID.ToString())
            If Not itemAccount Is Nothing Then itemAccount.Selected = True
        End Sub

        Private Sub LoadTools()
            Dim resources As IList(Of ResourceModel) = CacheManager.Current.Resources()
            ddlGranularityTool.AppendDataBoundItems = True
            ddlGranularityTool.DataSource = resources.OrderBy(Function(x) x.ResourceName)
            ddlGranularityTool.DataBind()
        End Sub

        Private Sub LoadGranularityValues()
            Dim values As Integer() = PropertiesManager.GetGranularityValues(Convert.ToInt32(ddlGranularityTool.SelectedValue))
            txtGranularityValues.Text = String.Join(",", values)
        End Sub
#End Region

#Region " Kiosk DataGrid Events "
        Private Sub dgKiosk_ItemCommand(ByVal source As Object, ByVal e As DataGridCommandEventArgs) Handles dgKiosk.ItemCommand
            Dim KioskID As Integer
            Select Case e.CommandName
                Case "AddNewRow"
                    Dim dr As DataRow = dtKiosk.NewRow
                    dr("KioskName") = CType(e.Item.FindControl("txbNewKioskName"), TextBox).Text
                    dr("KioskIP") = CType(e.Item.FindControl("txbNewKioskIP"), TextBox).Text
                    dtKiosk.Rows.Add(dr)

                Case "Edit"
                    dgKiosk.EditItemIndex = e.Item.ItemIndex
                    dgKiosk.ShowFooter = False

                Case "Cancel"
                    dgKiosk.EditItemIndex = -1
                    dgKiosk.ShowFooter = True

                Case "Update"
                    KioskID = Convert.ToInt32(e.Item.Cells(0).Text)
                    Dim dr As DataRow = dtKiosk.Rows.Find(KioskID)
                    dr("KioskName") = CType(e.Item.FindControl("txbKioskName"), TextBox).Text
                    dr("KioskIP") = CType(e.Item.FindControl("txbKioskIP"), TextBox).Text
                    dgKiosk.EditItemIndex = -1
                    dgKiosk.ShowFooter = True

                Case "Delete"
                    KioskID = Convert.ToInt32(e.Item.Cells(0).Text)
                    Dim dr As DataRow = dtKiosk.Rows.Find(KioskID)
                    If Not dr Is Nothing Then dr.Delete()
            End Select
            LoadKiosks()
        End Sub

        Private Sub dgKiosk_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgKiosk.ItemDataBound
            If e.Item.ItemType = ListItemType.AlternatingItem Or e.Item.ItemType = ListItemType.Item Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                CType(e.Item.FindControl("lblKioskName"), Label).Text = di("KioskName").ToString()
                CType(e.Item.FindControl("lblKioskIP"), Label).Text = di("KioskIP").ToString()
            ElseIf e.Item.ItemType = ListItemType.EditItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                CType(e.Item.FindControl("txbKioskName"), TextBox).Text = di("KioskName").ToString()
                CType(e.Item.FindControl("txbKioskIP"), TextBox).Text = di("KioskIP").ToString()
            End If
        End Sub
#End Region

        Private Sub SetAlertMessage(msg As String, Optional alertType As String = "danger")
            If String.IsNullOrEmpty(msg) Then
                litAlertMessage.Text = String.Empty
            Else
                litAlertMessage.Text = String.Format("<div class=""alert alert-{0}"" role=""alert"" style=""margin-top: 10px;"">{1}</div>", alertType, msg)
            End If
        End Sub

        Private Sub btnSubmit_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSubmit.Click
            SetAlertMessage(String.Empty)

            ' Error Checking
            If String.IsNullOrEmpty(txtLateChargePenalty.Text.Trim()) Then
                SetAlertMessage("Error: Please enter late charge penalty multiplier.")
                Exit Sub
            End If

            If String.IsNullOrEmpty(txtAuthExpWarning.Text.Trim()) Then
                SetAlertMessage("Error: Please enter authorization expiration warning.")
                Exit Sub
            End If

            Dim dblAuthExpWarning As Double = Convert.ToDouble(txtAuthExpWarning.Text)
            If dblAuthExpWarning > 1 Then
                SetAlertMessage("Please enter a float point number less than 1 for Authorization Expiration Warning.")
                Exit Sub
            End If


            Properties.Current.LateChargePenaltyMultiplier = Convert.ToDouble(txtLateChargePenalty.Text)
            Properties.Current.AuthExpWarning = Convert.ToDouble(txtAuthExpWarning.Text)
            Properties.Current.Admin = DA.Current.Single(Of Client)(Convert.ToInt32(ddlAdmin.SelectedValue))
            Properties.Current.ResourceIPPrefix = txtResourceIPPrefix.Text

            ' Update Database
            Properties.Current.Save()
            kioskDB.Update(dtKiosk)

            SetAlertMessage("Global Properties have been successfully modified.", "success")
        End Sub

        Protected Sub ddlGranularityTool_SelectedIndexChanged(sender As Object, e As EventArgs)

        End Sub
    End Class
End Namespace