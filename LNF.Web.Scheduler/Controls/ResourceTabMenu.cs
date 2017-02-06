using LNF.Scheduler;
using LNF.Cache;
using LNF.Models.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class ResourceTabMenu : UserControl
    {
        #region Controls
        protected Repeater rptTabs;
        protected Literal litHeaderText;
        #endregion

        public int SelectedIndex { get; set; }

        public ResourceTabMenu()
        {
            SelectedIndex = 0;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadHeader();
                LoadTabs();
            }
        }

        private void LoadHeader()
        {
            litHeaderText.Text = GetHeader();
        }

        private void LoadTabs()
        {
            rptTabs.DataSource = GetTabs().Where(x => x.Visible);
            rptTabs.DataBind();
        }

        protected virtual string GetHeader()
        {
            ResourceModel res = PathInfo.Current.GetResource();

            if (res == null)
                return string.Empty;
            else
                return string.Format("<span>{0} &gt; {1} &gt; {2} &gt; </span><span class=\"tabs-resource-name\">{3} [{4}]</span>",
                    res.BuildingName, res.LabDisplayName, res.ProcessTechName, res.ResourceName, res.ResourceID);
        }

        protected virtual IList<TabItem> GetTabs()
        {
            ClientAuthLevel authLevel = CacheManager.Current.GetAuthLevel(PathInfo.Current.ResourceID, CacheManager.Current.ClientID);
            bool authorized = (authLevel & ClientAuthLevel.ToolEngineer) > 0;

            List<TabItem> tabs = new List<TabItem>();
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(0), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?TabIndex=0&Path={0}", PathInfo.Current)), Text = "Day", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(1), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?TabIndex=1&Path={0}", PathInfo.Current)), Text = "Week", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(2), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceClients.aspx?Path={0}", PathInfo.Current)), Text = "Clients", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(3), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceContact.aspx?Path={0}", PathInfo.Current)), Text = "Helpdesk", Visible = true });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(4), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceConfig.aspx?Path={0}", PathInfo.Current)), Text = "Configuration", Visible = authorized });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(5), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceMaintenance.aspx?Path={0}", PathInfo.Current)), Text = "Repair", Visible = authorized });
            tabs.Add(new TabItem() { CssClass = GetTabCssClass(6), NavigateUrl = VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDocs.aspx?Path={0}", PathInfo.Current)), Text = "Docs", Visible = authorized });

            return tabs;
        }

        protected void btnQuickReservation_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
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
