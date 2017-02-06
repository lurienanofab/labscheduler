﻿Imports System.IO
Imports LabScheduler.AppCode.DBAccess
Imports LNF.CommonTools
Imports LNF.Models.Scheduler
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

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            'If user types url directly, we have to return immediately if resDB is not loaded
            If PathInfo.Current.ResourceID = 0 Then
                Return
            End If

            If Not IsPostBack Then
                LoadDocs()
            End If
        End Sub

        Private Sub LoadDocs()
            dtDocs = dbDocs.SelectDocs(PathInfo.Current.ResourceID)
            dgDocs.DataSource = dtDocs
            dgDocs.DataBind()
        End Sub

        Private Sub dgDocs_ItemDataBound(ByVal sender As Object, ByVal e As System.Web.UI.WebControls.DataGridItemEventArgs) Handles dgDocs.ItemDataBound
            If e.Item.ItemType = ListItemType.Item Or e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                Dim lblDocName As Label = CType(e.Item.FindControl("lblDocName"), Label)
                lblDocName.Text = di("DocName").ToString()
                Dim ibtnDelete As ImageButton = CType(e.Item.FindControl("ibtnDelete"), ImageButton)
                ibtnDelete.Attributes.Add("onclick", "return confirm('Are you sure you want to delete this doc?');")
                Dim hplView As HyperLink = CType(e.Item.FindControl("hplView"), HyperLink)
                hplView.NavigateUrl = Application("DocServer").ToString() + "ToolDocs/" + di("FileName").ToString()
            ElseIf e.Item.ItemType = ListItemType.EditItem Then
                Dim di As New DataItemHelper(e.Item.DataItem)
                Dim txbDocName As TextBox = CType(e.Item.FindControl("txbDocName"), TextBox)
                txbDocName.Text = di("DocName").ToString()
            End If
        End Sub

        Private Sub dgDocs_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.DataGridCommandEventArgs) Handles dgDocs.ItemCommand
            Dim DocID As Integer
            Dim strExt As String, FileName As String
            Try
                Select Case e.CommandName
                    Case "Add"
                        Dim txbNewDocName As TextBox = CType(e.Item.FindControl("txbNewDocName"), TextBox)
                        If String.IsNullOrEmpty(txbNewDocName.Text.Trim()) Then Throw New Exception("Please enter document name.")

                        Dim fileNewDoc As HtmlInputFile = CType(e.Item.FindControl("fileNewDoc"), HtmlInputFile)
                        If String.IsNullOrEmpty(fileNewDoc.Value.Trim()) Then Throw New Exception("Please select a file to upload.")

                        strExt = IO.Path.GetExtension(fileNewDoc.Value).ToLower
                        If Not ValidFileExtensions.Contains(strExt) Then Throw New Exception("Please upload .doc and .pdf files only.")

                        DocID = dbDocs.InsertDoc(PathInfo.Current.ResourceID, txbNewDocName.Text, strExt)
                        FileName = "doc" + DocID.ToString().PadLeft(5, Char.Parse("0")) + strExt

                        ' Upload doc to server
                        UploadDoc(fileNewDoc, FileName)
                        LoadDocs()

                    Case "Update"
                        DocID = Convert.ToInt32(e.Item.Cells(0).Text)
                        Dim txbDocName As TextBox = CType(e.Item.FindControl("txbDocName"), TextBox)
                        If String.IsNullOrEmpty(txbDocName.Text.Trim()) Then
                            Throw New Exception("Please enter document name.")
                        End If

                        Dim fileDoc As HtmlInputFile = CType(e.Item.FindControl("fileDoc"), HtmlInputFile)
                        If Not String.IsNullOrEmpty(fileDoc.Value.Trim()) Then
                            ' Upload new doc to server
                            strExt = Path.GetExtension(fileDoc.Value).ToLower
                            If Not ValidFileExtensions.Contains(strExt) Then
                                Throw New Exception("Please upload .doc and .pdf files only.")
                            End If

                            FileName = "doc" + DocID.ToString().PadLeft(5, Char.Parse("0")) + strExt
                            DeleteDoc(e.Item.Cells(1).Text) ' Delete old doc
                            UploadDoc(fileDoc, FileName)    ' Upload new doc
                        Else
                            FileName = e.Item.Cells(1).Text
                        End If
                        dbDocs.UpdateDoc(DocID, txbDocName.Text, FileName)

                        dgDocs.EditItemIndex = -1
                        dgDocs.ShowFooter = True
                        LoadDocs()

                    Case "Delete"
                        DocID = Convert.ToInt32(e.Item.Cells(0).Text)
                        dbDocs.DeleteDoc(DocID)
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