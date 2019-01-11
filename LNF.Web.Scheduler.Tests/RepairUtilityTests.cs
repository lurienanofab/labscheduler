using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class RepairUtilityTests : TestBase
    {
        [TestMethod]
        public void CanUpdateReservationsAffectedByRepair()
        {
            int resourceId = 80010;
            int id1 = 0;
            int id2 = 0;
            int repairId = 0;
            Reservation rsv = null;
            Reservation repair = null;
            ResourceItem res = null;

            // actual repair start and end
            DateTime sd = DateTime.Now.AddHours(-1);
            DateTime ed = DateTime.Now.AddHours(2);

            using (ContextManager.StartRequest(1301))
            {
                // Start with a clean slate
                PurgeReservations(resourceId, DateTime.Now.Date, DateTime.Now.Date.AddDays(2));
                ResourceUtility.UpdateState(resourceId, ResourceState.Online, string.Empty);
            }

            using (ContextManager.StartRequest(1301))
            {
                // Create two reservations, one that will be canceled/forgiven when the repair is created, and one that
                // is canceled/forgiven when the repair is extended

                // First reservation starts one hour from now, lasting for 15 minutes
                rsv = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData(null, null)
                {
                    ResourceID = resourceId,
                    ClientID = 1301,
                    AccountID = 67,
                    ActivityID = 6,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "test reservation #1",
                    ReservationDuration = new ReservationDuration(DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(1), TimeSpan.FromMinutes(15))
                });

                id1 = rsv.ReservationID;

                // Second reservation starts 3 hours from now, lasting for 15 minutes

                rsv = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData(null, null)
                {
                    ResourceID = resourceId,
                    ClientID = 1301,
                    AccountID = 67,
                    ActivityID = 6,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "test reservation #1",
                    ReservationDuration = new ReservationDuration(DateTime.Now.Date.AddHours(DateTime.Now.Hour).AddHours(3), TimeSpan.FromMinutes(15))
                });

                id2 = rsv.ReservationID;
            }


            using (ContextManager.StartRequest(1301))
            {
                // Create a repair that started one hour ago and lasts until two hours from now. This will overlap with the first reservation
                // that starts one hour from now but not the second that starts 3 hours from now.

                res = CacheManager.Current.ResourceTree().GetResource(resourceId).GetResourceItem();
                repair = RepairUtility.StartRepair(res, ResourceState.Offline, sd, ed, "test repair");

                Assert.IsNotNull(repair);

                repairId = repair.ReservationID;
            }

            using (ContextManager.StartRequest(1301))
            {
                // Make sure the first reservation is canceled/forgiven, but not the second.

                rsv = DA.Current.Single<Reservation>(id1);
                Assert.AreEqual(0, rsv.ChargeMultiplier);
                Assert.AreEqual(false, rsv.IsActive);

                rsv = DA.Current.Single<Reservation>(id2);
                Assert.AreEqual(1, rsv.ChargeMultiplier);
                Assert.AreEqual(true, rsv.IsActive);
            }

            using (ContextManager.StartRequest(1301))
            {
                // Extend the repair over the second reservation.

                res = CacheManager.Current.ResourceTree().GetResource(resourceId).GetResourceItem();
                repair = RepairUtility.UpdateRepair(res, sd, ed.AddHours(5), "Extending three additional hours");
                Assert.IsNotNull(repair);
            }

            // make sure the reservation was forgiven
            using (ContextManager.StartRequest(1301))
            {
                rsv = DA.Current.Single<Reservation>(id1);
                Assert.AreEqual(0, rsv.ChargeMultiplier);
                Assert.AreEqual(false, rsv.IsActive);

                rsv = DA.Current.Single<Reservation>(id2);
                Assert.AreEqual(0, rsv.ChargeMultiplier);
                Assert.AreEqual(false, rsv.IsActive);
            }

            // clean up
            using (ContextManager.StartRequest(1301))
            {
                PurgeReservations(new[] { id1, id2, repairId });
                ResourceUtility.UpdateState(resourceId, ResourceState.Online, string.Empty);
            }
        }
    }
}
