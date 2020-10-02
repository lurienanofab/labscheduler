using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace LNF.Web.Scheduler.Models
{
    public class LabCleanConfiguration
    {
        public static LabCleanConfiguration GetCurrentConfiguration()
        {
            var filePath = Path.Combine(ConfigurationManager.AppSettings["SecurePath"], "scheduler", "labclean.json");

            if (!File.Exists(filePath))
                throw new Exception($"Cannot find labclean config file. [path = '{filePath}']");

            var content = File.ReadAllText(filePath);
            var result = JsonConvert.DeserializeObject<LabCleanConfiguration>(content);
            return result;
        }

        [JsonProperty("items")] public IEnumerable<LabCleanItem> Items { get; set; }
    }
}
