using LNF.Scheduler;

namespace LNF.Web.Scheduler.TreeView
{
    public interface INode
    {
        IProvider Provider { get; }
        INode Parent { get; }
        TreeViewItemCollection Children { get; }
        ResourceTreeItemCollection ResourceTree { get; }
        NodeType Type { get; }
        int ID { get; }
        string Name { get; }
        string Description { get; }
        string Value { get; }
        string CssClass { get; }
        string ToolTip { get; }
        PathInfo GetPath();
    }
}
