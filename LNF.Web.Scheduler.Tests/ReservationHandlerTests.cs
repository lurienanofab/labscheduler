using LNF.Web.Scheduler.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Web;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationHandlerTests : TestBase
    {
        [TestMethod]
        public void CanProcessRequest()
        {
            using (var mgr = ContextManager.StartRequest(1600, "reservation.ashx", "http://lnf-dev.eecs.umich.edu/sselscheduler/ajax/reservation.ashx", "Command=test&ReservationID=0"))
            {
                var handler = new ReservationHandler();
                handler.ProcessRequest(HttpContext.Current);
                var content = mgr.GetResponse();
                var result = ServiceProvider.Current.Serialization.Json.DeserializeAnonymous(content, new { Error = false, Message = "" });
                Assert.AreEqual(result.Error, false);
                Assert.AreEqual(result.Message, "ok");
            }
        }
    }
}
