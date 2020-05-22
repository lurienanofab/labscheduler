using LNF.Impl.Repository.Data;
using LNF.Impl.Repository.Scheduler;
using LNF.Repository;
using LNF.Scheduler;
using System;
using System.Collections.Generic;

namespace LNF.Web.Scheduler
{
    public static class FacilityDownTimeUtility
    {
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

        public static InsertFacilityDownTimeResult InsertFacilityDownTime(int resourceId, int groupId, int clientId, DateTime beginDateTime, DateTime endDateTime, string notes, int modifiedByClientId)
        {
            IList<CanceledReservation> canceled = new List<CanceledReservation>();

            // Find and Remove any un-started reservations made during time of repair
            var query = ServiceProvider.Current.Scheduler.Reservation.SelectByResource(resourceId, beginDateTime, endDateTime, false);

            foreach (var existing in query)
            {
                // Only if the reservation has not begun
                if (existing.ActualBeginDateTime == null)
                {
                    ServiceProvider.Current.Scheduler.Reservation.CancelReservation(existing.ReservationID, modifiedByClientId);
                    ServiceProvider.Current.Scheduler.Email.EmailOnCanceledByRepair(existing, true, "LNF Facility Down", "Facility is down, thus we have to disable the tool.", endDateTime, modifiedByClientId);
                    canceled.Add(new CanceledReservation(existing.ReservationID));
                }
                else
                {
                    // We have to disable all those reservations that have been activated by setting isActive to 0.  
                    // The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                    // have to charge the user for that reservation
                }
            }

            var rsv = ServiceProvider.Current.Scheduler.Reservation.InsertFacilityDownTime(resourceId, clientId, groupId, beginDateTime, endDateTime, notes, modifiedByClientId);

            var result = new InsertFacilityDownTimeResult(rsv.ReservationID, canceled);

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
