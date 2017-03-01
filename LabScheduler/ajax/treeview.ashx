<%--
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
--%>

<%@ WebHandler Language="C#" Class="LabScheduler.Api.TreeView" %>

using LNF.Web.Content;
using LNF.Web.Scheduler;
using LNF.Web.Scheduler.Controls;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace LabScheduler.Api
{
    public class TreeView : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            context.Response.Write(RenderControlPartial(""));
        }

        private string RenderControlPartial(string virtualPath)
        {
            using (StringWriter writer = new StringWriter())
            {
                TreeViewPartial partial = new TreeViewPartial();
                HttpContext.Current.Server.Execute(partial, writer, false);
                return writer.ToString();
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }

    public class TreeViewPartial : LNFPage
    {
        public TreeViewPartial()
        {
            ResourceTreeView treeview = (ResourceTreeView)LoadControl("~/UserControls/ResourceTreeView.ascx");
            treeview.SelectedPath = PathInfo.Current;
            Controls.Add(treeview);
        }
    }
}