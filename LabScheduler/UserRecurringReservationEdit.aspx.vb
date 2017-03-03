Imports LNF.Cache
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class UserRecurringReservationEdit
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                litMessage.Text = String.Empty

                hidClientID.Value = CacheManager.Current.CurrentUser.ClientID.ToString()

                Dim id As Integer
                If Integer.TryParse(Request.QueryString("id"), id) Then
                    'EditReservationRecurrence(id)
                    divRecurrenceDetail.Attributes("data-id") = id.ToString()
                Else
                    DisplayError("Missing paramter: id")
                End If
            End If
        End Sub

        Private Sub EditReservationRecurrence(id As Integer)
            Dim resourceUrl As String = VirtualPathUtility.ToAbsolute("~/ResourceDayWeek.aspx?resource=%id")
            Dim returnUrl As String = VirtualPathUtility.ToAbsolute("~/UserRecurringReservations2.aspx")
            Dim data As Object() = {New With {.RecurrenceID = id, .ResourceUrl = resourceUrl, .ReturnUrl = returnUrl}}
            rptRecurrence.DataSource = data
            rptRecurrence.DataBind()
        End Sub

        Private Sub DisplayError(msg As String)
            litMessage.Text = String.Format("<div style=""color: red;"">{0}</div>", msg)
        End Sub

    End Class
End Namespace