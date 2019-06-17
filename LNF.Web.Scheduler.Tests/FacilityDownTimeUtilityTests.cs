using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class FacilityDownTimeUtilityTests : TestBase
    {
        [TestMethod]
        public void CanInsertFacilityDownTime()
        {
            int resourceId = 80010; //Zygo NewView 5000
            int rid = 0;
            int fdtid = 0;

            using (ContextManager.StartRequest(1600))
            {
                // Create a reservation that starts at 10 am tomorrow.

                var data = new ReservationData()
                {
                    ClientID = 1600,
                    ResourceID = resourceId,
                    ActivityID = 6,
                    AccountID = 67,
                    Notes = "Test Reservation",
                    AutoEnd = true,
                    KeepAlive = true,
                    ReservationDuration = new ReservationDuration(DateTime.Now.Date.AddDays(1).AddHours(10), TimeSpan.FromMinutes(5))
                };

                var rsv = ReservationUtility.Create(data);
                rid = rsv.ReservationID;
            }

            using (ContextManager.StartRequest(155))
            {
                int clientId = HttpContext.Current.CurrentUser().ClientID;

                DateTime sd = DateTime.Now.Date.AddDays(1).AddHours(9);
                DateTime ed = sd.AddHours(4);

                ReservationGroup group = FacilityDownTimeUtility.CreateFacilityDownTimeGroup(clientId, sd, ed);

                var result = FacilityDownTimeUtility.InsertFacilityDownTime(
                    resourceId: 80010,
                    groupId: group.GroupID,
                    clientId: clientId,
                    beginDateTime: sd,
                    endDateTime: ed,
                    notes: "EECS building fire drill"
                );

                Assert.AreEqual(1, result.Existing.Count());
                Assert.AreEqual(rid, result.Existing.ElementAt(0).ReservationID);

                fdtid = result.ReservationID;

                var rsv = DA.Current.Single<Reservation>(fdtid);
                Assert.IsTrue(rsv.Activity.IsFacilityDownTime);
            }

            using (ContextManager.StartRequest(1301))
            {
                // clean up
                PurgeReservations(new[] { rid, fdtid });
            }
        }
    }
}
