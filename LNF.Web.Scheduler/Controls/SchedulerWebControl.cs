using LNF.Models.Data;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class SchedulerWebControl : WebControl
    {
        public SchedulerWebControl(HtmlTextWriterTag tag) : base(tag)
        {
            ContextBase = new HttpContextWrapper(Context);
        }

        public IProvider Provider => ServiceProvider.Current;

        public HttpContextBase ContextBase { get; }

        public IClient CurrentUser => ContextBase.CurrentUser();

        public PathInfo SelectedPathFromViewState
        {
            get
            {
                PathInfo result;

                if (ViewState["SelectedPath"] == null)
                {
                    result = ContextBase.Request.SelectedPath();
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
    }
}
