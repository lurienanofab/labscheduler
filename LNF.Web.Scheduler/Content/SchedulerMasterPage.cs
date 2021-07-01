using LNF.Web.Content;
using System;

namespace LNF.Web.Scheduler.Content
{
    public class SchedulerMasterPage : LNFMasterPage
    {
        public override bool ShowMenu
        {
            get { return true; }
        }

        public SchedulerPage SchedulerPage
        {
            get
            {
                if (Page == null) return null;

                if(typeof(SchedulerPage).IsAssignableFrom(Page.GetType()))
                    return (SchedulerPage)Page;

                throw new Exception($"Cannot convert {Page.GetType().Name} to SchedulerPage.");
            }
        }

        public SchedulerContextHelper Helper => SchedulerPage.Helper;
    }
}
