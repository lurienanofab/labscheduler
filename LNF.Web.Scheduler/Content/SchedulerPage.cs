using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Content;
using System;

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

        public virtual IResource GetCurrentResource()
        {
            return ContextBase.GetCurrentResourceTreeItem();
        }

        /// <summary>
        /// Gets the current ViewType from session
        /// </summary>
        public ViewType GetCurrentView()
        {
            return ContextBase.GetCurrentViewType();
        }

        /// <summary>
        /// Sets the current ViewType session variable
        /// </summary>
        public void SetCurrentView(ViewType value)
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

        public ReservationUtility GetReservationUtility(DateTime now)
        {
            return new ReservationUtility(now, Provider);
        }
    }
}