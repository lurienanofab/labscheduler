Imports LNF.CommonTools
Imports LNF.Web

Namespace Pages
    Public Class MasterPageBlank
        Inherits System.Web.UI.MasterPage

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        End Sub

        Protected Function GetStaticUrl(path As String) As String
            Return Utility.GetStaticUrl(path)
        End Function
    End Class
End Namespace