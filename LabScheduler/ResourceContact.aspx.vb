﻿Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ResourceContact
        Inherits SchedulerPage

        Public Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            If PathInfo.Current.ResourceID = 0 Then
                Response.Redirect("~/")
            End If
            If Not Page.IsPostBack Then
                Helpdesk1.ResourceID = PathInfo.Current.ResourceID
            End If
        End Sub

    End Class
End Namespace