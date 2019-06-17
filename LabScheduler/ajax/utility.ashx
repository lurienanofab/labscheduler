<%@ WebHandler Language="C#" Class="LabScheduler.Api.Utility" %>

using LNF.Cache;
using LNF.Models.Data;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.SessionState;
using LNF.Web;

namespace LabScheduler.Api
{
    public class Utility : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            var ctx = new HttpContextWrapper(context);

            ctx.Response.ContentType = "application/json";

            string command = ctx.Request.QueryString["command"];

            switch (command)
            {
                case "IsStaff":
                    ctx.Response.Write(JsonConvert.SerializeObject(new { isStaff = IsStaff(ctx) }));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsStaff(HttpContextBase context)
        {
            return  context.CurrentUser().HasPriv(ClientPrivilege.Staff | ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}