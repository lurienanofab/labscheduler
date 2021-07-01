using LNF.Cache;
using LNF.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace LNF.Web.Scheduler.Controls
{
    public class ResourceTabMenu : SchedulerWebControl
    {
        public ResourceTabMenu() : base(HtmlTextWriterTag.Div) { }

        public int SelectedIndex { get; set; }

        protected override void CreateChildControls()
        {
            var divRoot = new HtmlGenericControl("div");
            divRoot.Attributes.Add("class", "resource-tab-menu");

            LoadHeader(divRoot);

            LoadTabs(divRoot);

            Controls.Add(divRoot);
        }


        private void LoadHeader(HtmlGenericControl root)
        {
            var divTabsTitle = new HtmlGenericControl("div");
            divTabsTitle.Attributes.Add("class", "tabs-title");

            var h5 = new HtmlGenericControl("h5")
            {
                InnerHtml = GetHeaderText()
            };

            divTabsTitle.Controls.Add(h5);

            root.Controls.Add(divTabsTitle);
        }

        private void LoadTabs(HtmlGenericControl root)
        {
            var ul = new HtmlGenericControl("ul");

            ul.Attributes.Add("class", "nav nav-tabs");
            ul.Attributes.Add("role", "tablist");

            var tabs = GetTabs().Where(x => x.Visible);

            foreach (var t in tabs)
            {
                var li = new HtmlGenericControl("li");
                li.Attributes.Add("role", "presentation");
                li.Attributes.Add("class", t.CssClass);

                var a = new HtmlAnchor();
                a.Attributes.Add("role", "tab");
                a.HRef = t.NavigateUrl;
                a.InnerText = t.Text;

                li.Controls.Add(a);

                ul.Controls.Add(li);
            }

            root.Controls.Add(ul);
        }

        protected virtual string GetHeaderText()
        {
            var path = PathInfo.Parse(SelectedPath);
            IResource res = Helper.GetResourceTreeItemCollection().GetResource(path.ResourceID);

            if (res == null)
                return string.Empty;
            
            string result = $"<span>{res.BuildingName} &gt; {res.LabDisplayName} &gt; {res.ProcessTechName} &gt; </span><span class=\"tabs-resource-name\"><a href=\"{Page.Request.Url}\">{res.ResourceName} [{res.ResourceID}]</a></span>";

            var loc = GetLabLocationByResource(res.ResourceID);

            if (loc != null)
            {
                var locationPath = LocationPathInfo.Create(loc);
                var url = VirtualPathUtility.ToAbsolute($"~/LabLocation.aspx?LocationPath={locationPath.UrlEncode()}&Date={ContextBase.Request.SelectedDate():yyyy-MM-dd}");
                result += $" <span clas\"lab-location\">(<a href=\"{url}\">{loc.LocationName}</a>)</span>";
            }

            return result;
        }

        protected ILabLocation GetLabLocationByResource(int resourceId)
        {
            IEnumerable<ILabLocation> labLocations = SchedulerPage.Helper.LabLocations();
            IEnumerable<IResourceLabLocation> resourceLabLocations = SchedulerPage.Helper.ResourceLabLocations();

            ILabLocation result = null;

            var rll = resourceLabLocations.FirstOrDefault(x => x.ResourceID == resourceId);

            if (rll != null)
            {
                result = labLocations.FirstOrDefault(x => x.LabLocationID == rll.LabLocationID);
            }

            return result;
        }

        protected virtual IList<TabItem> GetTabs()
        {
            ClientAuthLevel authLevel = CacheManager.Current.GetAuthLevel(PathInfo.Parse(SelectedPath).ResourceID, CurrentUser);

            bool authorized = (authLevel & ClientAuthLevel.ToolEngineer) > 0;

            List<TabItem> tabs = new List<TabItem>
            {
                new TabItem() { CssClass = GetTabCssClass(0), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?TabIndex=0&Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Day", Visible = true },
                new TabItem() { CssClass = GetTabCssClass(1), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?TabIndex=1&Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Week", Visible = true },
                new TabItem() { CssClass = GetTabCssClass(2), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceClients.aspx?Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Clients", Visible = true },
                new TabItem() { CssClass = GetTabCssClass(3), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceContact.aspx?Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Helpdesk", Visible = true },
                new TabItem() { CssClass = GetTabCssClass(4), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceConfig.aspx?Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Configuration", Visible = authorized },
                new TabItem() { CssClass = GetTabCssClass(5), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceMaintenance.aspx?Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Repair", Visible = authorized },
                new TabItem() { CssClass = GetTabCssClass(6), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDocs.aspx?Path={0}&Date={1:yyyy-MM-dd}", SelectedPath, ContextBase.Request.SelectedDate())), Text = "Docs", Visible = authorized }
            };

            return tabs;
        }

        public bool IsSelected(int index)
        {
            return SelectedIndex == index;
        }

        protected string GetTabCssClass(int index)
        {
            if (IsSelected(index))
                return "active";
            else
                return string.Empty;
        }
    }

    public class TabItem
    {
        public string Text { get; set; }
        public string NavigateUrl { get; set; }
        public string CssClass { get; set; }
        public bool Visible { get; set; }
    }
}
