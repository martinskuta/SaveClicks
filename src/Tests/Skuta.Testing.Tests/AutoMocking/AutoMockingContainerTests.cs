#region Usings

using NUnit.Framework;
using Rhino.Mocks;
using Skuta.Testing.AutoMocking;

#endregion

namespace Skuta.Testing.Tests.AutoMocking
{
    [TestFixture]
    public class AutoMockingContainerTests
    {
        private class ClassWithDependnecies
        {
            public ClassWithDependnecies(IAmCoolService coolService)
            {
                CoolService = coolService;
            }

            public IAmCoolService CoolService { get; }
        }

        public interface IAmCoolService
        {
            string DoSomethingCool();
        }

        [Test]
        public void Resolve_ClassWithNotRegisteredDependencies_ExpectDependenciesToBeAutoMocked()
        {
            //Arrange
            var container = new AutoMockingContainer<ClassWithDependnecies>();

            //Act
            var resolvedClass = container.Resolve<ClassWithDependnecies>();

            //Assert
            Assert.That(resolvedClass, Is.Not.Null);
            Assert.That(resolvedClass.CoolService, Is.Not.Null);
        }

        [Test]
        public void Resolve_StubDependencyInContainer_ExpectThatTheStubbedDependencyIsUsedWhenResolving()
        {
            //Arrange
            var container = new AutoMockingContainer<ClassWithDependnecies>();

            //Act
            var stub = container.Resolve<IAmCoolService>();
            stub.Stub(x => x.DoSomethingCool()).Return("Stubbed");

            var resolvedClass = container.Resolve<ClassWithDependnecies>();

            //Assert
            Assert.That(resolvedClass.CoolService, Is.Not.Null);
            Assert.That(resolvedClass.CoolService.DoSomethingCool(), Is.EqualTo("Stubbed"));
        }
    }
}