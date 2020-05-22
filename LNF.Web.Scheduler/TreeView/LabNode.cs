using LNF.Scheduler;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class LabNode : TreeViewNode<ILab>
    {
        public LabNode(SchedulerResourceTreeView view, ILab item, INode parent) : base(view, item, parent)
        {
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~/Lab.aspx?Path={0}&Date={1:yyyy-MM-dd}", context.Server.UrlEncode(Value), context.Request.SelectedDate()));
        }

        public override string GetImageUrl(HttpContextBase context)
        {
            return string.Format("/scheduler/image/lab_icon/{0}", ID);
        }

        public override bool IsExpanded(string path) => PathInfo.Parse(path).LabID == ID;

        protected override void Load()
        {
            ID = Item.LabID;
            Name = Item.LabDisplayName;
            Description = Item.LabDescription;
            var procTechs = View.ResourceTree.ProcessTechs().Where(x => x.ProcessTechIsActive && x.LabID == Item.LabID).ToArray();
            Children = new TreeViewNodeCollection(procTechs.Select(x => new ProcessTechNode(View, x, this)).ToArray());
        }
    }
}
