Imports LNF
Imports LNF.Cache
Imports LNF.CommonTools
Imports LNF.Data
Imports LNF.Models.Data
Imports LNF.Repository
Imports LNF.Repository.Data
Imports LNF.Repository.Scheduler
Imports LNF.Scheduler

Public Class OnTheFlyImpl

    Private ReadOnly key As Integer = 0
    Private ReadOnly cardswipedata As String = Nothing
    Private ReadOnly cardNum As String = Nothing
    Private otfresource As OnTheFlyResource = Nothing
    Private guid As Guid
    Private cardSwipeTime As Date
    Private clientWhoSwipedTheCard As Client = Nothing
    Private ReadOnly reservationID As Integer = -1  ' this may not be available in cases like first time request
    Private allReservationListAtTimeOfSwipe As IList(Of Reservation) = Nothing
    Private currentlyRunningReservationByUser As Reservation = Nothing
    Private currentlyRunningReservationMayNotBeUsers As Reservation = Nothing
    Private nextReservationInMinTime As Reservation = Nothing
    Private rreq As ResRequest = New ResRequest()
    Private logArray As List(Of OnTheFlyLog) = New List(Of OnTheFlyLog)
    Private returnMessage As String = ""
    Private isProcessFail As Boolean = False
    Private Shared ReadOnly otfActivity As Activity = DA.Current.Single(Of Activity)(6)
    Private physicalAccessUtil As PhysicalAccess.PhysicalAccessUtility

    Protected ReadOnly Property ReservationManager As IReservationManager
        Get
            Return ServiceProvider.Current.Use(Of IReservationManager)()
        End Get
    End Property

    Sub New(potfResource As OnTheFlyResource, pcardswipedata As String, ipAddr As String)
        otfresource = potfResource
        cardswipedata = pcardswipedata
        cardNum = ReservationOnTheFlyUtil.GetCardNumber(pcardswipedata)
        rreq.CardNum = cardNum
        rreq.IPAddress = ipAddr
        rreq.OnTheFlyName = otfresource.Resource.ResourceName
        rreq.ResourceID = otfresource.Resource.ResourceID
        physicalAccessUtil = New PhysicalAccess.PhysicalAccessUtility(ipAddr)
        Init()
    End Sub

    Private Sub Init()
        guid = Guid.NewGuid()
        cardSwipeTime = Date.Now
        ' get the user details from the cardnum     
        Dim bcc As BadgeCardClient = GetBadgeCardClient(cardNum)
        If bcc IsNot Nothing Then
            clientWhoSwipedTheCard = bcc.Client
        End If
    End Sub

    Public Function GetReturnMessage() As String
        Return returnMessage
    End Function

    Public Function IsProcessFailed() As Boolean
        Return IsProcessFailed
    End Function

    Public Shared Function GetBadgeCardClient(ByVal cardnum As String) As BadgeCardClient
        Dim bcc As BadgeCardClient = DA.Current.Query(Of BadgeCardClient).Where(Function(x) x.CardNumber = cardnum).FirstOrDefault()
        Return bcc
    End Function

    Private Sub Log(ByVal funcName As String, ByVal res As Boolean)
        Log(funcName, res.ToString())
    End Sub

    Public Sub Log(ByVal funcName As String, ByVal data As String)
        ' Append to returnMessage to send it back to client
        returnMessage += "[" + funcName + ", " + data + "]"

        Dim le As New OnTheFlyLog With {
            .LogGuid = guid,
            .LogTimeStamp = DateTime.Now,
            .ActionName = funcName,
            .ActionData = data,
            .ResourceID = rreq.ResourceID,
            .IPAddress = rreq.IPAddress
        }

        logArray.Add(le)
    End Sub

    '---------------------------_condition _Functions --------------------
    Public Function _IsCabinet() As Boolean
        Return otfresource.IsCabinet()
    End Function

    Public Function _ExistingReservation() As Boolean  'is there currently any reservation(which ever state, running or not activated) at this time on this tool ?
        If allReservationListAtTimeOfSwipe Is Nothing Then
            allReservationListAtTimeOfSwipe = ReservationManager.SelectExisting(GetResource())
            If (allReservationListAtTimeOfSwipe.Count > 1) Then
                Log("_[1]ExistingReservation", "There is more than one current reservation, how can this be??")
            End If
            'allReservationListAtTimeOfSwipe = LNF.Repository.Scheduler.Reservation.DataAccess.SelectByResource(getResource().ResourceID, cardSwipeTime, cardSwipeTime.AddMinutes(1), False)   '--------Checking what reservations in a min
            If allReservationListAtTimeOfSwipe.Count > 0 Then
                Log("_[1]ExistingReservation", "True")
                'isExistingReservation = True
                Return True
            End If
        End If
        'Log("_ExistingReservation", "{Result:" + result.ToString() + ",ReservationID:" + existingRes.ReservationID.ToString() + "}")
        Log("_[1]ExistingReservation", "False")
        'isExistingReservation = False
        Return False
    End Function

    Public Function _Running() As Boolean ' is that reservation activated or still need to be activated(this is the scheduler reservation)
        If allReservationListAtTimeOfSwipe IsNot Nothing Then
            Dim running As Reservation = allReservationListAtTimeOfSwipe.FirstOrDefault(Function(x) x.IsRunning())
            If running IsNot Nothing Then
                Log("_[2]Running", True)
                currentlyRunningReservationMayNotBeUsers = running
                Return True
            End If
        End If

        Log("_[2]Running", False)
        Return False
    End Function

    Public Function _IsExistingAnOTFReservation() As Boolean
        If currentlyRunningReservationMayNotBeUsers IsNot Nothing Then
            Dim ronf As ReservationOnTheFly = DA.Current.Query(Of ReservationOnTheFly)().FirstOrDefault(Function(x) x.Reservation.ReservationID = currentlyRunningReservationMayNotBeUsers.ReservationID)
            If ronf IsNot Nothing Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function _IsAnotherReservationStartInMinimumTime() As Boolean
        Dim reservationsInMinimumTime As IList(Of Reservation) = GetResource().InGranularityWindow()
        If reservationsInMinimumTime IsNot Nothing Then
            If reservationsInMinimumTime.Count > 0 Then
                If currentlyRunningReservationMayNotBeUsers IsNot Nothing Then
                    If currentlyRunningReservationMayNotBeUsers.ReservationID <> reservationsInMinimumTime.ElementAt(0).ReservationID Then
                        Log("_[4]_isAnotherReservationStartInMinimumTime", True)
                        Return True
                    End If
                End If
            End If
        End If
        Log("_[4]_isAnotherReservationStartInMinimumTime:1", False)
        Return False
    End Function

    'Public Sub EndCurrentAndCreateStartNewReservation()
    '	EndExisitingIfExistsAndStartNextReservation()
    'End Sub

    Public Function _InGroupExisitingReservation() As Boolean ' scheduler path
        ' is current user in group of an exisiting and running reservation ?
        ' get the list of group who are belong to the current reservation
        If allReservationListAtTimeOfSwipe IsNot Nothing Then
            For Each aRsv As Reservation In allReservationListAtTimeOfSwipe
                If IsUserInTheReservationInviteesList(aRsv, clientWhoSwipedTheCard) Then
                    currentlyRunningReservationByUser = aRsv
                    Log("_[3]InGroupExisitingReservation", True)
                    Return True
                End If
            Next
        End If

        Log("_[3]InGroupExisitingReservation", False)
        Return False
    End Function

    Public Function _IsCreateAndStart() As Boolean
        Return otfresource.IsCreateAndStart()
    End Function

    Public Function _DoesAnotherReservationStartWithInGranularity() As Boolean
        Dim reservationsInGranularityPeriod As IList(Of Reservation) = GetResource().InGranularityWindow()

        If reservationsInGranularityPeriod IsNot Nothing Then
            If reservationsInGranularityPeriod.Count > 0 Then
                If GetCurrentReservationIfExists().ReservationID <> reservationsInGranularityPeriod.ElementAt(0).ReservationID Then
                    Log("_[4]DoesAnotherReservationStartWithInGranularity", True)
                    Return True
                Else
                    Log("_[4]DoesAnotherReservationStartWithInGranularity:1", False)
                    Return False
                End If
            End If
        End If

        Log("_[4]DoesAnotherReservationStartWithInGranularity:2", False)

        Return False
    End Function

    Private Function GetReservationWhichStartsInMinReservationTime() As Reservation
        ' what is minmum reservation time for this tool.
        ' in currenttime + minimum reservation time is there a new reservation ?
        Dim nextReservation As Reservation = GetNextReservationIfExists()
        If nextReservation IsNot Nothing Then
            If (Date.Now.AddMinutes(GetResource().MinReservTime)) > nextReservation.BeginDateTime Then
                Return nextReservation
            End If
        End If
        Return Nothing
    End Function

    Public Function _DoesAnotherReservationStartWithInMinReservationTime() As Boolean
        Dim nexRestInMinTime = GetReservationWhichStartsInMinReservationTime()
        If nexRestInMinTime IsNot Nothing Then
            Log("_[2.2]DoesAnotherReservationStartWithInMinReservationTime", "True")
            Return True
        End If

        Log("_[2.2]DoesAnotherReservationStartWithInMinReservationTime", "False")
        Return False
    End Function

    Public Function _InGroupOfNextReservation() As Boolean
        ' there is next reservation. this is already checked, 
        ' who are all(group) there in the next reservation ?
        ' is current user(who swiped the card) is in the next reservation ?
        Dim result As Boolean = False
        Dim nextReservation As Reservation = GetNextReservationIfExists()
        If nextReservation IsNot Nothing Then
            result = IsUserInTheReservationInviteesList(nextReservation, clientWhoSwipedTheCard)
        End If

        Log("_[2.3]InGroupOfNextReservation", result)
        Return result
    End Function

    Public Function _AfterGracePeriod() As Boolean
        ' is it after the next reservation  and in the graceperiod ?
        Dim result As Boolean = False
        Dim currentReseration As Reservation = GetCurrentReservationIfExists()
        If currentReseration IsNot Nothing Then
            Dim dt As Date = currentReseration.EndDateTime.AddMinutes(GetResource().GracePeriod)
            If cardSwipeTime < dt Then
                result = True
            End If
        End If
        Log("_AfterGracePeriod", result)
        Return result
    End Function

    Public Function _KeepAlive() As Boolean
        ' is KeepAlive True for the current reservation ?
        Dim result As Boolean = False
        Dim currentReseration As Reservation = GetCurrentReservationIfExists()
        If currentReseration IsNot Nothing Then
            result = currentReseration.KeepAlive
        End If

        Log("_KeepAlive", result)
        Return result
    End Function

    Public Function _IsUserAuthorizedOnTool() As Boolean
        If IsNothing(clientWhoSwipedTheCard) Then
            Return False
        End If

        If clientWhoSwipedTheCard.HasPriv(ClientPrivilege.Staff Or ClientPrivilege.Administrator) Then
            Log("[1]_IsUserAuthOnTool", "True")
            Return True
        End If

        Dim resoID = GetResource().ResourceID

        'is it for everyone? ResourceClient   ResourceID AuthLevel is 2 Expiration date is > datetimenow and ClientID is -1
        Dim isForEveryOne As IEnumerable(Of ResourceClient) = DA.Current.Query(Of ResourceClient)().Where(Function(x) x.ClientID = -1 AndAlso x.Resource.ResourceID = resoID AndAlso (x.Expiration Is Nothing OrElse x.Expiration.Value > Date.Now))
        If isForEveryOne IsNot Nothing Then
            If isForEveryOne.Count > 0 Then
                Return True
            End If
        End If

        Dim allRcs As IEnumerable(Of ResourceClient) = DA.Current.Query(Of ResourceClient)().Where(Function(x) x.ClientID = clientWhoSwipedTheCard.ClientID AndAlso x.Resource.ResourceID = resoID AndAlso x.AuthLevel > 1)
        If allRcs IsNot Nothing Then
            For Each resClient As ResourceClient In allRcs
                'Dim resClient As ResourceClient = allRcs
                If resClient IsNot Nothing Then
                    Log("[2]_IsUserAuthOnTool", "True")
                    Return True
                End If
            Next
        End If

        Log("_IsUserAuthOnTool", "False")
        Return False
    End Function

    '------------------------Utility methods -------------------------
    Private Function IsUserInTheReservationInviteesList(ByVal rsv As Reservation, ByVal currentClient As Client) As Boolean
        If rsv IsNot Nothing Then
            Dim allInvs As IList(Of ReservationInvitee) = ReservationManager.GetInvitees(rsv.ReservationID).ToList()
            If allInvs IsNot Nothing Then
                Dim isReservedOrIsInvted As ReservationInvitee = allInvs.Where(Function(x) x.Invitee.ClientID = currentClient.ClientID).FirstOrDefault()

                If isReservedOrIsInvted Is Nothing Then
                    If rsv.Client.ClientID = clientWhoSwipedTheCard.ClientID Then
                        Return True
                    End If
                    Return False
                Else
                    Return True
                End If

                'If allInvs.Count > 0 Then
                '    For Each eachInvitee In allInvs
                '        If eachInvitee.Invitee.ClientID = currentClient.ClientID Then
                '            Return True
                '        End If
                '    Next
                'End If
            End If
        End If
        Return False
    End Function

    Private Function GetCurrentReservationIfExists() As Reservation
        Return currentlyRunningReservationByUser
    End Function

    Private Function GetNextReservationIfExists() As Reservation  ' startable next reservation
        If nextReservationInMinTime Is Nothing Then
            Dim minResTime As Integer = GetResource().MinReservTime
            Dim nextNearestReservationInMinimumTime As IList(Of Reservation) = ReservationManager.SelectByResource(GetResourceID(), cardSwipeTime, cardSwipeTime.AddMinutes(minResTime), False)

            If nextNearestReservationInMinimumTime IsNot Nothing Then
                If nextNearestReservationInMinimumTime.Count > 0 Then
                    'res = nextNearestReservationInMinimumTime.OrderBy(Function(x) x.BeginDateTime).FirstOrDefault()
                    nextReservationInMinTime = nextNearestReservationInMinimumTime.Where(Function(x) x.ActualBeginDateTime Is Nothing).FirstOrDefault() ' TODO: does this also need to check if there is more than one reservation in min time, which one starts first ?
                End If
            End If
        End If
        If nextReservationInMinTime Is Nothing Then
            Log("getNextReservationIfExists", "False")
        Else
            Log("getNextReservationIfExists", "True")
        End If

        Return nextReservationInMinTime
    End Function

    Private Function GetResource() As Resource
        Return otfresource.Resource
    End Function

    Private Function GetResourceID() As Integer
        Return otfresource.Resource.ResourceID
    End Function

    Private Function CreateAndStartReservation() As Integer
        'create and start reservation  ------------
        Dim rsv As Reservation = New Reservation()

        Dim res As Resource = GetResource()
        Dim rr As ResRequest = GetResRequest()

        rsv.Resource = res

        Dim defaultMinResTime As Integer = 5 ' 5 minutes
        If 0 = res.MinReservTime Then
            res.MinReservTime = defaultMinResTime
        End If

        Dim reservationStartDatetTime As Date = GetNearestStartTime(res.MinReservTime)
        Dim resDuration As Double = res.MinReservTime

        rsv.RecurrenceID = -1 'always -1 for non-recurring reservation
        rsv.MaxReservedDuration = resDuration
        rsv.Duration = resDuration
        rsv.Notes = "On The Fly Reservation"
        rsv.AutoEnd = True
        rsv.CreatedOn = Date.Now
        rsv.Activity = otfActivity 'Processing
        rsv.BeginDateTime = reservationStartDatetTime
        rsv.EndDateTime = reservationStartDatetTime.AddMinutes(defaultMinResTime)
        rsv.OriginalBeginDateTime = rsv.BeginDateTime
        rsv.OriginalEndDateTime = rsv.EndDateTime
        rsv.LastModifiedOn = Date.Now
        rsv.Client = clientWhoSwipedTheCard

        Dim account As Account = ClientPreferenceUtility.OrderAccountsByUserPreference(clientWhoSwipedTheCard.ClientID).FirstOrDefault()  ' first account in the list is default account

        If account IsNot Nothing Then
            rsv.Account = account
            ReservationManager.Insert(rsv, clientWhoSwipedTheCard.ClientID)
        Else
            Throw New InvalidOperationException("The default account could not be determined.")
        End If

        StartReservation(clientWhoSwipedTheCard, rsv.CreateReservationItemWithInvitees(), rr, res.MinReservTime)

        Return rsv.ReservationID
    End Function

    Private Function GetReservationClientItem(rsv As Models.Scheduler.ReservationItemWithInvitees, client As Client) As Models.Scheduler.ReservationClientItem
        Dim resourceClients As IEnumerable(Of Models.Scheduler.ResourceClientItem) = ReservationManager.GetResourceClients(rsv.ResourceID)
        Return New Models.Scheduler.ReservationClientItem With {
             .ClientID = client.ClientID,
             .ReservationID = rsv.ReservationID,
             .ResourceID = rsv.ResourceID,
             .IsReserver = rsv.ClientID = client.ClientID,
             .IsInvited = rsv.Invitees.Any(Function(x) x.ClientID = client.ClientID),
             .InLab = physicalAccessUtil.ClientInLab(client.ClientID, rsv.LabID),
             .UserAuth = ReservationManager.GetAuthLevel(resourceClients, client)
        }
    End Function

    Private Sub StartReservation(aclient As Client, rsv As Models.Scheduler.ReservationItemWithInvitees, rr As ResRequest, Optional reservationTime As Integer = -1)
        If rsv Is Nothing Then
            Throw New ArgumentNullException("rsv", "A null Reservation object is not allowed. [LabScheduler.AppCode.OnTheFlyImpl.StartReservation]")
        End If

        If rr Is Nothing Then
            Throw New ArgumentNullException("rr", "A null ResRequest object is not allowed.")
        End If

        ' also create a reservation row in the ReservationOnTF table
        Dim ronfly As ReservationOnTheFly = New ReservationOnTheFly With {
            .Reservation = DA.Current.Single(Of Reservation)(rsv.ReservationID)
        }

        ReservationManager.StartReservation(rsv, GetReservationClientItem(rsv, aclient))

        rr.ReservationID = rsv.ReservationID
        rr.ReservationTime = Convert.ToInt32(reservationTime)
        ronfly.CardNumber = rr.CardNum
        ronfly.IPAddress = rr.IPAddress
        ronfly.OnTheFlyName = rr.OnTheFlyName

        DA.Current.Insert(ronfly)
    End Sub

    Private Shared Function GetNearestStartTime(ByVal minResTimeInMins As Integer) As Date
        Dim pDt = New Date(Date.Now.Year, Date.Now.Month, Date.Now.Day, Date.Now.Hour, 0, 0)
        Dim resDT As Date = pDt.AddMinutes(minResTimeInMins * -1)

        While True
            resDT = resDT.AddMinutes(minResTimeInMins)
            If resDT > Date.Now Then
                Return resDT
            End If
        End While

        Return Nothing ' this will never be reached
    End Function

    Public Function GetResRequest() As ResRequest
        Return rreq
    End Function

    '------------------------Action methods --------------------------
    Public Sub EndExistingReservation()
        Dim currentReservation As Reservation = GetCurrentReservationIfExists()
        If currentReservation IsNot Nothing Then
            ReservationManager.EndReservation(currentReservation.ReservationID)
        End If
    End Sub

    Public Sub EndExisitingIfExistsAndStartNextReservation()  ' there is already next reservation reserved from scheduler
        EndExistingReservation()
        'start next
        Dim reservationId As Integer = CreateAndStartReservation()
        Log(String.Format("EndExisitingIfExistsAndStartNextReservation [ReservationID = {0}, WagoEnabled = {1}]", reservationId, CacheManager.Current.WagoEnabled()), True)
    End Sub

    Public Sub CreateAndStartNewReservation()
        Dim reservationId As Integer = CreateAndStartReservation()
        Log(String.Format("CreateAndStartNewReservation [ReservationID = {0}, WagoEnabled = {1}]", reservationId, CacheManager.Current.WagoEnabled()), True)
    End Sub

    Public Sub StartExistingReservation()
        If currentlyRunningReservationByUser.IsStarted Then
            Log("StartExistingReservation:AlreadyStarted", True)
        Else
            StartReservation(clientWhoSwipedTheCard, currentlyRunningReservationByUser.CreateReservationItemWithInvitees(), GetResRequest())
            Log("StartExistingReservation:Started", True)
        End If
    End Sub

    Public Sub StartNextReservation()
        Dim nexRestInMinTime = GetReservationWhichStartsInMinReservationTime()
        StartReservation(clientWhoSwipedTheCard, nexRestInMinTime.CreateReservationItemWithInvitees(), GetResRequest())
    End Sub

    Public Sub ExtendExistingReservation()
        Log("[5]ExtendExistingReservation", True)
        If currentlyRunningReservationByUser IsNot Nothing Then
            Dim minTimeInMinutes As Integer = -1
            Dim extraTime As Integer = GetResource().Granularity
            currentlyRunningReservationByUser.EndDateTime = currentlyRunningReservationByUser.EndDateTime.AddMinutes(extraTime)
        End If
    End Sub

    Public Sub Fail(func As String, Optional msg As String = "")
        isProcessFail = True
        Log("Fail-" + func, msg)
    End Sub

    Public Sub Swipe() 'starting point for OnTheFlyImpl

        Try
            Log("--SwipeStart--", cardNum)
            If _IsUserAuthorizedOnTool() Then
                OnTheFlyRules.Apply(Me)
            Else
                Fail("Swipe", "User not authorized on the tool")
            End If
            Log("--SwipeEnd--", cardNum)
            DA.Current.Insert(logArray)
        Catch ex As Exception
            Fail("Swipe", ex.Message)

            'Error
            Dim err As New ErrorLog With {
                .Application = "OnTheFly",
                .Message = ex.Message.Clip(500),
                .StackTrace = ex.StackTrace.Clip(4000),
                .ErrorDateTime = Date.Now,
                .ClientID = GetSwipedByClientID(),
                .PageUrl = "?"
            }

            DA.Current.Insert(err)
        Finally
            Log("--SwipeEnd--", cardNum)
        End Try
    End Sub

    Private Function GetSwipedByClientID() As Integer
        If clientWhoSwipedTheCard Is Nothing Then
            Return 0
        Else
            Return clientWhoSwipedTheCard.ClientID
        End If
    End Function

    Public Function EndReservation() As Integer
        Log("--SwipeStart-ForEnd--", cardNum)
        Dim result As Integer = 0
        If _IsUserAuthorizedOnTool() Then
            OnTheFlyRules.EndReservation(Me)
        Else
            result = -1
        End If
        Log("--SwipeEnd-ForEnd--", cardNum)
        DA.Current.Insert(logArray)
        Return (result)
    End Function
