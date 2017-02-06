using Newtonsoft.Json;
using OnlineServices.Api;
using OnlineServices.Api.Authorization;
using OnlineServices.Api.Authorization.Credentials;
using System;
using System.Threading.Tasks;
using System.Web;

namespace LNF.Web.Scheduler.Handlers
{
    public class AuthHandler : HttpTaskAsyncHandler
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                var auth = await Authorize(context);
                context.Response.Write(JsonConvert.SerializeObject(new { success = true, token = auth.AccessToken }));
            }
            catch (Exception ex)
            {
                context.Response.Write(JsonConvert.SerializeObject(new { success = false, message = ex.Message }));
            }
        }

        private async Task<AuthorizationAccess> Authorize(HttpContext context)
        {
            AuthorizationClient authClient = new AuthorizationClient();
            var auth = await authClient.Authorize(new ClientCredentials());

            var cookie = new HttpCookie("lnf_api_token");
            cookie.Value = auth.AccessToken;
            cookie.Expires = auth.ExpirationDate;
            cookie.HttpOnly = false;
            context.Response.Cookies.Add(cookie);

            return auth;
        }
    }
}
