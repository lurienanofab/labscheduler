using LNF.Scheduler;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class BuildingNode : TreeViewNode<IBuilding>
    {
        public BuildingNode(SchedulerResourceTreeView view, IBuilding item) : base(view, item)
        {
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~/Building.aspx?Path={0}&Date={1:yyyy-MM-dd}", context.Server.UrlEncode(Value), context.Request.SelectedDate()));
        }

        public override string GetImageUrl(HttpContextBase context)
        {
            return string.Format("/scheduler/image/building_icon/{0}", ID);
        }

        public override bool IsExpanded(string path) => PathInfo.Parse(path).BuildingID == ID;

        protected override void Load()
        {
            ID = Item.BuildingID;
            Name = Item.BuildingName;
            Description = Item.BuildingDescription;
            var labs = View.ResourceTree.Labs().Where(x => x.LabIsActive && x.BuildingID == Item.BuildingID).ToArray();
            Children = new TreeViewNodeCollection(labs.Select(x => new LabNode(View, x, this)).ToArray());
        }
    }
}
