using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Models.Worker;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LNF.Web.Scheduler
{
    public static class RepairUtility
    {
        public static IContext Context => ServiceProvider.Current.Context;

        public static IResourceManager ResourceManager => ServiceProvider.Current.Use<IResourceManager>();

        public static IReservationManager ReservationManager => ServiceProvider.Current.Use<IReservationManager>();

        public static IEmailManager EmailManager => ServiceProvider.Current.Use<IEmailManager>();

        public static ClientItem CurrentUser => CacheManager.Current.CurrentUser;

        public static Reservation StartRepair(ResourceItem res, ResourceState resourceState, DateTime actualBeginDateTime, DateTime actualEndDateTime, string notes)
        {
            Reservation repair = null;

            if (res.HasState(ResourceState.Online))
            {
                // Create new offline reservation or new limited mode status
                //ResourceState resourceState = rdoStatusOffline.Checked ? ResourceState.Offline : ResourceState.Limited;

                if (resourceState == ResourceState.Offline)
                {
                    // User wants to create new offline reservation

                    // Determine BeginDateTime for repair reservation
                    DateTime beginDateTime, endDateTime;

                    beginDateTime = ResourceManager.GetNextGranularity(res, actualBeginDateTime, GranularityDirection.Previous);
                    endDateTime = ResourceManager.GetNextGranularity(res, actualEndDateTime, GranularityDirection.Next);

                    // Insert the new repair reservation
                    repair = ReservationManager.InsertRepair(res.ResourceID, CurrentUser.ClientID, beginDateTime, endDateTime, actualBeginDateTime, notes, CurrentUser.ClientID);

                    // Remove invitees and process info that might be in the session
                    Context.RemoveSessionValue("ReservationInvitees");
                    Context.RemoveSessionValue("ReservationProcessInfos");

                    // Set the state into resource table and session object
                    ResourceUtility.UpdateState(res.ResourceID, ResourceState.Offline, string.Empty);

                    UpdateAffectedReservations(repair);
                }
                else
                {
                    // User sets the tool into limited mode
                    // Set Resource State, txtNotes.Text is saved with Resource table only in limited mode, since limited mode has no reservation record
                    ResourceUtility.UpdateState(res.ResourceID, ResourceState.Limited, notes);
                }
            }

            return repair;
        }

        public static Reservation UpdateRepair(ResourceItem res, DateTime actualBeginDateTime, DateTime actualEndDateTime, string notes)
        {
            Reservation repair = null;

            if (res.HasState(ResourceState.Offline))
            {
                // Determine BeginDateTime for repair reservation
                DateTime endDateTime;

                var rip = ReservationManager.GetRepairReservationInProgress(res.ResourceID);

                if (rip != null)
                {
                    repair = DA.Current.Single<Reservation>(rip.ReservationID);

                    if (repair != null)
                    {
                        endDateTime = ResourceManager.GetNextGranularity(res, actualEndDateTime, GranularityDirection.Next);

                        // Modify existing repair reservation
                        repair.EndDateTime = endDateTime;
                        repair.Notes = notes;
                        ReservationManager.Update(repair, CurrentUser.ClientID);

                        UpdateAffectedReservations(repair);
                    }
                }
            }
            else
            {
                // modifying limited mode, only StateNotes is modifiable in this case
                ResourceUtility.UpdateState(res.ResourceID, ResourceState.Limited, notes);
            }

            return repair;
        }

        public static Reservation EndRepair(ResourceItem res)
        {
            Reservation repair = null;

            if (res.HasState(ResourceState.Offline))
            {
                var rip = ReservationManager.GetRepairReservationInProgress(res.ResourceID);

                if (rip != null)
                {
                    repair = DA.Current.Single<Reservation>(rip.ReservationID);

                    if (repair != null)
                    {
                        if (res.IsSchedulable)
                        {
                            // Set Scheduled EndDateTime = next grain boundary in future
                            repair.EndDateTime = ResourceManager.GetNextGranularity(res, DateTime.Now, GranularityDirection.Next);
                            ReservationManager.Update(repair, CurrentUser.ClientID);

                            // End the repair reservation now
                            ReservationManager.End(repair, CurrentUser.ClientID, CurrentUser.ClientID);

                            UpdateAffectedReservations(repair);
                        }
                    }
                }
            }

            // Set Resource State
            ResourceUtility.UpdateState(res.ResourceID, ResourceState.Online, string.Empty);

            return repair;
        }

        /// <summary>
        /// Checks for all reservations affected by the repair and either cancels or deletes them (depending of if they are started or not), and then forgives them.
        /// </summary>
        /// <param name="repair">The repair reservation.</param>
        private static bool UpdateAffectedReservations(Reservation repair)
        {
            // Might be null when resource state is Limited
            if (repair == null) return false;

            // Find and end reservations that are in progress (Endable) for this resource
            IList<Reservation> endableReservations = ReservationManager.SelectEndableReservations(repair.Resource.ResourceID);
            foreach (Reservation endable in endableReservations.Where(x => x.ReservationID != repair.ReservationID))
            {
                ReservationManager.EndForRepair(endable, CurrentUser.ClientID, CurrentUser.ClientID);
                EmailManager.EmailOnCanceledByRepair(endable, false, "Offline", repair.Notes, repair.EndDateTime);
                EmailManager.EmailOnForgiveCharge(endable, 100, true, CurrentUser.ClientID);
            }

            // Find and remove any unstarted reservations made during time of repair
            IList<Reservation> unstartedReservations = ReservationManager.SelectByResource(repair.Resource.ResourceID, repair.BeginDateTime, repair.EndDateTime, false);
            foreach (Reservation unstarted in unstartedReservations)
            {
                // Do nothing if already canceled
                if (unstarted.IsActive)
                {
                    // If the reservation has not begun
                    if (!unstarted.ActualBeginDateTime.HasValue)
                    {
                        ReservationManager.Delete(unstarted, CurrentUser.ClientID);
                        EmailManager.EmailOnCanceledByRepair(unstarted, true, "Offline", repair.Notes, repair.EndDateTime);
                        EmailManager.EmailOnForgiveCharge(unstarted, 100, true, CurrentUser.ClientID);
                    }
                }
            }

            // 2009-05-21 Make the old reservations that were covered by the repair to be forgiven
            // Get all the past active reservations that were covered by this specific repair period

            // [2013-05-20 jg] We also need cancelled reservations so booking fee is forgiven

            // [2017-08-24 jg] Changing to beginDateTime and endDateTime so that any existing reservations in the entire repair range are forgiven.
            //      The previous date range only covered reservations scheduled to start between the actualBeginDateTime (the time the repair began
            //      without going to the previous granularity) and the current time. The range is now between the repair begin (to previous granularity)
            //      to repair end (to next granularity).


            // [2018-08-31 jg] This is now just a catch all. Nothing will happen if the reservation is already 100% forgiven.
            //      Overlapping reservations (started and unstarted) are forgiven above and an email is sent. This will catch
            //      unstarted overlapping reservations where IsActive == true and ActualBeginDateTime.HasValue == true, or
            //      IsActive == false since these are skipped above.
            IList<Reservation> query = ReservationManager.SelectHistoryToForgiveForRepair(repair.Resource.ResourceID, repair.BeginDateTime, repair.EndDateTime);

            var result = false;

            foreach (Reservation rsv in query)
            {
                // Avoid resending the email if the reservation was already forgiven
                if (rsv.ChargeMultiplier > 0)
                {
                    // Set charge multiplier to zero
                    ReservationManager.UpdateCharges(rsv, 0, true, CurrentUser.ClientID);

                    // Email User after everything is done.
                    EmailManager.EmailOnForgiveCharge(rsv, 100, true, CurrentUser.ClientID);

                    // The session variable is set now and then checked for on the next page load.
                    result = true;
                }
            }

            // Update forgiven charge on FinOps. Enqueue an UpdateBilling request. This will
            // return immediately and the update will process in the background using OnlineServicesWorker.
            DateTime sd = repair.BeginDateTime.FirstOfMonth();
            ServiceProvider.Current.Worker.Execute(new UpdateBillingWorkerRequest(sd, 0, new[] { "tool", "room" }));

            return result;
        }
    }
}
