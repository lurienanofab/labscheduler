using LNF.Cache;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ClientSettingTests : TestBase
    {
        protected override void Prepare()
        {
            ContextManager.CurrentClient = new ClientItem()
            {
                ClientID = 728,
                UserName = "codrin",
                Privs = (ClientPrivilege)1541
            };
        }

        [TestMethod]
        public void ClientSettingTests_CanGetLab()
        {
            ContextManager.StartRequest();

            using (Providers.DataAccess.StartUnitOfWork())
            {
                Assert.AreEqual(-1, CacheManager.Current.GetClientSetting().LabID);
                Assert.AreEqual(ClientSetting.DefaultLabID, CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID);
            }

            CacheManager.Current.GetClientSetting().LabID = 9;

            using (Providers.DataAccess.StartUnitOfWork())
            {
                DA.Current.SaveOrUpdate(CacheManager.Current.GetClientSetting());
            }

            using (Providers.DataAccess.StartUnitOfWork())
            {
                Assert.AreEqual(9, CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID);
            }

            CacheManager.Current.GetClientSetting().LabID = -1;

            using (Providers.DataAccess.StartUnitOfWork())
            {
                DA.Current.SaveOrUpdate(CacheManager.Current.GetClientSetting());
            }

            using (Providers.DataAccess.StartUnitOfWork())
            {
                Assert.AreEqual(-1, CacheManager.Current.GetClientSetting().LabID);
                Assert.AreEqual(1, CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID);
            }
        }
    }
}
