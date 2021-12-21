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

    public class TreeViewPartial : OnlineServicesPage
    {
        public TreeViewPartial()
        {
            ResourceTreeView treeview = (ResourceTreeView)LoadControl("~/UserControls/ResourceTreeView.ascx");
            treeview.SelectedPath = ContextBase.Request.SelectedPath().ToString();
            Controls.Add(treeview);
        }
    }
}