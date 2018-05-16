using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Scheduler.TreeView;
using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class ResourceTreeView : WebControl
    {
        #region Control Definitions
        protected HtmlInputHidden hidPathDelimiter;
        protected Repeater rptBuilding;
        #endregion

        public ResourceTreeView() : base(HtmlTextWriterTag.Div) { }

        public PathInfo SelectedPath
        {
            get
            {
                PathInfo result;

                if (ViewState["SelectedPath"] == null)
                {
                    result = Page.Request.SelectedPath();
                    ViewState["SelectedPath"] = result.ToString();
                }
                else
                {
                    result = PathInfo.Parse(ViewState["SelectedPath"].ToString());
                }

                return result;
            }
            set
            {
                ViewState["SelectedPath"] = value.ToString();
            }
        }

        protected override void CreateChildControls()
        {
            DateTime startTime = DateTime.Now;
            LoadTree();
            RequestLog.Append("ResourceTreeView.CreateChildControls: {0}", DateTime.Now - startTime);
        }

        private void LoadTree()
        {
            var divTreeView = new HtmlGenericControl("div");
            divTreeView.Attributes.Add("class", "treeview");

            if (SelectedPath.IsEmpty())
            {
                // no path specified, need to use client defaults
                var lab = CacheManager.Current.GetClientSetting().GetLabOrDefault();

                if (lab != null)
                    SelectedPath = PathInfo.Create(lab);
            }

            var hidSelectedPath = new HtmlInputHidden();
            hidSelectedPath.Attributes.Add("class", "selected-path");
            hidSelectedPath.Value = SelectedPath.ToString();

            var hidPathDelimiter = new HtmlInputHidden();
            hidPathDelimiter.Attributes.Add("class", "path-delimiter");
            hidPathDelimiter.Value = PathInfo.PathDelimiter;

            divTreeView.Controls.Add(hidSelectedPath);
            divTreeView.Controls.Add(hidPathDelimiter);

            TreeItemCollection buildingItems = SchedulerTreeView.Current.Buildings;

            if (buildingItems != null)
            {
                var ul = CreateTreeView(buildingItems);
                divTreeView.Controls.Add(ul);
            }

            Controls.Add(divTreeView);
        }

        private HtmlGenericControl CreateTreeView(TreeItemCollection buildings)
        {
            HtmlGenericControl ulBuildings = new HtmlGenericControl("ul");
            ulBuildings.Attributes.Add("class", "root buildings");

            foreach (var bldg in buildings)
            {
                var liBuilding = CreateNode(bldg, "branch");

                if (bldg.Children.Count > 0)
                {
                    var ulLabs = new HtmlGenericControl("ul");
                    ulLabs.Attributes.Add("class", "child labs");

                    foreach (var lab in bldg.Children)
                    {
                        var liLab = CreateNode(lab, "branch");

                        if (lab.Children.Count > 0)
                        {
                            var ulProcTechs = new HtmlGenericControl("ul");
                            ulProcTechs.Attributes.Add("class", "child proctechs");

                            foreach(var pt in lab.Children)
                            {
                                var liProcTech = CreateNode(pt, "branch");

                                if (pt.Children.Count > 0)
                                {
                                    var ulResources = new HtmlGenericControl("ul");
                                    ulResources.Attributes.Add("class", "child resources");

                                    foreach (var res in pt.Children)
                                    {
                                        var liResource = CreateNode(res, "leaf");
                                        ulResources.Controls.Add(liResource);
                                    }

                                    liProcTech.Controls.Add(ulResources);
                                }

                                ulProcTechs.Controls.Add(liProcTech);
                            }

                            liLab.Controls.Add(ulProcTechs);
                        }

                        ulLabs.Controls.Add(liLab);
                    }

                    liBuilding.Controls.Add(ulLabs);
                }

                ulBuildings.Controls.Add(liBuilding);
            }

            return ulBuildings;
        }

        private HtmlGenericControl CreateNode(ITreeItem item, string current)
        {
            var result = new HtmlGenericControl("li");
            result.Attributes.Add("data-id", item.ID.ToString());
            result.Attributes.Add("data-value", item.Value);
            result.Attributes.Add("class", GetNodeCssClass(item, current));

            var divNodeText = new HtmlGenericControl("div");
            divNodeText.Attributes.Add("class", "node-text");
            divNodeText.Attributes.Add("title", item.ToolTip);

            var tblNodeTextTable = new HtmlTable();
            tblNodeTextTable.Attributes.Add("class", "node-text-table");

            var row = new HtmlTableRow();
            HtmlTableCell cell;

            // cell #1 - click area
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "node-text-clickarea");
            cell.InnerHtml = "&nbsp;";
            row.Cells.Add(cell);

            // cell #2 - image (maybe) and text
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", item.CssClass);
            var img = GetImage(item);
            if (img != null) cell.Controls.Add(img);
            var a = new HtmlAnchor();
            a.HRef = GetNodeUrl(item);
            a.InnerText = item.Name;
            cell.Controls.Add(a);
            row.Cells.Add(cell);

            // Add the row
            tblNodeTextTable.Rows.Add(row);

            // Add the table
            divNodeText.Controls.Add(tblNodeTextTable);

            // Add the div
            result.Controls.Add(divNodeText);

            // at this point we only have the li without child nodes

            return result;
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
                        ResourceModel plasma790 = CacheManager.Current.ResourceTree().Resources().FirstOrDefault(x => x.ResourceID == 10030);

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

        private HtmlImage GetImage(ITreeItem item)
        {
            if (ShowImages())
            {
                string imageUrl = string.Format("/scheduler/image/{0}_icon/{1}", TreeViewUtility.TreeItemTypeToString(item.Type), item.ID);
                var result = new HtmlImage();
                result.Src = imageUrl;
                result.Attributes.Add("onerror", "handleMissingImage(this);");
                result.Alt = "icon";
                return result;
            }
            else
                return null;
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
                    return VirtualPathUtility.ToAbsolute(string.Format("~/Building.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Page.Request.SelectedDate()));
                case TreeItemType.Lab:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/Lab.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Page.Request.SelectedDate()));
                case TreeItemType.ProcessTech:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/ProcessTech.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Page.Request.SelectedDate()));
                case TreeItemType.Resource:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", HttpUtility.UrlEncode(item.Value), Page.Request.SelectedDate()));
                default:
                    return VirtualPathUtility.ToAbsolute(string.Format("~/?Date={0:yyyy-MM-dd}", Page.Request.SelectedDate()));
            }
        }
    }
}