End Class

Public Class OnTheFlyRules   '-------------------------------------------------------------------------------
    Private Shared Sub Check_KeepAlive(ByVal oi As OnTheFlyImpl)
        If oi._KeepAlive() Then
            oi.Fail("_KeepAlive")
        Else
            oi.CreateAndStartNewReservation()
        End If
    End Sub

    Private Shared Sub Check_InGroupOfNextReservation(ByVal oi As OnTheFlyImpl)
        If oi._InGroupOfNextReservation() Then
            If oi._ExistingReservation() Then
                oi.EndExisitingIfExistsAndStartNextReservation()
            Else
                oi.StartNextReservation()
            End If
        Else
            oi.Fail("Check_InGroupOfNextReservation", "Not in group of next Reservation")
        End If
    End Sub

    Private Shared Sub Check_DoesAnotherReservationStartWithInMinimumTime(ByVal oi As OnTheFlyImpl)
        If oi._IsAnotherReservationStartInMinimumTime() Then
            'In group of Next reservation ?
            Check_InGroupOfNextReservation(oi)  ' is this same behaviour or different ?
        Else
            'End Current and Create and Start New
            oi.EndExisitingIfExistsAndStartNextReservation()
        End If
    End Sub

    Private Shared Sub Check_OTF_Reservation(ByVal oi As OnTheFlyImpl)
        If oi._IsExistingAnOTFReservation() Then
            Check_DoesAnotherReservationStartWithInMinimumTime(oi)
        Else
            oi.Fail("Check_OTF_Reservation")
        End If
    End Sub

    Private Shared Sub Check_InGroupExistingReservation_WhileRunning(ByVal oi As OnTheFlyImpl)
        If oi._InGroupExisitingReservation() Then
            Check_DoesAnotherReservationStartWithInGranularity(oi)
        Else
            Check_OTF_Reservation(oi) '   ------------------------- new flow 'is existing OTF
        End If
    End Sub

    Private Shared Sub Check_DoesAnotherReservationStartWithInGranularity(ByVal oi As OnTheFlyImpl)
        If oi._DoesAnotherReservationStartWithInGranularity() Then
            Check_InGroupOfNextReservation(oi)
        Else
            oi.ExtendExistingReservation()
        End If
    End Sub
    Private Shared Sub Check_AfterGracePeriod(ByVal oi As OnTheFlyImpl)
        If oi._AfterGracePeriod() Then
            Check_KeepAlive(oi)
        Else
            oi.Fail("Check_AfterGracePeriod")
        End If
    End Sub

    Private Shared Sub Check_InGroupExistingReservation_NotRunning(ByVal oi As OnTheFlyImpl)
        If oi._InGroupExisitingReservation() Then
            oi.StartExistingReservation()
        Else
            Check_AfterGracePeriod(oi)
        End If
    End Sub

    Private Shared Sub ApplyIsCreateAndStart(ByVal oi As OnTheFlyImpl)
        If oi._IsCreateAndStart() Then
            ApplyNonExistingReservation(oi)
        Else
            ' reservation_start_only is already handled in earlier step
            oi.Fail("ApplyIsCreateAndStart", "Unreachable code, Start-Already-Existing-Reservation, this is should be be handled in earlier step")
        End If
    End Sub

    Private Shared Sub ApplyNonExistingReservation(ByVal oi As OnTheFlyImpl)
        If oi._DoesAnotherReservationStartWithInMinReservationTime() Then
            Check_InGroupOfNextReservation(oi)
        Else
            oi.CreateAndStartNewReservation()
        End If
    End Sub

    Private Shared Sub ApplyExistingReservation(ByVal oi As OnTheFlyImpl)
        If oi._Running() Then
            Check_InGroupExistingReservation_WhileRunning(oi)
        Else
            Check_InGroupExistingReservation_NotRunning(oi)
        End If
    End Sub

    Private Shared Sub ApplyCabinet(ByVal oi As OnTheFlyImpl)
        oi.EndExisitingIfExistsAndStartNextReservation()
    End Sub

    Private Shared Sub ApplyTool(ByVal oi As OnTheFlyImpl)
        If oi._ExistingReservation() Then
            ApplyExistingReservation(oi)
        Else
            ApplyIsCreateAndStart(oi) 'ApplyNonExistingReservation(oi)
        End If
    End Sub

    Public Shared Sub Apply(ByVal oi As OnTheFlyImpl)
        If oi._IsCabinet() Then
            ApplyCabinet(oi)
        Else
            ApplyTool(oi)
        End If
    End Sub

    Public Shared Function EndReservation(ByVal oi As OnTheFlyImpl) As Integer
        If oi._ExistingReservation() Then ' is there an existing reservation?
            Return EndUserExistingReservation(oi)
        End If

        Return -1
    End Function

    Public Shared Function EndUserExistingReservation(ByVal oi As OnTheFlyImpl) As Integer
        If oi._InGroupExisitingReservation() Then
            oi.EndExistingReservation()
            Return 1
        End If

        Return -1
    End Function
End Class

