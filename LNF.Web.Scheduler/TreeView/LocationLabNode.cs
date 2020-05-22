using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            PathInfo pathInfo = PathInfo.Create(Item.BuildingID, Item.LabID, 0, 0);
            return $"~/Lab.aspx?View=locations&Path={context.Server.UrlEncode(Value)}&Date={context.Request.SelectedDate():yyyy-MM-dd}";
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
