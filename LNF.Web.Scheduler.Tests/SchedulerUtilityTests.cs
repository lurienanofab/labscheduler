using LNF.Cache;
using LNF.Data;
using LNF.Models.Data;
using LNF.Models.Scheduler;
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
        public void SchedulerUtilityTests_CanGetCurrentUserActiveClientAccounts()
        {
            ContextManager.StartRequest(new ClientItem()
            {
                ClientID = 1475,
                UserName = "1ben",
                Privs = (ClientPrivilege)1541
            });

            using (Providers.DataAccess.StartUnitOfWork())
            {
                var accts = CacheManager.Current.CurrentUserActiveClientAccounts();
                Assert.IsNotNull(accts);
                Assert.AreEqual(1, accts.Count);
                Assert.AreEqual(3649, accts[0].ClientAccountID);
            }
        }

        [TestMethod]
        public void SchedulerUtilityTests_CanCreateAndModifyReservationWithoutInsertForModification()
        {
            Reservation rsv1, rsv2;
            ResourceModel res;

            int resourceId = 62020;
            DateTime beginDateTime = DateTime.Parse("2016-10-02 08:00:00");

            using (Providers.DataAccess.StartUnitOfWork())
            {
                // Step 0: Purge
                int purgedRows = PurgeReservations(resourceId, beginDateTime, beginDateTime.AddHours(24));
                Console.WriteLine("Purged rows: {0}", purgedRows);

                // Step 1: Get a resource
                res = CacheManager.Current.GetResource(resourceId);
                Assert.AreEqual(res.ResourceName, "EnerJet Evaporator");

                // Step 2: Create a reservation

                // Step 2.1: Init process info, and add
                SchedulerUtility.LoadProcessInfo(0);
                SchedulerUtility.AddReservationProcessInfo(res.ResourceID, 51, 110, 0, "100", false);

                // Step 2.2: Init invitees
                SchedulerUtility.LoadReservationInvitees(0);
                SchedulerUtility.LoadAvailableInvitees(0, res.ResourceID, 6, 1301);
                SchedulerUtility.LoadRemovedInvitees();

                // Step 2.3: Create the new Reservation
                rsv1 = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData()
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

            using (Providers.DataAccess.StartUnitOfWork())
            {
                // Step 4: Modify existing

                // Step 4.1: Init process info, change value
                SchedulerUtility.LoadProcessInfo(rsv1.ReservationID);
                CacheManager.Current.ReservationProcessInfos().First().Value = 200;

                // Step 4.2: Init invitees
                SchedulerUtility.LoadReservationInvitees(rsv1.ReservationID);
                SchedulerUtility.LoadAvailableInvitees(rsv1.ReservationID, rsv1.Resource.ResourceID, rsv1.Activity.ActivityID, rsv1.Client.ClientID);
                SchedulerUtility.LoadRemovedInvitees();

                // Step 4.3: Modify the existing Reservation
                rsv2 = SchedulerUtility.ModifyExistingReservation(rsv1, new SchedulerUtility.ReservationData()
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

            using (Providers.DataAccess.StartUnitOfWork())
            {
                SchedulerUtility.LoadProcessInfo(rsv2.ReservationID);
                Assert.AreEqual(1, CacheManager.Current.ReservationProcessInfos().Count);
                Assert.AreEqual(200, CacheManager.Current.ReservationProcessInfos().First().Value);
            }
        }

        [TestMethod]
        public void SchedulerUtilityTests_CanCreateAndModifyReservationWithInsertForModification()
        {
            Reservation rsv1, rsv2;
            ResourceModel res;

            int resourceId = 62020;
            DateTime beginDateTime = DateTime.Parse("2016-10-02 08:00:00");

            using (Providers.DataAccess.StartUnitOfWork())
            {
                // Step 0: Purge
                int purgedRows = PurgeReservations(resourceId, beginDateTime, beginDateTime.AddHours(24));
                Console.WriteLine("Purged rows: {0}", purgedRows);

                // Step 1: Get a resource
                res = CacheManager.Current.GetResource(resourceId);
                Assert.AreEqual(res.ResourceName, "EnerJet Evaporator");

                // Step 2: Create a reservation

                // Step 2.1: Init process info, and add
                SchedulerUtility.LoadProcessInfo(0);
                SchedulerUtility.AddReservationProcessInfo(resourceId, 51, 110, 0, "100", false);

                // Step 2.2: Init invitees
                SchedulerUtility.LoadReservationInvitees(0);
                SchedulerUtility.LoadAvailableInvitees(0, res.ResourceID, 6, 1301);
                SchedulerUtility.LoadRemovedInvitees();

                // Step 2.3: Create the new Reservation
                rsv1 = SchedulerUtility.CreateNewReservation(new SchedulerUtility.ReservationData()
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

            using (Providers.DataAccess.StartUnitOfWork())
            {
                // Step 4: Modify existing

                // Step 4.1: Init process info, change value
                SchedulerUtility.LoadProcessInfo(rsv1.ReservationID);
                CacheManager.Current.ReservationProcessInfos().First().Value = 200;

                // Step 4.2: Init invitees
                SchedulerUtility.LoadReservationInvitees(rsv1.ReservationID);
                SchedulerUtility.LoadAvailableInvitees(rsv1.ReservationID, rsv1.Resource.ResourceID, rsv1.Activity.ActivityID, rsv1.Client.ClientID);
                SchedulerUtility.LoadRemovedInvitees();

                // Step 4.3: Modify the existing Reservation
                rsv2 = SchedulerUtility.ModifyExistingReservation(rsv1, new SchedulerUtility.ReservationData()
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

            using (Providers.DataAccess.StartUnitOfWork())
            {
                SchedulerUtility.LoadProcessInfo(rsv1.ReservationID);
                Assert.AreEqual(1, CacheManager.Current.ReservationProcessInfos().Count);
                Assert.AreEqual(100, CacheManager.Current.ReservationProcessInfos().First().Value);

                SchedulerUtility.LoadProcessInfo(rsv2.ReservationID);
                Assert.AreEqual(1, CacheManager.Current.ReservationProcessInfos().Count);
                Assert.AreEqual(200, CacheManager.Current.ReservationProcessInfos().First().Value);
            }
        }
    }
}
