using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class ReservationWeekView : UserControl
    {
        protected Repeater rptReservationGrid;

        public DateTime SelectedDate { get; set; }

        /// <summary>
        /// Returns the Sunday before the SelectedDate
        /// </summary>
        public DateTime GetWeekStartDate()
        {
            return SelectedDate.AddDays(-(int)SelectedDate.DayOfWeek);
        }
        
        protected DateTime GetHeaderDate(int index)
        {
            return GetWeekStartDate().AddDays(index);
        }
    }
}
