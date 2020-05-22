using LNF.Impl.DataAccess;
using NHibernate.Glimpse;
using Owin;

namespace LNF.Web.Scheduler
{
    public static class GlimpseHelper
    {
        public static void ConfigureGlimpse(this IAppBuilder app, ISessionManager mgr)
        {
            var factory = mgr.GetSessionFactory();
            Plugin.RegisterSessionFactory(factory);
        }
    }
}
