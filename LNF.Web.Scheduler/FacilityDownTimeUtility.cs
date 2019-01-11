using LNF.Cache;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Collections.Generic;

namespace LNF.Web.Scheduler
{
    public static class FacilityDownTimeUtility
    {
        public static IReservationManager ReservationManager => ServiceProvider.Current.Use<IReservationManager>();
        public static IEmailManager EmailManager => ServiceProvider.Current.Use<IEmailManager>();

        public static ReservationGroup CreateFacilityDownTimeGroup(int clientId, DateTime beginDateTime, DateTime endDateTime)
        {
            //FacilityDownTimeDB.CreateNew()
            /*
            Dim groupId As Integer
            Using dba As New SQLDBAccess("cnSselData")
                With dba.SelectCommand
                    .AddParameter("@Action", "InsertNew")
                    .AddParameter("@GroupID", groupId, ParameterDirection.Output)
                    .AddParameter("@ClientID", clientId)
                    .AddParameter("@AccountID", 67)
                    .AddParameter("@ActivityID", 23)
                    .AddParameter("@BeginDateTime", beginDateTime)
                    .AddParameter("@EndDateTime", endDateTime)
                    .AddParameter("@IsActive", True)
                    .AddParameter("@CreatedOn", Date.Now)
                End With
                dba.ExecuteNonQuery("sselScheduler.dbo.procReservationGroupInsert")
                groupId = dba.GetParameterValue(Of Integer)("@GroupID")
                Return groupId
            End Using 

            sselScheduler.dbo.procReservationGroupInsert

            INSERT INTO dbo.ReservationGroup(ClientID, AccountID, ActivityID, BeginDateTime, EndDateTime, CreatedOn, IsActive)
		    VALUES(@ClientID, @AccountID, @ActivityID, @BeginDateTime, @EndDateTime, @CreatedOn, @IsActive)
		
		    SET @GroupID = SCOPE_IDENTITY()
            */

            ReservationGroup result = new ReservationGroup()
            {
                Client = DA.Current.Single<Client>(clientId),
                Account = DA.Current.Single<Account>(Properties.Current.LabAccount.AccountID),
                Activity = DA.Current.Single<Activity>(Properties.Current.Activities.FacilityDownTime.ActivityID),
                BeginDateTime = beginDateTime,
                EndDateTime = endDateTime,
                CreatedOn = DateTime.Now,
                IsActive = true
            };

            DA.Current.Insert(result);

            return result;
        }

        public static InsertFacilityDownTimeResult InsertFacilityDownTime(int resourceId, int groupId, int clientId, DateTime beginDateTime, DateTime endDateTime, string notes)
        {
            Reservation rsv = new Reservation
            {
                Resource = DA.Current.Single<Resource>(resourceId),
                RecurrenceID = -1, //always -1 for non-recurring reservation
                GroupID = groupId,
                Client = DA.Current.Single<Client>(clientId),
                BeginDateTime = beginDateTime,
                EndDateTime = endDateTime,
                ActualBeginDateTime = beginDateTime,
                ActualEndDateTime = endDateTime,
                Account = DA.Current.Single<Account>(Properties.Current.LabAccount.AccountID),
                Activity = DA.Current.Single<Activity>(Properties.Current.Activities.FacilityDownTime.ActivityID),
                Duration = (beginDateTime - endDateTime).TotalMinutes,
                Notes = notes,
                AutoEnd = false,
                HasProcessInfo = false,
                HasInvitees = false,
                CreatedOn = DateTime.Now
            };

            IList<CanceledReservation> canceled = new List<CanceledReservation>();

            // Find and Remove any un-started reservations made during time of repair
            var query = ReservationManager.SelectByResource(resourceId, beginDateTime, endDateTime, false);
            foreach (var existing in query)
            {
                // Only if the reservation has not begun
                if (existing.ActualBeginDateTime == null)
                {
                    ReservationManager.Delete(existing, CacheManager.Current.CurrentUser.ClientID);
                    EmailManager.EmailOnCanceledByRepair(existing, true, "LNF Facility Down", "Facility is down, thus we have to disable the tool.", endDateTime);
                    canceled.Add(new CanceledReservation(existing.ReservationID));
                }
                else
                {
                    // We have to disable all those reservations that have been activated by setting isActive to 0.  
                    // The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                    // have to charge the user for that reservation
                }
            }

            ReservationManager.InsertFacilityDownTime(rsv, CacheManager.Current.CurrentUser.ClientID);

            InsertFacilityDownTimeResult result = new InsertFacilityDownTimeResult(rsv.ReservationID, canceled);

            return result;
        }
    }

    public struct InsertFacilityDownTimeResult
    {
        public int ReservationID { get; }
        public IEnumerable<CanceledReservation> Existing { get; }

        public InsertFacilityDownTimeResult(int reservationId, IEnumerable<CanceledReservation> existing)
        {
            ReservationID = reservationId;
            Existing = existing;
        }
    }

    public struct CanceledReservation
    {
        public int ReservationID { get; }

        public CanceledReservation(int reservationId)
        {
            ReservationID = reservationId;
        }
    }
}
