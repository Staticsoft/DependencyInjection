using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Staticsoft.DependencyInjection.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void CanReuseDependency()
        {
            var factory = new ServiceCollection()
                .AddSingleton<ImplementationA>()
                .ReuseSingleton<InterfaceA, ImplementationA>()
                .BuildServiceProvider();
            var interfaceA = factory.GetRequiredService<InterfaceA>();
            var implementationA = factory.GetRequiredService<ImplementationA>();
            Assert.Equal(implementationA, interfaceA);
        }

        [Fact]
        public void CanDecorateDependency()
        {
            var factory = new ServiceCollection()
                .AddSingleton<InterfaceB, ImplementationB>()
                .DecorateSingleton<InterfaceB, DecoratedInterfaceB>()
                .BuildServiceProvider();
            var decorated = factory.GetRequiredService<InterfaceB>();
            Assert.Equal("DecoratedImplementation", decorated.Get());
        }

        [Fact]
        public void CanDecorateDependencyTwice()
        {
            var factory = new ServiceCollection()
                .AddSingleton<InterfaceB, ImplementationB>()
                .DecorateSingleton<InterfaceB, DecoratedInterfaceB>()
                .DecorateSingleton<InterfaceB, DecoratedInterfaceB>()
                .BuildServiceProvider();
            var decorated = factory.GetRequiredService<InterfaceB>();
            Assert.Equal("DecoratedDecoratedImplementation", decorated.Get());
        }

        [Fact]
        public void CanUseDecoratorWhichHasMultipleDependencies()
        {
            var factory = new ServiceCollection()
                .AddSingleton<InterfaceB, ImplementationB>()
                .AddSingleton<InterfaceC, ImplementationC>()
                .DecorateSingleton<InterfaceB, DecoratedInterfaceB>()
                .DecorateSingleton<InterfaceB, DoubleDecoratedInterfaceB>()
                .BuildServiceProvider();
            var decorated = factory.GetRequiredService<InterfaceB>();
            Assert.Equal("DecoratedImplementation42", decorated.Get());
        }
    }

    public interface InterfaceA { }

    public class ImplementationA : InterfaceA { }

    public interface InterfaceB
    {
        string Get();
    }

    public class ImplementationB : InterfaceB
    {
        public string Get()
            => "Implementation";
    }

    public class DecoratedInterfaceB : InterfaceB
    {
        readonly InterfaceB Dependency;

        public DecoratedInterfaceB(InterfaceB dependency)
            => Dependency = dependency;

        public string Get()
            => $"Decorated{Dependency.Get()}";
    }

    public interface InterfaceC
    {
        int Get();
    }

    public class ImplementationC : InterfaceC
    {
        public int Get()
            => 42;
    }

    public class DoubleDecoratedInterfaceB : InterfaceB
    {
        readonly InterfaceB DependencyB;
        readonly InterfaceC DependencyC;

        public DoubleDecoratedInterfaceB(InterfaceB dependencyB, InterfaceC dependencyC)
            => (DependencyB, DependencyC) = (dependencyB, dependencyC);

        public string Get()
            => $"{DependencyB.Get()}{DependencyC.Get()}";
    }
}
