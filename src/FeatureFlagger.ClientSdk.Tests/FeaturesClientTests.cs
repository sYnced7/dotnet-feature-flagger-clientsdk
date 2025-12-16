using AutoFixture;

using RichardSzalay.MockHttp;

using Xunit;

namespace FeatureFlagger.ClientSdk.Tests;

using FeatureFlags.ClientSdk;

public class FeaturesClientTests
{
  private readonly static Fixture fixture = new Fixture();
  private readonly MockHttpMessageHandler handler;
  private readonly FeaturesClientSettings featuresClientSettings;


  public FeaturesClientTests()
  {
    featuresClientSettings = fixture.Freeze<FeaturesClientSettings>();
    handler = new MockHttpMessageHandler();
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnDefaultValue_ForNonExistingToggle()
  {
    // Arrange
    var toggleName = "non-existing-toggle";
    var defaultValue = false;

    handler.Expect("*")
      .Respond("application/json", string.Empty);

    var httpClient = handler.ToHttpClient();

    var client = new FeaturesClient(featuresClientSettings, httpClient);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.Equal(defaultValue, result);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnToggleValue_ForExistingToggle()
  {
    // Arrange
    var toggleName = "existing-toggle";
    var defaultValue = false;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""existing-toggle"",
          ""enabled"": true,
          ""strategies"": []
        }
      ]
    }";

    handler.Expect("*")
      .Respond("application/json", jsonResponse);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.True(result);
  }
  
  [Fact]
  public async Task IsEnabledAsync_ShouldReturnDefaultValue_OnHttpRequestException()
  {
    // Arrange
    var toggleName = "any-toggle";
    var defaultValue = false;
    
    handler.Expect("*")
      .Throw(new HttpRequestException("Network error"));
    
    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient);
    
    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);
    
    // Assert
    Assert.Equal(defaultValue, result);
    }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnDefaultValue_ForNonExistingFeature()
  {
    
  }
    
    
}