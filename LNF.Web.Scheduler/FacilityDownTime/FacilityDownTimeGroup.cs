using System;
using System.Collections.Generic;

namespace LNF.Web.Scheduler.FacilityDownTime
{
    public class FacilityDownTimeGroup
    {
        public int GroupID { get; set; }
        public int ClientID { get; set; }
        public string DisplayName { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public IEnumerable<FacilityDownTimeReservation> Reservations { get; set; }
    }
}
