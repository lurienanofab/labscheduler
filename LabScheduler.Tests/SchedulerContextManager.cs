using LNF.Impl.Testing;
using Moq;
using System.Collections;
using System.Collections.Specialized;
using System.Web;

namespace LabScheduler.Tests
{
    internal sealed class SchedulerContextManager : ContextManager
    {
        public SchedulerContextManager(string ipaddr = null, string username = null, IDictionary contextItems = null, SessionItemCollection sessionItems = null, NameValueCollection queryString = null) : base(ipaddr, username, contextItems, sessionItems, queryString) { }

        public override void ConfigureMockSession(Mock<HttpSessionStateBase> session)
        {
            session.SetupSet(x => x["CurrentViewType"] = It.IsAny<object>()).Callback<string, object>(SessionItems.Set);
            session.SetupSet(x => x["ActiveReservationMessage"] = It.IsAny<object>()).Callback<string, object>(SessionItems.Set);
            session.SetupSet(x => x["ErrorMessage"] = It.IsAny<object>()).Callback<string, object>(SessionItems.Set);
            session.SetupSet(x => x["ReservationProcessInfoJsonData"] = It.IsAny<object>()).Callback<string, object>(SessionItems.Set);
            session.SetupSet(x => x["ReservationInvitees"] = It.IsAny<object>()).Callback<string, object>(SessionItems.Set);
            session.SetupSet(x => x["IsRecurring"] = It.IsAny<object>()).Callback<string, object>(SessionItems.Set);
        }
    }
}
