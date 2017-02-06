using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Controllers
{
    public class CalendarController : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string command = context.Request.QueryString["Command"];

            switch (command)
            {
                case "ChangeDate":
                    // ReturnTo should contain an absolute path and all querystring parameters (including Path when appropriate)
                    string returnTo = context.Request.QueryString["ReturnTo"];

                    if (string.IsNullOrEmpty(returnTo))
                        throw new InvalidOperationException("ReturnTo cannot be empty.");

                    DateTime date = DateTime.Parse(context.Request.QueryString["Date"]);

                    var userState = CacheManager.Current.CurrentUserState();
                    if (date.Date != userState.Date)
                    {
                        userState.SetDate(date);
                        userState.AddAction("Changed Date to {0:yyyy-MM-dd}", date);
                    }

                    context.Response.Redirect(returnTo);
                    break;
                default:
                    throw new Exception("unknown command");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
