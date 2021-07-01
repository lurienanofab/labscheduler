using LNF;
using LNF.Impl;
using LNF.Impl.DataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationInfoTests
    {
        [TestMethod]
        public void CanSelectByDateRange()
        {
            var container = new Container();
            var config = new ThreadStaticContainerConfiguration(container);
            config.RegisterAllTypes();
            var mgr = container.GetInstance<ISessionManager>();
            var provider = container.GetInstance<IProvider>();
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
