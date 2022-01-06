Imports LNF
Imports LNF.Cache
Imports LNF.Data
Imports LNF.DataAccess
Imports LNF.Impl.Repository.Scheduler
Imports LNF.PhysicalAccess
Imports LNF.Repository
Imports LNF.Scheduler

Public Class OnTheFlyImpl

    Private ReadOnly cardNum As String = Nothing
    Private ReadOnly otfresource As OnTheFlyResource = Nothing
    Private guid As Guid
    Private cardSwipeTime As Date
    Private clientWhoSwipedTheCard As IClient = Nothing
    Private allReservationListAtTimeOfSwipe As IEnumerable(Of IReservationItem) = Nothing
    Private currentlyRunningReservationByUser As IReservationItem = Nothing
    Private currentlyRunningReservationMayNotBeUsers As IReservationItem = Nothing
    Private nextReservationInMinTime As IReservationItem = Nothing
    Private ReadOnly rreq As New ResRequest()
    Private ReadOnly logArray As New List(Of OnTheFlyLog)
    Private returnMessage As String = ""
    Private ReadOnly otfActivity As IActivity
    Private ReadOnly physicalAccessUtil As PhysicalAccessUtility
    Private ReadOnly resourceItem As IResource = Nothing

    Public Property Now As Date = Date.Now

    Protected ReadOnly Property Provider As IProvider

    Protected ReadOnly Property DataSession As ISession
        Get
            Return Provider.DataAccess.Session
        End Get
    End Property

    Protected ReadOnly Property ReservationRepository As IReservationRepository
        Get
            Return Provider.Scheduler.Reservation
        End Get
    End Property

    Protected ReadOnly Property ProcessInfoManager As IProcessInfoRepository
        Get
            Return Provider.Scheduler.ProcessInfo
        End Get
    End Property

    Protected ReadOnly Property EmailManager As IEmailRepository
        Get
            Return Provider.Scheduler.Email
        End Get
    End Property

    Sub New(provider As IProvider, potfResource As OnTheFlyResource, pcardswipedata As String, ipAddr As String)
        Me.Provider = provider
        otfActivity = provider.Scheduler.Activity.GetActivity(6)
        otfresource = potfResource
        resourceItem = provider.Scheduler.Resource.GetResource(otfresource.ResourceID)
        cardNum = ReservationOnTheFlyUtil.GetCardNumber(pcardswipedata)
        rreq.CardNum = cardNum
        rreq.IPAddress = ipAddr
        rreq.OnTheFlyName = resourceItem.ResourceName
        rreq.ResourceID = otfresource.ResourceID
        Dim inlab As IEnumerable(Of Badge) = provider.PhysicalAccess.GetCurrentlyInArea("all")
        Dim isOnKiosk As Boolean = Kiosks.Create(provider.Scheduler.Kiosk).IsOnKiosk(ipAddr)
        physicalAccessUtil = New PhysicalAccessUtility(inlab, isOnKiosk)
        Init()
    End Sub

    Private Sub Init()
        guid = Guid.NewGuid()
        cardSwipeTime = Date.Now
        ' get the user details from the cardnum     
        Dim cc As IClient = GetCardClient(cardNum)
        clientWhoSwipedTheCard = cc
    End Sub

    Public Function GetReturnMessage() As String
        Return returnMessage
    End Function

    Public Function IsProcessFailed() As Boolean
        Return IsProcessFailed
    End Function

    Public Function GetCardClient(ByVal cardnum As String) As IClient
        Dim card As Card = Provider.PhysicalAccess.GetCard(cardnum)
        Dim client As IClient = Provider.Data.Client.GetClient(card.ClientID)
        Return client
        'Dim bcc As BadgeCardClient = DA.Current.Query(Of BadgeCardClient).Where(Function(x) x.CardNumber = cardnum).FirstOrDefault()
        'Return bcc
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
    Public Function IsCabinet() As Boolean
        Return otfresource.IsCabinet()
    End Function

    Public Function ExistingReservation() As Boolean  'is there currently any reservation(which ever state, running or not activated) at this time on this tool ?
        If allReservationListAtTimeOfSwipe Is Nothing Then
            allReservationListAtTimeOfSwipe = ReservationRepository.SelectExisting(GetResourceItem().ResourceID)
            If (allReservationListAtTimeOfSwipe.Count > 1) Then
                Log("_[1]ExistingReservation", "There is more than one current reservation, how can this be??")
            End If
            'allReservationListAtTimeOfSwipe = LNF.Impl.Repository.Scheduler.Reservation.DataAccess.SelectByResource(getResource().ResourceID, cardSwipeTime, cardSwipeTime.AddMinutes(1), False)   '--------Checking what reservations in a min
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

    Public Function Running() As Boolean ' is that reservation activated or still need to be activated(this is the scheduler reservation)
        If allReservationListAtTimeOfSwipe IsNot Nothing Then
            Dim rsvRunning As IReservationItem = allReservationListAtTimeOfSwipe.FirstOrDefault(Function(x) x.IsRunning())
            If rsvRunning IsNot Nothing Then
                Log("_[2]Running", True)
                currentlyRunningReservationMayNotBeUsers = rsvRunning
                Return True
            End If
        End If

        Log("_[2]Running", False)
        Return False
    End Function

    Public Function IsExistingAnOTFReservation() As Boolean
        If currentlyRunningReservationMayNotBeUsers IsNot Nothing Then
            Dim ronf As ReservationOnTheFly = DataSession.Query(Of ReservationOnTheFly)().FirstOrDefault(Function(x) x.Reservation.ReservationID = currentlyRunningReservationMayNotBeUsers.ReservationID)
            If ronf IsNot Nothing Then
                Return True
            End If
        End If
        Return False
    End Function

    Public Function IsAnotherReservationStartInMinimumTime() As Boolean
        Dim reservationsInMinimumTime As IEnumerable(Of IReservation) = ReservationRepository.ReservationsInGranularityWindow(GetResourceItem())
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

    Public Function InGroupExisitingReservation() As Boolean ' scheduler path
        ' is current user in group of an exisiting and running reservation ?
        ' get the list of group who are belong to the current reservation
        If allReservationListAtTimeOfSwipe IsNot Nothing Then
            For Each aRsv As IReservationItem In allReservationListAtTimeOfSwipe
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

    Public Function IsCreateAndStart() As Boolean
        Return otfresource.IsCreateAndStart()
    End Function

    Public Function DoesAnotherReservationStartWithInGranularity() As Boolean
        Dim reservationsInGranularityPeriod As IEnumerable(Of IReservation) = ReservationRepository.ReservationsInGranularityWindow(GetResourceItem())

        If reservationsInGranularityPeriod IsNot Nothing Then
            If reservationsInGranularityPeriod.Count() > 0 Then
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

    Private Function GetReservationWhichStartsInMinReservationTime() As IReservationItem
        ' what is minmum reservation time for this tool.
        ' in currenttime + minimum reservation time is there a new reservation ?
        Dim nextReservation As IReservationItem = GetNextReservationIfExists()
        If nextReservation IsNot Nothing Then
            If (Now.AddMinutes(GetResourceItem().MinReservTime)) > nextReservation.BeginDateTime Then
                Return nextReservation
            End If
        End If
        Return Nothing
    End Function

    Public Function DoesAnotherReservationStartWithInMinReservationTime() As Boolean
        Dim nexRestInMinTime = GetReservationWhichStartsInMinReservationTime()
        If nexRestInMinTime IsNot Nothing Then
            Log("_[2.2]DoesAnotherReservationStartWithInMinReservationTime", "True")
            Return True
        End If

        Log("_[2.2]DoesAnotherReservationStartWithInMinReservationTime", "False")
        Return False
    End Function

    Public Function InGroupOfNextReservation() As Boolean
        ' there is next reservation. this is already checked, 
        ' who are all(group) there in the next reservation ?
        ' is current user(who swiped the card) is in the next reservation ?
        Dim result As Boolean = False
        Dim nextReservation As IReservationItem = GetNextReservationIfExists()
        If nextReservation IsNot Nothing Then
            result = IsUserInTheReservationInviteesList(nextReservation, clientWhoSwipedTheCard)
        End If

        Log("_[2.3]InGroupOfNextReservation", result)
        Return result
    End Function

    Public Function AfterGracePeriod() As Boolean
        ' is it after the next reservation  and in the graceperiod ?
        Dim result As Boolean = False
        Dim currentReseration As IReservationItem = GetCurrentReservationIfExists()
        If currentReseration IsNot Nothing Then
            Dim dt As Date = currentReseration.EndDateTime.AddMinutes(GetResourceItem().GracePeriod)
            If cardSwipeTime < dt Then
                result = True
            End If
        End If
        Log("_AfterGracePeriod", result)
        Return result
    End Function

    Public Function KeepAlive() As Boolean
        ' is KeepAlive True for the current reservation ?
        Dim result As Boolean = False
        Dim currentReseration As IReservationItem = GetCurrentReservationIfExists()
        If currentReseration IsNot Nothing Then
            result = currentReseration.KeepAlive
        End If

        Log("_KeepAlive", result)
        Return result
    End Function

    Public Function IsUserAuthorizedOnTool() As Boolean
        If IsNothing(clientWhoSwipedTheCard) Then
            Return False
        End If

        If clientWhoSwipedTheCard.HasPriv(ClientPrivilege.Staff Or ClientPrivilege.Administrator) Then
            Log("[1]_IsUserAuthOnTool", "True")
            Return True
        End If

        Dim resoID = GetResourceItem().ResourceID

        'is it for everyone? ResourceClient   ResourceID AuthLevel is 2 Expiration date is > datetimenow and ClientID is -1
        Dim isForEveryOne As IEnumerable(Of ResourceClient) = DataSession.Query(Of ResourceClient)().Where(Function(x) x.ClientID = -1 AndAlso x.ResourceID = resoID AndAlso (x.Expiration Is Nothing OrElse x.Expiration.Value > Date.Now))
        If isForEveryOne IsNot Nothing Then
            If isForEveryOne.Count > 0 Then
                Return True
            End If
        End If

        Dim allRcs As IEnumerable(Of ResourceClient) = DataSession.Query(Of ResourceClient)().Where(Function(x) x.ClientID = clientWhoSwipedTheCard.ClientID AndAlso x.ResourceID = resoID AndAlso x.AuthLevel > 1)
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
    Private Function IsUserInTheReservationInviteesList(ByVal rsv As IReservationItem, ByVal currentClient As IClient) As Boolean
        If rsv IsNot Nothing Then
            Dim allInvs As IList(Of IReservationInviteeItem) = ReservationRepository.GetInvitees(rsv.ReservationID).ToList()
            If allInvs IsNot Nothing Then
                Dim isReservedOrIsInvted As IReservationInviteeItem = allInvs.Where(Function(x) x.InviteeID = currentClient.ClientID).FirstOrDefault()

                If isReservedOrIsInvted Is Nothing Then
                    If rsv.ClientID = clientWhoSwipedTheCard.ClientID Then
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

    Private Function GetCurrentReservationIfExists() As IReservationItem
        Return currentlyRunningReservationByUser
    End Function

    Private Function GetNextReservationIfExists() As IReservationItem  ' startable next reservation
        If nextReservationInMinTime Is Nothing Then
            Dim minResTime As TimeSpan = TimeSpan.FromMinutes(GetResourceItem().MinReservTime)
            Dim nextNearestReservationInMinimumTime As IEnumerable(Of IReservationItem) = ReservationRepository.SelectByResource(GetResourceID(), cardSwipeTime, cardSwipeTime.Add(minResTime), False)

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

    Private Function GetResourceItem() As IResource
        Return resourceItem
    End Function

    Private Function GetResourceID() As Integer
        Return otfresource.ResourceID
    End Function

    Private Function CreateAndStartReservation() As Integer
        'create and start reservation  ------------
        Dim args As New InsertReservationArgs()

        Dim res As IResource = GetResourceItem()
        Dim rr As ResRequest = GetResRequest()

        args.ResourceID = res.ResourceID

        Dim defaultMinResTime As Integer = 5 ' 5 minutes
        If 0 = res.MinReservTime Then
            res.MinReservTime = defaultMinResTime
        End If

        Dim reservationStartDatetTime As Date = GetNearestStartTime(res.MinReservTime)
        Dim resDuration As Double = res.MinReservTime

        args.ClientID = clientWhoSwipedTheCard.ClientID
        args.RecurrenceID = -1 'always -1 for non-recurring reservation
        args.Notes = "On The Fly Reservation"
        args.AutoEnd = True
        args.ActivityID = otfActivity.ActivityID 'Processing
        args.BeginDateTime = reservationStartDatetTime
        args.EndDateTime = reservationStartDatetTime.AddMinutes(resDuration)
        args.Now = Now
        args.ModifiedByClientID = clientWhoSwipedTheCard.ClientID

        Dim util As New ClientPreferenceUtility(Provider)
        Dim accounts As IEnumerable(Of IAccount) = Provider.Data.Client.GetActiveAccounts(clientWhoSwipedTheCard.ClientID)
        Dim clientSetting As IClientSetting = Provider.Scheduler.ClientSetting.GetClientSettingOrDefault(clientWhoSwipedTheCard.ClientID)
        Dim orderedAccts = ClientPreferenceUtility.OrderListByUserPreference(clientSetting, accounts, Function(x) x.AccountID, Function(x) x.AccountName)
        Dim account As IAccount = orderedAccts.FirstOrDefault()  ' first account in the list is default account

        Dim rsv As IReservationItem

        If account IsNot Nothing Then
            args.AccountID = account.AccountID
            rsv = ReservationRepository.InsertReservation(args)
        Else
            Throw New InvalidOperationException("The default account could not be determined.")
        End If

        StartReservation(clientWhoSwipedTheCard, rsv, rr, res.MinReservTime)

        Return rsv.ReservationID
    End Function

    Private Function GetReservationClient(rsv As IReservationItem, client As IClient) As ReservationClient
        Dim resourceClients As IEnumerable(Of IResourceClient) = ReservationRepository.GetResourceClients(rsv.ResourceID)
        Dim invitees As IEnumerable(Of IReservationInviteeItem) = ReservationRepository.GetInvitees(rsv.ReservationID)
        Dim inLab As Boolean = Kiosks.OverrideIsOnKiosk OrElse physicalAccessUtil.ClientInLab(client.ClientID, rsv.LabID)

        Return New ReservationClient With {
             .ClientID = client.ClientID,
             .ReservationID = rsv.ReservationID,
             .ResourceID = rsv.ResourceID,
             .IsReserver = rsv.ClientID = client.ClientID,
             .IsInvited = invitees.Any(Function(x) x.InviteeID = client.ClientID),
             .InLab = inLab,
             .UserAuth = Reservations.GetAuthLevel(resourceClients, client)
        }
    End Function

    Private Sub StartReservation(aclient As IClient, rsv As IReservationItem, rr As ResRequest, Optional reservationTime As Double = -1)
        If rsv Is Nothing Then
            Throw New ArgumentNullException("rsv", "A null Reservation object is not allowed. [LabScheduler.AppCode.OnTheFlyImpl.StartReservation]")
        End If

        If rr Is Nothing Then
            Throw New ArgumentNullException("rr", "A null ResRequest object is not allowed.")
        End If

        ' also create a reservation row in the ReservationOnTheFly table
        Dim ronfly As New ReservationOnTheFly With {
            .Reservation = DataSession.Single(Of Reservation)(rsv.ReservationID)
        }

        Reservations.Create(Provider, Now).Start(Provider.Scheduler.Reservation.GetReservation(rsv.ReservationID), GetReservationClient(rsv, aclient), aclient.ClientID)

        rr.ReservationID = rsv.ReservationID
        rr.ReservationTime = reservationTime
        ronfly.CardNumber = rr.CardNum
        ronfly.IPAddress = rr.IPAddress
        ronfly.OnTheFlyName = rr.OnTheFlyName

        DataSession.Insert(ronfly)
    End Sub

    Private Shared Function GetNearestStartTime(minResTimeInMins As Double) As Date
        Dim pDt = New DateTime(Date.Now.Year, Date.Now.Month, Date.Now.Day, Date.Now.Hour, 0, 0)
        Dim resDT As DateTime = pDt.AddMinutes(minResTimeInMins * -1)

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
        Dim currentReservation As IReservationItem = GetCurrentReservationIfExists()
        If currentReservation IsNot Nothing Then
            Reservations.Create(Provider, Now).End(Provider.Scheduler.Reservation.GetReservation(currentReservation.ReservationID), Now, GetSwipedByClientID())
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
            StartReservation(clientWhoSwipedTheCard, currentlyRunningReservationByUser, GetResRequest())
            Log("StartExistingReservation:Started", True)
        End If
    End Sub

    Public Sub StartNextReservation()
        Dim nexRestInMinTime = GetReservationWhichStartsInMinReservationTime()
        StartReservation(clientWhoSwipedTheCard, nexRestInMinTime, GetResRequest())
    End Sub

    Public Sub ExtendExistingReservation()
        Log("[5]ExtendExistingReservation", True)
        If currentlyRunningReservationByUser IsNot Nothing Then
            Dim extraTime As Integer = GetResourceItem().Granularity
            currentlyRunningReservationByUser.EndDateTime = currentlyRunningReservationByUser.EndDateTime.AddMinutes(extraTime)
            ' Allow modifying EndDateTime only for on-the-fly. Normal reservations should be cancelled for modification.
            Provider.Scheduler.Reservation.ExtendReservation(currentlyRunningReservationByUser.ReservationID, extraTime, clientWhoSwipedTheCard.ClientID)
        End If
    End Sub

    Public Sub Fail(func As String, Optional msg As String = "")
        Log("Fail-" + func, msg)
    End Sub

    Public Sub Swipe() 'starting point for OnTheFlyImpl

        'Try
        Log("--SwipeStart--", cardNum)
        If IsUserAuthorizedOnTool() Then
            OnTheFlyRules.Apply(Me)
        Else
            Fail("Swipe", "User not authorized on the tool")
        End If
        Log("--SwipeEnd--", cardNum)
        DataSession.Insert(logArray)
        'Catch ex As Exception
        '    Fail("Swipe", ex.Message)

        '    'Error
        '    Dim err As New ErrorLog With {
        '        .Application = "OnTheFly",
        '        .Message = ex.Message.Clip(500),
        '        .StackTrace = ex.StackTrace.Clip(4000),
        '        .ErrorDateTime = Date.Now,
        '        .ClientID = GetSwipedByClientID(),
        '        .PageUrl = "?"
        '    }


        '    DA.Current.Insert(err)
        'Finally
        Log("--SwipeEnd--", cardNum)
        'End Try
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
        If IsUserAuthorizedOnTool() Then
            OnTheFlyRules.EndReservation(Me)
        Else
            result = -1
        End If
        Log("--SwipeEnd-ForEnd--", cardNum)
        DataSession.Insert(logArray)
        Return (result)
    End Function
