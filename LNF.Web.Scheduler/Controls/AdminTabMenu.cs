using System.Collections.Generic;
using System.Web;

namespace LNF.Web.Scheduler.Controls
{
    public class AdminTabMenu : ResourceTabMenu
    {
        protected override string GetHeaderText()
        {
            return "Administration";
        }

        protected override IList<TabItem> GetTabs()
        {
            var tabs = new List<TabItem>();
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(0), NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminActivities.aspx"), Text = "Activities", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(1), NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminBuildings.aspx"), Text = "Buildings", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(2), NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminLabs.aspx"), Text = "Labs", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(3), NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminProcessTechs.aspx"), Text = "Process Techs", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(4), NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminResources.aspx"), Text = "Resources", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(5), NavigateUrl = VirtualPathUtility.ToAbsolute("~/AdminProperties.aspx"), Text = "Scheduler Properties", Visible = true });
            return tabs;
        }
    }
}
