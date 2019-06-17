using LNF.Models.Data;
using LNF.Scheduler;
using LNF.Web.Content;
using System;

namespace LNF.Web.Scheduler.Content
{
    public class SchedulerUserControl : LNFUserControl
    {
        public SchedulerMasterPage Master
        {
            get { return Page.Master; }
        }

        protected IClient CurrentUser
        {
            get { return Page.CurrentUser; }
        }

        public new SchedulerPage Page
        {
            get { return (SchedulerPage)base.Page; }
        }

        public ReservationUtility GetReservationUtility(DateTime now)
        {
            return new ReservationUtility(now, ServiceProvider.Current);
        }

        public IProvider Provider => ServiceProvider.Current;
    }
}
