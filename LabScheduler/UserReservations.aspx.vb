Imports LNF.Data
Imports LNF.Feeds
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class UserReservations
        Inherits SchedulerPage

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim sw As Stopwatch = Stopwatch.StartNew()
            Helper.AppendLog($"UserReservations.Page_Load: Started...")

            If Not Page.IsPostBack Then
                litDate.Text = ContextBase.Request.SelectedDate().ToLongDateString()

                If CurrentUser IsNot Nothing Then
                    litCurrentUser.Text = CurrentUser.DisplayName
                    txtCalendarURL.Text = FeedGenerator.Scheduler.Reservations.GetUrl(FeedFormats.Calendar, CurrentUser.UserName, "all", "user-reservations", Request.Url)
                    If CurrentUser.HasPriv(ClientPrivilege.Staff) Then
                        hypRecurringPage.Visible = True
                    End If
                Else
                    litCurrentUser.Text = "[unknown user]"
                End If

                Dim clientLab = Helper.ClientLab()
                Dim labDisplayName = If(clientLab Is Nothing, String.Empty, clientLab.LabDisplayName)

                litLocation.Text = $"{If(Helper.IsInLab(), "Inside " + labDisplayName, "Outside")}"

                litComputer.Text = $"IP={Request.UserHostAddress}, Browser={GetBrowser()}, Kiosk={If(Helper.IsOnKiosk(), "Yes", "No")}, Https={If(Request.IsSecureConnection, "Yes", "No")}"

                hypRecurringPage.NavigateUrl = String.Format("~/UserRecurringReservation.aspx?Date={0:yyyy-MM-dd}", ContextBase.Request.SelectedDate())
            End If

            SetCurrentView(ViewType.UserView)

            Helper.AppendLog($"UserReservations.Page_Load: Completed in {sw.Elapsed.TotalSeconds:0.0000} seconds")
            sw.Stop()
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