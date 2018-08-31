using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class BuildingNode : TreeViewNode<BuildingItem>
    {
        public BuildingNode(BuildingItem item) : base(item, null) { }

        public override NodeType Type { get { return NodeType.Building; } }

        protected override void Load(BuildingItem item)
        {
            ID = item.BuildingID;
            Name = item.BuildingName;
            Description = item.BuildingDescription;
            var labs = CacheManager.Current.ResourceTree().Labs().Where(x => x.LabIsActive && x.BuildingID == item.BuildingID).ToList();
            Children = new TreeViewItemCollection(labs.Select(x => new LabNode(x, this)));
        }
    }
}
