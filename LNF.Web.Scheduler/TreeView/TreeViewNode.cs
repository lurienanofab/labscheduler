using LNF.Scheduler;
using System;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public abstract class TreeViewNode<T> : INode
    {
        public INode Parent { get; }
        public TreeViewNodeCollection Children { get; protected set; }
        public SchedulerResourceTreeView View { get; }
        public int ID { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }
        public T Item { get; }
        protected abstract void Load();

        protected TreeViewNode(SchedulerResourceTreeView view, T item)
        {
            Item = item;
            View = view;
            Parent = null;
        }

        protected TreeViewNode(SchedulerResourceTreeView view, T item, INode parent)
        {
            Item = item;
            View = view;
            Parent = parent ?? throw new ArgumentNullException("parent");
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

        public abstract string GetUrl(HttpContextBase context);

        public virtual string GetImageUrl(HttpContextBase context) => null;

        public abstract bool IsExpanded(string path);

        public virtual bool HasChildren() => Children != null && Children.Count > 0;
    }
}
