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
                var result = HandleCommand(ctx);
                ctx.Response.Write(JsonConvert.SerializeObject(result));
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 500;
                ctx.Response.Write(JsonConvert.SerializeObject(new { Success = false, ErrorMessage = ex.Message, ex.StackTrace }));
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }

        private object HandleCommand(HttpContextBase context)
        {
            string command = context.Request["command"];
            switch (command)
            {
                case "send-hardware-issue-email":
                    int sent = SendHardwareTicketEmails(context);
                    return new { Success = sent > 0, Message = $"Emails sent: {sent}" };
                default:
                    throw new Exception("Invalid command");
            }
        }

        private int SendHardwareTicketEmails(HttpContextBase context)
        {
            string subject = context.Request["subject"];
            string message = context.Request["message"];

            if (!int.TryParse(context.Request["resourceId"], out int resourceId))
                throw new Exception("Invalid parameter: resourceId");

            var res = Provider.Scheduler.Resource.GetResource(resourceId);

            int sent = HelpdeskUtility.SendHardwareIssueEmail(res, context.CurrentUser(Provider).ClientID, subject, message);
            return sent;
        }
    }
}
