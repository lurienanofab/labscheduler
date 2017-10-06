using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class ResourceTreeManager
    {
        // Stores the data used to display the resource tree view. The data is retrieved once per request in stored in
        // the request context. The current tree is dependent on the logged in user.

        public static ResourceTreeManager Current
        {
            get
            {
                ResourceTreeManager result = null;

                if (HttpContext.Current.Items["ResourceTreeManager"] == null)
                {
                    result = new ResourceTreeManager();
                    HttpContext.Current.Items["ResourceTree"] = result;
                }
                else
                {
                    result = (ResourceTreeManager)HttpContext.Current.Items["ResourceTreeManager"];
                }

                return result;
            }
        }

        private IList<ResourceTree> _items;

        public ResourceTreeManager()
        {
            // create a new ResourceTreeManager based on the currently logged in user
            var clientId = CacheManager.Current.CurrentUser.ClientID;
            _items = DA.Current.Query<ResourceTree>().Where(x => x.ClientID == clientId).ToList();
        }

        public static ResourceModel CreateResourceModel(ResourceTree item)
        {
            return new ResourceModel()
            {
                ResourceID = item.ResourceID,
                ResourceName = item.ResourceName,
                BuildingID = item.BuildingID,
                BuildingName = item.BuildingName,
                LabID = item.LabID,
                LabName = item.LabName,
                LabDisplayName = item.LabDisplayName,
                ProcessTechID = item.ProcessTechID,
                ProcessTechName = item.ProcessTechName,
                ResourceDescription = item.ResourceDescription,
                Granularity = TimeSpan.FromMinutes(item.Granularity),
                ReservFence = TimeSpan.FromMinutes(item.ReservFence),
                MinReservTime = TimeSpan.FromMinutes(item.MinReservTime),
                MaxReservTime = TimeSpan.FromMinutes(item.MaxReservTime),
                MaxAlloc = TimeSpan.FromMinutes(item.MaxAlloc),
                Offset = TimeSpan.FromHours(item.Offset),
                GracePeriod = TimeSpan.FromMinutes(item.GracePeriod),
                AutoEnd = TimeSpan.FromMinutes(item.AutoEnd),
                MinCancelTime = TimeSpan.FromMinutes(item.MinCancelTime),
                UnloadTime = TimeSpan.FromMinutes(item.UnloadTime),
                AuthDuration = item.AuthDuration,
                AuthState = item.AuthState,
                IsSchedulable = item.IsSchedulable,
                State = item.State,
                StateNotes = item.StateNotes,
                IsReady = item.IsReady,
                ResourceIsActive = item.ResourceIsActive,
                HelpdeskEmail = item.HelpdeskEmail,
                WikiPageUrl = item.WikiPageUrl
            };
        }

        public static ProcessTechModel CreateProcessTechModel(ResourceTree item)
        {
            return new ProcessTechModel()
            {
                ProcessTechID = item.ProcessTechID,
                ProcessTechName = item.ProcessTechName,
                ProcessTechDescription = item.ProcessTechDescription,
                ProcessTechIsActive = item.ProcessTechIsActive,
                GroupID = item.ProcessTechGroupID,
                GroupName = item.ProcessTechGroupName,
                LabID = item.LabID,
                LabName = item.LabName,
                LabDisplayName = item.LabDisplayName,
                LabIsActive = item.LabIsActive,
                BuildingID = item.BuildingID,
                BuildingName = item.BuildingName,
                BuildingIsActive = item.BuildingIsActive
            };
        }

        public static LabModel CreateLabModel(ResourceTree item)
        {
            return new LabModel()
            {
                LabID = item.LabID,
                LabName = item.LabName,
                LabDisplayName = item.LabDisplayName,
                LabDescription = item.LabDescription,
                LabIsActive = item.LabIsActive,
                RoomID = item.RoomID,
                RoomName = item.RoomName,
                BuildingID = item.BuildingID,
                BuildingName = item.BuildingName,
                BuildingIsActive = item.BuildingIsActive
            };
        }

        public static BuildingModel CreateBuildingModel(ResourceTree item)
        {
            return new BuildingModel()
            {
                BuildingID = item.BuildingID,
                BuildingName = item.BuildingName,
                BuildingDescription = item.BuildingDescription,
                BuildingIsActive = item.BuildingIsActive
            };
        }

        public IEnumerable<ResourceTree> GetItems()
        {
            return _items.AsEnumerable();
        }

        public IEnumerable<ResourceModel> GetResources()
        {
            var result = _items.Select(CreateResourceModel).OrderBy(x => x.BuildingID).ThenBy(x => x.LabID).ThenBy(x => x.ProcessTechID).ThenBy(x => x.ResourceID);
            return result;
        }

        public IEnumerable<ProcessTechModel> GetProcessTechs()
        {
            var distinct = _items.Select(CreateProcessTechModel).Distinct();
            var result = distinct.OrderBy(x => x.BuildingID).ThenBy(x => x.LabID).ThenBy(x => x.ProcessTechID).ToList();
            return result;
        }

        public IEnumerable<LabModel> GetLabs()
        {
            var distinct = _items.Select(CreateLabModel).Distinct();
            var result = distinct.OrderBy(x => x.BuildingID).ThenBy(x => x.LabID).ToList();
            return result;
        }

        public IEnumerable<BuildingModel> GetBuildings()
        {
            var distinct = _items.Select(CreateBuildingModel).Distinct();
            var result = distinct.OrderBy(x => x.BuildingID).ToList();
            return result;
        }

        public ResourceModel GetResource(int resourceId)
        {
            var item = _items.FirstOrDefault(x => x.ResourceID == resourceId);

            if (item == null) return null;

            var result = CreateResourceModel(item);

            return result;
        }

        public ProcessTechModel GetProcessTech(int procTechId)
        {
            var item = _items.FirstOrDefault(x => x.ProcessTechID == procTechId);

            if (item == null) return null;

            var result = CreateProcessTechModel(item);

            return result;
        }

        public LabModel GetLab(int labId)
        {
            var item = _items.FirstOrDefault(x => x.LabID == labId);

            if (item == null) return null;

            var result = CreateLabModel(item);

            return result;
        }

        public BuildingModel GetBuilding(int buildingId)
        {
            var item = _items.FirstOrDefault(x => x.BuildingID == buildingId);

            if (item == null) return null;

            var result = CreateBuildingModel(item);

            return result;
        }
    }
}
