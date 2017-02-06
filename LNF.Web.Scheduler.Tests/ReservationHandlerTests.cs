using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LNF.Web.Scheduler.Handlers;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationHandlerTests : TestBase
    {
        [TestMethod]
        public async Task CanProcessRequestAsync()
        {
            var handler = new ReservationHandler();
            HttpRequestManager mgr = ContextManager.StartRequest("reservation.ashx", "http://lnf-dev.eecs.umich.edu/sselscheduler/ajax/reservation.ashx", "Command=test&ReservationID=0");
            await handler.ProcessRequestAsync(mgr.Context);
            var content = mgr.GetResponse();
            var result = Providers.Serialization.Json.DeserializeAnonymous(content, new { Error = false, Message = "" });
            Assert.AreEqual(result.Error, false);
            Assert.AreEqual(result.Message, "ok");

            //mgr = HttpContextManager.Create("reservation.ashx", "http://lnf-dev.eecs.umich.edu/sselscheduler/ajax/reservation.ashx", "Command=save-reservation-history&ReservationID=");
        }
    }
}
