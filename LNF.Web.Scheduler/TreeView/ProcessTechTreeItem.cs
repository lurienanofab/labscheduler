using LNF.Models.Scheduler;
using System.Linq;
using LNF.Scheduler;
using LNF.Cache;

namespace LNF.Web.Scheduler.TreeView
{
    public class ProcessTechTreeItem : TreeItem<ProcessTechModel>
    {
        public int LabID { get; }

        public ProcessTechTreeItem(ProcessTechModel item, ITreeItem parent) : base(item, parent) { }

        public override TreeItemType Type { get { return TreeItemType.ProcessTech; } }

        protected override void Load(ProcessTechModel item)
        {
            ID = item.ProcessTechID;
            Name = item.ProcessTechName;
            Description = item.ProcessTechDescription;
            var resources = CacheManager.Current.ResourceTree().Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == item.ProcessTechID).ToList();
            Children = new TreeItemCollection(resources.Select(x => new ResourceTreeItem(x, this)));
        }
    }
}
