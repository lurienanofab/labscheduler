Imports LNF.Cache
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
                lblDate.Text = Request.GetCurrentDate().ToLongDateString()

                If CurrentUser IsNot Nothing Then
                    litCurrentUser.Text = CurrentUser.DisplayName
                    txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, CurrentUser.UserName, "all", "user-reservations")
                    If CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                        hypRecurringPage.Visible = True
                    End If
                Else
                    litCurrentUser.Text = "[unknown user]"
                End If

                hypRecurringPage.NavigateUrl = String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", Request.GetCurrentDate())

                CacheManager.Current.CurrentUserState().AddAction("Viewing My Reservations page.")
            End If

            SetCurrentView(ViewType.UserView)
        End Sub
    End Class
End Namespace