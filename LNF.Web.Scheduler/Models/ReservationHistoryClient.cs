using System.Collections.Generic;

namespace LNF.Web.Scheduler.Models
{
    public class ReservationHistoryClient
    {
        public int ClientID { get; set; }
        public string DisplayName { get; set; }
    }

    public class ReservationHistoryClientComparer : IEqualityComparer<ReservationHistoryClient>
    {
        public bool Equals(ReservationHistoryClient x, ReservationHistoryClient y)
        {
            if (x == null) return false;
            if (y == null) return false;
            return x.ClientID == y.ClientID;
        }

        public int GetHashCode(ReservationHistoryClient obj)
        {
            return obj.ClientID.GetHashCode();
        }
    }
}
