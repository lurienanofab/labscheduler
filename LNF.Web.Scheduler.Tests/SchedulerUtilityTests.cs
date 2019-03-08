using LNF.Cache;
using LNF.Data;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class SchedulerUtilityTests : TestBase
    {
        [TestMethod]
        public void CanGetCurrentUserActiveClientAccounts()
        {
            using (ContextManager.StartRequest(1475))
            {
                var accts = DA.Current.Query<ClientAccountInfo>().Where(x => x.ClientAccountActive && x.ClientOrgActive && x.ClientID == 1475);
                Assert.IsNotNull(accts);
                Assert.AreEqual(1, accts.Count());
                Assert.AreEqual(3649, accts.First().ClientAccountID);
            }
        }

        [TestMethod]
        public void CanCreateAndModifyReservationWithoutInsertForModification()
        {
            Reservation rsv1, rsv2;
            ResourceItem res;

            int resourceId = 62020;
            DateTime beginDateTime = DateTime.Parse("2016-10-02 08:00:00");

            using (ContextManager.StartRequest(1475))
            {
                // Step 0: Purge
                int purgedRows = PurgeReservations(resourceId, beginDateTime, beginDateTime.AddHours(24));
                Console.WriteLine("Purged rows: {0}", purgedRows);

                // Step 1: Get a resource
                res = CacheManager.Current.ResourceTree().GetResource(resourceId).GetResourceItem();
                Assert.AreEqual(res.ResourceName, "EnerJet Evaporator");

                // Step 2: Create a reservation

                // Step 2.1: Init process info, and add
                var processInfos = new Models.Scheduler.ReservationProcessInfoItem[]
                {
                    new Models.Scheduler.ReservationProcessInfoItem()
                    {
                        ProcessInfoLineID = 110,
                        Value = 100
                    }
                };

                // Step 2.2: Create the new Reservation
                rsv1 = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData(processInfos)
                {
                    ClientID = 1301,
                    ResourceID = res.ResourceID,
                    AccountID = 67,
                    ActivityID = 6,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "created by unit test",
                    ReservationDuration = new ReservationDuration(beginDateTime, res.Granularity)
                });

                Assert.IsTrue(rsv1.ReservationID > 0);
                Console.WriteLine("Created ReservationID: {0}", rsv1.ReservationID);
            }

            using (ContextManager.StartRequest(1475))
            {
                // Step 4: Modify existing

                // Step 4.1: Init process info, change value
                var processInfos = ServiceProvider.Current.Use<IProcessInfoManager>().GetReservationProcessInfos(rsv1.ReservationID);
                processInfos.First().Value = 200;

                // Step 4.2: Modify the existing Reservation
                rsv2 = SchedulerUtility.ModifyExistingReservation(rsv1, new SchedulerUtility.ReservationData(processInfos)
                {
                    ClientID = 1301,
                    ResourceID = res.ResourceID,
                    AccountID = rsv1.Account.AccountID,
                    ActivityID = rsv1.Activity.ActivityID,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "modified by unit test",
                    ReservationDuration = new ReservationDuration(beginDateTime, res.Granularity)
                });

                Assert.IsTrue(ReferenceEquals(rsv1, rsv2));
                Assert.AreEqual(rsv1.ReservationID, rsv2.ReservationID);
                Console.WriteLine("Modified ReservationID: {0}", rsv2.ReservationID);
            }

            using (ContextManager.StartRequest(1475))
            {
                var processInfos = ServiceProvider.Current.Use<IProcessInfoManager>().GetReservationProcessInfos(rsv2.ReservationID);
                Assert.AreEqual(1, processInfos.Count);
                Assert.AreEqual(200, processInfos.First().Value);
            }
        }

        [TestMethod]
        public void CanCreateAndModifyReservationWithInsertForModification()
        {
            Reservation rsv1, rsv2;
            ResourceItem res;

            int resourceId = 62020;
            DateTime beginDateTime = DateTime.Parse("2016-10-02 08:00:00");

            using (ContextManager.StartRequest(1600))
            {
                // Step 0: Purge
                int purgedRows = PurgeReservations(resourceId, beginDateTime, beginDateTime.AddHours(24));
                Console.WriteLine("Purged rows: {0}", purgedRows);

                // Step 1: Get a resource
                res = CacheManager.Current.ResourceTree().GetResource(resourceId).GetResourceItem();
                Assert.AreEqual(res.ResourceName, "EnerJet Evaporator");

                // Step 2: Create a reservation

                // Step 2.1: Init process info, and add
                var processInfos = new Models.Scheduler.ReservationProcessInfoItem[]
                {
                    new Models.Scheduler.ReservationProcessInfoItem()
                    {
                         ProcessInfoLineID = 110,
                         Value = 100
                    }
                };

                // Step 2.2: Create the new Reservation
                rsv1 = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData(processInfos)
                {
                    ClientID = 1301,
                    ResourceID = res.ResourceID,
                    AccountID = 67,
                    ActivityID = 6,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "created by unit test",
                    ReservationDuration = new ReservationDuration(beginDateTime, res.Granularity)
                });

                Assert.IsTrue(rsv1.ReservationID > 0);
                Console.WriteLine("Created ReservationID: {0}", rsv1.ReservationID);
            }

            using (ContextManager.StartRequest(1600))
            {
                // Step 4: Modify existing

                // Step 4.1: Init process info, change value
                var processInfos = ServiceProvider.Current.Use<IProcessInfoManager>().GetReservationProcessInfos(rsv1.ReservationID);
                processInfos.First().Value = 200;

                // Step 4.2: Modify the existing Reservation
                rsv2 = SchedulerUtility.ModifyExistingReservation(rsv1, new SchedulerUtility.ReservationData(processInfos)
                {
                    ClientID = 1301,
                    ResourceID = res.ResourceID,
                    AccountID = rsv1.Account.AccountID,
                    ActivityID = rsv1.Activity.ActivityID,
                    AutoEnd = true,
                    KeepAlive = true,
                    Notes = "modified by unit test",
                    ReservationDuration = ReservationDuration.FromMinutes(beginDateTime, res.Granularity.TotalMinutes * 2)
                });

                Assert.IsFalse(ReferenceEquals(rsv1, rsv2));
                Assert.IsTrue(rsv2.ReservationID > 0);
                Console.WriteLine("Modified ReservationID: {0}", rsv2.ReservationID);
            }

            using (ContextManager.StartRequest(1600))
            {
                var processInfos1 = ServiceProvider.Current.Use<IProcessInfoManager>().GetReservationProcessInfos(rsv1.ReservationID);
                Assert.AreEqual(1, processInfos1.Count);
                Assert.AreEqual(100, processInfos1.First().Value);

                var processInfos2 = ServiceProvider.Current.Use<IProcessInfoManager>().GetReservationProcessInfos(rsv2.ReservationID);
                Assert.AreEqual(1, processInfos2.Count);
                Assert.AreEqual(200, processInfos2.First().Value);
            }
        }
    }
}
