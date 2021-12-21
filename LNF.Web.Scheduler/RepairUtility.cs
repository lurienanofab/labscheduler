using LNF.CommonTools;
using LNF.Data;
using LNF.Scheduler;
using LNF.Worker;
using System;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class RepairUtility
    {
        public RepairUtility(IResourceTree res, IClient currentUser, IProvider provider)
        {
            Resource = res;
            CurrentUser = currentUser;
            Provider = provider;
        }

        public IResourceTree Resource { get; }
        public IClient CurrentUser { get; }
        public IProvider Provider { get; }

        public IReservation StartRepair(HttpContextBase context, ResourceState resourceState, DateTime actualBeginDateTime, DateTime actualEndDateTime, string notes)
        {
            IReservation repair = null;

            if (Resource.HasState(ResourceState.Online))
            {
                // Create new offline reservation or new limited mode status
                //ResourceState resourceState = rdoStatusOffline.Checked ? ResourceState.Offline : ResourceState.Limited;

                if (resourceState == ResourceState.Offline)
                {
                    // User wants to create new offline reservation

                    // Determine BeginDateTime for repair reservation
                    DateTime beginDateTime, endDateTime;

                    beginDateTime = Resource.GetNextGranularity(actualBeginDateTime, GranularityDirection.Previous);
                    endDateTime = Resource.GetNextGranularity(actualEndDateTime, GranularityDirection.Next);

                    // Insert the new repair reservation
                    repair = Provider.Scheduler.Reservation.InsertRepair(Resource.ResourceID, CurrentUser.ClientID, beginDateTime, endDateTime, actualBeginDateTime, notes, CurrentUser.ClientID);

                    // Remove invitees and process info that might be in the session
                    context.Session.Remove($"ReservationInvitees#{Resource.ResourceID}");
                    context.Session.Remove($"ReservationProcessInfos#{Resource.ResourceID}");

                    // Set the state into resource table and session object
                    Resources.UpdateState(Resource.ResourceID, ResourceState.Offline, string.Empty);

                    UpdateAffectedReservations(repair);
                }
                else
                {
                    // User sets the tool into limited mode
                    // Set Resource State, txtNotes.Text is saved with Resource table only in limited mode, since limited mode has no reservation record
                    Resources.UpdateState(Resource.ResourceID, ResourceState.Limited, notes);
                }
            }

            return repair;
        }

        public IReservation UpdateRepair(DateTime actualBeginDateTime, DateTime actualEndDateTime, string notes)
        {
            IReservation repair = null;
            IReservation result = null;

            if (Resource.HasState(ResourceState.Offline))
            {
                // Determine BeginDateTime for repair reservation
                DateTime endDateTime;

                var rip = Reservations.GetRepairInProgress(Resource);

                if (rip != null)
                {
                    repair = Provider.Scheduler.Reservation.GetReservation(rip.ReservationID);

                    if (repair != null)
                    {
                        endDateTime = Resource.GetNextGranularity(actualEndDateTime, GranularityDirection.Next);

                        // Modify existing repair reservation
                        result = Provider.Scheduler.Reservation.UpdateRepair(repair.ReservationID, endDateTime, notes, CurrentUser.ClientID);

                        // result has the modified end datetime, so affected reservations will be updated

                        UpdateAffectedReservations(result);
                    }
                }
            }
            else
            {
                // modifying limited mode, only StateNotes is modifiable in this case
                Resources.UpdateState(Resource.ResourceID, ResourceState.Limited, notes);
            }

            return result;
        }

        public IReservation EndRepair(DateTime now)
        {
            IReservation repair = null;

            if (Resource.HasState(ResourceState.Offline))
            {
                var rip = Reservations.GetRepairInProgress(Resource);

                if (rip != null)
                {
                    repair = Provider.Scheduler.Reservation.GetReservation(rip.ReservationID);

                    if (repair != null)
                    {
                        if (Resource.IsSchedulable)
                        {
                            // Set Scheduled EndDateTime = next gran boundary in future
                            var endDateTime = Resource.GetNextGranularity(now, GranularityDirection.Next);

                            Provider.Scheduler.Reservation.UpdateRepair(repair.ReservationID, endDateTime, repair.Notes, CurrentUser.ClientID);

                            // End the repair reservation now
                            var util = Reservations.Create(Provider, now);
                            util.End(repair, now, CurrentUser.ClientID, CurrentUser.ClientID);

                            UpdateAffectedReservations(repair);
                        }
                    }
                }
            }

            // Set Resource State
            Resources.UpdateState(Resource.ResourceID, ResourceState.Online, string.Empty);

            return repair;
        }

        /// <summary>
        /// Checks for all reservations affected by the repair and either cancels or deletes them (depending of if they are started or not), and then forgives them.
        /// </summary>
        /// <param name="repair">The repair reservation.</param>
        private bool UpdateAffectedReservations(IReservationItem repair)
        {
            // Might be null when resource state is Limited
            if (repair == null) return false;

            // Find and end reservations that are in progress (Endable) for this resource
            var endableReservations = Provider.Scheduler.Reservation.SelectEndableReservations(repair.ResourceID);
            foreach (var endable in endableReservations.Where(x => x.ReservationID != repair.ReservationID))
            {
                Provider.Scheduler.Reservation.EndAndForgiveForRepair(endable.ReservationID, "Ended and forgiven for repair.", CurrentUser.ClientID, CurrentUser.ClientID);
                Provider.Scheduler.Email.EmailOnCanceledByRepair(endable.ReservationID, false, "Offline", repair.Notes, repair.EndDateTime, CurrentUser.ClientID);
                Provider.Scheduler.Email.EmailOnForgiveCharge(endable.ReservationID, 100, true, CurrentUser.ClientID);
            }

            // Find and remove any unstarted reservations made during time of repair
            var unstartedReservations = Provider.Scheduler.Reservation.SelectUnstarted(repair.ResourceID, repair.BeginDateTime, repair.EndDateTime);
            foreach (var unstarted in unstartedReservations)
            {
                Provider.Scheduler.Reservation.CancelReservation(unstarted.ReservationID, "Cancelled and forgiven for repair.", CurrentUser.ClientID);
                Provider.Scheduler.Email.EmailOnCanceledByRepair(unstarted.ReservationID, true, "Offline", repair.Notes, repair.EndDateTime, CurrentUser.ClientID);
                // Don't send forgiveness email yet, this will happen below...
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
            var query = Provider.Scheduler.Reservation.SelectHistoryToForgiveForRepair(repair.ResourceID, repair.BeginDateTime, repair.EndDateTime);

            var result = false;

            foreach (var rsv in query)
            {
                // Avoid resending the email if the reservation was already forgiven
                if (rsv.ChargeMultiplier > 0)
                {
                    // Set charge multiplier to zero, notes have already been appended
                    Provider.Scheduler.Reservation.UpdateCharges(rsv.ReservationID, string.Empty, 0, true, CurrentUser.ClientID);

                    // Email User after everything is done.
                    Provider.Scheduler.Email.EmailOnForgiveCharge(rsv.ReservationID, 100, true, CurrentUser.ClientID);

                    // The session variable is set now and then checked for on the next page load.
                    result = true;
                }
            }

            // Update forgiven charge on FinOps. Enqueue an UpdateBilling request. This will
            // return immediately and the update will process in the background using OnlineServicesWorker.
            DateTime sd = repair.BeginDateTime.FirstOfMonth();
            Provider.Worker.Execute(new UpdateBillingWorkerRequest(sd, 0, new[] { "tool", "room" }));

            return result;
        }
    }
}
