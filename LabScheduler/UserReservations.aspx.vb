Imports LNF.Feeds
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class UserReservations
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not Page.IsPostBack Then
                lblDate.Text = Request.SelectedDate().ToLongDateString()

                If CurrentUser IsNot Nothing Then
                    litCurrentUser.Text = CurrentUser.DisplayName
                    txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, CurrentUser.UserName, "all", "user-reservations")
                    If CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                        hypRecurringPage.Visible = True
                    End If
                Else
                    litCurrentUser.Text = "[unknown user]"
                End If

                litLocation.Text = $"{If(Context.ClientInLab(), "Inside", "Outside")} lab, kiosk: {If(KioskUtility.IsKiosk(Request.UserHostAddress), "yes", "no")} [{Request.UserHostAddress}]"

                hypRecurringPage.NavigateUrl = String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", Request.SelectedDate())
            End If

            SetCurrentView(ViewType.UserView)
        End Sub
    End Class
End Namespace