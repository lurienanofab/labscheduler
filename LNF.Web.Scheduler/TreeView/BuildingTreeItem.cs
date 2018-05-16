using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class BuildingTreeItem : TreeItem<BuildingModel>
    {
        public BuildingTreeItem(BuildingModel item) : base(item, null) { }

        public override TreeItemType Type { get { return TreeItemType.Building; } }

        protected override void Load(BuildingModel item)
        {
            ID = item.BuildingID;
            Name = item.BuildingName;
            Description = item.BuildingDescription;
            var labs = CacheManager.Current.ResourceTree().Labs().Where(x => x.LabIsActive && x.BuildingID == item.BuildingID).ToList();
            Children = new TreeItemCollection(labs.Select(x => new LabTreeItem(x, this)));
        }
    }
}
