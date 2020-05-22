using LNF.Data;
using LNF.Web.Scheduler.Content;
using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Controls
{
    public class SchedulerWebControl : WebControl
    {
        public SchedulerWebControl(HtmlTextWriterTag tag) : base(tag) { }

        public SchedulerPage SchedulerPage
        {
            get
            {
                if (Page == null) return null;

                if (typeof(SchedulerPage).IsAssignableFrom(Page.GetType()))
                    return (SchedulerPage)Page;

                throw new Exception($"Cannot convert {Page.GetType().Name} to SchedulerPage.");
            }
        }

        public ContextHelper Helper => SchedulerPage.Helper;

        public IProvider Provider => Helper.Provider;

        public HttpContextBase ContextBase => Helper.Context;

        public IClient CurrentUser => Helper.CurrentUser();

        public virtual string SelectedPath
        {
            get
            {
                string result;

                if (ViewState["SelectedPath"] == null)
                {
                    result = ContextBase.Request.SelectedPath().ToString();
                    ViewState["SelectedPath"] = result;
                }
                else
                    result = ViewState["SelectedPath"].ToString();

                return result;
            }
            set
            {
                ViewState["SelectedPath"] = value;
            }
        }
    }
}
