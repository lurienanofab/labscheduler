using LNF.Scheduler;
using System;

namespace LNF.Web.Scheduler.TreeView
{
    public abstract class TreeViewNode<T> : INode
    {
        public IProvider Provider { get; }
        public INode Parent { get; }
        public TreeViewItemCollection Children { get; protected set; }
        public ResourceTreeItemCollection ResourceTree { get; }
        public abstract NodeType Type { get; }
        public int ID { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        protected abstract void Load(T item);

        protected TreeViewNode(IProvider provider, ResourceTreeItemCollection resourceTree, T item)
        {
            Provider = provider;
            ResourceTree = resourceTree;
            Parent = null;
            Load(item);
        }

        protected TreeViewNode(T item, INode parent)
        {
            Provider = parent.Provider;
            ResourceTree = parent.ResourceTree;
            Parent = parent ?? throw new ArgumentNullException("parent");
            Load(item);
        }

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
    }
}
