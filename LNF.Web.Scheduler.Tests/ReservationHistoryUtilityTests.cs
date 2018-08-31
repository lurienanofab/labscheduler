using LNF.Cache;
using LNF.CommonTools;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlineServices.Api.Billing;
using OnlineServices.Api.Scheduler;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LNF.Web.Scheduler.Tests
{
    [TestClass]
    public class ReservationHistoryTests : TestBase
    {
        private Reservation rsv1 { get { return DA.Current.Single<Reservation>(695380); } } // ended 2016-09-19 16:30:12.000
        private Reservation rsv2 { get { return DA.Current.Single<Reservation>(691771); } } // ended 2016-08-31 22:54:58.000
        private Reservation rsv3 { get { return DA.Current.Single<Reservation>(681828); } } // ended 2016-07-25 18:20:25.000
        private Reservation rsv4 { get { return DA.Current.Single<Reservation>(692137); } } // ended 2016-09-02 10:25:09.000
        private Reservation rsv5 { get { return DA.Current.Single<Reservation>(693413); } } // ended 2016-09-08 17:32:30.000

        private ClientItem sandrine;
        private ClientItem greg;
        private ClientItem weibin;
        private ClientItem davidPellinen;

        protected override void Prepare()
        {
            sandrine = CacheManager.Current.GetClient(155); // Sandrine (admin)
            greg = CacheManager.Current.GetClient(4); // Greg Allion (staff)
            weibin = CacheManager.Current.GetClient(268); // Weibin Zhu (normal lab user)
            davidPellinen = CacheManager.Current.GetClient(189); // David Pellinen (normal lab user - not the reserver)
        }

        [TestMethod]
        public void TestReservationCanBeForgiven()
        {
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //false because normal lab users cannot forgive reservations

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot forgive before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //true because 'now' is still in the same day as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //false because 'now' is after the 3rd business day after the reservation - the 3 business days are 1, 2, and 6 because 3 and 4 were weekend days and 5 was a holiday - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //true because admin can always forgive
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //true because admin can always forgive

            //*****************************

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv3, DateTime.Parse("2016-07-25 20:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv3, DateTime.Parse("2016-07-26 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv3, DateTime.Parse("2016-07-27 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv3, DateTime.Parse("2016-07-28 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv3, DateTime.Parse("2016-07-29 00:00:00"))); //false because normal lab users cannot forgive reservations

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot forgive before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv3, DateTime.Parse("2016-07-25 20:00:00"))); //true because 'now' is still in the same day as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv3, DateTime.Parse("2016-07-26 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv3, DateTime.Parse("2016-07-27 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv3, DateTime.Parse("2016-07-28 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv3, DateTime.Parse("2016-07-29 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 3 because there are no weekend days or holidays during the first 3 days of August - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv3, DateTime.Parse("2016-07-25 20:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv3, DateTime.Parse("2016-07-26 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv3, DateTime.Parse("2016-07-27 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv3, DateTime.Parse("2016-07-28 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv3, DateTime.Parse("2016-07-29 00:00:00"))); //true because admin can always change the acct

            //*****************************
            // ended 2016-09-08 17:32:30.000
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-08 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-08 20:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-09 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-10 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-11 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-12 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-13 00:00:00"))); //false because normal lab users cannot forgive reservations
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(weibin, rsv5, DateTime.Parse("2016-09-14 00:00:00"))); //false because normal lab users cannot forgive reservations

            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-08 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot forgive before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-08 20:00:00"))); //true because 'now' is still in the same day as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-09 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-10 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-11 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-12 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-13 00:00:00"))); //true because 'now' is before the 3rd business day after the reservation
            Assert.IsFalse(ReservationHistoryUtility.ReservationCanBeForgiven(greg, rsv5, DateTime.Parse("2016-09-14 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 9, 12, and 13 because 10 and 11 are weekend days - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-08 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-08 20:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-09 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-10 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-11 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-12 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-13 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationCanBeForgiven(sandrine, rsv5, DateTime.Parse("2016-09-14 00:00:00"))); //true because admin can always change the acct
        }

        [TestMethod]
        public void TestReservationAccountCanBeChanged()
        {
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //false because 'now' is before when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month\
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 6 because 3 and 4 were weekend days and 5 was a holiday - normal lab user cannot change accts after the 3rd business day

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot modify before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 6 because 3 and 4 were weekend days and 5 was a holiday - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //true because admin can always change the acct

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-08-31 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-08-31 23:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-01 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-02 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-03 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-04 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-05 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-06 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv2, DateTime.Parse("2016-09-07 00:00:00"))); //false because pellinen is a normal lab user and not the reserver

            //*****************************

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //false because 'now' is before when the reservation ended - normal lab user cannot modify before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv3, DateTime.Parse("2016-07-31 00:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv3, DateTime.Parse("2016-08-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv3, DateTime.Parse("2016-08-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv3, DateTime.Parse("2016-08-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(weibin, rsv3, DateTime.Parse("2016-08-04 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 3 because there are no weekend days or holidays during the first 3 days of August - normal lab user cannot change accts after the 3rd business day

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //false because 'now' is before when the reservation ended - staff cannot modify before the reservation has ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv3, DateTime.Parse("2016-07-31 00:00:00"))); //true because 'now' is still in the same month as when the reservation ended
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv3, DateTime.Parse("2016-08-01 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv3, DateTime.Parse("2016-08-02 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv3, DateTime.Parse("2016-08-03 00:00:00"))); //true because 'now' is before the 3rd business day after the 1st of the following month
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(greg, rsv3, DateTime.Parse("2016-08-04 00:00:00"))); //false because 'now' is after the 3rd business day - the 3 business days are 1, 2, and 3 because there are no weekend days or holidays during the first 3 days of August - staff cannot change accts after the 3rd business day

            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv3, DateTime.Parse("2016-07-31 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv3, DateTime.Parse("2016-08-01 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv3, DateTime.Parse("2016-08-02 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv3, DateTime.Parse("2016-08-03 00:00:00"))); //true because admin can always change the acct
            Assert.IsTrue(ReservationHistoryUtility.ReservationAccountCanBeChanged(sandrine, rsv3, DateTime.Parse("2016-08-04 00:00:00"))); //true because admin can always change the acct

            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv3, DateTime.Parse("2016-07-25 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv3, DateTime.Parse("2016-07-31 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv3, DateTime.Parse("2016-08-01 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv3, DateTime.Parse("2016-08-02 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv3, DateTime.Parse("2016-08-03 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
            Assert.IsFalse(ReservationHistoryUtility.ReservationAccountCanBeChanged(davidPellinen, rsv3, DateTime.Parse("2016-08-04 00:00:00"))); //false because pellinen is a normal lab user and not the reserver
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
        public async Task CanSaveReservationHistoryAndUpdateBilling()
        {
            ContextManager.StartRequest(sandrine);
                
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
            result = await ReservationHistoryUtility.SaveReservationHistory(687470, 1056, 1, "Changing back to original incorrect acct for testing.", false);
            Assert.IsTrue(result);

            // Step 2: check to see if the acct was changed
            using (var sc = new SchedulerClient())
            {
                rsv = await sc.GetReservation(687470);
                Assert.AreEqual(1056, rsv.AccountID);
            }

            DateTime period = rsv.ChargeBeginDateTime.FirstOfMonth();

            //ReservationHistoryUtility.UpdateBillingResult updateBillingResult;

            // Step 3: updating billing to make the change propagate through the billing data
            ReservationHistoryUtility.SendUpdateBillingRequest(period, period.AddMonths(1), rsv.ClientID, new[] { "tool", "room" });

            // Step 4: save reservation history with correct acct
            result = await ReservationHistoryUtility.SaveReservationHistory(687470, 1071, 1, "On The Fly Reservation", false);

            // Step 5: check to see if the acct was changed
            using (var sc = new SchedulerClient())
            {
                rsv = await sc.GetReservation(687470);
                Assert.AreEqual(1071, rsv.AccountID);
            }

            // Step 6: updating billing to make the change propagate through the billing data
            ReservationHistoryUtility.SendUpdateBillingRequest(period, period.AddMonths(1), rsv.ClientID, new[] { "tool", "room" });
        }
    }
}
