using LNF.Models.Data;
using LNF.Web.Content;

namespace LNF.Web.Scheduler.Content
{
    public class SchedulerUserControl : LNFUserControl
    {
        public SchedulerMasterPage Master
        {
            get { return Page.Master; }
        }

        protected ClientItem CurrentUser
        {
            get { return Page.CurrentUser; }
        }

        public new SchedulerPage Page
        {
            get { return (SchedulerPage)base.Page; }
        }
    }
}
