using LNF.Impl.Testing;
using LNF.Web.Scheduler.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationControllerTests
    {
        [TestMethod]
        public void CanGetReservationAction()
        {
            var sessionItems = new SessionItemCollection();

            var queryString = new NameValueCollection
            {
                ["ReservationID"] = "123456",
                ["State"] = "PastOther",
                ["Date"] = DateTime.Now.ToString("yyyy-MM-dd")
            };

            var contextItems = new Dictionary<object, object>();

            using (var testContext = new SchedulerContextManager("141.213.6.57", "jgett", contextItems, sessionItems, queryString))
            {
                var rc = new ReservationController();

                testContext.Login("jgett");
                testContext.QueryString["State"] = "PastOther";
                var redirectUrl = rc.GetReservationAction(testContext.ContextBase);
                Assert.AreEqual($"~/Contact.aspx?ReservationID=123456&Path=4-1-8-62040&Date={DateTime.Now:yyyy-MM-dd}", redirectUrl);

                testContext.Login("junyang");
                testContext.QueryString["State"] = "PastSelf";
                redirectUrl = rc.GetReservationAction(testContext.ContextBase);
                Assert.AreEqual($"~/ReservationRunNotes.aspx?ReservationID=123456&Path=4-1-8-62040&Date={DateTime.Now:yyyy-MM-dd}", redirectUrl);


            }
        }
    }
}
