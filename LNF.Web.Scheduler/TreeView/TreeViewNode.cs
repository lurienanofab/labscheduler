namespace LNF.Web.Scheduler.TreeView
{
    public abstract class TreeViewNode<T> : INode
    {
        public INode Parent { get; }
        public TreeViewItemCollection Children { get; protected set; }
        public abstract NodeType Type { get; }
        public int ID { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        protected abstract void Load(T item);

        private string _Value;

        public virtual string Value
        {
            get
            {
                if (string.IsNullOrEmpty(_Value))
                {
                    if (Parent != null)
                        _Value = string.Format("{0}{1}{2}", Parent.Value, PathInfo.PathDelimiter, ID);
                    else
                        _Value = ID.ToString();
                }
                return _Value;
            }
        }

        public virtual string CssClass { get { return "node-text-content"; } }

        public virtual string ToolTip { get { return TreeViewUtility.Coalesce(Description, Name); } }

        public PathInfo GetPath()
        {
            return PathInfo.Parse(Value);
        }

        protected TreeViewNode(T item, INode parent)
        {
            Parent = parent;
            Load(item);
        }
    }
}
