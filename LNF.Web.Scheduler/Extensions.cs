using LNF.Cache;
using LNF.Data;
using LNF.PhysicalAccess;
using LNF.Scheduler;
using LNF.Web.Scheduler.Models;
using LNF.Web.Scheduler.TreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class SchedulerContextHelper : ContextHelper
    {
        public SchedulerContextHelper(HttpContextBase context, IProvider provider) : base(context, provider) { }

        public IEnumerable<Badge> CurrentlyInLab()
        {
            if (!Context.Items.Contains("CurrentlyInLab"))
                Context.Items["CurrentlyInLab"] = GetPhysicalAccessUtility().CurrentlyInLab;

            var result = (IEnumerable<Badge>)Context.Items["CurrentlyInLab"];

            return result;
        }

        public bool IsInLab() => GetPhysicalAccessUtility().IsInLab(CurrentUser().ClientID);

        public PhysicalAccessUtility GetPhysicalAccessUtility()
        {
            PhysicalAccessUtility util;

            if (!Context.Items.Contains("PhysicalAccessUtility"))
            {
                var inlab = Provider.PhysicalAccess.GetCurrentlyInArea("all");
                util = new PhysicalAccessUtility(inlab, Context.Request.UserHostAddress);
                Context.Items["PhysicalAccessUtility"] = util;
            }
            else
            {
                util = (PhysicalAccessUtility)Context.Items["PhysicalAccessUtility"];
            }

            return util;
        }

        public bool ClientInLab()
        {
            if (!Context.Items.Contains("ClientInLab"))
                Context.Items["ClientInLab"] = GetPhysicalAccessUtility().ClientInLab(CurrentUser().ClientID);

            var result = (bool)Context.Items["ClientInLab"];

            return result;
        }

        public bool ClientInLab(int labId)
        {
            var clientId = CurrentUser().ClientID;
            return GetPhysicalAccessUtility().ClientInLab(clientId, labId);
        }

        public ILab ClientLab()
        {
            if (!Context.Items.Contains("ClientLab"))
            {
                var inlab = GetPhysicalAccessUtility().CurrentlyInLab;
                var badge = inlab.FirstOrDefault(x => x.ClientID == CurrentUser().ClientID);
                ILab lab = null;
                if (badge != null)
                    lab = CacheManager.Current.GetLab(badge.CurrentAreaName);
                Context.Items["ClientLab"] = lab;
            }

            var result = (ILab)Context.Items["ClientLab"];

            return result;
        }

        public IClientSetting GetClientSetting()
        {
            IClientSetting result;

            if (Context.Session["ClientSetting"] == null)
            {
                result = Provider.Scheduler.ClientSetting.GetClientSettingOrDefault(CurrentUser().ClientID);
                Context.Session["ClientSetting"] = result;
            }
            else
            {
                result = (IClientSetting)Context.Session["ClientSetting"];
            }

            return result;
        }

        public IReservation GetReservation()
        {
            if (int.TryParse(Context.Request.QueryString["ReservationID"], out int reservationId))
            {
                var result = Provider.Scheduler.Reservation.GetReservation(reservationId);

                if (result == null)
                    throw new InvalidOperationException($"Cannot find a Reservation with ReservationID = {reservationId}");

                return result;
            }
            else
                throw new InvalidOperationException("Missing query string parameter: ReservationID");
        }

        public IReservationWithInvitees GetReservationWithInvitees()
        {
            if (int.TryParse(Context.Request.QueryString["ReservationID"], out int reservationId))
            {
                var result = Provider.Scheduler.Reservation.GetReservationWithInvitees(reservationId);

                if (result == null)
                    throw new InvalidOperationException($"Cannot find a Reservation with ReservationID = {reservationId}");

                return result;
            }
            else
                throw new InvalidOperationException("Missing query string parameter: ReservationID");
        }

        public ReservationClient GetReservationClientItem(IReservationWithInvitees rsv) => GetReservationClientItem(rsv, CurrentUser());

        public ReservationClient GetReservationClientItem(IReservationWithInvitees rsv, IClient client)
        {
            var resourceClients = Provider.Scheduler.Reservation.GetResourceClients(rsv.ResourceID);
            var userAuth = Reservations.GetAuthLevel(resourceClients, client);

            var result = new ReservationClient
            {
                ClientID = client.ClientID,
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.ResourceID,
                IsReserver = rsv.ClientID == client.ClientID,
                IsInvited = rsv.Invitees.Any(x => x.InviteeID == client.ClientID),
                InLab = ClientInLab(rsv.LabID),
                UserAuth = userAuth
            };

            return result;
        }

        public ReservationClient GetReservationClientItem(IReservation rsv) => GetReservationClientItem(rsv, CurrentUser());

        public ReservationClient GetReservationClientItem(IReservation rsv, IClient client)
        {
            var resourceClients = Provider.Scheduler.Reservation.GetResourceClients(rsv.ResourceID);
            var userAuth = Reservations.GetAuthLevel(resourceClients, client);
            var invitees = Provider.Scheduler.Reservation.GetInvitees(rsv.ReservationID);
            var isReserver = rsv.ClientID == client.ClientID;
            var isInvited = invitees.Any(x => x.InviteeID == client.ClientID);

            var physicalAccessUtil = GetPhysicalAccessUtility();
            var inLab = physicalAccessUtil.ClientInLab(client.ClientID, rsv.LabID);

            var result = new ReservationClient
            {
                ClientID = client.ClientID,
                ReservationID = rsv.ReservationID,
                ResourceID = rsv.ResourceID,
                IsReserver = isReserver,
                IsInvited = isInvited,
                InLab = inLab,
                UserAuth = userAuth
            };

            return result;
        }

        public IEnumerable<IResourceClient> GetResourceClients(int resourceId)
        {
            string key = "ResourceClients#" + resourceId;

            var result = (IEnumerable<IResourceClient>)Context.Items[key];

            if (result == null || result.Count() == 0)
            {
                result = Provider.Scheduler.Resource.GetResourceClients(resourceId);
                Context.Items[key] = result;
            }

            return result;
        }

        public IEnumerable<IResourceTree> ResourceTree()
        {
            IEnumerable<IResourceTree> resources;

            if (Context.Items["CurrentResourceTree"] == null)
            {
                resources = Provider.Scheduler.Resource.GetResourceTree(CurrentUser().ClientID);
                Context.Items["CurrentResourceTree"] = resources;
            }
            else
            {
                resources = (IEnumerable<IResourceTree>)Context.Items["CurrentResourceTree"];
            }

            return resources;
        }

        public ResourceTreeItemCollection GetResourceTreeItemCollection()
        {
            ResourceTreeItemCollection tree;

            if (Context.Items["CurrentResourceTreeItemCollection"] == null)
            {
                // always for the current user
                var resources = ResourceTree();
                tree = new ResourceTreeItemCollection(resources);
                Context.Items["CurrentResourceTreeItemCollection"] = tree;
            }
            else
            {
                tree = (ResourceTreeItemCollection)Context.Items["CurrentResourceTreeItemCollection"];
            }

            return tree;
        }

        public IEnumerable<ILabLocation> LabLocations()
        {
            IEnumerable<ILabLocation> labLocations;

            if (Context.Session["LabLocations"] == null)
            {
                labLocations = Provider.Scheduler.LabLocation.GetLabLocations();
                Context.Session["LabLocations"] = labLocations;
            }
            else
            {
                labLocations = (IEnumerable<ILabLocation>)Context.Session["LabLocations"];
            }

            return labLocations;
        }

        public IEnumerable<IResourceLabLocation> ResourceLabLocations()
        {
            IEnumerable<IResourceLabLocation> resourceLabLocations;

            if (Context.Session["ResourceLabLocations"] == null)
            {
                resourceLabLocations = Provider.Scheduler.LabLocation.GetResourceLabLocations();
                Context.Session["ResourceLabLocations"] = resourceLabLocations;
            }
            else
            {
                resourceLabLocations = (IEnumerable<IResourceLabLocation>)Context.Session["ResourceLabLocations"];
            }

            return resourceLabLocations;
        }

        public LocationTreeItemCollection GetLocationTreeItemCollection()
        {
            LocationTreeItemCollection tree;

            if (Context.Items["CurrentLocationTreeItemCollection"] == null)
            {
                // always for the current user
                var resources = ResourceTree();
                var labLocations = LabLocations();
                var resourceLabLocations = ResourceLabLocations();
                tree = new LocationTreeItemCollection(resources, labLocations, resourceLabLocations);
                Context.Items["CurrentLocationTreeItemCollection"] = tree;
            }
            else
            {
                tree = (LocationTreeItemCollection)Context.Items["CurrentLocationTreeItemCollection"];
            }

            return tree;
        }

        /// <summary>
        /// The current resource treeview for this request.
        /// </summary>
        public SchedulerResourceTreeView CurrentResourceTreeView()
        {
            SchedulerResourceTreeView result;

            if (Context.Items["SchedulerTreeView"] == null)
            {
                result = CreateResourceTreeView();
                Context.Items["SchedulerTreeView"] = result;
            }
            else
            {
                result = (SchedulerResourceTreeView)Context.Items["SchedulerTreeView"];
            }

            return result;
        }

        public SchedulerResourceTreeView CreateResourceTreeView()
        {
            var result = new SchedulerResourceTreeView(GetResourceTreeItemCollection());
            var buildings = result.ResourceTree.Buildings().Where(x => x.BuildingIsActive).OrderBy(x => x.BuildingName).ToArray();
            result.Root = new TreeViewNodeCollection(buildings.Select(x => new BuildingNode(result, x)).ToArray());
            return result;
        }

        /// <summary>
        /// The current location treeview for this request.
        /// </summary>
        public SchedulerResourceTreeView CurrentLocationTreeView()
        {
            SchedulerResourceTreeView result;

            if (Context.Items["LocationTreeView"] == null)
            {
                result = CreateLocationTreeView();
                Context.Items["LocationTreeView"] = result;
            }
            else
            {
                result = (SchedulerResourceTreeView)Context.Items["LocationTreeView"];
            }

            return result;
        }

        public SchedulerResourceTreeView CreateLocationTreeView()
        {
            var locationTree = GetLocationTreeItemCollection();
            var include = locationTree.GetLabLocations().Select(x => x.LabID).Distinct().ToArray();
            var result = new SchedulerResourceTreeView(locationTree);
            var labs = result.ResourceTree.Labs().Where(x => x.BuildingIsActive && x.LabIsActive && include.Contains(x.LabID)).OrderBy(x => x.BuildingName).ThenBy(x => x.LabDisplayName).ToArray();
            result.Root = new TreeViewNodeCollection(labs.Select(x => new LocationLabNode(result, locationTree, x)).ToArray());
            return result;
        }

        public IEnumerable<ResourceTableItem> GetResourceTableItemList(int buildingId)
        {
            List<ResourceTableItem> result = new List<ResourceTableItem>();
            var resourceTree = GetResourceTreeItemCollection();
            var bldg = resourceTree.GetBuilding(buildingId);

            if (bldg != null)
            {
                foreach (var lab in resourceTree.Labs().Where(x => x.LabIsActive && x.BuildingID == bldg.BuildingID).OrderBy(x => x.LabDisplayName).ToList())
                {
                    foreach (var pt in resourceTree.ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == lab.LabID).OrderBy(x => x.ProcessTechName).ToList())
                    {
                        foreach (var res in resourceTree.Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == pt.ProcessTechID).OrderBy(x => x.ResourceName).ToList())
                            result.Add(new ResourceTableItem(Context, bldg, lab, pt, res));
                    }
                }
            }

            return result;
        }

        public IBuilding GetCurrentBuilding()
        {
            var pathInfo = Context.Request.SelectedPath();

            if (pathInfo.BuildingID > 0)
                return GetResourceTreeItemCollection().GetBuilding(pathInfo.BuildingID);
            else
                return null;
        }

        public ILab GetCurrentLab()
        {
            var pathInfo = Context.Request.SelectedPath();

            if (pathInfo.LabID > 0)
                return GetResourceTreeItemCollection().GetLab(pathInfo.LabID);
            else
                return null;
        }

        public IProcessTech GetCurrentProcessTech()
        {
            var pathInfo = Context.Request.SelectedPath();

            if (pathInfo.ProcessTechID > 0)
                return GetResourceTreeItemCollection().GetProcessTech(pathInfo.ProcessTechID);
            else
                return null;
        }

        public IResource GetCurrentResource()
        {
            var pathInfo = Context.Request.SelectedPath();
            return GetResource(pathInfo);
        }

        public IResource GetResource(PathInfo path)
        {
            return GetResource(path.ResourceID);
        }

        public IResource GetResource(int resourceId)
        {
            if (resourceId > 0)
                return GetResourceTreeItemCollection().GetResource(resourceId);
            else
                return null;
        }

        public IResourceTree GetCurrentResourceTreeItem()
        {
            var pathInfo = Context.Request.SelectedPath();

            if (pathInfo.ResourceID > 0)
                return GetResourceTreeItemCollection().GetResourceTree(pathInfo.ResourceID);
            else
                return null;
        }

        public IResourceClient GetCurrentResourceClient(int resourceId)
        {
            // will return Everyone user if available because of the OrderBy
            // will return null if both CurrentUser and Everyone is not AuthorizedUser
            int clientId = CurrentUser().ClientID;
            return CacheManager.Current
                .ResourceClients(resourceId).OrderBy(x => x.ClientID)
                .FirstOrDefault(x => x.ClientID == clientId || x.ClientID == -1);
        }

        public ClientAuthLevel GetCurrentAuthLevel(int resourceId)
        {
            return CacheManager.Current.GetAuthLevel(resourceId, CurrentUser().ClientID);
        }
    }

    public static class Extensions
    {
        public static SchedulerContextHelper ContextHelper(this HttpContextBase context, IProvider provider)
        {
            return new SchedulerContextHelper(context, provider);
        }

        public static DateTime SelectedDate(this HttpRequestBase request)
        {
            if (!string.IsNullOrEmpty(request.QueryString["Date"]))
            {
                if (DateTime.TryParse(request.QueryString["Date"], out DateTime result))
                    return result;
            }

            return DateTime.Now.Date;
        }

        public static PathInfo SelectedPath(this HttpRequestBase request) => PathInfo.Parse(request.QueryString["Path"]);

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

        public static void SetWeekStartDate(this HttpContextBase context, DateTime value)
        {
            context.Session["WeekStartDate"] = value;
        }
    }
}
