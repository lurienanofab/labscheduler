using LNF.Scheduler;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Handlers
{
    public class HelpdeskHandler : IHttpHandler, IReadOnlySessionState
    {
        [Inject] public IProvider Provider { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            var ctx = new HttpContextWrapper(context);

            ctx.Response.ContentType = "application/json";

            try
            {
                HandleCommand(ctx);
                ctx.Response.Write(JsonConvert.SerializeObject(new { Success = true }));
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.Write(JsonConvert.SerializeObject(new { Success = false, ErrorMessage = ex.Message, StackTrace = ex.StackTrace }));
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private void HandleCommand(HttpContextBase context)
        {
            string command = context.Request["command"];
            switch (command)
            {
                case "send-hardware-issue-email":
                    SendHardwareTicketEmails(context);
                    break;
                default:
                    throw new Exception("Invalid command");
            }
        }

        private void SendHardwareTicketEmails(HttpContextBase context)
        {
            string subject = context.Request["subject"];
            string message = context.Request["message"];

            if (!int.TryParse(context.Request["resourceId"], out int resourceId))
                throw new Exception("Invalid parameter: resourceId");

            var res = Provider.Scheduler.Resource.GetResource(resourceId);

            HelpdeskUtility.SendHardwareIssueEmail(res, context.CurrentUser(Provider).ClientID, subject, message);
        }
    }
}
