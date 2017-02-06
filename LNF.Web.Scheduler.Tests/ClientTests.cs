using LNF.Data;
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

            using (Providers.DataAccess.StartUnitOfWork())
            {
                c = ClientUtility.Find(1301);
                c.SetPassword("lnf123");
            }

            using (Providers.DataAccess.StartUnitOfWork())
            {
                c = ClientUtility.Find(1301);

                Assert.IsTrue(c.CheckPassword(Providers.DataAccess.UniversalPassword));
                Assert.IsTrue(c.CheckPassword("lnf123"));

                // fail for non existing user
                try
                {
                    c = ClientUtility.Find(-1);
                    Assert.IsTrue(c.CheckPassword(Providers.DataAccess.UniversalPassword));
                }
                catch (Exception ex)
                {
                    Assert.IsTrue(ex is NullReferenceException);
                }
            }
        }
    }
}
