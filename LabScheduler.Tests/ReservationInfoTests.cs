using LNF;
using LNF.Impl.DataAccess;
using LNF.Impl.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationInfoTests
    {
        [TestMethod]
        public void CanSelectByDateRange()
        {
            ContainerContextFactory.Current.NewThreadScopedContext();
            var context = ContainerContextFactory.Current.GetContext();
            var config = new ThreadStaticContainerConfiguration(context);
            config.RegisterAllTypes();
            var mgr = context.GetInstance<ISessionManager>();
            var provider = context.GetInstance<IProvider>();
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
