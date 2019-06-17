using LNF.Models.Scheduler;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class ProcessTechNode : TreeViewNode<ProcessTechItem>
    {
        public int LabID { get; }

        public ProcessTechNode(ProcessTechItem item, INode parent) : base(item, parent) { }

        public override NodeType Type { get { return NodeType.ProcessTech; } }

        protected override void Load(ProcessTechItem item)
        {
            ID = item.ProcessTechID;
            Name = item.ProcessTechName;
            Description = item.ProcessTechDescription;
            var resources = ResourceTree.Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == item.ProcessTechID).ToList();
            Children = new TreeViewItemCollection(resources.Select(x => new ResourceNode(x, this)));
        }
    }
}
