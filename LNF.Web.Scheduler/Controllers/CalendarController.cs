using LNF.Cache;
using LNF.Scheduler;
using System;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Controllers
{
    public class CalendarController : IHttpHandler, IRequiresSessionState
    {
        // The ReturnTo QueryString parameter should only container the file name, e.g. UserReservations.aspx

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

                    CacheManager.Current.CurrentUserState().AddAction("Changed Date to {0:yyyy-MM-dd}", date);

                    string redirectUrl = string.Format("{0}?Date={1:yyyy-MM-dd}", returnTo, date);

                    if (!PathInfo.Current.IsEmpty())
                        redirectUrl += string.Format("&Path={0}", PathInfo.Current.UrlEncode());

                    foreach (var key in context.Request.QueryString.AllKeys)
                    {
                        if (key != "Date" && key != "Path" && key != "Command" && key != "ReturnTo")
                        {
                            redirectUrl += string.Format("&{0}={1}", key, context.Request.QueryString[key]);
                        }
                    }

                    context.Response.Redirect(redirectUrl);
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
