using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Content;
using System;

namespace LNF.Web.Scheduler.Content
{
    public abstract class SchedulerPage : OnlineServicesPage
    {
        public new SchedulerContextHelper Helper => NewHelper();

        public SchedulerMasterPage SchedulerMaster
        {
            get
            {
                if (Master == null) return null;

                if (typeof(SchedulerMasterPage).IsAssignableFrom(Master.GetType()))
                    return (SchedulerMasterPage)Master;

                throw new Exception($"Cannot convert {Master.GetType().Name} to SchedulerMasterPage.");
            }
        }

        public override ClientPrivilege AuthTypes
        {
            get { return PageSecurity.DefaultAuthTypes; }
        }

        public virtual IResource GetCurrentResource() => Helper.GetCurrentResourceTreeItem();

        /// <summary>
        /// Gets the current ViewType from session
        /// </summary>
        public virtual ViewType GetCurrentView()
        {
            return ContextBase.GetCurrentViewType();
        }

        /// <summary>
        /// Sets the current ViewType session variable
        /// </summary>
        public virtual void SetCurrentView(ViewType value)
        {
            ContextBase.SetCurrentViewType(value);
        }

        /// <summary>
        /// Redirects to page adding Path and Date querystring parameters.
        /// </summary>
        /// <param name="page">The page to redirect to including. For example "ResourceDayWeek.aspx"</param>
        protected void Redirect(string page)
        {
            Response.Redirect(string.Format("~/{0}?Path={1}&Date={2:yyyy-MM-dd}", page, ContextBase.Request.SelectedPath().UrlEncode(), ContextBase.Request.SelectedDate()));
        }

        /// <summary>
        /// Redirects to page adding Path and Date querystring parameters.
        /// </summary>
        /// <param name="page">The page to redirect to including. For example "ResourceDayWeek.aspx"</param>
        /// <param name="endResponse">Indicates whether execution of the current page should terminate.</param>
        protected void Redirect(string page, bool endResponse)
        {
            Response.Redirect(string.Format("~/{0}?Path={1}&Date={2:yyyy-MM-dd}", page, ContextBase.Request.SelectedPath().UrlEncode(), ContextBase.Request.SelectedDate()), endResponse);
        }

        /// <summary>
        /// Redirects to page adding Date and ResevationID querystring parameters.
        /// </summary>
        /// <param name="page">The page to redirect to including. For example "Reservation.aspx"</param>
        /// <param name="reservationId">The ReservationID to add to the querystring.</param>
        protected void Redirect(string page, int reservationId)
        {
            Response.Redirect(string.Format("~/{0}?Date={1:yyyy-MM-dd}&ReservationID={2}", page, ContextBase.Request.SelectedDate(), reservationId));
        }

        private SchedulerContextHelper NewHelper()
        {
            return new SchedulerContextHelper(ContextBase, Provider);
        }
    }
}