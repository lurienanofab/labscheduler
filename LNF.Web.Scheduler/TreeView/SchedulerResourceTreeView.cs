using LNF.Scheduler;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class SchedulerTreeView
    {
        public TreeViewNodeCollection Root { get; set; }
    }

    public class SchedulerResourceTreeView
    {        
        public TreeItemCollection ResourceTree { get; }

        public SchedulerResourceTreeView(TreeItemCollection resourceTree)
        {
            ResourceTree = resourceTree;
        }

        public TreeViewNodeCollection Root { get; set; }
    }
}
