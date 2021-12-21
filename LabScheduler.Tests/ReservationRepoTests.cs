using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationRepoTests
    {
        [TestMethod]
        public void CanAddAutoEndLog()
        {
            var repo = new LNF.Web.Scheduler.Repository.ReservationRepository();
            var autoEndLog = repo.AddAutoEndLog(123456, "autoend");
            Assert.AreEqual(autoEndLog.ReservationID, 123456);
            Assert.AreEqual(autoEndLog.ResourceID, 62040);
            Assert.AreEqual(autoEndLog.ResourceName, "SJ-20 Evaporator");
            Assert.AreEqual(autoEndLog.ClientID, 322);
            Assert.AreEqual(autoEndLog.DisplayName, "Yang, Jun");
            Assert.AreEqual(autoEndLog.Action, "autoend");
        }
    }
}
