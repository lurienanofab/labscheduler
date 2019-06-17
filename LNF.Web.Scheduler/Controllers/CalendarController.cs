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
            HttpContextBase ctx = new HttpContextWrapper(context);

            ctx.Response.ContentType = "text/plain";

            string command = ctx.Request.QueryString["Command"];

            switch (command)
            {
                case "ChangeDate":
                    // ReturnTo should contain an absolute path and all querystring parameters (including Path when appropriate)
                    string returnTo = ctx.Request.QueryString["ReturnTo"];

                    if (string.IsNullOrEmpty(returnTo))
                        throw new InvalidOperationException("ReturnTo cannot be empty.");

                    DateTime date = DateTime.Parse(ctx.Request.QueryString["Date"]);

                    string redirectUrl = string.Format("{0}?Date={1:yyyy-MM-dd}", returnTo, date);

                    if (!ctx.Request.SelectedPath().IsEmpty())
                        redirectUrl += string.Format("&Path={0}", ctx.Request.SelectedPath().UrlEncode());

                    foreach (var key in ctx.Request.QueryString.AllKeys)
                    {
                        if (key != "Date" && key != "Path" && key != "Command" && key != "ReturnTo")
                        {
                            redirectUrl += string.Format("&{0}={1}", key, ctx.Request.QueryString[key]);
                        }
                    }

                    ctx.Response.Redirect(redirectUrl);
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
