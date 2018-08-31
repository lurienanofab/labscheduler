namespace LNF.Web.Scheduler.TreeView
{
    public interface INode
    {
        INode Parent { get; }
        TreeViewItemCollection Children { get; }
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
