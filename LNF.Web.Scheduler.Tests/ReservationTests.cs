using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationTests
    {
        [TestMethod]
        public void ReservationTests_CanCreateCheck()
        {
            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
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
                    DA.Use<IReservationManager>().CanCreateCheck(rsv);
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
        }
    }
}
