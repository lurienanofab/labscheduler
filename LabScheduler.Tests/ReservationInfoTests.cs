using System;
using System.Linq;
using LNF;
using LNF.Billing.Process;
using LNF.Data;
using LNF.DataAccess;
using LNF.Impl.DataAccess;
using LNF.Impl.Repository.Scheduler;
using LNF.Impl.Scheduler;
using LNF.Repository;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationInfoTests
    {
        [TestMethod]
        public void CanSelectByDateRange()
        {
            var resolver = new LNF.Impl.ThreadStaticResolver();
            var mgr = resolver.GetInstance<ISessionManager>();
            var provider = resolver.GetInstance<IProvider>();
            ServiceProvider.Setup(provider);

            using (var uow = new NHibernateUnitOfWork(mgr))
            {
                var sd = DateTime.Parse("2020-05-18");
                var ed = DateTime.Parse("2020-05-19");
                provider.Scheduler.Reservation.SelectByDateRange(sd, ed, true);
            }
        }
    }
}
