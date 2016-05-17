#region Usings

using NUnit.Framework;
using Rhino.Mocks;
using stdole;

#endregion

namespace Skuta.Testing.Tests
{
    [TestFixture]
    public class TestBaseTests : TestBase<TestBaseTests.SuperNotImportantClass>
    {
        public class SuperNotImportantClass
        {
            public SuperNotImportantClass(IShouldBeMocked someDependency, IUnknown comDependency)
            {
                Dependency = someDependency;
                ComDependency = comDependency;
            }

            public IShouldBeMocked Dependency { get; }

            public IUnknown ComDependency { get; }
        }

        public interface IShouldBeMocked
        {
            string StubMe();
        }

        [Test]
        public void ClassUnderTest_DependencyIsStubbed_TheStubIsUsedWhenResolvingClassUnderTest()
        {
            //Arrange
            //Act
            var stub = GetMock<IShouldBeMocked>();
            stub.Stub(x => x.StubMe()).Return("stubbed");

            var classUnderTest = ClassUnderTest;

            //Assert
            Assert.That(classUnderTest, Is.Not.Null);
            Assert.That(classUnderTest.Dependency, Is.Not.Null);
            Assert.That(classUnderTest.Dependency.StubMe(), Is.EqualTo("stubbed"));
        }

        [Test]
        public void ClassUnderTest_GetterCalled_ClassIsResolvedAndDependciesAreMocked()
        {
            //Arrange
            //Act
            var classUnderTest = ClassUnderTest;

            //Assert
            Assert.That(classUnderTest, Is.Not.Null);
            Assert.That(classUnderTest.Dependency, Is.Not.Null);
        }

        [Test]
        public void GetMock_CalledTwice_ExpectThatSameInstanceIsReturned()
        {
            //Arrange
            //Act
            var stub1 = GetMock<IShouldBeMocked>();
            stub1.Stub(x => x.StubMe()).Return("stub1");

            var stub2 = GetMock<IShouldBeMocked>();

            //Assert
            Assert.That(stub2.StubMe(), Is.EqualTo("stub1"));
        }

        [Test]
        public void GetMock_MockingCOMInterface_ExpectThatSameInstanceIsUsedToResolveClassUnderTest()
        {
            //Arrange
            //Act
            var comMock = GetMock<IUnknown>();

            var classUnderTest = ClassUnderTest;

            //Assert
            Assert.That(classUnderTest.ComDependency, Is.EqualTo(comMock));
        }
    }
}