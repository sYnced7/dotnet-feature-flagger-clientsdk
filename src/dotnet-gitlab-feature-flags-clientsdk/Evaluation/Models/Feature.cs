namespace FeatureFlags.ClientSdk.Evaluation.Models
{
    public sealed class Feature
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public IEnumerable<Strategy> Strategies { get; set; }
    }
}
