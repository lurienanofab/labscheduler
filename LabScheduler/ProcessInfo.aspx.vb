Public Class ProcessInfo
    Inherits Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim resourceId As Integer
        If Integer.TryParse(Request.QueryString("ResourceID"), resourceId) Then
            phProcessInfo.Visible = True
            phSelectTool.Visible = False
            divProcessInfo.Attributes.Add("data-resource-id", resourceId.ToString())
            divProcessInfo.Attributes.Add("data-ajax-url", VirtualPathUtility.ToAbsolute("~/ajax/processinfo.ashx"))
        Else
            phProcessInfo.Visible = False
            phSelectTool.Visible = True
        End If
    End Sub

    Protected Sub BtnSelectTool_Click(sender As Object, e As EventArgs)
        Dim resourceId As Integer
        If Integer.TryParse(txtResourceID.Text, resourceId) Then
            Response.Redirect($"~/ProcessInfo.aspx?ResourceID={resourceId}")
        End If
    End Sub
End Class