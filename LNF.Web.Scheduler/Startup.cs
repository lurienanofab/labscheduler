using LNF.Impl.DataAccess;
using LNF.Impl.DependencyInjection.Web;
using Microsoft.Owin;
using NHibernate.Glimpse;
using Owin;

[assembly: OwinStartup(typeof(LNF.Web.Scheduler.Startup))]

namespace LNF.Web.Scheduler
{
    public class Startup : OwinStartup
    {
        public override void Configuration(IAppBuilder app)
        {
            ServiceProvider.Current = IOC.Resolver.GetInstance<ServiceProvider>();

            // setup for viewing NHibernate queries with Glimpse
            Plugin.RegisterSessionFactory(IOC.Resolver.GetInstance<ISessionManager>().GetSessionFactory());

            base.Configuration(app);
        }

        public override void ConfigureRoutes(System.Web.Routing.RouteCollection routes)
        {
            // nothing to do here
        }
    }
}
