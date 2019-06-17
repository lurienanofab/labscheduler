using LNF.Impl.Context;
using LNF.Impl.DataAccess;
using LNF.Impl.DependencyInjection.Web;
using Microsoft.Owin;
using NHibernate.Glimpse;
using Owin;
using System.Web.Routing;

[assembly: OwinStartup(typeof(LNF.Web.Scheduler.Startup))]

namespace LNF.Web.Scheduler
{
    public class Startup : OwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            var ioc = new IOC();
            ServiceProvider.Configure(ioc.Resolver);

            // setup for viewing NHibernate queries with Glimpse
            Plugin.RegisterSessionFactory(ioc.Resolver.GetInstance<ISessionManager>().GetSessionFactory());

            base.Configuration(app);
        }

        public override void ConfigureRoutes(RouteCollection routes)
        {
            // nothing to do here
        }
    }
}
