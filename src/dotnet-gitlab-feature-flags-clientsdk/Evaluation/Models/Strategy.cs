namespace FeatureFlags.ClientSdk.Evaluation.Models
{
    using System.Collections.Generic;
    
    public sealed class Strategy
    {
        public string Name { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}
