using LNF.Models.Authorization;
using Newtonsoft.Json;
using System;
using System.Web;

namespace LNF.Web.Scheduler.Handlers
{
    public class AuthHandler : IHttpHandler
    {
        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                var auth = Authorize(context);
                context.Response.Write(JsonConvert.SerializeObject(new { success = true, token = auth.AccessToken }));
            }
            catch (Exception ex)
            {
                context.Response.Write(JsonConvert.SerializeObject(new { success = false, message = ex.Message }));
            }
        }

        private IAuthorizationAccess Authorize(HttpContext context)
        {
            var auth = ServiceProvider.Current.Authorization.Authorize(new LNF.Models.Authorization.Credentials.ClientCredentials());

            var cookie = new HttpCookie("lnf_api_token")
            {
                Value = auth.AccessToken,
                Expires = auth.ExpirationDate,
                HttpOnly = false
            };

            context.Response.Cookies.Add(cookie);

            return auth;
        }
    }
}
