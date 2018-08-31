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
    public class SchedulerResourceTreeView
    {
        public SchedulerResourceTreeView()
        {        
            var buildings = CacheManager.Current.ResourceTree().Buildings().Where(x => x.BuildingIsActive).OrderBy(x => x.BuildingName).ToArray();
            Buildings = new TreeViewItemCollection(buildings.Select(x => new BuildingNode(x)));
        }

        public TreeViewItemCollection Buildings { get; }

        /// <summary>
        /// The current resource treeview for this request.
        /// </summary>
        public static SchedulerResourceTreeView Current
        {
            get
            {
                SchedulerResourceTreeView result;

                if (HttpContext.Current.Items["SchedulerTreeView"] == null)
                {
                    result = new SchedulerResourceTreeView();
                    HttpContext.Current.Items["SchedulerTreeView"] = result;
                }
                else
                {
                    result = (SchedulerResourceTreeView)HttpContext.Current.Items["SchedulerTreeView"];
                }

                return result;
            }
        }
    }
}
