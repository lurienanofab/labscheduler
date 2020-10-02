using System;
using System.Linq;
using LNF.Impl.DataAccess;
using LNF.Impl.Repository;
using LNF.Impl.Repository.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationProcessInfoTests
    {
        [TestMethod]
        public void CanDeleteReservationProcessInfo()
        {
            using (ThreadStaticSession.Current.StartUnitOfWork())
            {
                int reservationId = 994204;
                var Session = ThreadStaticSession.Current.GetNHibernateSession();
                Session.DeleteMany(Session.Query<ReservationProcessInfo>().Where(x => x.Reservation.ReservationID == reservationId));
            }
        }
    }
}
