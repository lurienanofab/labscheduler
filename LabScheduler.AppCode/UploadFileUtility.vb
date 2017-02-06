Imports System.Drawing
Imports System.IO
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.HtmlControls

Public Class UploadFileUtility
    ' Uploads image and save original image and icon image
    Public Shared Sub UploadImage(inputFile As HtmlInputFile, path As String, id As String)
        If Not String.IsNullOrEmpty(inputFile.Value) Then
            If inputFile.PostedFile.ContentLength < 5242880 Then
                ' Check for existing files
                Dim imagePhysicalPath As String = GetImagePhysicalPath(path, id)
                Dim iconPhysicalPath As String = GetIconPhysicalPath(path, id)
                If File.Exists(imagePhysicalPath) Then File.Delete(imagePhysicalPath)
                If File.Exists(iconPhysicalPath) Then File.Delete(iconPhysicalPath)

                ' Save original image
                Dim img As New Bitmap(inputFile.PostedFile.InputStream)
                img.Save(imagePhysicalPath, Imaging.ImageFormat.Png)

                ' Save icon image
                Dim iconHeight As Integer = 32
                Dim iconWidth As Integer = Convert.ToInt32(img.Width * iconHeight / img.Height)
                img = New Bitmap(img, iconWidth, iconHeight)
                img.Save(iconPhysicalPath, Imaging.ImageFormat.Png)

                img.Dispose()
                img = Nothing
            End If
        End If
    End Sub

    Public Shared Sub DisplayImage(ByRef img As WebControls.Image, ByVal path As String, ByVal id As String)
        Dim url As String = GetImageURL(path, id)
        If String.IsNullOrEmpty(url) Then
            img.Visible = False
        Else
            img.ImageUrl = url
            img.Visible = True
        End If
    End Sub

    Public Shared Sub DisplayIcon(img As WebControls.Image, path As String, id As String)
        Dim url As String = GetIconURL(path, id)
        If String.IsNullOrEmpty(url) Then
            img.Visible = False
        Else
            img.ImageUrl = url
            img.Visible = True
        End If
    End Sub

    Public Shared Sub DeleteImages(ByVal path As String, ByVal id As String)
        Dim imagePhysicalPath As String = GetImagePhysicalPath(path, id)
        Dim iconPhysicalPath As String = GetIconPhysicalPath(path, id)
        If File.Exists(imagePhysicalPath) Then File.Delete(imagePhysicalPath)
        If File.Exists(iconPhysicalPath) Then File.Delete(iconPhysicalPath)
    End Sub

    Public Shared Function GetIconPhysicalPath(path As String, id As String) As String
        Return String.Format("{0}images\{1}\{1}{2}_icon.png", HttpContext.Current.Request.PhysicalApplicationPath, path, id)
    End Function

    Public Shared Function GetIconURL(path As String, id As String) As String
        Dim physicalPath As String = GetIconPhysicalPath(path, id)
        If Not File.Exists(physicalPath) Then
            Return String.Empty
        Else
            Return String.Format("{0}/images/{1}/{1}{2}_icon.png", HttpContext.Current.Request.ApplicationPath, path, id)
        End If
    End Function

    Public Shared Function GetImagePhysicalPath(path As String, id As String) As String
        Return String.Format("{0}images\{1}\{1}{2}.png", HttpContext.Current.Request.PhysicalApplicationPath, path, id)
    End Function

    Public Shared Function GetImageURL(path As String, id As String) As String
        Dim physicalPath As String = GetImagePhysicalPath(path, id)
        If Not File.Exists(physicalPath) Then
            Return String.Empty
        Else
            Return String.Format("{0}/images/{1}/{1}{2}.png", HttpContext.Current.Request.ApplicationPath, path, id)
        End If
    End Function
End Class

