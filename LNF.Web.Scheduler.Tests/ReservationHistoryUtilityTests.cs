using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Models.Worker;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlineServices.Api.Scheduler;
using System;
using System.Threading.Tasks;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationHistoryTests : TestBase
    {
        protected IReservationManager ReservationManager => ServiceProvider.Current.Use<IReservationManager>();

        private ReservationItem Reservation1 { get { return ServiceProvider.Current.Scheduler.GetReservation(695380); } } // ended 2016-09-19 16:30:12.000
        private ReservationItem Reservation2 { get { return ServiceProvider.Current.Scheduler.GetReservation(691771); } } // ended 2016-08-31 22:54:58.000
        private ReservationItem Reservation3 { get { return ServiceProvider.Current.Scheduler.GetReservation(681828); } } // ended 2016-07-25 18:20:25.000
        private ReservationItem Reservation4 { get { return ServiceProvider.Current.Scheduler.GetReservation(692137); } } // ended 2016-09-02 10:25:09.000
        private ReservationItem Reservation5 { get { return ServiceProvider.Current.Scheduler.GetReservation(693413); } } // ended 2016-09-08 17:32:30.000

        private readonly int sandrine = 155;         // Sandrine (admin)
        private readonly int greg = 4;               // Greg Allion (staff)
        private readonly int weibin = 268;           // Weibin Zhu (normal lab user)
        private readonly int davidPellinen = 189;    // David Pellinen (normal lab user - not the reserver)

        [TestInitialize]
        public void TestInit()
        {
            ContextManager.StartRequest(1600);
        }

        [TestMethod]
        public void TestReservationCanBeForgiven()
        {
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //false because normal lab users cannot forgive reservations

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot forgive before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //true because 'now' is still in the same day as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //false because 'now' is after the 3rd business day after the reservation - the 3 business days are 1, 2, and 6 because 3 and 4 were weekend days and 5 was a holiday - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //true because admin can always forgive

            //*****************************

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-25 20:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-26 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-27 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-28 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-29 00:00:00"))); //false because normal lab users cannot forgive reservations

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot forgive before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation3, DateTime.Parse("2016-07-25 20:00:00"))); //true because 'now' is still in the same day as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation3, DateTime.Parse("2016-07-26 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation3, DateTime.Parse("2016-07-27 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation3, DateTime.Parse("2016-07-28 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation3, DateTime.Parse("2016-07-29 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 3 because there are no weekend days or holidays during the first 3 days of August - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-25 20:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-26 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-27 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-28 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-29 00:00:00"))); //true because admin can always change the acct

            //*****************************
            // ended 2016-09-08 17:32:30.000
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-08 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-08 20:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-09 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-10 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-11 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-12 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-13 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(weibin), Reservation5, DateTime.Parse("2016-09-14 00:00:00"))); //false because normal lab users cannot forgive reservations

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-08 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot forgive before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-08 20:00:00"))); //true because 'now' is still in the same day as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-09 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-10 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-11 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-12 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-13 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(greg), Reservation5, DateTime.Parse("2016-09-14 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 9, 12, and 13 because 10 and 11 are weekend days - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-08 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-08 20:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-09 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-10 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-11 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-12 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-13 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(GetClient(sandrine), Reservation5, DateTime.Parse("2016-09-14 00:00:00"))); //true because admin can always change the acct
        }

        [TestMethod]
        public void TestReservationAccountCanBeChanged()
        {
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //false because 'now' is before when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month\
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 6 because 3 and 4 were weekend days and 5 was a holiday - normal lab user cannot change accts after the 3rd business day

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot modify before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 6 because 3 and 4 were weekend days and 5 was a holiday - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //true because admin can always change the acct

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-08-31 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-08-31 23:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-01 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-02 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-03 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-04 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-05 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-06 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation2, DateTime.Parse("2016-09-07 00:00:00"))); //false because pellinen is a normal lab user and not the reserver

            //*****************************

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //false because 'now' is before when the reservation ended - normal lab user cannot modify before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation3, DateTime.Parse("2016-07-31 00:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation3, DateTime.Parse("2016-08-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation3, DateTime.Parse("2016-08-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation3, DateTime.Parse("2016-08-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(weibin), Reservation3, DateTime.Parse("2016-08-04 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 3 because there are no weekend days or holidays during the first 3 days of August - normal lab user cannot change accts after the 3rd business day

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot modify before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation3, DateTime.Parse("2016-07-31 00:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation3, DateTime.Parse("2016-08-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation3, DateTime.Parse("2016-08-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation3, DateTime.Parse("2016-08-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(greg), Reservation3, DateTime.Parse("2016-08-04 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 3 because there are no weekend days or holidays during the first 3 days of August - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation3, DateTime.Parse("2016-07-31 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation3, DateTime.Parse("2016-08-01 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation3, DateTime.Parse("2016-08-02 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation3, DateTime.Parse("2016-08-03 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(sandrine), Reservation3, DateTime.Parse("2016-08-04 00:00:00"))); //true because admin can always change the acct

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation3, DateTime.Parse("2016-07-25 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation3, DateTime.Parse("2016-07-31 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation3, DateTime.Parse("2016-08-01 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation3, DateTime.Parse("2016-08-02 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation3, DateTime.Parse("2016-08-03 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(GetClient(davidPellinen), Reservation3, DateTime.Parse("2016-08-04 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
        }

        [TestMethod]
        public void CanGetStartAndEndDates()
        {
            Assert.AreEqual(DateTime.Parse("2016-09-23"), ReservationHistoryUtility.GetStartDate("09/23/2016"));
            Assert.AreEqual(DateTime.Now.Date, ReservationHistoryUtility.GetStartDate(DateTime.Now.ToString()));
            Assert.AreEqual(DateTime.Now.Date, ReservationHistoryUtility.GetStartDate("invalid text"));
            Assert.AreEqual(Reservation.MinReservationBeginDate, ReservationHistoryUtility.GetStartDate(null));
            Assert.AreEqual(Reservation.MinReservationBeginDate, ReservationHistoryUtility.GetStartDate(""));

            Assert.AreEqual(DateTime.Parse("2016-09-24"), ReservationHistoryUtility.GetEndDate("09/23/2016"));
            Assert.AreEqual(DateTime.Now.Date.AddDays(1), ReservationHistoryUtility.GetEndDate(DateTime.Now.ToString()));
            Assert.AreEqual(DateTime.Now.Date.AddDays(1), ReservationHistoryUtility.GetEndDate("invalid text"));
            Assert.AreEqual(Reservation.MaxReservationEndDate, ReservationHistoryUtility.GetEndDate(null));
            Assert.AreEqual(Reservation.MaxReservationEndDate, ReservationHistoryUtility.GetEndDate(""));
        }

        [TestMethod]
        public void CanSaveReservationHistoryAndUpdateBilling()
        {
            using (ContextManager.StartRequest(155))
            {
                // 687470 is a chemical cabinet reservation made by Brian VanderElzen [245]
                //      BeginDateTime: 2016-08-15 15:15:00.000
                //      EndDateTime: 2016-08-15 15:20:00.000
                //      ActualBeginDateTime: 2016-08-15 15:10:47.000
                //      ActualEndDateTime: 2016-08-15 15:20:05.000
                //
                //      Original AccountID: 1056 [Harvard-Jorgolli] - was used because of default account client setting
                //      Changed to AccountID: 1071 [BMV Solutions]

                ReservationItem rsv;

                bool result;

                // Step 1: save reservation history with original incorrect acct
                result = ServiceProvider.Current.Scheduler.UpdateReservationHistory(new ReservationHistoryUpdate { ClientID = 155, ReservationID = 687470, AccountID = 1056, ChargeMultiplier = 1, Notes = "Changing back to original incorrect acct for testing.", EmailClient = false });
                Assert.IsTrue(result);

                // Step 2: check to see if the acct was changed
                rsv = ServiceProvider.Current.Scheduler.GetReservation(687470);
                Assert.AreEqual(1056, rsv.AccountID);

                DateTime period = rsv.ChargeBeginDateTime.FirstOfMonth();

                // Step 3: save reservation history with correct acct, and updating billing to make the change propagate through the billing data
                var saveHistoryResult = ReservationManager.SaveReservationHistory(rsv, 1071, 1, "On The Fly Reservation", false);

                // Step 4: check to see if the acct was changed
                rsv = ServiceProvider.Current.Scheduler.GetReservation(687470);
                Assert.AreEqual(1071, rsv.AccountID);

                // Step 5: updating billing to make the change propagate through the billing data
                ServiceProvider.Current.Worker.Execute(new UpdateBillingWorkerRequest(period, rsv.ClientID, new[] { "tool", "room" }));
            }
        }
    }
}
