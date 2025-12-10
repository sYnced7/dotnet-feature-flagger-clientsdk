namespace FeatureFlags.ClientSdk
{
    public sealed class GitLabUnleashResponse
    {
        public IEnumerable<GitLabFeature> Features { get; set; }
    }

    public sealed class GitLabFeature
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public IEnumerable<GitLabStrategy> Strategies { get; set; }
    }

    public sealed class GitLabStrategy
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
