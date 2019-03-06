﻿using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class ResourceTableItem
    {
        public int BuildingID { get; set; }
        public int LabID { get; set; }
        public int ProcessTechID { get; set; }
        public int ResourceID { get; set; }
        public string BuildingName { get; set; }
        public string LabName { get; set; }
        public string ProcessTechName { get; set; }
        public string ResourceName { get; set; }
        public string LabUrl { get; set; }
        public string ProcessTechUrl { get; set; }
        public string ResourceUrl { get; set; }

        private ResourceTableItem() { }

        public static ResourceTableItem Create(BuildingItem bld, LabItem lab, ProcessTechItem pt, IResource res)
        {
            ResourceTableItem result = new ResourceTableItem
            {
                BuildingID = bld.BuildingID,
                LabID = lab.LabID,
                ProcessTechID = pt.ProcessTechID,
                ResourceID = res.ResourceID,
                BuildingName = bld.BuildingName,
                LabName = lab.LabDisplayName,
                ProcessTechName = pt.ProcessTechName,
                ResourceName = res.ResourceName,
                LabUrl = VirtualPathUtility.ToAbsolute(string.Format("~/Lab.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(lab), HttpContext.Current.Request.SelectedDate())),
                ProcessTechUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(pt), HttpContext.Current.Request.SelectedDate())),
                ResourceUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), HttpContext.Current.Request.SelectedDate()))
            };

            return result;
        }

        public static List<ResourceTableItem> List(int buildingId)
        {
            List<ResourceTableItem> result = new List<ResourceTableItem>();
            var bldg = CacheManager.Current.ResourceTree().GetBuilding(buildingId);

            if (bldg != null)
            {
                foreach (var lab in CacheManager.Current.ResourceTree().Labs().Where(x => x.LabIsActive && x.BuildingID == bldg.BuildingID).OrderBy(x => x.LabDisplayName))
                {
                    foreach (var pt in CacheManager.Current.ResourceTree().ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == lab.LabID).OrderBy(x => x.ProcessTechName))
                    {
                        foreach (var res in CacheManager.Current.ResourceTree().Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == pt.ProcessTechID).OrderBy(x => x.ResourceName))
                            result.Add(Create(bldg, lab, pt, res));
                    }
                }
            }

            return result;
        }
    }
}