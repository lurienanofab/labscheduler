using LNF.Models.Data;
using LNF.Models.PhysicalAccess;
using LNF.Models.Scheduler;
using LNF.PhysicalAccess;
using LNF.Repository;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
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
                context.Items["CurrentlyInLab"] = context.GetPhysicalAccessUtility().CurrentlyInLab;

            var result = (IEnumerable<Badge>)context.Items["CurrentlyInLab"];

            return result;
        }

        public static bool IsInLab(this HttpContext context)
        {
            return context.GetPhysicalAccessUtility().IsInLab(context.CurrentUser().ClientID);
        }

        public static bool ClientInLab(this HttpContext context)
        {
            if (!context.Items.Contains("ClientInLab"))
                context.Items["ClientInLab"] = context.GetPhysicalAccessUtility().ClientInLab(context.CurrentUser().ClientID);

            var result = (bool)context.Items["ClientInLab"];

            return result;
        }

        public static bool ClientInLab(this HttpContext context, int labId)
        {
            return context.GetPhysicalAccessUtility().ClientInLab(context.CurrentUser().ClientID, labId);
        }

        public static PhysicalAccessUtility GetPhysicalAccessUtility(this HttpContext context)
        {
            if (!context.Items.Contains("PhysicalAccessUtility"))
                context.Items["PhysicalAccessUtility"] = new PhysicalAccessUtility(context.Request.UserHostAddress);

            var result = (PhysicalAccessUtility)context.Items["PhysicalAccessUtility"];

            return result;
        }

        public static ReservationClientItem GetReservationClientItem(this HttpContext context, ReservationItemWithInvitees rsv)
        {
            return context.GetReservationClientItem(rsv, context.CurrentUser());
        }

        public static ReservationClientItem GetReservationClientItem(this HttpContext context, ReservationItemWithInvitees rsv, ClientItem client)
        {
            var mgr = ServiceProvider.Current.Use<IReservationManager>();
            var resourceClients = mgr.GetResourceClients(rsv.ResourceID);
            var userAuth = mgr.GetAuthLevel(resourceClients, client);

            var result = new ReservationClientItem
            {
                ClientID = client.ClientID,
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.ResourceID,
                IsReserver = rsv.ClientID == client.ClientID,
                IsInvited = rsv.Invitees.Any(x => x.ClientID == client.ClientID),
                InLab = context.ClientInLab(rsv.LabID),
                UserAuth = userAuth
            };

            return result;
        }
    }
}
