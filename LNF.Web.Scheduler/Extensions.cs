using LNF.Models.PhysicalAccess;
using LNF.PhysicalAccess;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Web;

namespace LNF.Web.Scheduler
{
    public static class Extensions
    {
        public static DateTime SelectedDate(this HttpRequest request)
        {
            if (!string.IsNullOrEmpty(request.QueryString["Date"]))
            {
                if (DateTime.TryParse(request.QueryString["Date"], out DateTime result))
                    return result;
            }

            return DateTime.Now.Date;
        }

        public static PathInfo SelectedPath(this HttpRequest request)
        {
            return PathInfo.Parse(request.QueryString["Path"]);
        }

        public static IEnumerable<Badge> CurrentlyInLab(this HttpContext context)
        {
            if (!context.Items.Contains("CurrentlyInLab"))
                context.Items["CurrentlyInLab"] = PhysicalAccessUtility.CurrentlyInLab();

            var result = (IEnumerable<Badge>)context.Items["CurrentlyInLab"];

            return result;
        }

        public static bool IsInLab(this HttpContext context)
        {
            return PhysicalAccessUtility.IsInLab(context.CurrentUser().ClientID);
        }
        
        public static bool ClientInLab(this HttpContext context)
        {
            if (!context.Items.Contains("ClientInLab"))
                context.Items["ClientInLab"] = PhysicalAccessUtility.ClientInLab(context.CurrentUser().ClientID, context.Request.UserHostAddress);

            var result = (bool)context.Items["ClientInLab"];

            return result;
        }

        public static bool ClientInLab(this HttpContext context, int labId)
        {
            return PhysicalAccessUtility.ClientInLab(context.CurrentUser().ClientID, context.Request.UserHostAddress, labId);
        }
    }
}
