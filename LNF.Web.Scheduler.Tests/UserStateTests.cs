using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LNF;
using LNF.Web.Scheduler;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class UserStateTests
    {
        [TestMethod]
        public void UserStateTests_CanGetUserState()
        {
            using (Providers.DataAccess.StartUnitOfWork())
            {
                int clientId = 1301;
                var state = MongoRepository.Current.GetUserState(clientId);
                Assert.IsNotNull(state);
            }
        }
    }
}
