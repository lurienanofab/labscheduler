using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class LabNode : TreeViewNode<LabItem>
    {
        public LabNode(LabItem item, INode parent) : base(item, parent) { }

        public override NodeType Type { get { return NodeType.Lab; } }

        protected override void Load(LabItem item)
        {
            ID = item.LabID;
            Name = item.LabDisplayName;
            Description = item.LabDescription;
            var procTechs = ResourceTree.ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == item.LabID).ToList();
            Children = new TreeViewItemCollection(procTechs.Select(x => new ProcessTechNode(x, this)));
        }
    }
}
