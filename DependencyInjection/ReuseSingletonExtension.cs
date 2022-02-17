namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReuseSingletonExtension
    {
        public static IServiceCollection ReuseSingleton<TInterface, TImplementation>(this IServiceCollection services)
            where TInterface : class
            where TImplementation : class, TInterface
            => services.AddSingleton<TInterface>(p => p.GetRequiredService<TImplementation>());
    }
}
