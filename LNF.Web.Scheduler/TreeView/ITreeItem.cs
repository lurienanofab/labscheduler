namespace LNF.Web.Scheduler.TreeView
{
    public interface ITreeItem
    {
        ITreeItem Parent { get; }
        TreeItemCollection Children { get; }
        TreeItemType Type { get; }
        int ID { get; }
        string Name { get; }
        string Description { get; }
        string Value { get; }
        PathInfo GetPath();
    }
}
