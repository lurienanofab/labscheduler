Imports LNF.Cache
Imports LNF.Models.Scheduler
Imports LNF.Scheduler
Imports LNF.Web
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace UserControls
    Public Class ResourceInfo
        Inherits SchedulerUserControl

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            Dim startTime As Date = Date.Now

            If Not Page.IsPostBack Then
                If PathInfo.Current.ResourceID = 0 Then
                    Visible = False
                    Return
                End If

                ' Resource Engineers
                Dim res As ResourceModel = PathInfo.Current.GetResource()

                'Session("tool-engineers") = DA.Current.Query(Of ResourceClient)() _
                '    .Where(Function(x) x.Resource.ResourceID = res.ResourceID).ToArray() _
                '    .Where(Function(x) x.HasAuth(ClientAuthLevel.ToolEngineer)).ToArray()

                Dim item As New ResourceInfoItem()
                item.ResourceID = res.ResourceID
                item.ResourceName = String.Format("{0}: {1}", res.ResourceID, res.ResourceName)
                item.ReservationFence = Convert.ToInt32(res.ReservFence.TotalHours)
                item.MinReservationTime = Convert.ToInt32(res.MinReservTime.TotalMinutes)
                item.MaxReservationTime = Convert.ToInt32(res.MaxReservTime.TotalHours)
                item.MaxAlloc = Convert.ToInt32(res.MaxAlloc.TotalHours)
                item.MinCancelTime = Convert.ToInt32(res.MinCancelTime.TotalMinutes)
                item.GracePeriod = Convert.ToInt32(res.GracePeriod.TotalMinutes)
                item.AutoEnd = Convert.ToInt32(res.AutoEnd.TotalMinutes)

                Dim perUseCost As Decimal = 0
                Dim perHourCost As Decimal = 0

                Dim costs As IList(Of ResourceCostModel) = CacheManager.Current.ToolCosts(Date.Now, res.ResourceID, CacheManager.Current.MaxChargeTypeID)

                If costs.Count > 0 Then
                    perUseCost = costs(0).AddVal
                    perHourCost = costs(0).MulVal
                End If

                item.HourlyCost = String.Format("{0:C}/use + {1:C}/hr", perUseCost, perHourCost)

                hidResourceID.Value = res.ResourceID.ToString()

                rptResourceInfo.DataSource = {item}
                rptResourceInfo.DataBind()
            End If

            RequestLog.Append("ResourceInfo.PageLoad: {0}", Date.Now - startTime)
        End Sub

        Private Function GetToolEngineers() As List(Of ToolEngineerItem)
            Dim result As List(Of ToolEngineerItem) = New List(Of ToolEngineerItem)()
            Dim toolEngineers As IList(Of ResourceClientModel) = CacheManager.Current.ToolEngineers(PathInfo.Current.ResourceID)

            If String.IsNullOrEmpty(hidResourceID.Value) OrElse toolEngineers Is Nothing OrElse toolEngineers.Count = 0 Then
                'tdEngineers.InnerText = "Unknown"
            Else
                'Sometimes dtEngineers contains every tool engineer. This happens when we are in the
                'Resources Administration tab. This means we should always select the engineers for
                'the current resource.
                For Each te As ResourceClientModel In toolEngineers
                    Dim item As New ToolEngineerItem()
                    item.ClientID = te.ClientID
                    item.DisplayName = te.DisplayName
                    item.Email = te.Email
                    result.Add(item)
                Next
            End If

            Return result
        End Function

        Protected Sub rptResourceInfo_ItemDataBound(sender As Object, e As RepeaterItemEventArgs)
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

        Public ReadOnly Property Url As String
            Get
                Return VirtualPathUtility.ToAbsolute(String.Format("~/Contact.aspx?ClientID={0}&Path={1}", ClientID, PathInfo.Current))
            End Get
        End Property
    End Class
End Namespace