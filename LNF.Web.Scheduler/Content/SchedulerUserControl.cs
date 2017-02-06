using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Web.Content;

namespace LNF.Web.Scheduler.Content
{
    public class SchedulerUserControl : LNFUserControl
    {
        public SchedulerMasterPage Master
        {
            get { return (SchedulerMasterPage)Page.Master; }
        }

        protected ClientModel CurrentUser
        {
            get { return Page.CurrentUser; }
        }

        protected ResourceModel GetCurrentResource()
        {
            return PathInfo.Current.GetResource();
        }
    }
}
