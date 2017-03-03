using LNF.Web.Content;

namespace LNF.Web.Scheduler.Content
{
    public class SchedulerMasterPage : LNFMasterPage
    {
        public override bool ShowMenu
        {
            get { return true; }
        }

        public new SchedulerPage Page
        {
            get { return (SchedulerPage)base.Page; }
        }
    }
}
