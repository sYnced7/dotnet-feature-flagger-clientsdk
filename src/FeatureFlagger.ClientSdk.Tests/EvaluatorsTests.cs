namespace FeatureFlagger.ClientSdk.Tests;

using AutoFixture;

using FeatureFlags.ClientSdk.Evaluation;
using FeatureFlags.ClientSdk.Evaluation.Models;

using Xunit;

public class EvaluatorsTests
{
  private readonly static Fixture fixture = new Fixture();
  
  [Fact]
  public void IsEnabled_WithDefaultStrategy_ShouldReturnTrue()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies, new Strategy[] { new Strategy() { Name = "default" } })
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, new FlagContext());

    // Assert
    Assert.True(result);
  }
  
  [Fact]
  public void IsEnabled_WithDisabledFeature_ShouldReturnFalse()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, false)
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, new FlagContext());

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void IsEnabled_WithNoStrategies_ShouldReturnFeatureEnabledState()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies, Array.Empty<Strategy>())
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, new FlagContext());

    // Assert
    Assert.True(result);
  }
  
  [Fact]
  public void IsEnabled_WithUnknownStrategy_ShouldReturnFalse()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies, new Strategy[] { new Strategy() { Name = "unknown-strategy" } })
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, new FlagContext());

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void IsEnabled_WithNullContext_ShouldReturnFalseForStrategiesRequiringContext()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "flexibleRollout",
            Parameters = new Dictionary<string, string>
              { { "rolloutPercentage", "50" }, { "stickiness", "default" } }
          }
        })
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, null);

    // Assert
    Assert.False(result);
  }


  [Fact]
  public void IsEnabled_WithFlexibleRolloutStrategy_ShouldReturnBoolean()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "flexibleRollout",
            Parameters = new Dictionary<string, string>
              { { "rollout", "100" }, { "stickiness", "default" }, { "groupId", "test-group" } }
          }
        })
      .Create();

    var context = new FlagContext { UserId = "test-user" };

    // Act
    var result = Evaluators.IsEnabled(feature, context);

    // Assert
    Assert.True(result);
  }


  [Fact]
  public void IsEnabled_WithFlexibleRolloutStrategyBelowThreshold_ShouldReturnFalse()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "flexibleRollout",
            Parameters = new Dictionary<string, string>
              { { "rollout", "0" }, { "stickiness", "default" }, { "groupId", "test-group" } }
          }
        })
      .Create();

    var context = new FlagContext { UserId = "test-user" };

    // Act
    var result = Evaluators.IsEnabled(feature, context);

    // Assert
    Assert.False(result);
  }
  
  [Fact]
  public void IsEnabled_WithMultipleStrategies_ShouldReturnTrueIfAnyStrategyEnablesFeature()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "unknown-strategy"
          },
          new Strategy()
          {
            Name = "default"
          }
        })
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, new FlagContext());

    // Assert
    Assert.True(result);
  }


  [Fact]
  public void IsEnabled_WithMultipleStrategies_ShouldReturnFalseIfNoStrategyEnablesFeature()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "unknown-strategy"
          },
          new Strategy()
          {
            Name = "flexibleRollout",
            Parameters = new Dictionary<string, string>
              { { "rollout", "0" }, { "stickiness", "default" }, { "groupId", "test-group" } }
          }
        })
      .Create();

    var context = new FlagContext { UserId = "test-user" };

    // Act
    var result = Evaluators.IsEnabled(feature, context);

    // Assert
    Assert.False(result);
  }

  [Fact]
  public void IsEnabled_WithStrategyUserWithId_ShouldReturnTrue()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "userWithId",
            Parameters = new Dictionary<string, string>
              { { "userIds", "user1,user2,user3" } }
          }
        })
      .Create();

    var context = new FlagContext { UserId = "user2" };

    // Act
    var result = Evaluators.IsEnabled(feature, context);

    // Assert
    Assert.True(result);
  }
  
  [Fact]
  public void IsEnabled_WithStrategyUserWithId_ShouldReturnFalse()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "userWithId",
            Parameters = new Dictionary<string, string>
              { { "userIds", "user1,user2,user3" } }
          }
        })
      .Create();
    var context = new FlagContext { UserId = "user4" };
    
    // Act
    var result = Evaluators.IsEnabled(feature, context);
    
    // Assert
    Assert.False(result);
  }
  
  [Fact]
  public void IsEnabled_WithStrategyUserWithIdAndNullContext_ShouldReturnFalse()
  {
    // Arrange
    var feature = fixture
      .Build<Feature>()
      .With(x => x.Enabled, true)
      .With(x => x.Strategies,
        new Strategy[]
        {
          new Strategy()
          {
            Name = "userWithId",
            Parameters = new Dictionary<string, string>
              { { "userIds", "user1,user2,user3" } }
          }
        })
      .Create();

    // Act
    var result = Evaluators.IsEnabled(feature, null);
    
    // Assert
    Assert.False(result);
  }
}