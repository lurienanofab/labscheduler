Imports LNF.Cache
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace UserControls
    Public Class ResourceInfo
        Inherits SchedulerUserControl

        Private _selectedPath As PathInfo

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim startTime As Date = Date.Now

            _selectedPath = ContextBase.Request.SelectedPath()

            If Not Page.IsPostBack Then
                If _selectedPath.ResourceID = 0 Then
                    Visible = False
                    Return
                End If

                ' Resource Engineers
                Dim res As IResource = Nothing

                Try
                    res = Helper.GetResource(_selectedPath)
                    If res Is Nothing Then
                        Return
                    End If
                Catch ex As Exception
                    Return
                End Try

                'Session("tool-engineers") = DA.Current.Query(Of ResourceClient)() _
                '    .Where(Function(x) x.Resource.ResourceID = res.ResourceID).ToArray() _
                '    .Where(Function(x) x.HasAuth(ClientAuthLevel.ToolEngineer)).ToArray()

                Dim item As New ResourceInfoItem() With {
                    .ResourceID = res.ResourceID,
                    .ResourceName = String.Format("{0}: {1}", res.ResourceID, res.ResourceName),
                    .ReservationFence = Convert.ToInt32(TimeSpan.FromMinutes(res.ReservFence).TotalHours),
                    .MinReservationTime = Convert.ToInt32(res.MinReservTime),
                    .MaxReservationTime = Convert.ToInt32(TimeSpan.FromMinutes(res.MaxReservTime).TotalHours),
                    .MaxAlloc = Convert.ToInt32(TimeSpan.FromMinutes(res.MaxAlloc).TotalHours),
                    .MinCancelTime = Convert.ToInt32(res.MinCancelTime),
                    .GracePeriod = Convert.ToInt32(res.GracePeriod),
                    .AutoEnd = Convert.ToInt32(res.ResourceAutoEnd)
                }

                Dim perUseCost As Decimal = 0
                Dim perHourCost As Decimal = 0

                Dim cost As IResourceCost = CacheManager.Current.GetResourceCost(res.ResourceID, CurrentUser.MaxChargeTypeID)

                If cost IsNot Nothing Then
                    perUseCost = cost.PerUseRate()
                    perHourCost = cost.HourlyRate()
                End If

                item.HourlyCost = String.Format("{0:C}/use + {1:C}/hr", perUseCost, perHourCost)

                hidResourceID.Value = res.ResourceID.ToString()

                rptResourceInfo.DataSource = {item}
                rptResourceInfo.DataBind()
            End If

            RequestLog.Append("ResourceInfo.PageLoad: {0}", Date.Now - startTime)
        End Sub

        Private Function GetToolEngineers() As List(Of ToolEngineerItem)
            Dim selectedDate As Date = ContextBase.Request.SelectedDate()

            Dim result As List(Of ToolEngineerItem) = New List(Of ToolEngineerItem)()
            Dim toolEngineers As IList(Of IResourceClient) = CacheManager.Current.ToolEngineers(_selectedPath.ResourceID).ToList()

            If String.IsNullOrEmpty(hidResourceID.Value) OrElse toolEngineers Is Nothing OrElse toolEngineers.Count = 0 Then
                'tdEngineers.InnerText = "Unknown"
            Else
                'Sometimes dtEngineers contains every tool engineer. This happens when we are in the
                'Resources Administration tab. This means we should always select the engineers for
                'the current resource.

                For Each te As IResourceClient In toolEngineers
                    Dim item As New ToolEngineerItem(_selectedPath, selectedDate) With {
                        .ClientID = te.ClientID,
                        .DisplayName = te.DisplayName,
                        .Email = te.Email
                    }
                    result.Add(item)
                Next
            End If

            Return result
        End Function

        Protected Sub RptResourceInfo_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
            If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
                Dim rptToolEngineers As Repeater = CType(e.Item.FindControl("rptToolEngineers"), Repeater)
                rptToolEngineers.DataSource = GetToolEngineers()
                rptToolEngineers.DataBind()
            End If
        End Sub
    End Class

    Public Class ResourceInfoItem
        Public Property ResourceID As Integer
        Public Property ResourceName As String
        Public Property ReservationFence As Integer
        Public Property MinReservationTime As Integer
        Public Property MaxReservationTime As Double
        Public Property MaxAlloc As Integer
        Public Property MinCancelTime As Integer
        Public Property GracePeriod As Integer
        Public Property HourlyCost As String
        Public Property AutoEnd As Integer
    End Class

    Public Class ToolEngineerItem
        Public Property ClientID As Integer
        Public Property DisplayName As String
        Public Property Email As String
        Public Property SelectedPath As PathInfo
        Public Property SelectedDate As Date

        Public Sub New(selectedPath As PathInfo, selectedDate As Date)
            Me.SelectedPath = selectedPath
            Me.SelectedDate = selectedDate
        End Sub

        Public ReadOnly Property Url As String
            Get
                Return VirtualPathUtility.ToAbsolute(String.Format("~/Contact.aspx?ClientID={0}&Path={1}&Date={2:yyyy-MM-dd}", ClientID, SelectedPath, SelectedDate))
            End Get
        End Property
    End Class
End Namespace