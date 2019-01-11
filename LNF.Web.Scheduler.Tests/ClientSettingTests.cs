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
        [TestMethod]
        public void CanGetLab()
        {
            using (ContextManager.StartRequest(1600))
            {
                Assert.AreEqual(-1, CacheManager.Current.GetClientSetting().LabID);
                Assert.AreEqual(ClientSetting.DefaultLabID, CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID);
            }

            using (ContextManager.StartRequest(1600))
            {
                CacheManager.Current.GetClientSetting().LabID = 9;
                DA.Current.SaveOrUpdate(CacheManager.Current.GetClientSetting());
            }

            using (ContextManager.StartRequest(1600))
            {
                Assert.AreEqual(9, CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID);
            }

            using (ContextManager.StartRequest(1600))
            {
                CacheManager.Current.GetClientSetting().LabID = -1;
                DA.Current.SaveOrUpdate(CacheManager.Current.GetClientSetting());
            }

            using (ContextManager.StartRequest(1600))
            {
                Assert.AreEqual(-1, CacheManager.Current.GetClientSetting().LabID);
                Assert.AreEqual(1, CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID);
            }
        }
    }
}
