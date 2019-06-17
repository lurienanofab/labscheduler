using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class ResourceTreeManager
    {
        private IList<ResourceTree> _items;

        public ResourceTreeManager(int clientId)
        {
            // create a new ResourceTreeManager based on the currently logged in user
            _items = DA.Current.Query<ResourceTree>().Where(x => x.ClientID == clientId).ToList();
        }

        public static ProcessTechItem CreateProcessTechModel(ResourceTree item)
        {
            return new ProcessTechItem()
            {
                ProcessTechID = item.ProcessTechID,
                ProcessTechName = item.ProcessTechName,
                ProcessTechDescription = item.ProcessTechDescription,
                ProcessTechIsActive = item.ProcessTechIsActive,
                ProcessTechGroupID = item.ProcessTechGroupID,
                ProcessTechGroupName = item.ProcessTechGroupName,
                LabID = item.LabID,
                LabName = item.LabName,
                LabDisplayName = item.LabDisplayName,
                LabIsActive = item.LabIsActive,
                BuildingID = item.BuildingID,
                BuildingName = item.BuildingName,
                BuildingIsActive = item.BuildingIsActive
            };
        }

        public static LabItem CreateLabModel(ResourceTree item)
        {
            return new LabItem()
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

        public static BuildingItem CreateBuildingModel(ResourceTree item)
        {
            return new BuildingItem()
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

        public IEnumerable<IResource> GetResources()
        {
            var result = _items.AsQueryable().CreateModels<ResourceTreeItem>().OrderBy(x => x.BuildingID).ThenBy(x => x.LabID).ThenBy(x => x.ProcessTechID).ThenBy(x => x.ResourceID).ToList();
            return result;
        }

        public IEnumerable<ProcessTechItem> GetProcessTechs()
        {
            var distinct = _items.Select(CreateProcessTechModel).Distinct();
            var result = distinct.OrderBy(x => x.BuildingID).ThenBy(x => x.LabID).ThenBy(x => x.ProcessTechID).ToList();
            return result;
        }

        public IEnumerable<LabItem> GetLabs()
        {
            var distinct = _items.Select(CreateLabModel).Distinct();
            var result = distinct.OrderBy(x => x.BuildingID).ThenBy(x => x.LabID).ToList();
            return result;
        }

        public IEnumerable<BuildingItem> GetBuildings()
        {
            var distinct = _items.Select(CreateBuildingModel).Distinct();
            var result = distinct.OrderBy(x => x.BuildingID).ToList();
            return result;
        }

        public IResource GetResource(int resourceId)
        {
            var item = _items.FirstOrDefault(x => x.ResourceID == resourceId);

            if (item == null) return null;

            var result = item.CreateModel<ResourceTreeItem>();

            return result;
        }

        public ProcessTechItem GetProcessTech(int procTechId)
        {
            var item = _items.FirstOrDefault(x => x.ProcessTechID == procTechId);

            if (item == null) return null;

            var result = CreateProcessTechModel(item);

            return result;
        }

        public LabItem GetLab(int labId)
        {
            var item = _items.FirstOrDefault(x => x.LabID == labId);

            if (item == null) return null;

            var result = CreateLabModel(item);

            return result;
        }

        public BuildingItem GetBuilding(int buildingId)
        {
            var item = _items.FirstOrDefault(x => x.BuildingID == buildingId);

            if (item == null) return null;

            var result = CreateBuildingModel(item);

            return result;
        }
    }
}
