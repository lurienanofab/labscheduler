<%@ WebHandler Language="C#" Class="LabScheduler.Api.Utility" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;
using LNF.Data;
using LNF.Repository;
using LNF.Repository.Data;

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
            return Client.Current.HasPriv(ClientPrivilege.Staff | ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }
        
        public bool IsReusable
        {
            get { return false; }
        }
    }
}