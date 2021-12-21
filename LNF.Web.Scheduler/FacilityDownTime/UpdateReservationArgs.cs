using Newtonsoft.Json;
using System;

namespace LNF.Web.Scheduler.FacilityDownTime
{
    public class UpdateReservationArgs : IDateRange
    {
        [JsonProperty("clientId")]
        public int ClientID { get; set; }

        [JsonProperty("groupId")]
        public int GroupID { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
}
