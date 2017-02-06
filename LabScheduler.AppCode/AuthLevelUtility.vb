Imports System.Web.UI.WebControls

Public Class AuthLevelUtility
    ' Given a list of auth levels, returns the auth level value
    Public Shared Function GetAuthLevelValue(ByRef items As ListItemCollection) As Integer
        Dim Value As Integer = 0
        For i As Integer = 0 To items.Count - 1
            If items(i).Selected Then Value += Convert.ToInt32(items(i).Value)
        Next
        Return Value
    End Function

    Public Shared Function GetAuthLevelValue(items As RepeaterItemCollection, checkboxId As String) As Integer
        Dim result As Integer = 0
        For Each item As RepeaterItem In items
            Dim chk As CheckBox = CType(item.FindControl(checkboxId), CheckBox)
            Dim hid As HiddenField = CType(item.FindControl("hidAuthLevelValue"), HiddenField)
            If chk IsNot Nothing AndAlso hid IsNot Nothing AndAlso chk.Checked Then
                result += Convert.ToInt32(hid.Value)
            End If
        Next
        Return result
    End Function

    ' Given a list of auth levels and the auth level, checks the list with approp. values
    Public Shared Sub SetAuthLevel(ByRef items As ListItemCollection, ByVal Value As Integer)
        For i As Integer = 0 To items.Count - 1
            items(i).Selected = LNF.CommonTools.Utility.CompareFlags(Convert.ToInt32(items(i).Value), Value)
        Next
    End Sub

    Public Shared Sub SetAuthLevel(items As RepeaterItemCollection, value As Integer, checkboxId As String)
        For Each item As RepeaterItem In items
            Dim chk As CheckBox = CType(item.FindControl(checkboxId), CheckBox)
            Dim hid As HiddenField = CType(item.FindControl("hidAuthLevelValue"), HiddenField)
            If chk IsNot Nothing AndAlso hid IsNot Nothing Then
                Dim flag As Integer
                If Integer.TryParse(hid.Value, flag) Then
                    chk.Checked = LNF.CommonTools.Utility.CompareFlags(Convert.ToInt32(hid.Value), value)
                End If
            End If
        Next
    End Sub
End Class
