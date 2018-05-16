using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ClientTests : TestBase
    {
        [TestMethod]
        public void ClientTests_CanCheckPassword()
        {
            Client c;

            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                c = DA.Current.Single<Client>(1301);
                c.SetPassword("lnf123");
            }

            using (ServiceProvider.Current.DataAccess.StartUnitOfWork())
            {
                c = DA.Current.Single<Client>(1301);

                Assert.IsTrue(c.CheckPassword(ServiceProvider.Current.DataAccess.UniversalPassword));
                Assert.IsTrue(c.CheckPassword("lnf123"));

                // fail for non existing user
                try
                {
                    c = DA.Current.Single<Client>(-1);
                    Assert.IsTrue(c.CheckPassword(ServiceProvider.Current.DataAccess.UniversalPassword));
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is NullReferenceException);
                }
            }
        }
    }
}