End Class

Public Class OnTheFlyRules   '-------------------------------------------------------------------------------
    Private Shared Sub Check_KeepAlive(ByVal oi As OnTheFlyImpl)
        If oi.KeepAlive() Then
            oi.Fail("_KeepAlive")
        Else
            oi.CreateAndStartNewReservation()
        End If
    End Sub

    Private Shared Sub Check_InGroupOfNextReservation(ByVal oi As OnTheFlyImpl)
        If oi.InGroupOfNextReservation() Then
            If oi.ExistingReservation() Then
                oi.EndExisitingIfExistsAndStartNextReservation()
            Else
                oi.StartNextReservation()
            End If
        Else
            oi.Fail("Check_InGroupOfNextReservation", "Not in group of next Reservation")
        End If
    End Sub

    Private Shared Sub Check_DoesAnotherReservationStartWithInMinimumTime(ByVal oi As OnTheFlyImpl)
        If oi.IsAnotherReservationStartInMinimumTime() Then
            'In group of Next reservation ?
            Check_InGroupOfNextReservation(oi)  ' is this same behaviour or different ?
        Else
            'End Current and Create and Start New
            oi.EndExisitingIfExistsAndStartNextReservation()
        End If
    End Sub

    Private Shared Sub Check_OTF_Reservation(ByVal oi As OnTheFlyImpl)
        If oi.IsExistingAnOTFReservation() Then
            Check_DoesAnotherReservationStartWithInMinimumTime(oi)
        Else
            oi.Fail("Check_OTF_Reservation")
        End If
    End Sub

    Private Shared Sub Check_InGroupExistingReservation_WhileRunning(ByVal oi As OnTheFlyImpl)
        If oi.InGroupExisitingReservation() Then
            Check_DoesAnotherReservationStartWithInGranularity(oi)
        Else
            Check_OTF_Reservation(oi) '   ------------------------- new flow 'is existing OTF
        End If
    End Sub

    Private Shared Sub Check_DoesAnotherReservationStartWithInGranularity(ByVal oi As OnTheFlyImpl)
        If oi.DoesAnotherReservationStartWithInGranularity() Then
            Check_InGroupOfNextReservation(oi)
        Else
            oi.ExtendExistingReservation()
        End If
    End Sub
    Private Shared Sub Check_AfterGracePeriod(ByVal oi As OnTheFlyImpl)
        If oi.AfterGracePeriod() Then
            Check_KeepAlive(oi)
        Else
            oi.Fail("Check_AfterGracePeriod")
        End If
    End Sub

    Private Shared Sub Check_InGroupExistingReservation_NotRunning(ByVal oi As OnTheFlyImpl)
        If oi.InGroupExisitingReservation() Then
            oi.StartExistingReservation()
        Else
            Check_AfterGracePeriod(oi)
        End If
    End Sub

    Private Shared Sub ApplyIsCreateAndStart(ByVal oi As OnTheFlyImpl)
        If oi.IsCreateAndStart() Then
            ApplyNonExistingReservation(oi)
        Else
            ' reservation_start_only is already handled in earlier step
            oi.Fail("ApplyIsCreateAndStart", "Unreachable code, Start-Already-Existing-Reservation, this is should be be handled in earlier step")
        End If
    End Sub

    Private Shared Sub ApplyNonExistingReservation(ByVal oi As OnTheFlyImpl)
        If oi.DoesAnotherReservationStartWithInMinReservationTime() Then
            Check_InGroupOfNextReservation(oi)
        Else
            oi.CreateAndStartNewReservation()
        End If
    End Sub

    Private Shared Sub ApplyExistingReservation(ByVal oi As OnTheFlyImpl)
        If oi.Running() Then
            Check_InGroupExistingReservation_WhileRunning(oi)
        Else
            Check_InGroupExistingReservation_NotRunning(oi)
        End If
    End Sub

    Private Shared Sub ApplyCabinet(ByVal oi As OnTheFlyImpl)
        oi.EndExisitingIfExistsAndStartNextReservation()
    End Sub

    Private Shared Sub ApplyTool(ByVal oi As OnTheFlyImpl)
        If oi.ExistingReservation() Then
            ApplyExistingReservation(oi)
        Else
            ApplyIsCreateAndStart(oi) 'ApplyNonExistingReservation(oi)
        End If
    End Sub

    Public Shared Sub Apply(ByVal oi As OnTheFlyImpl)
        If oi.IsCabinet() Then
            ApplyCabinet(oi)
        Else
            ApplyTool(oi)
        End If
    End Sub

    Public Shared Function EndReservation(ByVal oi As OnTheFlyImpl) As Integer
        If oi.ExistingReservation() Then ' is there an existing reservation?
            Return EndUserExistingReservation(oi)
        End If

        Return -1
    End Function

    Public Shared Function EndUserExistingReservation(ByVal oi As OnTheFlyImpl) As Integer
        If oi.InGroupExisitingReservation() Then
            oi.EndExistingReservation()
            Return 1
        End If

        Return -1
    End Function
End Class

