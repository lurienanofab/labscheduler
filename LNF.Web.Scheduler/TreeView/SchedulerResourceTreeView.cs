using LNF.Scheduler;
using System;
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

        public ResourceNode FindResourceNode(PathInfo path)
        {
            if (path.BuildingID > 0)
            { 
                var bldg = Root.Find(path.BuildingID);

                if (bldg == null) return null;

                if (path.LabID > 0 && bldg.HasChildren())
                {
                    var lab = bldg.Children.Find(path.LabID);

                    if (lab == null) return null;

                    if (path.ProcessTechID > 0 && lab.HasChildren())
                    {
                        var pt = lab.Children.Find(path.ProcessTechID);

                        if (pt == null) return null;

                        if (path.ResourceID > 0 && pt.HasChildren())
                        {
                            var res = pt.Children.Find(path.ResourceID);

                            return res as ResourceNode;
                        }
                    }
                }
            }

            return null;
        }
    }
}
