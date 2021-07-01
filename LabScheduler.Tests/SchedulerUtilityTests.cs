using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Scheduler;
using LNF.Web.Scheduler.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LabScheduler.Tests
{
    [TestClass]
    public class SchedulerUtilityTests : TestBase
    {
        [TestMethod]
        public void CanShowLabCleanWarning()
        {
            bool isLabCleanTime;
            DateTime beginDateTime;
            DateTime endDateTime;

            using (StartUnitOfWork())
            {
                // Monday
                beginDateTime = DateTime.Parse("2020-09-14 08:00:00");
                endDateTime = DateTime.Parse("2020-09-14 10:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 07:00:00");
                endDateTime = DateTime.Parse("2020-09-14 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 07:00:00");
                endDateTime = DateTime.Parse("2020-09-14 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 07:00:00");
                endDateTime = DateTime.Parse("2020-09-14 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 10:15:00");
                endDateTime = DateTime.Parse("2020-09-14 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 10:10:00");
                endDateTime = DateTime.Parse("2020-09-14 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                // Thursday
                beginDateTime = DateTime.Parse("2020-09-17 08:00:00");
                endDateTime = DateTime.Parse("2020-09-17 10:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 07:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 07:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 07:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 10:15:00");
                endDateTime = DateTime.Parse("2020-09-17 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 10:10:00");
                endDateTime = DateTime.Parse("2020-09-17 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                // Wednesday
                beginDateTime = DateTime.Parse("2020-09-16 07:00:00");
                endDateTime = DateTime.Parse("2020-09-16 19:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                // Multi-day
                beginDateTime = DateTime.Parse("2020-09-16 23:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-16 23:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-18 23:00:00");
                endDateTime = DateTime.Parse("2020-09-21 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-18 23:00:00");
                endDateTime = DateTime.Parse("2020-09-21 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                // Holiday
                beginDateTime = DateTime.Parse("2020-09-08 07:00:00");
                endDateTime = DateTime.Parse("2020-09-08 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-08 10:10:00");
                endDateTime = DateTime.Parse("2020-09-09 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-08 10:15:00");
                endDateTime = DateTime.Parse("2020-09-09 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-06 08:15:00");
                endDateTime = DateTime.Parse("2020-09-07 10:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-07 08:0:00");
                endDateTime = DateTime.Parse("2020-09-07 11:00:00");
                isLabCleanTime = SchedulerUtility.Create(Provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);
            }
        }

        [TestMethod]
        public void CanGetLabCleanConfiguration()
        {
            LabCleanConfiguration config = LabCleanConfiguration.GetCurrentConfiguration();
            Assert.AreEqual(1, config.Items.Count());
            Assert.AreEqual(1, config.Items.ElementAt(0).Days[0]);
            Assert.AreEqual(4, config.Items.ElementAt(0).Days[1]);
            Assert.AreEqual(510, config.Items.ElementAt(0).StartTime.TotalMinutes);
            Assert.AreEqual(570, config.Items.ElementAt(0).EndTime.TotalMinutes);
            Assert.AreEqual(0, config.Items.ElementAt(0).StartPadding);
            Assert.AreEqual(45, config.Items.ElementAt(0).EndPadding);
            Assert.IsTrue(config.Items.ElementAt(0).Active);
        }

        [TestMethod]
        public void CanDeleteReservationTest()
        {
            using (StartUnitOfWork())
            {
                int tom = 2691; //tom daunais
                int kevin = 1116; //kevin owen
                int test = 1600; //test user
                int processing = 6; //processing activity
                int sem = 85051; //sem inline
                bool inlab = true;
                //bool notInlab = false;
                //bool outside = false;
                bool canDelete = true;
                bool canModify = true;
                //bool cannotDelete = false;
                bool cannotModify = false;

                DateTime startsInAnWhile = RoundDown(DateTime.Now.AddHours(1), TimeSpan.FromMinutes(5));
                DateTime startsSoon = RoundDown(DateTime.Now.AddMinutes(5), TimeSpan.FromMinutes(5));

                // kevin can delete because he is tool engineer
                // kevin cannot modify because he is not the resever
                ReservationStateTester(sem, tom, kevin, test, processing, startsInAnWhile, 60, inlab, canDelete, cannotModify);
                ReservationStateTester(sem, tom, kevin, test, processing, startsSoon, 60, inlab, canDelete, cannotModify);

                // tom can delete because he is the reserver
                // tom can modify because he is the reserver
                ReservationStateTester(sem, tom, tom, test, processing, startsInAnWhile, 60, inlab, canDelete, canModify);
                ReservationStateTester(sem, tom, tom, test, processing, startsSoon, 60, inlab, canDelete, canModify);

                // kevin can delete because he is the reserver
                // kevin can modify because he is the reserver
                ReservationStateTester(sem, kevin, kevin, test, processing, startsInAnWhile, 60, inlab, canDelete, canModify);
                ReservationStateTester(sem, kevin, kevin, test, processing, startsSoon, 60, inlab, canDelete, canModify);

                // test can delete becaause they are invited
                // test cannot modify because they are not the reserver
                ReservationStateTester(sem, tom, test, test, processing, startsInAnWhile, 60, inlab, canDelete, cannotModify);
                ReservationStateTester(sem, tom, test, test, processing, startsSoon, 60, inlab, canDelete, cannotModify);

                // tom can delete because he is invited
                // tom cannot modify because he is not the reserver
                ReservationStateTester(sem, kevin, tom, tom, processing, startsInAnWhile, 60, inlab, canDelete, cannotModify);
                ReservationStateTester(sem, kevin, tom, tom, processing, startsSoon, 60, inlab, canDelete, cannotModify);
            }
        }

        private void ReservationStateTester(int resourceId, int reserverClientId, int currentClientId, int inviteeClientId, int activityId, DateTime beginDateTime, int duration, bool inlab, bool canDelete, bool canModify)
        {
            var now = DateTime.Now;

            DateTime endDateTime = beginDateTime.AddMinutes(duration);

            IReservationItem rsv = GetMockReservation(123456, resourceId, reserverClientId, activityId, beginDateTime, endDateTime, null, null);

            ReservationStateArgs args = GetReservationStateArgs(rsv, currentClientId, inviteeClientId, inlab, now);
            ReservationState state = ReservationStateUtility.Create(now).GetReservationState(args);

            bool actual;

            actual = SchedulerUtility.CanDeleteReservation(state, args, now);
            Assert.AreEqual(canDelete, actual);

            actual = SchedulerUtility.CanModifyReservation(state, args, now);
            Assert.AreEqual(canModify, actual);
        }

        private ReservationStateArgs GetReservationStateArgs(IReservationItem rsv, int currentUserClientId, int inviteeClientId, bool inlab, DateTime now)
        {
            IClient client = Provider.Data.Client.GetClient(currentUserClientId);
            IEnumerable<IResourceClient> resourceClients = Provider.Scheduler.Resource.GetResourceClients(rsv.ResourceID);
            IEnumerable<IReservationInviteeItem> invitees = new[] { GetMockReservationInvitee(inviteeClientId, rsv.ReservationID) };

            ReservationClient rc = Helper.GetReservationClient(rsv, client, resourceClients, invitees);
            rc.InLab = inlab;
            ReservationStateArgs result = ReservationStateArgs.Create(rsv, rc, now);

            return result;
        }

        private IReservationInviteeItem GetMockReservationInvitee(int inviteeId, int reservationId)
        {
            var mock = new Mock<IReservationInviteeItem>();
            mock.SetupProperty(x => x.InviteeID, inviteeId)
                .SetupProperty(x => x.ReservationID, reservationId);
            return mock.Object;
        }

        private IReservationItem GetMockReservation(int reservationId, int resourceId, int clientId, int activityId, DateTime beginDateTime, DateTime endDateTime, DateTime? actualBeginDateTime, DateTime? actualEndDateTime)
        {
            IResource res = Provider.Scheduler.Resource.GetResource(resourceId);
            IActivity act = Provider.Scheduler.Activity.GetActivity(activityId);

            var mock = new Mock<IReservationItem>();
            mock.SetupProperty(x => x.ReservationID, reservationId)
                .SetupProperty(x => x.ResourceID, resourceId)
                .SetupProperty(x => x.LabID, res.LabID)
                .SetupProperty(x => x.ClientID, clientId)
                .SetupProperty(x => x.ActivityID, activityId)
                .SetupProperty(x => x.BeginDateTime, beginDateTime)
                .SetupProperty(x => x.EndDateTime, endDateTime)
                .SetupProperty(x => x.ActualBeginDateTime, actualBeginDateTime)
                .SetupProperty(x => x.ActualEndDateTime, actualEndDateTime)
                .SetupProperty(x => x.StartEndAuth, act.StartEndAuth)
                .SetupProperty(x => x.Editable, act.Editable)
                .SetupProperty(x => x.IsFacilityDownTime, act.IsFacilityDownTime)
                .SetupProperty(x => x.MinCancelTime, res.MinCancelTime)
                .SetupProperty(x => x.MinReservTime, res.MinReservTime);

            var rsv = mock.Object;

            return rsv;
        }

        public DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }
    }
}
