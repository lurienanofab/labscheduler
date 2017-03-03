/*
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

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
