using LNF.Cache;
using LNF.Scheduler;
using System.Linq;

namespace LNF.Web.Scheduler.TreeView
{
    public class SchedulerTreeView
    {
        public SchedulerTreeView()
        {
            var buildings = CacheManager.Current.Buildings().Where(x => x.BuildingIsActive).OrderBy(x => x.BuildingName).ToList();
            Buildings = new TreeItemCollection(buildings.Select(x => new BuildingTreeItem(x)));
        }

        public TreeItemCollection Buildings { get; }
    }
}
