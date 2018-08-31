using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LNF.Web.Scheduler.TreeView
{
    public class TreeViewItemCollection : IEnumerable<INode>
    {
        private List<INode> _items = new List<INode>();

        public int Count
        {
            get { return _items.Count; }
        }

        public TreeViewItemCollection(IEnumerable<INode> items)
        {
            _items = new List<INode>();

            INode prevItem = null;

            foreach(var item in items.OrderBy(x=>x.Name))
            {
                if (prevItem == null || item.Name != prevItem.Name)
                {
                    prevItem = item;
                    _items.Add(item);
                }
                else
                    throw new Exception(string.Format("duplicate detected: {0}, value = {1}", item.Name, item.Value));
            }
        }

        public void Add(INode item)
        {
            _items.Add(item);
        }

        public void Remove(INode item)
        {
            _items.Remove(item);
        }

        public INode this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public INode Find(int id)
        {
            return _items.FirstOrDefault(x => x.ID == id);
        }

        public void Set(int id, INode item)
        {
            int index = _items.FindIndex(x => x.ID == id);
            if (index != -1)
                _items[index] = item;
        }

        public IEnumerator<INode> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
