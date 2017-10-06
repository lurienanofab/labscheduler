using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Content;

namespace LNF.Web.Scheduler.Content
{
    public abstract class SchedulerPage : LNFPage
    {
        public new SchedulerMasterPage Master
        {
            get { return (SchedulerMasterPage)Page.Master; }
        }

        //Public Shadows ReadOnly Property Master As MasterPageScheduler
        //Get
        //        Return CType(Page.Master, MasterPageScheduler)
        //    End Get
        //End Property

        public override ClientPrivilege AuthTypes
        {
            get { return PageSecurity.DefaultAuthTypes; }
        }

        public virtual ResourceModel GetCurrentResource()
        {
            return Request.SelectedPath().GetResource();
        }

        /// <summary>
        /// Gets the current ViewType from session
        /// </summary>
        public ViewType GetCurrentView()
        {
            return CacheManager.Current.CurrentViewType();
        }

        /// <summary>
        /// Sets the current ViewType session variable
        /// </summary>
        public void SetCurrentView(ViewType value)
        {
            CacheManager.Current.CurrentViewType(value);
        }
    }
}