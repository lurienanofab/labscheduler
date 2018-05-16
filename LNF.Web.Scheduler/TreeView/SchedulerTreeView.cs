using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System.Linq;
using System.Web;
using System.Collections.Generic;
using repo = LNF.Repository.Scheduler;
using LNF.Repository;
using LNF.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class SchedulerTreeView
    {
        public SchedulerTreeView()
        {        
            var buildings = CacheManager.Current.ResourceTree().Buildings().Where(x => x.BuildingIsActive).OrderBy(x => x.BuildingName).ToArray();
            Buildings = new TreeItemCollection(buildings.Select(x => new BuildingTreeItem(x)));
        }

        public TreeItemCollection Buildings { get; }

        /// <summary>
        /// The current resource treeview for this request.
        /// </summary>
        public static SchedulerTreeView Current
        {
            get
            {
                SchedulerTreeView result;

                if (HttpContext.Current.Items["SchedulerTreeView"] == null)
                {
                    result = new SchedulerTreeView();
                    HttpContext.Current.Items["SchedulerTreeView"] = result;
                }
                else
                {
                    result = (SchedulerTreeView)HttpContext.Current.Items["SchedulerTreeView"];
                }

                return result;
            }
        }
    }
}
