namespace FeatureFlags.ClientSdk.Evaluation
{
    using System.Linq;
    
    using FeatureFlags.ClientSdk.Evaluation.Models;

    internal sealed class Evaluators
    {
      public static bool IsEnabled(Feature feature, FlagContext context)
      {
            if (!feature.Enabled)
            {
              return false;
            }

            if (!feature.Strategies.Any())
            {
              return feature.Enabled;
            }
                
            foreach (var strategy in feature.Strategies)
            {
              if (EvaluateStrategy(strategy, feature, context))
              {
                return true;
              }
            }

            return false;
        }
        private static int GetNormalizedNumber(string identifier, int groupId)
        {
            var hash = MurmurHash.MurmurHash3(identifier + groupId);
            return (int)(hash % 100) + 1;
        }

        private static bool EvaluateFlexibleRollout(
            string featureName,
            FlagContext context,
            int rolloutPercentage,
            int stickiness,
            string groupId)
        {
            if (context is null)
            {
                return false;
            }

            var seed = string.Empty;

            switch (stickiness)
            {
                case 1: seed = context.UserId; 
                    break;
                case 2: seed = context.SessionId; 
                    break;
                case 3: seed = context.RemoteAddress; 
                    break;
                default:
                    seed = context.UserId ?? context.SessionId ?? context.RemoteAddress;
                    break;
            }

            if (string.IsNullOrWhiteSpace(seed))
            {
                return false;
            }
                
            string id = $"{featureName}:{seed}";
            int bucket = GetNormalizedNumber(id, groupId.GetHashCode());

            return bucket <= rolloutPercentage;
        }

        private static bool EvaluateStrategy(Strategy strategy, Feature feature, FlagContext ctx)
        {
            switch (strategy.Name)
            {
                case "default":
                    return true;

                case "flexibleRollout":
                    int percentage = int.Parse(strategy.Parameters["rollout"]);
                    string groupId = strategy.Parameters["groupId"];
                    int stickiness = 0;
                    return EvaluateFlexibleRollout(feature.Name, ctx, percentage, stickiness, groupId);

                case "userWithId":
                    string[] allowed = strategy.Parameters["userIds"]
                        .Split(',')
                        .Select(s => s.Trim())
                        .ToArray();

                    return allowed.Contains(ctx?.UserId ?? string.Empty);

                default:
                    return false;
            }
        }
    }
}
