<%--
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
--%>

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