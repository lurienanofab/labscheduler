using System;

namespace LNF.Web.Scheduler.FacilityDownTime
{
    public class FacilityDownTimeReservation
    {
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; internal set; }
        public string ResourceDisplayName { get; set; }
        public int ProcessTechID { get; set; }
        public string ProcessTechName { get; set; }
        public int LabID { get; set; }
        public string LabName { get; set; }
        public string LabDisplayName { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? ActualBeginDateTime { get; set; }
        public DateTime? ActualEndDateTime { get; set; }
        public bool Editable { get; internal set; }
    }
}
