using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;
using LNF.Repository;
using LNF.Repository.Scheduler;

namespace LNF.Web.Scheduler.Handlers
{
    public class ResourceHandler : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            try
            {
                int x = await Task.FromResult(0);

                string command = context.Request.QueryString["Command"];

                switch (command)
                {
                    case "GetProcessInfoLineParams":
                        GetProcessInfoLineParams(context);
                        break;
                    default:
                        throw new HttpException(500, "Invalid parameter: command");
                }
            }
            catch (HttpException httpex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = httpex.GetHttpCode();
                context.Response.Write(JsonConvert.SerializeObject(new { Message = "An error has occurred.", ExceptionMessage = httpex.Message, ExceptionType = httpex.GetType().FullName, StackTrace = httpex.StackTrace }));
            }
            catch (Exception ex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;
                context.Response.Write(JsonConvert.SerializeObject(new { Message = "An error has occurred.", ExceptionMessage = ex.Message, ExceptionType = ex.GetType().FullName, StackTrace = ex.StackTrace }));
            }
        }

        public void GetProcessInfoLineParams(HttpContext context)
        {
            int resourceId;

            if (!int.TryParse(context.Request.QueryString["resourceId"], out resourceId))
                throw new HttpException(500, "Invalid parameter: resourceId");

            string[] paramNames = DA.Current.Query<ProcessInfoLineParam>().Where(x => x.Resource.ResourceID == resourceId).Select(x => x.ParameterName).ToArray();

            context.Response.ContentType = "application/json";
            context.Response.Write(JsonConvert.SerializeObject(paramNames));
        }
    }
}
