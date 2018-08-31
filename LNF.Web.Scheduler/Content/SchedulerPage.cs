using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Content;
using LNF.Data;
using LNF.Repository;

namespace LNF.Web.Scheduler.Content
{
    public abstract class SchedulerPage : LNFPage
    {
        public new SchedulerMasterPage Master
        {
            get { return (SchedulerMasterPage)Page.Master; }
        }

        public IClientManager ClientManager => DA.Use<IClientManager>();
        public IClientOrgManager ClientOrgManager => DA.Use<IClientOrgManager>();
        public IAccountManager AccountManager => DA.Use<IAccountManager>();
        public IReservationManager ReservationManager => DA.Use<IReservationManager>();
        public IReservationInviteeManager ReservationInviteeManager => DA.Use<IReservationInviteeManager>();
        public IResourceManager ResourceManager => DA.Use<IResourceManager>();
        public IEmailManager EmailManager => DA.Use<IEmailManager>();

        //Public Shadows ReadOnly Property Master As MasterPageScheduler
        //Get
        //        Return CType(Page.Master, MasterPageScheduler)
        //    End Get
        //End Property

        public override ClientPrivilege AuthTypes
        {
            get { return PageSecurity.DefaultAuthTypes; }
        }

        public virtual ResourceItem GetCurrentResource()
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