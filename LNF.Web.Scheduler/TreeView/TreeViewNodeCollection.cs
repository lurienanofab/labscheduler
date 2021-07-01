using LNF.CommonTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class TreeViewNodeCollection : IEnumerable<INode>
    {
        private List<INode> _items = new List<INode>();

        public int Count => _items.Count;

        public TreeViewNodeCollection(IEnumerable<INode> items)
        {
            Load(items);
        }

        public void Load(IEnumerable<INode> items)
        {
            INode prevItem = null;
            var count = items.Count();
            foreach (var item in items.OrderBy(x => x.Name))
            {
                if (prevItem == null || item.Name != prevItem.Name)
                {
                    prevItem = item;
                    Add(item);
                }
                else
                {
                    var currentUser = item.Helper.CurrentUser();
                    int clientId = currentUser.ClientID;
                    string username = currentUser.UserName;
                    string url = item.Helper.Context.Request.Url.ToString();
                    string logText = item.Helper.GetLogText();

                    string body = string.Empty;
                    body += $"duplicate detected: name = {item.Name}, value = {item.Value}";
                    body += $"{Environment.NewLine}username: {username}";
                    body += $"{Environment.NewLine}url: {url}";
                    body += $"{Environment.NewLine}type: {GetType().FullName}";
                    body += $"{Environment.NewLine}this: {item.Name} [{item.ID}]";
                    body += $"{Environment.NewLine}prev: {prevItem.Name} [{prevItem.ID}]";
                    body += $"{Environment.NewLine}count: {count}";
                    body += $"{Environment.NewLine}--------------------------------------------------";
                    body += $"{Environment.NewLine}{logText}";
                    SendEmail.Send(clientId, "LNF.Web.Scheduler.TreeViewNodeCollection.Load", $"Duplicate treeview node detected [{DateTime.Now:yyyy-MM-dd HH:mm:ss}]", body, SendEmail.SystemEmail, new[] { "lnf-debug@umich.edu" }, isHtml: false);
                }
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
