using LNF.Cache;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LNF.Impl.Cache;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class CacheTests : TestBase
    {
        [TestMethod]
        public void CacheTests_CanGetNullTimeSpan()
        {
            var rc = new RedisCache();
            rc.Set("test", null);
            var ts = rc.Get<TimeSpan>("test");
            Assert.AreEqual(TimeSpan.Zero, ts);

            rc.Set("test2", TimeSpan.FromMinutes(5));
            var ts2 = rc.Get<TimeSpan>("test2");
            Assert.AreEqual(TimeSpan.FromMinutes(5), ts2);
        }
    }
}
