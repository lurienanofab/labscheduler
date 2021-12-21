using LNF;
using LNF.DataAccess;
using LNF.Impl.DependencyInjection;
using LNF.Web.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SimpleInjector;
using System.Collections;
using System.Collections.Generic;
using System.Web;

namespace LabScheduler.Tests
{
    public abstract class TestBase
    {
        private Mock<HttpContextBase> _contextMock;
        private Mock<HttpRequestBase> _requestMock;
        private readonly IDictionary _items = new Dictionary<string, object>();

        public Container Container { get; private set; }
        public IProvider Provider => Container.GetInstance<IProvider>();
        public HttpContextBase Context { get; private set; }
        public SchedulerContextHelper Helper { get; private set; }

        [TestInitialize]
        public void Initialize()
        {
            _requestMock = new Mock<HttpRequestBase>();
            _requestMock.Setup(x => x.UserHostAddress).Returns("141.213.6.57");

            _contextMock = new Mock<HttpContextBase>();
            _contextMock.Setup(x => x.Request).Returns(_requestMock.Object);
            _contextMock.Setup(x => x.Items).Returns(_items);
            Context = _contextMock.Object;

            var containerContext = ContainerContextFactory.Current.NewThreadScopedContext();
            Container = containerContext.Container;
            var config = new ThreadStaticContainerConfiguration(containerContext);
            config.RegisterAllTypes();
            ServiceProvider.Setup(Provider);

            Helper = new SchedulerContextHelper(Context, Provider);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Container.Dispose();
        }

        public IUnitOfWork StartUnitOfWork()
        {
            return Provider.DataAccess.StartUnitOfWork();
        }
    }
}
