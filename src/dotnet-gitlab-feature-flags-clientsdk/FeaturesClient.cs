namespace FeatureFlags.ClientSdk
{
    using System.Text.Json;

    using FeatureFlags.ClientSdk.Evaluation;
    using FeatureFlags.ClientSdk.Evaluation.Models;
    
    public sealed class FeaturesClient
    {
        private readonly HttpClient httpClient;
        private readonly string apiUrl;

        public FeaturesClient(FeaturesClientSettings settings, HttpClient httpClient = null)
        {
            this.httpClient = httpClient ?? new HttpClient();
            apiUrl = $"https://gitlab.com/api/v4/feature_flags/unleash/{settings.ProjectId}/client/features";

            httpClient.DefaultRequestHeaders.Add("UNLEASH-APPNAME", settings.AppName);
            httpClient.DefaultRequestHeaders.Add("UNLEASH-INSTANCEID", settings.InstanceId);
        }

        public async Task<bool> IsEnabledAsync(string toggleName, FlagContext context = null)
        {
            var features = await LoadFeaturesAsync();

            var toggle = features.FirstOrDefault(f => f.Name == toggleName);

            return Evaluators.IsEnabled(toggle, context);
        }

        private async Task<IEnumerable<Feature>> LoadFeaturesAsync()
        {
            var json = await httpClient.GetStringAsync(apiUrl);

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
                }).ToList()
            }).AsEnumerable();
        }
    }
}
