using LNF.Cache;
using LNF.Impl;
using Microsoft.Owin;
using NHibernate.Context;
using NHibernate.Glimpse;
using Owin;

[assembly: OwinStartup(typeof(LNF.Web.Scheduler.Startup))]

namespace LNF.Web.Scheduler
{
    public class Startup : OwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            // setup for viewing NHibernate queries with Glimpse
            Plugin.RegisterSessionFactory(SessionManager<WebSessionContext>.Current.SessionFactory);

            base.Configuration(app);
        }

        public override void ConfigureRoutes(System.Web.Routing.RouteCollection routes)
        {
            // nothing to do here
        }
    }
}
