using LNF.Impl.DependencyInjection.Default;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security.Principal;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationTests
    {
        private IUnitOfWork _uow;

        public ReservationTests()
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();
            var ident = new GenericIdentity("jgett");
            var roles = new[] { "Staff", "LabUser", "StoreUser", "StoreAdmin", "Developer" };
            var user = new GenericPrincipal(ident, roles);
            ServiceProvider.Current.Context.User = user;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _uow = ServiceProvider.Current.DataAccess.StartUnitOfWork();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_uow != null)
                _uow.Dispose();
        }

        [TestMethod]
        public void ReservationTests_CanCreateCheck()
        {
            // in the past
            DateTime sd = DateTime.Parse("2017-01-17 08:00:00");
            DateTime ed = DateTime.Parse("2017-01-17 08:30:00");
            TimeSpan duration = ed - sd;

            var rsv = new Reservation()
            {
                Resource = DA.Current.Single<Resource>(41010),
                Client = DA.Current.Single<Client>(1301),
                Account = DA.Current.Single<Account>(67),
                Activity = DA.Current.Single<Activity>(6),
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

            try
            {
                var mgr = DA.Use<IReservationManager>();
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
                DA.Use<IReservationManager>().CanCreateCheck(rsv);
            }
            catch (Exception ex)
            {
                Assert.AreEqual("Your reservation was not created. Cannot create a reservation that ends before it starts.", ex.Message);
            }
        }

        [TestMethod]
        public void ReservationTests_CanModify()
        {
            var rsv1 = DA.Current.Single<Reservation>(835041);

            var data = GetReservationData(
                resourceId: rsv1.Resource.ResourceID,
                clientId: rsv1.Client.ClientID,
                accountId: rsv1.Account.AccountID,
                activityId: rsv1.Activity.ActivityID,
                autoEnd: rsv1.AutoEnd,
                keepAlive: rsv1.KeepAlive,
                notes: rsv1.Notes,
                selectedDate: DateTime.Parse("2018-08-02"),
                startTimeHour: 10,
                startTimeMinute: 0,
                duration: 0);

            var rsv2 = SchedulerUtility.ModifyExistingReservation(rsv1, data);

        }

        private SchedulerUtility.ReservationData GetReservationData(int resourceId, int clientId, int accountId, int activityId, bool autoEnd, bool keepAlive, string notes, DateTime selectedDate, int startTimeHour, int startTimeMinute, int duration)
        {
            var beginDateTime = selectedDate.AddHours(startTimeHour).AddMinutes(startTimeMinute);
            var rd = ReservationDuration.FromMinutes(beginDateTime, duration);

            var result = new SchedulerUtility.ReservationData
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
