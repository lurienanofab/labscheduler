using LNF.Cache;
using LNF.Email;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;

namespace LNF.Web.Scheduler.Handlers
{
    public class HelpdeskHandler : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                HandleCommand(context);
                context.Response.Write(JsonConvert.SerializeObject(new { Success = true }));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write(ServiceProvider.Current.Serialization.Json.SerializeObject(new { Success = false, ErrorMessage = ex.Message, StackTrace = ex.StackTrace }));
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private void HandleCommand(HttpContext context)
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

        private void SendHardwareTicketEmails(HttpContext context)
        {
            string subject = context.Request["subject"];
            string message = context.Request["message"];

            if (!int.TryParse(context.Request["resourceId"], out int resourceId))
                throw new Exception("Invalid parameter: resourceId");

            ResourceModel res = CacheManager.Current.ResourceTree().GetResource(resourceId);

            HelpdeskUtility.SendHardwareIssueEmail(res, CacheManager.Current.CurrentUser.ClientID, subject, message);
        }
    }
}
