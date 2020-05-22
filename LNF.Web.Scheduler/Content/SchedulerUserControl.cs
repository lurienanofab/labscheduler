using LNF.Web.Content;
using System;
using System.Web;

namespace LNF.Web.Scheduler.Content
{
    public class SchedulerUserControl : LNFUserControl
    {
        public ContextHelper Helper => SchedulerPage.Helper;
        public SchedulerMasterPage SchedulerMaster => SchedulerPage.SchedulerMaster;
        
        public SchedulerPage SchedulerPage
        {
            get
            {
                if (Page == null) return null;

                if (typeof(SchedulerPage).IsAssignableFrom(Page.GetType()))
                    return (SchedulerPage)Page;

                throw new Exception($"Cannot convert {Page.GetType().Name} to SchedulerPage.");
            }
        }
    }
}
