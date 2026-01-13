namespace FeatureFlags.ClientSdk
{
    public sealed class FeaturesClientSettings
    {
        public int ProjectId { get; set; }
        public string AppName { get; set; }
        public string InstanceId { get; set; }
        public bool UseMemoryCache { get; set; }
        public int MemoryCacheDurationInSeconds { get; set; } = 60;
    }
}
