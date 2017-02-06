using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNF.Web.Scheduler
{
    public static class PropertiesManager
    {
        public static int[] GetGranularityValues(int resourceId)
        {
            var props = MongoRepository.Current.GetProperties();
            return props.GetGranularityValues(resourceId);
        }
    }
}
