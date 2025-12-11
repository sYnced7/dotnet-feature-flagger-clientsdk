namespace FeatureFlags.ClientSdk.Evaluation.Models
{
    using System.Collections.Generic;
    
    public sealed class Feature
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public IEnumerable<Strategy> Strategies { get; set; }
    }
}
