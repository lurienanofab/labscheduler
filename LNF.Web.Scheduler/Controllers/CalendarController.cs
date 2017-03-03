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
