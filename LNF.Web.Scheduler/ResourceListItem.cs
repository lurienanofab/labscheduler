using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class ResourceListItem
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

        private ResourceListItem() { }

        public static ResourceListItem Create(BuildingModel bld, LabModel lab, ProcessTechModel pt, ResourceModel res)
        {
            ResourceListItem result = new ResourceListItem();
            result.BuildingID = bld.BuildingID;
            result.LabID = lab.LabID;
            result.ProcessTechID = pt.ProcessTechID;
            result.ResourceID = res.ResourceID;
            result.BuildingName = bld.BuildingName;
            result.LabName = lab.LabDisplayName;
            result.ProcessTechName = pt.ProcessTechName;
            result.ResourceName = res.ResourceName;
            result.LabUrl = VirtualPathUtility.ToAbsolute(string.Format("~/Lab.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(lab), HttpContext.Current.Request.SelectedDate()));
            result.ProcessTechUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(pt), HttpContext.Current.Request.SelectedDate()));
            result.ResourceUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), HttpContext.Current.Request.SelectedDate()));
            return result;
        }

        public static List<ResourceListItem> List(int buildingId)
        {
            List<ResourceListItem> result = new List<ResourceListItem>();
            var bldg = CacheManager.Current.GetBuilding(buildingId);

            if (bldg != null)
            {
                foreach (var lab in CacheManager.Current.Labs().Where(x => x.LabIsActive && x.BuildingID == bldg.BuildingID).OrderBy(x => x.LabDisplayName))
                {
                    foreach (var pt in CacheManager.Current.ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == lab.LabID).OrderBy(x => x.ProcessTechName))
                    {
                        foreach (var res in CacheManager.Current.Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == pt.ProcessTechID).OrderBy(x => x.ResourceName))
                            result.Add(Create(bldg, lab, pt, res));
                    }
                }
            }

            return result;
        }
    }
}