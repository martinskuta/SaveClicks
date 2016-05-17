using System.Runtime.InteropServices;
using Castle.Windsor;
using NUnit.Framework;
using Skuta.Testing.AutoMocking;

namespace Skuta.Testing
{
    public abstract class TestBase<T>
        where T : class
    {
        private IWindsorContainer _container;

        static TestBase()
        {
            //prevents TypeLoadException when mockin COM interface because of type equivalency
            //http://stackoverflow.com/questions/3444581/mocking-com-interfaces-using-rhino-mocks
            Castle.DynamicProxy.Generators.AttributesToAvoidReplicating.Add(typeof(TypeIdentifierAttribute));
        }

        protected T ClassUnderTest => _container.Resolve<T>();

        [SetUp]
        public void Setup()
        {
            _container = new AutoMockingContainer<T>();
        }

        [TearDown]
        public void TearDown()
        {
            _container.Dispose();
        }

        protected TMock GetMock<TMock>()
        {
            return _container.Resolve<TMock>();
        }
    }
}