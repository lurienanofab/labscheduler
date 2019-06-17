using LNF.Scheduler;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class SchedulerResourceTreeView
    {
        public SchedulerResourceTreeView(IProvider provider, ResourceTreeItemCollection resourceTree)
        {
            var buildings = resourceTree.Buildings().Where(x => x.BuildingIsActive).OrderBy(x => x.BuildingName).ToArray();
            Buildings = new TreeViewItemCollection(buildings.Select(x => new BuildingNode(provider, resourceTree, x)));
        }

        public TreeViewItemCollection Buildings { get; }
    }
}
