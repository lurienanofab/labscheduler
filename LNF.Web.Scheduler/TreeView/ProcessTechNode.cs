using LNF.Scheduler;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class ProcessTechNode : TreeViewNode<IProcessTech>
    {
        public int LabID { get; }

        public ProcessTechNode(SchedulerResourceTreeView view, IProcessTech item, INode parent) : base(view, item, parent)
        {
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", context.Server.UrlEncode(Value), context.Request.SelectedDate()));
        }

        public override string GetImageUrl(HttpContextBase context)
        {
            return string.Format("/scheduler/image/proctech_icon/{0}", ID);
        }

        public override bool IsExpanded(string path) => PathInfo.Parse(path).ProcessTechID == ID;

        protected override void Load()
        {
            ID = Item.ProcessTechID;
            Name = Item.ProcessTechName;
            Description = Item.ProcessTechDescription;
            var resources = View.ResourceTree.Resources().Where(x => x.ResourceIsActive && x.ProcessTechID == Item.ProcessTechID).ToArray();
            Children = new TreeViewNodeCollection(resources.Select(x => new ResourceNode(View, x, this)).ToArray());
        }
    }
}
