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
                litDate.Text = ContextBase.Request.SelectedDate().ToLongDateString()

                If CurrentUser IsNot Nothing Then
                    litCurrentUser.Text = CurrentUser.DisplayName
                    txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, CurrentUser.UserName, "all", "user-reservations")
                    If CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                        hypRecurringPage.Visible = True
                    End If
                Else
                    litCurrentUser.Text = "[unknown user]"
                End If

                Dim clientLab = ContextBase.ClientLab()
                Dim labDisplayName = If(clientLab Is Nothing, String.Empty, clientLab.LabDisplayName)

                litLocation.Text = $"{If(ContextBase.ClientInLab(), "Inside " + labDisplayName, "Outside")}"

                litComputer.Text = $"IP={Request.UserHostAddress}, Browser={GetBrowser()}, Kiosk={If(KioskUtility.IsKiosk(Request.UserHostAddress), "Yes", "No")}"

                hypRecurringPage.NavigateUrl = String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
            End If

            SetCurrentView(ViewType.UserView)
        End Sub

        Private Function GetBrowser() As String

            Dim result As String = Request.Browser.Type

            If result.StartsWith("Chrome") Then
                If Request.UserAgent.Contains("Edge") Then
                    result += " [Edge]"
                ElseIf Request.UserAgent.Contains("OPR") OrElse Request.UserAgent.Contains("Opera") Then
                    result += " [Opera]"
                End If
            End If

            Return result
        End Function
    End Class
End Namespace