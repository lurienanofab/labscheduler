﻿Imports LNF.Data
Imports LNF.Impl.Repository.Data
Imports LNF.Repository
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class ErrorPage
        Inherits SchedulerPage

        Private appName As String = "Scheduler"
        Private fromAddr As String = "system@lnf.umich.edu"
        Private toAddr As String() = {"lnf-it@umich.edu"}

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return 0
            End Get
        End Property

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
            Try
                Dim err As Integer() = ParseQueryStringValue(Request.QueryString("err"))
                If err.Length = 0 Then
                    panError.Visible = False
                    panNoError.Visible = True
                Else
                    rptError.DataSource = GetErrorData(err)
                    rptError.DataBind()
                End If
            Catch ex As Exception
                rptError.DataSource = New ErrorLog() {New ErrorLog() With {.Application = "Scheduler", .ClientID = 0, .ErrorDateTime = DateTime.Now, .ErrorLogID = 0, .Message = ex.Message, .StackTrace = ex.StackTrace, .PageUrl = Request.Url.ToString()}}
                rptError.DataBind()
            End Try
        End Sub

        Private Function ParseQueryStringValue(value As String) As Integer()
            If String.IsNullOrEmpty(value) Then
                Return New Integer() {}
            End If

            Dim result As New List(Of Integer)
            Dim splitter As String() = value.Split(","c)
            For Each s As String In splitter
                Dim i As Integer
                If Integer.TryParse(s, i) Then
                    result.Add(i)
                End If
            Next
            Return result.ToArray()
        End Function

        Private Function GetErrorData(err As Integer()) As ErrorLog()
            Dim query As ErrorLog() = DataSession.Query(Of ErrorLog)().Where(Function(x) err.Contains(x.ErrorLogID)).ToArray()
            Return query
        End Function
    End Class
End Namespace