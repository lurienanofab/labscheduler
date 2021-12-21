using LNF.Scheduler;
using System;
using System.Web;

namespace LNF.Web.Scheduler
{
    public static class UrlUtility
    {
        public static string GetReservationControllerUrl()
        {
            return VirtualPathUtility.ToAbsolute("~/ReservationController.ashx");
        }

        public static string GetChangeHourRangeUrl(string range, LocationPathInfo locationPath, DateTime selectedDate, ViewType view)
        {
            return $"{GetReservationControllerUrl()}?Command=ChangeHourRange&Range={range}&LocationPath={locationPath.UrlEncode()}&Date={selectedDate:yyyy-MM-dd}&View={view}";
        }

        public static string GetChangeHourRangeUrl(string range, PathInfo path, DateTime selectedDate, ViewType view)
        {
            return $"{GetReservationControllerUrl()}?Command=ChangeHourRange&Range={range}&Path={path.UrlEncode()}&Date={selectedDate:yyyy-MM-dd}&View={view}";
        }

        public static string GetDeleteReservationUrl(int reservationId, DateTime date, ReservationState state, ViewType view)
        {
            return $"{GetReservationControllerUrl()}?Command=DeleteReservation&ReservationID={reservationId}&Date={date:yyyy-MM-dd}&Time={date.TimeOfDay.TotalMinutes}&State={state}&View={view}";
        }

        public static string GetModifyReservationUrl(int reservationId, DateTime date, ReservationState state, ViewType view)
        {
            return $"{GetReservationControllerUrl()}?Command=ModifyReservation&ReservationID={reservationId}&Date={date:yyyy-MM-dd}&Time={date.TimeOfDay.TotalMinutes}&State={state}&View={view}";
        }
    }
}
