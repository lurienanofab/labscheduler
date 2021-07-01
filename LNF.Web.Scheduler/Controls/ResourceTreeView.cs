using LNF.Scheduler;
using LNF.Web.Scheduler.TreeView;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class ResourceTreeView : SchedulerWebControl
    {
        #region Control Definitions
        protected HtmlInputHidden hidPathDelimiter;
        protected Repeater rptBuilding;
        #endregion

        public SchedulerResourceTreeView View { get; set; }

        private PathInfo _currentPath = PathInfo.Empty;

        public PathInfo GetCurrentPath()
        {
            if (_currentPath.IsEmpty() && !string.IsNullOrEmpty(SelectedPath))
                _currentPath = PathInfo.Parse(SelectedPath);
            return _currentPath;
        }

        public override string SelectedPath
        {
            get
            {
                string result;

                if (ViewState["SelectedPath"] == null)
                {
                    result = string.Empty;
                    ViewState["SelectedPath"] = result;
                }
                else
                    result = ViewState["SelectedPath"].ToString();

                return result;
            }
            set => base.SelectedPath = value;
        }

        public ResourceTreeView() : base(HtmlTextWriterTag.Div) { }

        protected override void CreateChildControls()
        {
            var sw = Stopwatch.StartNew();
            LoadTree();
            Helper.AppendLog($"ResourceTreeView.CreateChildControls: completed in {sw.Elapsed.TotalSeconds:0.0000} seconds");
            sw.Stop();
        }

        private void LoadTree()
        {
            var divTreeView = new HtmlGenericControl("div");
            divTreeView.Attributes.Add("class", "treeview");

            if (string.IsNullOrEmpty(SelectedPath))
            {
                // no path specified, need to use client defaults
                var cs = Helper.GetClientSetting();
                var lab = GetLabOrDefault(cs);

                if (lab != null)
                {
                    SelectedPath = PathInfo.Create(lab).ToString();
                }
            }

            var hidSelectedPath = new HtmlInputHidden();
            hidSelectedPath.Attributes.Add("class", "selected-path");
            hidSelectedPath.Value = SelectedPath.ToString();

            var hidPathDelimiter = new HtmlInputHidden();
            hidPathDelimiter.Attributes.Add("class", "path-delimiter");
            hidPathDelimiter.Value = PathInfo.PathDelimiter;

            divTreeView.Controls.Add(hidSelectedPath);
            divTreeView.Controls.Add(hidPathDelimiter);

            if (View.Root != null)
            {
                var ul = CreateTreeView(View.Root);
                divTreeView.Controls.Add(ul);
            }

            Controls.Add(divTreeView);
        }

        private HtmlGenericControl CreateTreeView(TreeViewNodeCollection buildings)
        {
            HtmlGenericControl ulBuildings = new HtmlGenericControl("ul");
            ulBuildings.Attributes.Add("class", "root buildings");

            foreach (var bldg in buildings)
            {
                var liBuilding = CreateNode(bldg);

                if (bldg.HasChildren())
                {
                    var ulLabs = new HtmlGenericControl("ul");
                    ulLabs.Attributes.Add("class", "child labs");

                    foreach (var lab in bldg.Children)
                    {
                        var liLab = CreateNode(lab);

                        if (lab.HasChildren())
                        {
                            var ulProcTechs = new HtmlGenericControl("ul");
                            ulProcTechs.Attributes.Add("class", "child proctechs");

                            foreach (var pt in lab.Children)
                            {
                                var liProcTech = CreateNode(pt);

                                if (pt.HasChildren())
                                {
                                    var ulResources = new HtmlGenericControl("ul");
                                    ulResources.Attributes.Add("class", "child resources");

                                    foreach (var res in pt.Children)
                                    {
                                        var liResource = CreateNode(res);
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

        private HtmlGenericControl CreateNode(INode item)
        {
            string current = item.HasChildren() ? "branch" : "leaf";

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

            var a = new HtmlAnchor
            {
                HRef = item.GetUrl(ContextBase),
                InnerText = item.Name
            };

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

        protected void RptBuilding_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var parent = (INode)e.Item.DataItem;
                var rptLab = (Repeater)e.Item.FindControl("rptLab");
                var items = parent.Children;

                if (items != null)
                {
                    rptLab.DataSource = items;
                    rptLab.DataBind();
                }
            }
        }

        protected void RptLab_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var parent = (INode)e.Item.DataItem;
                var rptProcessTech = (Repeater)e.Item.FindControl("rptProcessTech");
                var items = parent.Children;

                if (items != null)
                {
                    rptProcessTech.DataSource = items;
                    rptProcessTech.DataBind();
                }
            }
        }

        protected void RptProcessTech_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var parent = (INode)e.Item.DataItem;
                var rptResource = (Repeater)e.Item.FindControl("rptResource");
                var items = parent.Children;

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

        private void HandleSpecialCases(INode parent, TreeViewNodeCollection items)
        {
            // We want Plasmatherm 790 (10030) to also show up under PECVD in addition to Plasma Etch, so same tool under two process techs.
            if (parent is ProcessTechNode)
            {
                if (parent.ID == 21)
                {
                    // only add if it isn't there already
                    if (!items.Any(x => x.ID == 10030))
                    {
                        var resourceTree = Helper.GetResourceTreeItemCollection();
                        var plasma790 = resourceTree.GetResourceTree(10030);

                        if (plasma790 != null)
                        {
                            ClientAuthLevel auth = Helper.GetCurrentAuthLevel(plasma790.ResourceID);
                            if ((plasma790.IsReady && plasma790.IsSchedulable) || auth == ClientAuthLevel.ToolEngineer)
                                items.Add(new ResourceNode(View, plasma790, parent));
                        }
                    }
                }
            }
        }

        private bool ShowImages()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["TreeView.ShowImages"], out bool result))
                return result;
            else
                return false;
        }

        private HtmlImage GetImage(INode item)
        {
            if (ShowImages())
            {
                string imageUrl = item.GetImageUrl(ContextBase);
                if (string.IsNullOrEmpty(imageUrl)) return null;
                var result = new HtmlImage() { Src = imageUrl };
                result.Attributes.Add("onerror", "handleMissingImage(this);");
                result.Alt = "icon";
                return result;
            }
            else
                return null;
        }

        protected string GetNodeCssClass(INode item, string current)
        {
            int buildingId = 0;
            int labId = 0;
            int procTechId = 0;
            int resourceId = 0;

            var sp = GetCurrentPath();

            if (sp.IsEmpty())
            {
                var cs = Helper.GetClientSetting();
                buildingId = GetBuildingOrDefault(cs).BuildingID;
                labId = GetLabOrDefault(cs).LabID;
            }
            else
            {
                buildingId = sp.BuildingID;
                labId = sp.LabID;
                procTechId = sp.ProcessTechID;
                resourceId = sp.ResourceID;
            }

            var path = PathInfo.Create(buildingId, labId, procTechId, resourceId);

            bool expanded = item.IsExpanded(path.ToString());
            bool selected = SelectedPath == item.Value;

            return string.Format("node{0}{1} {2}", expanded ? " expanded" : " collapsed", selected ? " selected" : string.Empty, current).Trim();
        }

        private IBuilding GetBuildingOrDefault(IClientSetting cs)
        {
            var buildingId = cs.GetBuildingID();
            return View.ResourceTree.GetBuilding(buildingId);
        }

        private ILab GetLabOrDefault(IClientSetting cs)
        {
            var labId = cs.GetLabID();
            return View.ResourceTree.GetLab(labId);
        }
    }
}
