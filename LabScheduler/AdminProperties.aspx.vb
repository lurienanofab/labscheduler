Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Impl.Repository.Data
Imports LNF.Repository
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
#End Region

#Region " Kiosk DataGrid Events "
        Protected Sub DgKiosk_ItemCommand(ByVal source As Object, ByVal e As DataGridCommandEventArgs)
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

        Protected Sub DgKiosk_ItemDataBound(ByVal sender As Object, ByVal e As DataGridItemEventArgs)
            If e.Item.ItemType = ListItemType.AlternatingItem Or e.Item.ItemType = ListItemType.Item Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                CType(e.Item.FindControl("lblKioskName"), Label).Text = di("KioskName").AsString
                CType(e.Item.FindControl("lblKioskIP"), Label).Text = di("KioskIP").AsString
            ElseIf e.Item.ItemType = ListItemType.EditItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                CType(e.Item.FindControl("txbKioskName"), TextBox).Text = di("KioskName").AsString
                CType(e.Item.FindControl("txbKioskIP"), TextBox).Text = di("KioskIP").AsString
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

        Protected Sub BtnSubmit_Click(ByVal sender As Object, ByVal e As EventArgs)
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
            Properties.Current.Admin = Provider.Data.Client.GetClient(Integer.Parse(ddlAdmin.SelectedValue))
            Properties.Current.ResourceIPPrefix = txtResourceIPPrefix.Text

            ' Update Database
            Properties.Current.Save()
            kioskDB.Update(dtKiosk)
            Kiosks.ClearCache()

            SetAlertMessage("Global Properties have been successfully modified.", "success")
        End Sub
    End Class
End Namespace