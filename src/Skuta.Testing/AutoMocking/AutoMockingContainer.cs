#region Usings

using System;
using System.Collections;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Rhino.Mocks;

#endregion

namespace Skuta.Testing.AutoMocking
{
    public class AutoMockingContainer<T> : WindsorContainer, ILazyComponentLoader
        where T : class
    {
        public AutoMockingContainer()
        {
            Register(Component.For<ILazyComponentLoader>().Instance(this).LifestyleSingleton());
            Register(Component.For<T>().ImplementedBy<T>().LifestyleSingleton());
        }

        public IRegistration Load(string name, Type service, IDictionary arguments)
        {
            return Component.For(service).Instance(MockRepository.GenerateMock(service, new[] {service})).LifestyleSingleton();
        }
    }
}