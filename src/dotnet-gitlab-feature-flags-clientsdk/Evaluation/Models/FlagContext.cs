namespace FeatureFlags.ClientSdk.Evaluation.Models
{
    using System.Collections.Generic;
    
    public sealed class FlagContext
    {
        public string UserId { get; set; }
        public string SessionId { get; set; }
        public string RemoteAddress { get; set; }
        public string Environment { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }
    }
}
