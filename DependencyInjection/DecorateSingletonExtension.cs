using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DecorateSingletonExtension
    {
        public static IServiceCollection DecorateSingleton<TInterface, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TDecorator : class, TInterface
            => services.Replace(CreateDecoratedServiceDescriptor<TInterface, TDecorator>(services));

        static ServiceDescriptor CreateDecoratedServiceDescriptor<TInterface, TDecorator>(IServiceCollection services)
            where TInterface : class
            where TDecorator : class, TInterface
            => CreateDecoratedServiceDescriptor<TInterface, TDecorator>(GetServiceDescriptor<TInterface>(services));

        static ServiceDescriptor GetServiceDescriptor<TInterface>(IServiceCollection services) where TInterface : class
        {
            try
            {
                return services.Single(s => s.ServiceType == typeof(TInterface));
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"{typeof(TInterface).Name} is not registered");
            }
        }

        static ServiceDescriptor CreateDecoratedServiceDescriptor<TInterface, TDecorator>(ServiceDescriptor undecorated)
            where TInterface : class
            where TDecorator : class, TInterface
            => ServiceDescriptor.Describe(typeof(TInterface), provider => CreateInstance<TInterface, TDecorator>(provider, undecorated), undecorated.Lifetime);

        static TInterface CreateInstance<TInterface, TDecorator>(IServiceProvider provider, ServiceDescriptor undecorated)
            where TInterface : class
            where TDecorator : class, TInterface
            => (TInterface)CreateObjectFactory<TInterface, TDecorator>()(provider, new[] { CreateInstance(provider, undecorated) });

        static ObjectFactory CreateObjectFactory<TInterface, TDecorator>()
            where TInterface : class
            where TDecorator : class, TInterface
            => ActivatorUtilities.CreateFactory(typeof(TDecorator), new[] { typeof(TInterface) });

        static object CreateInstance(IServiceProvider services, ServiceDescriptor descriptor) => descriptor switch
        {
            _ when descriptor.ImplementationInstance != null => descriptor.ImplementationInstance,
            _ when descriptor.ImplementationFactory != null => descriptor.ImplementationFactory(services),
            _ => ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType)
        };
    }
}
