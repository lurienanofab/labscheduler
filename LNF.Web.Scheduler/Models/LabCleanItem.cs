using Newtonsoft.Json;
using System;

namespace LNF.Web.Scheduler.Models
{
    public class LabCleanItem
    {
        [JsonProperty("days")] public int[] Days { get; set; }
        [JsonProperty("startTime")] public TimeSpan StartTime { get; set; }
        [JsonProperty("endTime")] public TimeSpan EndTime { get; set; }
        [JsonProperty("startPadding")] public int StartPadding { get; set; }
        [JsonProperty("endPadding")] public int EndPadding { get; set; }
        [JsonProperty("active")] public bool Active { get; set; }
    }
}
