using LNF.Scheduler;
using System.Web;

namespace LNF.Web.Scheduler
{
    public class ResourceTableItem
    {
        private HttpContextBase _context;

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

        public ResourceTableItem(HttpContextBase context, IBuilding bld, ILab lab, IProcessTech pt, IResource res)
        {
            _context = context;
            BuildingID = bld.BuildingID;
            LabID = lab.LabID;
            ProcessTechID = pt.ProcessTechID;
            ResourceID = res.ResourceID;
            BuildingName = bld.BuildingName;
            LabName = lab.LabDisplayName;
            ProcessTechName = pt.ProcessTechName;
            ResourceName = res.ResourceName;
            LabUrl = VirtualPathUtility.ToAbsolute(string.Format("~/Lab.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(lab), _context.Request.SelectedDate()));
            ProcessTechUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(pt), _context.Request.SelectedDate()));
            ResourceUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", PathInfo.Create(res), _context.Request.SelectedDate()));
        }
    }
}