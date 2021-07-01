using LNF;
using LNF.DataAccess;
using LNF.Impl;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using Moq;
using System.Web;
using LNF.Web.Scheduler;
using System.Collections.Generic;
using System.Collections;

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

            Container = new Container();
            var config = new ThreadStaticContainerConfiguration(Container);
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
