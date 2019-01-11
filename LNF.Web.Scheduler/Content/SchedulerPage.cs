using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Content;
using LNF.Data;
using LNF.Repository;
using LNF.Models.PhysicalAccess;
using OnlineServices.Api;
using System.Collections.Generic;
using OnlineServices.Api.PhysicalAccess;

namespace LNF.Web.Scheduler.Content
{
    public abstract class SchedulerPage : LNFPage
    {
        public new SchedulerMasterPage Master
        {
            get { return (SchedulerMasterPage)Page.Master; }
        }

        public IClientManager ClientManager => ServiceProvider.Current.Use<IClientManager>();
        public IClientOrgManager ClientOrgManager => ServiceProvider.Current.Use<IClientOrgManager>();
        public IAccountManager AccountManager => ServiceProvider.Current.Use<IAccountManager>();
        public IReservationManager ReservationManager => ServiceProvider.Current.Use<IReservationManager>();
        public IReservationInviteeManager ReservationInviteeManager => ServiceProvider.Current.Use<IReservationInviteeManager>();
        public IResourceManager ResourceManager => ServiceProvider.Current.Use<IResourceManager>();
        public IEmailManager EmailManager => ServiceProvider.Current.Use<IEmailManager>();
        public IProcessInfoManager ProcessInfoManager => ServiceProvider.Current.Use<IProcessInfoManager>();

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