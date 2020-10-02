using LNF.Data;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNF.Web.Scheduler
{
    public class CostEstimator
    {
        public IProvider Provider { get; }

        public CostEstimator(IProvider provider)
        {
            Provider = provider;
        }

        public decimal EstimateToolRunCost(int chargeTypeId, int resourceId, double duration)
        {
            //duration is minutes

            var costs = Provider.Data.Cost.FindToolCosts(resourceId, chargeTypeId: chargeTypeId);

            List<IResourceCost> resCosts = ResourceCost.CreateResourceCosts(costs).ToList();

            var rc = resCosts.FirstOrDefault();

            decimal result = 0;
            decimal dur = Convert.ToDecimal(duration) / 60M;

            if (rc != null)
            {
                result = (rc.HourlyRate() * dur) + rc.PerUseRate();
            }

            return result;
        }
    }
}
