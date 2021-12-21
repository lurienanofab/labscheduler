using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LNF.Web.Scheduler.FacilityDownTime
{
    public class InsertReservationArgs : IDateRange
    {
        [JsonProperty("clientId")]
        public int ClientID { get; set; }

        [JsonProperty("tools")]
        public IEnumerable<int> Tools { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }
    }
}
