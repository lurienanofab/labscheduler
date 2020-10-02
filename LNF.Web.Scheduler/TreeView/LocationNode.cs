using LNF.Scheduler;
using System;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public static class LocationNodeUtility
    {
        public static string GetLocationNodeUrl(int labId, int labLocationId, DateTime selectedDate)
        {
            var locationPath = LocationPathInfo.Create(labId, labLocationId);
            return $"~/LabLocation.aspx?LocationPath={locationPath.UrlEncode()}&Date={selectedDate:yyyy-MM-dd}";
        }
    }

    public class LocationNode : TreeViewNode<ILabLocation>
    {
        public LocationNode(SchedulerResourceTreeView view, ILabLocation item, INode parent) : base(view, item, parent)
        {
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            return LocationNodeUtility.GetLocationNodeUrl(Parent.ID, ID, context.Request.SelectedDate());
        }

        public override bool IsExpanded(string path)
        {
            return false;
        }

        protected override void Load()
        {
            ID = Item.LabLocationID;
            Name = Item.LocationName;
            Description = string.Empty;
        }
    }
}
