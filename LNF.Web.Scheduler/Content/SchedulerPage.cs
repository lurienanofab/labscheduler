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

        public override ClientPrivilege AuthTypes
        {
            get { return PageSecurity.DefaultAuthTypes; }
        }

        protected virtual ResourceModel GetCurrentResource()
        {
            return PathInfo.Current.GetResource();
        }

        /// <summary>
        /// Gets the current ViewType from session
        /// </summary>
        public ViewType GetCurrentView()
        {
            return CacheManager.Current.CurrentUserState().View;
        }

        /// <summary>
        /// Sets the current ViewType session variable
        /// </summary>
        public void SetCurrentView(ViewType value)
        {
            var userState = CacheManager.Current.CurrentUserState();
            if (userState.View != value)
            {
                userState.SetView(value);
                userState.AddAction("Changed view to {0}", value);
            }
        }
    }
}
