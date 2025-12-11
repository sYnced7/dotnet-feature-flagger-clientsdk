namespace FeatureFlags.ClientSdk.Extensions
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFeaturesClient(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration
                .GetSection(nameof(FeaturesClientSettings))
                .Get<FeaturesClientSettings>();

            services.AddSingleton(settings);
            services.AddHttpClient<FeaturesClient>();
            services.AddScoped<FeaturesClient>();

            return services;
        }
    }
}
