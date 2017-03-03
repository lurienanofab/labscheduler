<%@ WebHandler Language="C#" Class="LabScheduler.Api.Utility" %>

using LNF.Cache;
using LNF.Models.Data;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.SessionState;

namespace LabScheduler.Api
{
    public class Utility : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            string command = context.Request.QueryString["command"];

            switch (command)
            {
                case "IsStaff":
                    context.Response.Write(JsonConvert.SerializeObject(new { isStaff = IsStaff() }));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool IsStaff()
        {
            return CacheManager.Current.CurrentUser.HasPriv(ClientPrivilege.Staff | ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}