using LNF.Cache;
using LNF.Models.Data;
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
            ContextManager.StartRequest(new ClientItem()
            {
                ClientID = 1301,
                UserName = "jgett",
                Privs = (ClientPrivilege)3942
            });

            // There is an existing repair reservation (859058) on the STS Pegasus 4 that begins on 8/28 at 8:45 am
            // and ends on 8/29 at 4:15 pm.
            //
            // There is another existing reservation 
            //
            // Now this repair is getting extended by 2 hours to 7:15 pm and there is a unstarted reservation scheduled
            // on 8/29 at 8:30 pm.

            int resourceId = 14021;
            DateTime sd = DateTime.Parse("2018-08-28 08:45:00");
            DateTime ed = DateTime.Parse("2018-08-29 19:15:00");

            // add the 7:15 pm reservation
            Reservation rsv = null;
            int reservationId = 0;
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                rsv = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData()
                {
                    ResourceID = resourceId,
                    ClientID = 1301,
                    AccountID = 67,
                    ActivityID = 6,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "test reservation",
                    ReservationDuration = new ReservationDuration(DateTime.Parse("2018-08-29 19:15:00"), TimeSpan.FromMinutes(15))
                });

                reservationId = rsv.ReservationID;
            }

            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                var res = CacheManager.Current.ResourceTree().GetResource(resourceId).GetResourceItem();
                var repair = RepairUtility.UpdateRepair(res, sd, ed, "Extending to 7:15 pm.");
                RepairUtility.UpdateAffectedReservations(repair);
            }

            // make sure the reservation was forgiven
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                rsv = DA.Current.Single<Reservation>(reservationId);
                Assert.AreEqual(0, rsv.ChargeMultiplier);
            }

            // clean up
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                rsv = DA.Current.Single<Reservation>(reservationId);
                DA.Current.Delete(rsv);
                var hist = DA.Current.Query<ReservationHistory>().Where(x => x.Reservation.ReservationID == reservationId);
                DA.Current.Delete(hist);
            }
        }
    }
}
