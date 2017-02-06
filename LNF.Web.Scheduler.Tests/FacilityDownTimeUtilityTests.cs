using LNF.Repository.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class FacilityDownTimeUtilityTests : TestBase
    {
        [TestMethod]
        public void CanInsertFacilityDownTime()
        {
            int clientId = 155;
            DateTime sd = DateTime.Parse("2016-10-06 09:30:00.000");
            DateTime ed = DateTime.Parse("2016-10-06 10:00:00.000");

            ReservationGroup group = FacilityDownTimeUtility.CreateFacilityDownTimeGroup(clientId, sd, ed);

            var result = FacilityDownTimeUtility.InsertFacilityDownTime(
                resourceId: 10030,
                groupId: group.GroupID,
                clientId: 155,
                beginDateTime: sd,
                endDateTime: ed,
                notes: "EECS building fire drill"
            );
        }
    }
}
