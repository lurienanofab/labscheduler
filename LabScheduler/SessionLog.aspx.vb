Imports LNF
Imports LNF.Web.Models
Imports LNF.Web.Scheduler

Public Class SessionLog
    Inherits Page

    <Inject> Public Property Provider As IProvider

    Public Property ContextBase As HttpContextBase

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        ContextBase = New HttpContextWrapper(Context)
        Dim helper As New SchedulerContextHelper(ContextBase, Provider)

        Dim log As List(Of SessionLogMessage) = helper.GetLog().ToList()

        rptSessionLog.DataSource = log
        rptSessionLog.DataBind()
    End Sub
End Class