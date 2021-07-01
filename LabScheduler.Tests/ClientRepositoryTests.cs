using System;
using System.Linq;
using LNF.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ClientRepositoryTests:TestBase
    {
        [TestMethod]
        public void CanGetActiveClients()
        {
            using (StartUnitOfWork())
            {
                var clients = Provider.Data.Client.GetActiveClients(DateTime.Parse("2020-10-19"), DateTime.Parse("2020-11-18"), ClientPrivilege.LabUser | ClientPrivilege.Staff);
                var howers = clients.Where(x => x.UserName == "hower").ToList();
            }
        }
    }
}
