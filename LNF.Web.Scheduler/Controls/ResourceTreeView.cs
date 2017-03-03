using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Scheduler.Content;
using LNF.Web.Scheduler.TreeView;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class ResourceTreeView : SchedulerUserControl
    {
        #region Control Definitions
        protected HtmlInputHidden hidSelectedPath;
        protected HtmlInputHidden hidPathDelimiter;
        protected Repeater rptBuilding;
        #endregion

        private PathInfo _SelectedPath = PathInfo.Current;

        public PathInfo SelectedPath
        {
            get { return _SelectedPath; }
            set { _SelectedPath = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            DateTime startTime = DateTime.Now;

            if (!Page.IsPostBack)
                LoadTree();

            RequestLog.Append("ResourceTreeView.OnLoad: {0}", DateTime.Now - startTime);
        }

        private void LoadTree()
        {
            if (SelectedPath.IsEmpty())
            {
                // no path specified, need to use client defaults
                var lab = CacheManager.Current.GetClientSetting().GetLabOrDefault();

                if (lab != null)
                    SelectedPath = PathInfo.Create(lab);
            }

            hidSelectedPath.Value = SelectedPath.ToString();
            hidPathDelimiter.Value = PathInfo.PathDelimiter;

            SchedulerTreeView treeView = new SchedulerTreeView();
            TreeItemCollection buildingItems = treeView.Buildings;

            if (buildingItems != null)
            {
                rptBuilding.DataSource = buildingItems;
                rptBuilding.DataBind();
            }
        }

        protected void rptBuilding_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ITreeItem parent = (ITreeItem)e.Item.DataItem;
                Repeater rptLab = (Repeater)e.Item.FindControl("rptLab");
                TreeItemCollection items = parent.Children;
                if (items != null)
                {
                    rptLab.DataSource = items;
                    rptLab.DataBind();
                }
            }
        }

        protected void rptLab_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ITreeItem parent = (ITreeItem)e.Item.DataItem;
                Repeater rptProcessTech = (Repeater)e.Item.FindControl("rptProcessTech");
                TreeItemCollection items = parent.Children;
                if (items != null)
                {
                    rptProcessTech.DataSource = items;
                    rptProcessTech.DataBind();
                }
            }
        }

        protected void rptProcessTech_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ITreeItem parent = (ITreeItem)e.Item.DataItem;
                Repeater rptResource = (Repeater)e.Item.FindControl("rptResource");
                TreeItemCollection items = parent.Children;
                if (items != null)
                {
                    HandleSpecialCases(parent, items);
                    if (items.Count > 0)
                    {
                        rptResource.DataSource = items;
                        rptResource.DataBind();
                    }
                    else
                        e.Item.Visible = false;
                }
            }
        }

        private void HandleSpecialCases(ITreeItem parent, TreeItemCollection items)
        {
            // We want Plasmatherm 790 (10030) to also show up under PECVD in addition to Plasma Etch, so same tool under two process techs.
            if (parent.Type == TreeItemType.ProcessTech)
            {
                if (parent.ID == 21)
                {
                    // only add if it isn't there already
                    if (!items.Any(x => x.ID == 10030))
                    {
                        ResourceModel plasma790 = CacheManager.Current.Resources(x => x.ResourceID == 10030).FirstOrDefault();
                        if (plasma790 != null)
                        {
                            ClientAuthLevel auth = GetCurrentUserAuthLevel(plasma790.ResourceID);
                            if ((plasma790.IsReady && plasma790.IsSchedulable) || auth == ClientAuthLevel.ToolEngineer)
                                items.Add(new ResourceTreeItem(plasma790, parent));
                        }
                    }
                }
            }
        }

        private ClientAuthLevel GetCurrentUserAuthLevel(int resourceId)
        {
            return CacheManager.Current.GetAuthLevel(resourceId, CacheManager.Current.ClientID);
        }

        private bool ShowImages()
        {
            bool result;
            if (bool.TryParse(ConfigurationManager.AppSettings["TreeView.ShowImages"], out result))
                return result;
            else
                return false;
        }

        protected string GetImage(ITreeItem item)
        {
            if (ShowImages())
            {
                string imageUrl = string.Format("/scheduler/image/{0}_icon/{1}", TreeViewUtility.TreeItemTypeToString(item.Type), item.ID);
                return string.Format("<img src=\"{0}\" onerror=\"handleMissingImage(this);\" alt=\"icon\" />", imageUrl);
            }
            else
                return string.Empty;
        }

        protected string GetNodeCssClass(ITreeItem item, string current)
        {
            bool expanded = false;
            bool selected = false;

            int buildingId = 0;
            int labId = 0;
            int procTechId = 0;
            int resourceId = 0;

            if (SelectedPath.IsEmpty())
            {
                buildingId = CacheManager.Current.GetClientSetting().GetBuildingOrDeafult().BuildingID;
                labId = CacheManager.Current.GetClientSetting().GetLabOrDefault().LabID;
            }
            else
            {
                buildingId = SelectedPath.BuildingID;
                labId = SelectedPath.LabID;
                procTechId = SelectedPath.ProcessTechID;
                resourceId = SelectedPath.ResourceID;
            }

            switch (item.Type)
            {
                case TreeItemType.Building:
                    expanded = buildingId == item.ID;
                    selected = SelectedPath.ToString() == item.Value;
                    break;
                case TreeItemType.Lab:
                    expanded = labId == item.ID;
                    selected = SelectedPath.ToString() == item.Value;
                    break;
                case TreeItemType.ProcessTech:
                    expanded = procTechId == item.ID;
                    selected = SelectedPath.ToString() == item.Value;
                    break;
                case TreeItemType.Resource:
                    expanded = resourceId == item.ID;
                    selected = SelectedPath.ToString() == item.Value;
                    break;
            }

            return string.Format("node{0}{1} {2}", expanded ? " expanded" : " collapsed", selected ? " selected" : string.Empty, current).Trim();
        }

        protected string GetNodeUrl(ITreeItem item)
        {
            switch (item.Type)
            {
                case TreeItemType.Building:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/Building.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Request.GetCurrentDate()));
                case TreeItemType.Lab:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/Lab.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Request.GetCurrentDate()));
                case TreeItemType.ProcessTech:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Request.GetCurrentDate()));
                case TreeItemType.Resource:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Request.GetCurrentDate()));
                default:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/?Date={0:yyyy-MM-dd}", Request.GetCurrentDate()));
            }
        }
    }
}
