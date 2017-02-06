using LNF.Cache;
using LNF.Email;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System;
using System.Linq;
using System.Web;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Handlers
{
    public class HelpdeskHandler : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                context.Response.Write(Providers.Serialization.Json.SerializeObject(HandleCommand(context)));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write(Providers.Serialization.Json.SerializeObject(new { ErrorMessage = ex.Message, StackTrace = ex.StackTrace }));
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private object HandleCommand(HttpContext context)
        {
            string command = context.Request["command"];
            switch (command)
            {
                case "send-hardware-issue-email":
                    return SendHardwareTicketEmails(context);
                default:
                    throw new Exception("Invalid command");
            }
        }

        private object SendHardwareTicketEmails(HttpContext context)
        {
            string subject = context.Request["subject"];
            string message = context.Request["message"];
            int resourceId;

            if (!int.TryParse(context.Request["resourceId"], out resourceId))
                throw new Exception("Invalid parameter: resourceId");

            ResourceModel res = CacheManager.Current.GetResource(resourceId);

            SendMessageResult result = HelpdeskUtility.SendHardwareIssueEmail(res, CacheManager.Current.CurrentUser.ClientID, subject, message);

            return result;
        }
    }
}
