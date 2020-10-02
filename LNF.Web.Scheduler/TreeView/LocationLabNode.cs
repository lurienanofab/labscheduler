using LNF.Scheduler;
using System;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class LocationLabNode : TreeViewNode<ILab>
    {
        private LocationTreeItemCollection _locationTree;

        public LocationLabNode(SchedulerResourceTreeView view, LocationTreeItemCollection locationTree, ILab item) : base(view, item)
        {
            _locationTree = locationTree ?? throw new ArgumentNullException("locationTree");
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            PathInfo path = PathInfo.Create(Item.BuildingID, ID, 0, 0);
            LocationPathInfo locationPath = LocationPathInfo.Create(Item.LabID, 0);
            return $"~/Lab.aspx?View=locations&LocationPath={locationPath.UrlEncode()}&Path={path.UrlEncode()}&Date={context.Request.SelectedDate():yyyy-MM-dd}";
        }

        public override bool IsExpanded(string path)
        {
            return false;
        }

        protected override void Load()
        {
            ID = Item.LabID;
            Name = Item.LabDisplayName;
            Description = Item.LabDescription;
            var locations = _locationTree.GetLabLocations(ID).ToArray();
            var items = locations.Select(x => new LocationNode(View, x, this)).ToArray();
            Children = new TreeViewNodeCollection(items);
        }
    }
}
