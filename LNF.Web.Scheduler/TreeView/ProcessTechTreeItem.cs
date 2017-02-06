using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System;
using System.Linq;

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
            Description = item.Description;
            var resources = CacheManager.Current.Resources(x => x.ResourceIsActive && x.ProcessTechID == item.ProcessTechID).ToList();
            Children = new TreeItemCollection(resources.Select(x => new ResourceTreeItem(x, this)));
        }
    }
}
