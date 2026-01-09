namespace FeatureFlags.ClientSdk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Text.Json;

    using FeatureFlags.ClientSdk.Evaluation;
    using FeatureFlags.ClientSdk.Evaluation.Models;
    
    using Microsoft.Extensions.Caching.Memory;
    
    public sealed class FeaturesClient
    {
        private readonly HttpClient httpClient;
        private readonly string apiUrl;
        private readonly IMemoryCache memoryCache;
        private readonly FeaturesClientSettings settings;

        public FeaturesClient(FeaturesClientSettings settings, 
          HttpClient httpClient,
          IMemoryCache memoryCache)
        {
            this.settings = settings;
            this.httpClient = httpClient;
            apiUrl = $"https://gitlab.com/api/v4/feature_flags/unleash/{settings.ProjectId}/client/features";

            httpClient.DefaultRequestHeaders.Add("UNLEASH-APPNAME", settings.AppName);
            httpClient.DefaultRequestHeaders.Add("UNLEASH-INSTANCEID", settings.InstanceId);
            this.memoryCache = memoryCache;
        }

        public async Task<bool> IsEnabledAsync(string toggleName, bool defaultValue, FlagContext context = null)
        {
            var features = await LoadFeaturesAsync();

            var toggle = features.FirstOrDefault(f => f.Name == toggleName);

            if (toggle == null)
            {
              return defaultValue;
            }

            return Evaluators.IsEnabled(toggle, context);
        }


        private async Task<IEnumerable<Feature>> LoadFeaturesAsync()
        {
          try
          {
            if (settings.UseMemoryCache)
            {
              memoryCache.TryGetValue("features_cache_key", out string toggles);

              if (!string.IsNullOrWhiteSpace(toggles))
              {
                return ParseFeatures(toggles);
              }
            }
            
            var json = await httpClient.GetStringAsync(apiUrl);
            
            if (settings.UseMemoryCache)
            {
              var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(settings.MemoryCacheDurationInSeconds));

              memoryCache.Set("features_cache_key", json, cacheEntryOptions);
            }

            return ParseFeatures(json);
          }
          catch (Exception e)
          {
            return Enumerable.Empty<Feature>();
          }
        }
        
        private IEnumerable<Feature> ParseFeatures(string json)
        {
          var options = new JsonSerializerOptions
          {
            PropertyNameCaseInsensitive = true
          };
          var gitlabResponse = JsonSerializer.Deserialize<GitLabUnleashResponse>(json, options);

          return gitlabResponse.Features.Select(f => new Feature
          {
            Name = f.Name,
            Enabled = f.Enabled,
            Strategies = f.Strategies.Select(s => new Strategy
            {
              Name = s.Name,
              Parameters = s.Parameters
            })
          });
        }
    }
}
