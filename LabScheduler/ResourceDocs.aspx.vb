Imports System.IO
Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ResourceDocs
        Inherits SchedulerPage

        Private dbDocs As New ResourceDocDB
        Private dtDocs As DataTable

        Public ReadOnly Property ValidFileExtensions As String()
            Get
                Return New String() {".doc", ".pdf", ".zip"}
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            'If user types url directly, we have to return immediately if resDB is not loaded
            If ContextBase.Request.SelectedPath().ResourceID = 0 Then
                Return
            End If

            If Not IsPostBack Then
                LoadDocs()
            End If
        End Sub

        Private Sub LoadDocs()
            dtDocs = dbDocs.SelectDocs(ContextBase.Request.SelectedPath().ResourceID)
            dgDocs.DataSource = dtDocs
            dgDocs.DataBind()
        End Sub

        Private Sub DgDocs_ItemDataBound(ByVal sender As Object, ByVal e As DataGridItemEventArgs) Handles dgDocs.ItemDataBound
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                Dim lblDocName As Label = CType(e.Item.FindControl("lblDocName"), Label)
                lblDocName.Text = di("DocName").AsString
                Dim ibtnDelete As ImageButton = CType(e.Item.FindControl("ibtnDelete"), ImageButton)
                ibtnDelete.Attributes.Add("onclick", "return confirm('Are you sure you want to delete this doc?');")
                Dim hplView As HyperLink = CType(e.Item.FindControl("hplView"), HyperLink)
                hplView.NavigateUrl = Application("DocServer").ToString() + "ToolDocs/" + di("FileName").AsString
            ElseIf e.Item.ItemType = ListItemType.EditItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                Dim txbDocName As TextBox = CType(e.Item.FindControl("txbDocName"), TextBox)
                txbDocName.Text = di("DocName").AsString
            End If
        End Sub

        Private Sub DgDocs_ItemCommand(ByVal source As Object, ByVal e As DataGridCommandEventArgs) Handles dgDocs.ItemCommand
            Dim docId As Integer
            Dim fileExt As String, fileName As String
            Try
                Select Case e.CommandName
                    Case "Add"
                        Dim txbNewDocName As TextBox = CType(e.Item.FindControl("txbNewDocName"), TextBox)
                        If String.IsNullOrEmpty(txbNewDocName.Text.Trim()) Then Throw New Exception("Please enter document name.")

                        Dim fileNewDoc As HtmlInputFile = CType(e.Item.FindControl("fileNewDoc"), HtmlInputFile)
                        If String.IsNullOrEmpty(fileNewDoc.Value.Trim()) Then Throw New Exception("Please select a file to upload.")

                        fileExt = IO.Path.GetExtension(fileNewDoc.Value).ToLower
                        If Not ValidFileExtensions.Contains(fileExt) Then Throw New Exception("Please upload .doc and .pdf files only.")

                        docId = dbDocs.InsertDoc(ContextBase.Request.SelectedPath().ResourceID, txbNewDocName.Text, fileExt)
                        fileName = "doc" + docId.ToString().PadLeft(5, Char.Parse("0")) + fileExt

                        ' Upload doc to server
                        UploadDoc(fileNewDoc, fileName)
                        LoadDocs()

                    Case "Update"
                        docId = Convert.ToInt32(e.Item.Cells(0).Text)
                        Dim txbDocName As TextBox = CType(e.Item.FindControl("txbDocName"), TextBox)
                        If String.IsNullOrEmpty(txbDocName.Text.Trim()) Then
                            Throw New Exception("Please enter document name.")
                        End If

                        Dim fileDoc As HtmlInputFile = CType(e.Item.FindControl("fileDoc"), HtmlInputFile)
                        If Not String.IsNullOrEmpty(fileDoc.Value.Trim()) Then
                            ' Upload new doc to server
                            fileExt = Path.GetExtension(fileDoc.Value).ToLower
                            If Not ValidFileExtensions.Contains(fileExt) Then
                                Throw New Exception("Please upload .doc and .pdf files only.")
                            End If

                            fileName = "doc" + docId.ToString().PadLeft(5, Char.Parse("0")) + fileExt
                            DeleteDoc(e.Item.Cells(1).Text) ' Delete old doc
                            UploadDoc(fileDoc, fileName)    ' Upload new doc
                        Else
                            fileName = e.Item.Cells(1).Text
                        End If
                        dbDocs.UpdateDoc(docId, txbDocName.Text, fileName)

                        dgDocs.EditItemIndex = -1
                        dgDocs.ShowFooter = True
                        LoadDocs()

                    Case "Delete"
                        docId = Convert.ToInt32(e.Item.Cells(0).Text)
                        dbDocs.DeleteDoc(docId)
                        DeleteDoc(e.Item.Cells(1).Text)
                        LoadDocs()

                    Case "Edit"
                        dgDocs.EditItemIndex = e.Item.ItemIndex
                        dgDocs.ShowFooter = False
                        LoadDocs()

                    Case "Cancel"
                        dgDocs.EditItemIndex = -1
                        dgDocs.ShowFooter = True
                        LoadDocs()
                End Select
            Catch ex As Exception
                WebUtility.BootstrapAlert(phMessage, "danger", ex.Message, True)
            End Try
        End Sub

        Private Sub UploadDoc(ByRef fileDoc As HtmlInputFile, ByVal FileName As String)
            Dim FilePath As String = Application("DocStore").ToString() + "ToolDocs\" + FileName
            If File.Exists(FilePath) Then IO.File.Delete(FilePath)
            fileDoc.PostedFile.SaveAs(FilePath)
        End Sub

        Private Sub DeleteDoc(ByVal FileName As String)
            Dim FilePath As String = Application("DocStore").ToString() + "ToolDocs\" + FileName
            If IO.File.Exists(FilePath) Then IO.File.Delete(FilePath)
        End Sub
    End Class
End Namespace