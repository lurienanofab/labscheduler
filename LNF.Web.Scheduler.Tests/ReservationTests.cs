using LNF.Impl.DependencyInjection.Default;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Principal;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationTests
    {
        private IDisposable _uow;

        private void LogIn(int clientId)
        {
            var client = DA.Current.Single<Client>(clientId);
            var ident = new GenericIdentity(client.UserName);
            var roles = client.Roles();
            var user = new GenericPrincipal(ident, roles);
            ServiceProvider.Current.Context.User = user;
        }

        public ReservationTests()
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _uow = DA.StartUnitOfWork();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_uow != null)
                _uow.Dispose();
        }

        [TestMethod]
        public void CanCreateCheck()
        {
            LogIn(1301);

            // in the past
            DateTime sd = DateTime.Parse("2017-01-17 08:00:00");
            DateTime ed = DateTime.Parse("2017-01-17 08:30:00");
            TimeSpan duration = ed - sd;

            var rsv = new ReservationItem()
            {
                ResourceID = 41010,
                ClientID = 1301,
                AccountID = 67,
                ActivityID = 6,
                BeginDateTime = sd,
                EndDateTime = ed,
                ApplyLateChargePenalty = true,
                AutoEnd = false,
                ChargeMultiplier = 1,
                CreatedOn = DateTime.Now,
                LastModifiedOn = DateTime.Now,
                Duration = duration.TotalMinutes,
                HasInvitees = false,
                HasProcessInfo = false,
                IsActive = true,
                IsStarted = false,
                KeepAlive = false,
                IsUnloaded = false,
                MaxReservedDuration = duration.TotalMinutes,
                Notes = string.Empty
            };

            var mgr = ServiceProvider.Current.Use<IReservationManager>();

            try
            {
                mgr.CanCreateCheck(rsv);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Your reservation was not created. Cannot create a reservation in the past.", ex.Message);
            }

            // end before begin
            rsv.BeginDateTime = DateTime.Now.Date.AddDays(1).AddHours(8); //tomorrow at 8 am
            rsv.EndDateTime = rsv.BeginDateTime.AddMinutes(-15); //tomorrow at 7:45 am

            try
            {
                mgr.CanCreateCheck(rsv);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Your reservation was not created. Cannot create a reservation that ends before it starts.", ex.Message);
            }
        }

        [TestMethod]
        public void CanModify()
        {
            var rsv1 = DA.Current.Single<ReservationInfo>(835041).Model<ReservationItem>();

            var data = GetReservationData(
                resourceId: rsv1.ResourceID,
                clientId: rsv1.ClientID,
                accountId: rsv1.AccountID,
                activityId: rsv1.ActivityID,
                autoEnd: rsv1.AutoEnd,
                keepAlive: rsv1.KeepAlive,
                notes: rsv1.Notes,
                selectedDate: DateTime.Parse("2018-08-02"),
                startTimeHour: 10,
                startTimeMinute: 0,
                duration: 5);

            var rsv2 = ReservationUtility.Modify(rsv1, data);

            Assert.AreNotEqual(rsv1.ReservationID, rsv2.ReservationID);
            Assert.AreEqual(DateTime.Parse("2018-08-02 10:00:00"), rsv2.BeginDateTime);
            Assert.AreEqual(DateTime.Parse("2018-08-02 10:05:00"), rsv2.EndDateTime);

            int deleted = ServiceProvider.Current.Use<IReservationManager>().PurgeReservation(rsv2.ReservationID);

            Console.WriteLine("Deleted: {0}", deleted);
        }

        private ReservationData GetReservationData(int resourceId, int clientId, int accountId, int activityId, bool autoEnd, bool keepAlive, string notes, DateTime selectedDate, int startTimeHour, int startTimeMinute, int duration)
        {
            var beginDateTime = selectedDate.AddHours(startTimeHour).AddMinutes(startTimeMinute);
            var rd = ReservationDuration.FromMinutes(beginDateTime, duration);

            var result = new ReservationData()
            {
                ResourceID = resourceId,
                ClientID = clientId,
                AccountID = accountId,
                ActivityID = activityId,
                AutoEnd = autoEnd,
                KeepAlive = keepAlive,
                Notes = notes,
                ReservationDuration = rd
            };

            return result;
        }
    }
}
