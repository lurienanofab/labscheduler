using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class LocationNode : TreeViewNode<ILabLocation>
    {
        public LocationNode(SchedulerResourceTreeView view, ILabLocation item, INode parent) : base(view, item, parent)
        {
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            var locationPath = LocationPathInfo.Create(Parent.ID, ID);
            return $"~/LabLocation.aspx?LocationPath={locationPath.UrlEncode()}";
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
