using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationItemTests : TestBase
    {
        [TestMethod]
        public void CanGetReservationItem()
        {
            var item = Provider.Scheduler.Reservation.GetReservationItem(1038282);
            Assert.AreEqual(1038282, item.ReservationID);
        }

        [TestMethod]
        public void CanSelectByDateRange()
        {
            var sd = DateTime.Parse("2020-10-08");
            var ed = DateTime.Parse("2020-10-09");
            var includeDeleted = true;

            var items = Provider.Scheduler.Reservation.SelectByDateRange(sd, ed, includeDeleted);

            Assert.IsTrue(items.Count() > 0);
        }
    }
}
