using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.PhysicalAccess;
using LNF.Models.Scheduler;
using LNF.PhysicalAccess;
using LNF.Repository;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using LNF.Web.Scheduler.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public static class Extensions
    {
        public static DateTime SelectedDate(this HttpRequestBase request)
        {
            if (!string.IsNullOrEmpty(request.QueryString["Date"]))
            {
                if (DateTime.TryParse(request.QueryString["Date"], out DateTime result))
                    return result;
            }

            return DateTime.Now.Date;
        }

        public static PathInfo SelectedPath(this HttpRequestBase request)
        {
            return PathInfo.Parse(request.QueryString["Path"]);
        }

        public static IEnumerable<Badge> CurrentlyInLab(this HttpContextBase context)
        {
            if (!context.Items.Contains("CurrentlyInLab"))
                context.Items["CurrentlyInLab"] = context.GetPhysicalAccessUtility().CurrentlyInLab;

            var result = (IEnumerable<Badge>)context.Items["CurrentlyInLab"];

            return result;
        }

        public static bool IsInLab(this HttpContextBase context)
        {
            return context.GetPhysicalAccessUtility().IsInLab(context.CurrentUser().ClientID);
        }

        public static bool ClientInLab(this HttpContextBase context)
        {
            if (!context.Items.Contains("ClientInLab"))
                context.Items["ClientInLab"] = context.GetPhysicalAccessUtility().ClientInLab(context.CurrentUser().ClientID);

            var result = (bool)context.Items["ClientInLab"];

            return result;
        }

        public static ILab ClientLab(this HttpContextBase context)
        {
            if (!context.Items.Contains("ClientLab"))
            {
                var inlab = context.GetPhysicalAccessUtility().CurrentlyInLab;
                var badge = inlab.FirstOrDefault(x => x.ClientID == context.CurrentUser().ClientID);
                ILab lab = null;
                if (badge != null)
                    lab = CacheManager.Current.GetLab(badge.CurrentAreaName);
                context.Items["ClientLab"] = lab;
            }

            var result = (ILab)context.Items["ClientLab"];

            return result;
        }

        public static bool ClientInLab(this HttpContextBase context, int labId)
        {
            return context.GetPhysicalAccessUtility().ClientInLab(context.CurrentUser().ClientID, labId);
        }

        public static PhysicalAccessUtility GetPhysicalAccessUtility(this HttpContextBase context)
        {
            if (!context.Items.Contains("PhysicalAccessUtility"))
                context.Items["PhysicalAccessUtility"] = new PhysicalAccessUtility(context.Request.UserHostAddress);

            var result = (PhysicalAccessUtility)context.Items["PhysicalAccessUtility"];

            return result;
        }

        public static ClientSetting GetClientSetting(this HttpContextBase context)
        {
            var result = (ClientSetting)context.Items["ClientSetting"];

            if (result == null)
            {
                result = ClientSetting.GetClientSettingOrDefault(context.CurrentUser().ClientID);
                context.Items["ClientSetting"] = result;
            }

            return result;
        }

        public static bool GetDisplayDefaultHours(this HttpContextBase context)
        {
            if (context.Session["DisplayDefaultHours"] == null)
            {
                context.Session["DisplayDefaultHours"] = true;
            }

            var result = (bool)context.Session["DisplayDefaultHours"];

            return result;
        }

        public static void SetDisplayDefaultHours(this HttpContextBase context, bool value)
        {
            context.Session["DisplayDefaultHours"] = value;
        }

        public static IReservation GetReservation(this HttpContextBase context)
        {
            if (int.TryParse(context.Request.QueryString["ReservationID"], out int reservationId))
            {
                var result = ServiceProvider.Current.Scheduler.Reservation.GetReservation(reservationId);

                if (result == null)
                    throw new InvalidOperationException($"Cannot find a Reservation with ReservationID = {reservationId}");

                return result;
            }
            else
                throw new InvalidOperationException("Missing query string parameter: ReservationID");
        }

        public static IReservationWithInvitees GetReservationWithInvitees(this HttpContextBase context)
        {
            if (int.TryParse(context.Request.QueryString["ReservationID"], out int reservationId))
            {
                var result = ServiceProvider.Current.Scheduler.Reservation.GetReservationWithInvitees(reservationId);

                if (result == null)
                    throw new InvalidOperationException($"Cannot find a Reservation with ReservationID = {reservationId}");

                return result;
            }
            else
                throw new InvalidOperationException("Missing query string parameter: ReservationID");
        }

        public static ReservationClient GetReservationClientItem(this HttpContextBase context, IReservationWithInvitees rsv)
        {
            return context.GetReservationClientItem(rsv, context.CurrentUser());
        }

        public static ReservationClient GetReservationClientItem(this HttpContextBase context, IReservationWithInvitees rsv, IClient client)
        {
            var resourceClients = ServiceProvider.Current.Scheduler.Reservation.GetResourceClients(rsv.ResourceID);
            var userAuth = ReservationUtility.GetAuthLevel(resourceClients, client);

            var result = new ReservationClient
            {
                ClientID = client.ClientID,
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.ResourceID,
                IsReserver = rsv.ClientID == client.ClientID,
                IsInvited = rsv.Invitees.Any(x => x.InviteeID == client.ClientID),
                InLab = context.ClientInLab(rsv.LabID),
                UserAuth = userAuth
            };

            return result;
        }

        public static ReservationClient GetReservationClientItem(this HttpContextBase context, IReservation rsv)
        {
            return context.GetReservationClientItem(rsv, context.CurrentUser());
        }

        public static ReservationClient GetReservationClientItem(this HttpContextBase context, IReservation rsv, IClient client)
        {
            var resourceClients = ServiceProvider.Current.Scheduler.Reservation.GetResourceClients(rsv.ResourceID);
            var userAuth = ReservationUtility.GetAuthLevel(resourceClients, client);
            var invitees = ServiceProvider.Current.Scheduler.Reservation.GetInvitees(rsv.ReservationID);
            var isReserver = rsv.ClientID == client.ClientID;
            var isInvited = invitees.Any(x => x.InviteeID == client.ClientID);

            var result = new ReservationClient
            {
                ClientID = client.ClientID,
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.ResourceID,
                IsReserver = isReserver,
                IsInvited = isInvited,
                InLab = context.ClientInLab(rsv.LabID),
                UserAuth = userAuth
            };

            return result;
        }

        public static ViewType GetCurrentViewType(this HttpContextBase context)
        {
            if (context.Session["CurrentViewType"] == null)
                context.Session["CurrentViewType"] = ViewType.WeekView;
            return (ViewType)context.Session["CurrentViewType"];
        }

        public static void SetCurrentViewType(this HttpContextBase context, ViewType value)
        {
            context.Session["CurrentViewType"] = value;
        }

        public static IEnumerable<ResourceClientItem> GetResourceClients(this HttpContextBase context, int resourceId)
        {
            string key = "ResourceClients#" + resourceId;

            var result = (IEnumerable<ResourceClientItem>)context.Items[key];

            if (result == null || result.Count() == 0)
            {
                result = DA.Current.Query<ResourceClientInfo>().Where(x => x.ResourceID == resourceId).CreateModels<ResourceClientItem>();
                context.Items[key] = result;
            }

            return result;
        }

        public static ResourceTreeItemCollection ResourceTree(this HttpContextBase context)
        {
            // always for the current user
            return context.GetClientSetting().GetResourceTree();
        }

        /// <summary>
        /// The current resource treeview for this request.
        /// </summary>
        public static SchedulerResourceTreeView CurrentResourceTreeView(this HttpContextBase context, IProvider provider)
        {
            SchedulerResourceTreeView result;

            if (context.Items["SchedulerTreeView"] == null)
            {
                result = new SchedulerResourceTreeView(provider, context.ResourceTree());
                context.Items["SchedulerTreeView"] = result;
            }
            else
            {
                result = (SchedulerResourceTreeView)context.Items["SchedulerTreeView"];
            }

            return result;
        }

        public static IEnumerable<ResourceTableItem> GetResourceTableItemList(this HttpContextBase context, int buildingId)
        {
            List<ResourceTableItem> result = new List<ResourceTableItem>();
            var bldg = context.ResourceTree().GetBuilding(buildingId);

            if (bldg != null)
            {
                foreach (var lab in context.ResourceTree().Labs().Where(x => x.LabIsActive && x.BuildingID == bldg.BuildingID).OrderBy(x => x.LabDisplayName))
                {
                    foreach (var pt in context.ResourceTree().ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == lab.LabID).OrderBy(x => x.ProcessTechName))
                    {
                        foreach (var res in context.ResourceTree().Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == pt.ProcessTechID).OrderBy(x => x.ResourceName))
                            result.Add(new ResourceTableItem(context, bldg, lab, pt, res));
                    }
                }
            }

            return result;
        }

        public static BuildingItem GetCurrentBuilding(this HttpContextBase context)
        {
            var pathInfo = context.Request.SelectedPath();

            if (pathInfo.BuildingID > 0)
                return context.ResourceTree().GetBuilding(pathInfo.BuildingID);
            else
                return null;
        }

        public static LabItem GetCurrentLab(this HttpContextBase context)
        {
            var pathInfo = context.Request.SelectedPath();

            if (pathInfo.LabID > 0)
                return context.ResourceTree().GetLab(pathInfo.LabID);
            else
                return null;
        }

        public static ProcessTechItem GetCurrentProcessTech(this HttpContextBase context)
        {
            var pathInfo = context.Request.SelectedPath();

            if (pathInfo.ProcessTechID > 0)
                return context.ResourceTree().GetProcessTech(pathInfo.ProcessTechID);
            else
                return null;
        }

        public static IResource GetCurrentResource(this HttpContextBase context)
        {
            var pathInfo = context.Request.SelectedPath();

            if (pathInfo.ResourceID > 0)
                return context.ResourceTree().GetResource(pathInfo.ResourceID);
            else
                return null;
        }

        public static ResourceTreeItem GetCurrentResourceTreeItem(this HttpContextBase context)
        {
            var pathInfo = context.Request.SelectedPath();

            if (pathInfo.ResourceID > 0)
                return context.ResourceTree().Find(pathInfo.ResourceID);
            else
                return null;
        }

        public static ResourceClientItem GetCurrentResourceClient(this HttpContextBase context, int resourceId)
        {
            // will return Everyone user if available because of the OrderBy
            // will return null if both CurrentUser and Everyone is not AuthorizedUser
            int clientId = context.CurrentUser().ClientID;
            return CacheManager.Current
                .ResourceClients(resourceId).OrderBy(x => x.ClientID)
                .FirstOrDefault(x => x.ClientID == clientId || x.ClientID == -1);
        }

        public static ClientAuthLevel GetCurrentAuthLevel(this HttpContextBase context, int resourceId)
        {
            return CacheManager.Current.GetAuthLevel(resourceId, context.CurrentUser().ClientID);
        }

        public static void SetWeekStartDate(this HttpContextBase context, DateTime value)
        {
            context.Session["WeekStartDate"] = value;
        }

        public static DateTime GetWeekStartDate(this HttpContextBase context)
        {
            if (context.Session["WeekStartDate"] == null)
                context.Session["WeekStartDate"] = DateTime.Now.Date;

            var result = Convert.ToDateTime(context.Session["WeekStartDate"]);

            if (result < Reservation.MinReservationBeginDate)
            {
                if (!DateTime.TryParse(context.Request.QueryString["Date"], out result))
                    result = DateTime.Now.Date;

                context.Session["WeekStartDate"] = result;
            }

            return result;
        }

        public static IEnumerable<IReservationProcessInfo> ReservationProcessInfos(this HttpContextBase context)
        {
            if (context.Session["ReservationProcessInfos"] == null)
                context.Session["ReservationProcessInfos"] = new List<IReservationProcessInfo>();

            return (List<IReservationProcessInfo>)context.Session["ReservationProcessInfos"];
        }

        public static void ReservationProcessInfos(this HttpContextBase context, IEnumerable<IReservationProcessInfo> value)
        {
            context.Session["ReservationProcessInfos"] = value.ToList();
        }
    }
}
