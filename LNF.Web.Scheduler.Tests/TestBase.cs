using LNF.CommonTools;
using LNF.Data;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace LNF.Web.Scheduler.Tests
{
    public abstract class TestBase
    {
        private ClientInfo _CurrentClient;
        private HttpContextManager _ContextManager;

        public ClientInfo CurrentClient { get { return _CurrentClient; } }
        public HttpContextManager ContextManager { get { return _ContextManager; } }

        [TestInitialize]
        public void TestInit()
        {
            _ContextManager = new HttpContextManager();
            Prepare();
        }

        protected virtual void Prepare()
        {
            ContextManager.CurrentClient = new ClientModel()
            {
                ClientID = 1301,
                UserName = "jgett",
                Privs = (ClientPrivilege)3942
            };
        }

        protected int PurgeReservations(int resourceId, DateTime sd, DateTime ed)
        {
            int[] reservationIds;

            using (var dba = new SQLDBAccess("cnSselData"))
            {
                string sql = "SELECT ReservationID FROM sselScheduler.dbo.Reservation WHERE ResourceID = @resourceId AND (BeginDateTime < @ed AND EndDateTime > @sd)";
                DataTable dt = dba.CommandTypeText().ApplyParameters(new { resourceId, sd, ed }).FillDataTable(sql);
                reservationIds = dt.AsEnumerable().Select(x => x.Field<int>("ReservationID")).ToArray();
            }

            int result = 0;

            using (var dba = new SQLDBAccess("cnSselData"))
            {
                foreach (var id in reservationIds)
                {
                    string sql = "DELETE FROM sselScheduler.dbo.ReservationHistory WHERE ReservationID = @id; DELETE FROM sselScheduler.dbo.ReservationProcessInfo WHERE ReservationID = @id; DELETE FROM sselScheduler.dbo.ReservationInvitee WHERE ReservationID = @id; DELETE FROM sselScheduler.dbo.Reservation WHERE ReservationID = @id";
                    result += dba.CommandTypeText().ApplyParameters(new { id }).ExecuteNonQuery(sql);
                }
            }

            return result;
        }
    }

    public class HttpRequestManager
    {
        private StringWriter _writer = null;

        public HttpContext Context { get; }

        public HttpRequestManager(string filename, string url, string queryString, HttpSessionStateContainer sessionContainer)
        {
            var httpRequest = new HttpRequest(filename, url, queryString);
            _writer = new StringWriter();
            var httpResponse = new HttpResponse(_writer);
            Context = new HttpContext(httpRequest, httpResponse);

            SessionStateUtility.AddHttpSessionStateToContext(Context, sessionContainer);
        }

        public string GetResponse()
        {
            if (_writer != null)
                return _writer.ToString();
            else
                return null;
        }
    }

    public class HttpContextManager
    {
        private HttpSessionStateContainer _sessionContainer;

        public ClientModel CurrentClient { get; set; }

        public HttpContextManager()
        {
            _sessionContainer = new HttpSessionStateContainer(Guid.NewGuid().ToString("N"),
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(), 10, true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);
        }

        public HttpRequestManager StartRequest(ClientModel currentClient, string filename = "test.aspx", string url = "http://localhost/test.aspx", string queryString = "")
        {
            CurrentClient = currentClient;
            return StartRequest(filename, url, queryString);
        }

        public HttpRequestManager StartRequest(string filename = "test.aspx", string url = "http://localhost/test.aspx", string queryString = "")
        {
            HttpRequestManager result = new HttpRequestManager(filename, url, queryString, _sessionContainer);
            AddCurrentUserToContext(result.Context);
            HttpContext.Current = result.Context;
            return result;
        }

        private void AddCurrentUserToContext(HttpContext context)
        {
            IIdentity ident = new GenericIdentity(CurrentClient.UserName);
            IPrincipal user = new GenericPrincipal(ident, CurrentClient.Roles());
            context.User = user;

            HttpCookie authCookie = FormsAuthentication.GetAuthCookie(CurrentClient.UserName, true);
            FormsAuthenticationTicket formInfoTicket = FormsAuthentication.Decrypt(authCookie.Value);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(formInfoTicket.Version, formInfoTicket.Name, formInfoTicket.IssueDate, formInfoTicket.Expiration, formInfoTicket.IsPersistent, string.Join("|", CurrentClient.Roles()), formInfoTicket.CookiePath);
            authCookie.Value = FormsAuthentication.Encrypt(ticket);
            authCookie.Expires = formInfoTicket.Expiration;

            context.Request.Cookies.Add(authCookie);
            context.Response.Cookies.Add(authCookie);
        }
    }
}
