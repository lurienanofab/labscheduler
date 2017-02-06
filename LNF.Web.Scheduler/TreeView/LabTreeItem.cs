using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class LabTreeItem : TreeItem<LabModel>
    {
        public LabTreeItem(LabModel item, ITreeItem parent) : base(item, parent) { }

        public override TreeItemType Type { get { return TreeItemType.Lab; } }

        protected override void Load(LabModel item)
        {
            ID = item.LabID;
            Name = item.LabDisplayName;
            Description = item.Description;
            var procTechs = CacheManager.Current.ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == item.LabID).ToList();
            Children = new TreeItemCollection(procTechs.Select(x => new ProcessTechTreeItem(x, this)));
        }
    }
}
