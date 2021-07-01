using LNF.Scheduler;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public interface INode
    {
        SchedulerContextHelper Helper { get; }
        INode Parent { get; }
        TreeViewNodeCollection Children { get; }
        int ID { get; }
        string Name { get; }
        string Description { get; }
        string Value { get; }
        string CssClass { get; }
        string ToolTip { get; }
        string GetUrl(HttpContextBase context);
        string GetImageUrl(HttpContextBase context);
        bool IsExpanded(string path);
        bool HasChildren();
    }
}
