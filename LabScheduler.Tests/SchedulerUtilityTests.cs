using System;
using System.Linq;
using LNF;
using LNF.Impl;
using LNF.Web.Scheduler;
using LNF.Web.Scheduler.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;

namespace LabScheduler.Tests
{
    [TestClass]
    public class SchedulerUtilityTests
    {
        [TestMethod]
        public void CanShowLabCleanWarning()
        {
            bool isLabCleanTime;
            DateTime beginDateTime;
            DateTime endDateTime;

            var container = new Container();
            var cfg = new ThreadStaticContainerConfiguration(container);
            cfg.Configure();

            IProvider provider = container.GetInstance<IProvider>();

            using (provider.DataAccess.StartUnitOfWork())
            {
                // Monday
                beginDateTime = DateTime.Parse("2020-09-14 08:00:00");
                endDateTime = DateTime.Parse("2020-09-14 10:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 07:00:00");
                endDateTime = DateTime.Parse("2020-09-14 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 07:00:00");
                endDateTime = DateTime.Parse("2020-09-14 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 07:00:00");
                endDateTime = DateTime.Parse("2020-09-14 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 10:15:00");
                endDateTime = DateTime.Parse("2020-09-14 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-14 10:10:00");
                endDateTime = DateTime.Parse("2020-09-14 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                // Thursday
                beginDateTime = DateTime.Parse("2020-09-17 08:00:00");
                endDateTime = DateTime.Parse("2020-09-17 10:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 07:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 07:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 07:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 10:15:00");
                endDateTime = DateTime.Parse("2020-09-17 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-17 10:10:00");
                endDateTime = DateTime.Parse("2020-09-17 11:15:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                // Wednesday
                beginDateTime = DateTime.Parse("2020-09-16 07:00:00");
                endDateTime = DateTime.Parse("2020-09-16 19:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                // Multi-day
                beginDateTime = DateTime.Parse("2020-09-16 23:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-16 23:00:00");
                endDateTime = DateTime.Parse("2020-09-17 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-18 23:00:00");
                endDateTime = DateTime.Parse("2020-09-21 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-18 23:00:00");
                endDateTime = DateTime.Parse("2020-09-21 08:30:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                // Holiday
                beginDateTime = DateTime.Parse("2020-09-08 07:00:00");
                endDateTime = DateTime.Parse("2020-09-08 08:35:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-08 10:10:00");
                endDateTime = DateTime.Parse("2020-09-09 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsTrue(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-08 10:15:00");
                endDateTime = DateTime.Parse("2020-09-09 08:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-06 08:15:00");
                endDateTime = DateTime.Parse("2020-09-07 10:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);

                beginDateTime = DateTime.Parse("2020-09-07 08:0:00");
                endDateTime = DateTime.Parse("2020-09-07 11:00:00");
                isLabCleanTime = SchedulerUtility.Create(provider).ShowLabCleanWarning(beginDateTime, endDateTime);
                Assert.IsFalse(isLabCleanTime);
            }
        }

        [TestMethod]
        public void CanGetLabCleanConfiguration()
        {
            LabCleanConfiguration config = LabCleanConfiguration.GetCurrentConfiguration();
            Assert.AreEqual(1, config.Items.Count());
            Assert.AreEqual(1, config.Items.ElementAt(0).Days[0]);
            Assert.AreEqual(4, config.Items.ElementAt(0).Days[1]);
            Assert.AreEqual(510, config.Items.ElementAt(0).StartTime.TotalMinutes);
            Assert.AreEqual(570, config.Items.ElementAt(0).EndTime.TotalMinutes);
            Assert.AreEqual(0, config.Items.ElementAt(0).StartPadding);
            Assert.AreEqual(45, config.Items.ElementAt(0).EndPadding);
            Assert.IsTrue(config.Items.ElementAt(0).Active);
        }
    }
}
