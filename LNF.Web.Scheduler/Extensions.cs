using System;
using System.Web;

namespace LNF.Web.Scheduler
{
    public static class Extensions
    {
        public static DateTime GetCurrentDate(this HttpRequest request)
        {
            if (!string.IsNullOrEmpty(request.QueryString["Date"]))
            {
                DateTime result;
                if (DateTime.TryParse(request.QueryString["Date"], out result))
                    return result;
            }

            return DateTime.Now.Date;
        }
    }
}
