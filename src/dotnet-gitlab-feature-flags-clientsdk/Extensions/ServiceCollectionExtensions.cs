namespace FeatureFlags.ClientSdk.Extensions
{
    using System.Diagnostics.CodeAnalysis;
  
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFeaturesClient(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration
                .GetSection(nameof(FeaturesClientSettings))
                .Get<FeaturesClientSettings>();

            services.AddSingleton(settings);
            services.AddMemoryCache();
            services.AddHttpClient<FeaturesClient>();
            services.AddScoped<FeaturesClient>();

            return services;
        }
    }
}
