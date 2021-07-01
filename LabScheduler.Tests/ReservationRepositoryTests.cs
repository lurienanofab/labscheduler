using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationRepositoryTests : TestBase
    {
        [TestMethod]
        public void CanSelectByLabLocation()
        {
            using (StartUnitOfWork())
            {
                var sd = DateTime.Parse("2020-10-23");
                var ed = DateTime.Parse("2020-10-24");
                var reservations = Provider.Scheduler.Reservation.SelectByLabLocation(4, sd, ed, true);
            }
        }

        [TestMethod]
        public void SelectInviteesByLabLocation()
        {
            using (StartUnitOfWork())
            {
                var sd = DateTime.Parse("2020-10-23");
                var ed = DateTime.Parse("2020-10-24");
                var reservations = Provider.Scheduler.Reservation.SelectInviteesByLabLocation(4, sd, ed, true);
            }
        }

        [TestMethod]
        public void CanSelectOverwritable()
        {
            using (StartUnitOfWork())
            {
                var sd = DateTime.Parse("2020-10-23");
                var ed = DateTime.Parse("2020-10-24");
                var reservations = Provider.Scheduler.Reservation.SelectOverwritable(900209, DateTime.Parse("2020-10-28 23:40:00"), DateTime.Parse("2020-10-28 23:45:00"));
            }
        }
    }
}
