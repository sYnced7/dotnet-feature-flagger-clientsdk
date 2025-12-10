namespace FeatureFlags.ClientSdk.Evaluation.Models
{
    public sealed class Strategy
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
