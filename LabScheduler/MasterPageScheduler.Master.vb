﻿'Copyright 2017 University of Michigan

'Licensed under the Apache License, Version 2.0 (the "License");
'you may Not use this file except In compliance With the License.
'You may obtain a copy Of the License at

'http://www.apache.org/licenses/LICENSE-2.0

'Unless required by applicable law Or agreed To In writing, software
'distributed under the License Is distributed On an "AS IS" BASIS,
'WITHOUT WARRANTIES Or CONDITIONS Of ANY KIND, either express Or implied.
'See the License For the specific language governing permissions And
'limitations under the License.

Imports LNF.Cache
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class MasterPageScheduler
        Inherits SchedulerMasterPage

        Public Overrides ReadOnly Property ShowMenu As Boolean
            Get
                If Request.QueryString("menu") = "1" Then
                    Return True
                Else
                    Dim result As Boolean
                    If Boolean.TryParse(ConfigurationManager.AppSettings("ShowMenu"), result) Then
                        Return result
                    Else
                        Return False
                    End If
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property AddScripts As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property AddStyles As Boolean
            Get
                Return False
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Dim startTime As Date = Date.Now

            If Not String.IsNullOrEmpty(Request.QueryString("error")) Then
                Throw New Exception(Request.QueryString("error"))
            End If

            Calendar1.SelectedDate = CacheManager.Current.CurrentUserState().Date
            Calendar1.ReturnTo = Request.Url.PathAndQuery

            If Not IsPostBack Then
                If Not Page.User.IsInRole("Administrator") Then
                    hypAdmin.Visible = False
                    lblAdminSeparator.Visible = False
                End If

                If Not Page.User.IsInRole("Staff") Then
                    hypOther.Visible = False
                    lblOtherSeparator.Visible = False
                End If
            End If

            RequestLog.Append("MasterPageScheduler.Page_Load: {0}", Date.Now - startTime)
        End Sub
    End Class
End Namespace