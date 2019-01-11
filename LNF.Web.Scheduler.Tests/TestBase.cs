using LNF.Cache;
using LNF.CommonTools;
using LNF.Impl.DependencyInjection.Web;
using LNF.Models.Data;
using LNF.Repository;
using LNF.Repository.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
        public ClientInfo CurrentClient { get; } = null;
        public HttpContextManager ContextManager { get; private set; }

        public TestBase()
        {
            ContextManager = new HttpContextManager();
        }

        protected int PurgeReservations(int resourceId, DateTime sd, DateTime ed)
        {
            string sql;

            sql = "SELECT ReservationID FROM sselScheduler.dbo.Reservation WHERE ResourceID = @resourceId AND (BeginDateTime < @ed AND EndDateTime > @sd)";

            int[] reservationIds = DA.Command(CommandType.Text)
                .Param(new { resourceId, sd, ed })
                .FillDataTable(sql)
                .AsEnumerable()
                .Select(x => x.Field<int>("ReservationID"))
                .ToArray();

            return PurgeReservations(reservationIds);
        }

        protected int PurgeReservations(IEnumerable<int> reservationIds)
        {
            var sql = "DELETE FROM sselScheduler.dbo.ReservationHistory WHERE ReservationID IN (:p); DELETE FROM sselScheduler.dbo.ReservationProcessInfo WHERE ReservationID IN (:p); DELETE FROM sselScheduler.dbo.ReservationInvitee WHERE ReservationID IN (:p); DELETE FROM sselScheduler.dbo.Reservation WHERE ReservationID IN (:p)";

            int result = DA.Command(CommandType.Text)
                .ParamList("p", reservationIds)
                .ExecuteNonQuery(sql).Value;

            return result;
        }

        protected ClientItem GetClient(int clientId)
        {
            return CacheManager.Current.GetClient(clientId);
        }
    }

    public class HttpRequestManager : IDisposable
    {
        private StringWriter _responseWriter = null;
        private IUnitOfWork _uow = null;

        //public HttpContext Context { get; }

        public HttpRequestManager(string filename, string url, string queryString, HttpSessionStateContainer sessionContainer)
        {
            var httpRequest = new HttpRequest(filename, url, queryString);
            _responseWriter = new StringWriter();
            var httpResponse = new HttpResponse(_responseWriter);
            HttpContext.Current = new HttpContext(httpRequest, httpResponse);
            SessionStateUtility.AddHttpSessionStateToContext(HttpContext.Current, sessionContainer);
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();
            _uow = ServiceProvider.Current.DataAccess.StartUnitOfWork();
        }

        public string GetResponse()
        {
            if (_responseWriter != null)
                return _responseWriter.ToString();
            else
                return null;
        }

        public void Dispose()
        {
            if (_uow != null)
                _uow.Dispose();
        }
    }

    public class HttpContextManager
    {
        private HttpSessionStateContainer _sessionContainer;

        public HttpContextManager()
        {
            _sessionContainer = new HttpSessionStateContainer(Guid.NewGuid().ToString("N"),
                new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(), 10, true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);
        }

        public HttpRequestManager StartRequest(int clientId, string filename = "test.aspx", string url = "http://localhost/test.aspx", string queryString = "")
        {
            HttpRequestManager result = new HttpRequestManager(filename, url, queryString, _sessionContainer);
            AddClientToContext(CacheManager.Current.GetClient(clientId));
            return result;
        }

        private void AddClientToContext(ClientItem client)
        {
            IIdentity ident = new GenericIdentity(client.UserName);
            IPrincipal user = new GenericPrincipal(ident, client.Roles());
            HttpContext.Current.User = user;

            HttpCookie authCookie = FormsAuthentication.GetAuthCookie(client.UserName, true);
            FormsAuthenticationTicket formInfoTicket = FormsAuthentication.Decrypt(authCookie.Value);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(formInfoTicket.Version, formInfoTicket.Name, formInfoTicket.IssueDate, formInfoTicket.Expiration, formInfoTicket.IsPersistent, string.Join("|", client.Roles()), formInfoTicket.CookiePath);
            authCookie.Value = FormsAuthentication.Encrypt(ticket);
            authCookie.Expires = formInfoTicket.Expiration;

            HttpContext.Current.Request.Cookies.Add(authCookie);
            HttpContext.Current.Response.Cookies.Add(authCookie);
        }
    }
}
