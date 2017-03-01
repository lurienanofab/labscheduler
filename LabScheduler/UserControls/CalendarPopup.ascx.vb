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

Public Class CalendarPopup
    Inherits System.Web.UI.UserControl

    Public Enum ControlDisplayOption
        TextboxImage
    End Enum

    Private _ControlDisplay As ControlDisplayOption

    Public Property Text As String
        Get
            Return imgCalendarImage.ToolTip
        End Get
        Set(value As String)
            imgCalendarImage.ToolTip = value
        End Set
    End Property

    Public Property SelectedDate As DateTime
        Get
            Return DateTime.Parse(txtSelectedDate.Text)
        End Get
        Set(value As DateTime)
            txtSelectedDate.Text = value.ToString("MM/dd/yyyy")
        End Set
    End Property

    Public Property ImageUrl As String
        Get
            Return imgCalendarImage.ImageUrl
        End Get
        Set(value As String)
            imgCalendarImage.ImageUrl = value
        End Set
    End Property

    Public Property Width As Unit
        Get
            Return txtSelectedDate.Width
        End Get
        Set(value As Unit)
            txtSelectedDate.Width = value
        End Set
    End Property

    Public Property Enabled As Boolean
        Get
            Return txtSelectedDate.Enabled
        End Get
        Set(value As Boolean)
            txtSelectedDate.Enabled = value
        End Set
    End Property

    Public Property ShowGoToToday As Boolean

    Public Property ControlDisplay As ControlDisplayOption
        Get
            Return _ControlDisplay
        End Get
        Set(value As ControlDisplayOption)
            txtSelectedDate.Attributes("data-controldisplay") = [Enum].GetName(GetType(ControlDisplayOption), _ControlDisplay)
        End Set
    End Property

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        txtSelectedDate.Attributes.Add("data-controldisplay", [Enum].GetName(GetType(ControlDisplayOption), _ControlDisplay))
    End Sub

End Class