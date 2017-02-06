using System;
using System.Web.Security;

namespace LNF.Web.Scheduler.Tests
{
    public class TestContextProvider : IContextProvider
    {
        private readonly IContext _Current;

        public TestContextProvider()
        {
            _Current = new TestContext();
        }

        public IContext Current
        {
            get { return _Current; }
        }

        public string LoginUrl { get; set; }
    }

    public class TestContext : DefaultContext
    {
        public TestContext()
        {
            AuthCookieName = FormsAuthentication.FormsCookieName;
            AuthCookiePath = FormsAuthentication.FormsCookiePath;
            AuthCookieDomain = FormsAuthentication.CookieDomain;
        }
    }
}
