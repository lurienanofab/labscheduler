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
        public void CanCheckPassword()
        {
            Client c;

            using (ContextManager.StartRequest(1600))
            {
                c = DA.Current.Single<Client>(1600);
                c.SetPassword("lnf123");
            }

            using (ContextManager.StartRequest(1600))
            {
                c = DA.Current.Single<Client>(1600);

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
