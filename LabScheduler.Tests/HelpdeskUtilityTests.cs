using LNF.Helpdesk;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LabScheduler.Tests
{
    [TestClass]
    public class HelpdeskUtilityTests : TestBase
    {
        [TestMethod]
        public void CanGetCcEmailsForHardwareIssue()
        {
            using (StartUnitOfWork())
            {
                var resource = Provider.Scheduler.Resource.GetResource(85051);
                var emails = HelpdeskUtility.GetCcEmailsForHardwareIssue(resource, 1301);
                Assert.IsTrue(emails.Length > 0);
            }
        }

        [TestMethod]
        public void CanSendHardwareIssueEmail()
        {
            var resource = Provider.Scheduler.Resource.GetResource(999998);
            int sent = HelpdeskUtility.SendHardwareIssueEmail(resource, 1301, "test", "test hardware ticket email");
            Assert.AreEqual(0, sent);

            resource = Provider.Scheduler.Resource.GetResource(85051);
            sent = HelpdeskUtility.SendHardwareIssueEmail(resource, null, 1301, string.Empty, "test", "test hardware ticket email", TicketPriorty.HardwareIssue, new Uri("http://localhost/test"));
            Assert.IsTrue(sent > 0);
        }
    }
}
