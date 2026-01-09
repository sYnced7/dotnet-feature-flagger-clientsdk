namespace FeatureFlagger.ClientSdk.Tests;

using AutoFixture;

using FeatureFlags.ClientSdk;

using Microsoft.Extensions.Caching.Memory;

using RichardSzalay.MockHttp;

using Xunit;

public class FeaturesClientTests
{
  private readonly static Fixture fixture = new Fixture();
  private readonly MockHttpMessageHandler handler;
  private readonly FeaturesClientSettings featuresClientSettings;
  private readonly IMemoryCache memoryCache;


  public FeaturesClientTests()
  {
    featuresClientSettings = fixture.Freeze<FeaturesClientSettings>();
    featuresClientSettings.UseMemoryCache = false; // Default to no caching for most tests
    handler = new MockHttpMessageHandler();
    memoryCache = new MemoryCache(new MemoryCacheOptions());
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

    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

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
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

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
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);
    
    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);
    
    // Assert
    Assert.Equal(defaultValue, result);
    }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnDefaultValue_ForNonExistingFeature()
  {
    // Arrange
    var toggleName = "non-existing-feature";
    var defaultValue = true;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""some-other-feature"",
          ""enabled"": false,
          ""strategies"": []
        }
      ]
    }";

    handler.Expect("*")
      .Respond("application/json", jsonResponse);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.Equal(defaultValue, result);
  }
  
  [Fact]
  public async Task IsEnabledAsync_ShouldReturnDefaultValue_WhenApiResponseIsEmptyJson()
  {
    // Arrange
    var toggleName = "any-toggle";
    var defaultValue = true;

    handler.Expect("*")
      .Respond("application/json", "{}");

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.Equal(defaultValue, result);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnDefaultValue_WhenJsonDeserializationFails()
  {
    // Arrange
    var toggleName = "any-toggle";
    var defaultValue = false;

    var malformedJson = "{ invalid json }";

    handler.Expect("*")
      .Respond("application/json", malformedJson);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.Equal(defaultValue, result);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldUseCachedData_WhenCacheIsEnabled()
  {
    // Arrange
    var toggleName = "cached-feature";
    var defaultValue = false;
    
    featuresClientSettings.UseMemoryCache = true;
    featuresClientSettings.MemoryCacheDurationInSeconds = 60;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""cached-feature"",
          ""enabled"": true,
          ""strategies"": []
        }
      ]
    }";

    var requestCount = 0;
    handler.When("*")
      .Respond(_ =>
      {
        requestCount++;
        return new HttpResponseMessage
        {
          StatusCode = System.Net.HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };
      });

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var firstResult = await client.IsEnabledAsync(toggleName, defaultValue);
    var secondResult = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.True(firstResult);
    Assert.True(secondResult);
    Assert.Equal(1, requestCount);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldNotCache_WhenCacheIsDisabled()
  {
    // Arrange
    var toggleName = "non-cached-feature";
    var defaultValue = false;
    
    featuresClientSettings.UseMemoryCache = false;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""non-cached-feature"",
          ""enabled"": true,
          ""strategies"": []
        }
      ]
    }";

    var requestCount = 0;
    handler.When("*")
      .Respond(_ =>
      {
        requestCount++;
        return new HttpResponseMessage
        {
          StatusCode = System.Net.HttpStatusCode.OK,
          Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
        };
      });

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var firstResult = await client.IsEnabledAsync(toggleName, defaultValue);
    var secondResult = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.True(firstResult);
    Assert.True(secondResult);
    Assert.Equal(2, requestCount);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnFalse_WhenFeatureIsDisabled()
  {
    // Arrange
    var toggleName = "disabled-feature";
    var defaultValue = true;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""disabled-feature"",
          ""enabled"": false,
          ""strategies"": []
        }
      ]
    }";

    handler.Expect("*")
      .Respond("application/json", jsonResponse);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.False(result);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnTrue_WhenFeatureIsEnabledWithNoStrategies()
  {
    // Arrange
    var toggleName = "feature-no-strategies";
    var defaultValue = false;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""feature-no-strategies"",
          ""enabled"": true,
          ""strategies"": []
        }
      ]
    }";

    handler.Expect("*")
      .Respond("application/json", jsonResponse);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.True(result);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldReturnTrue_WhenFeatureHasDefaultStrategy()
  {
    // Arrange
    var toggleName = "feature-with-default";
    var defaultValue = false;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""feature-with-default"",
          ""enabled"": true,
          ""strategies"": [
            {
              ""name"": ""default"",
              ""parameters"": {}
            }
          ]
        }
      ]
    }";

    handler.Expect("*")
      .Respond("application/json", jsonResponse);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.True(result);
  }


  [Fact]
  public async Task IsEnabledAsync_ShouldHandleMultipleFeatures_AndReturnCorrectValue()
  {
    // Arrange
    var toggleName = "feature-two";
    var defaultValue = false;

    var jsonResponse = @"
    {
      ""features"": [
        {
          ""name"": ""feature-one"",
          ""enabled"": false,
          ""strategies"": []
        },
        {
          ""name"": ""feature-two"",
          ""enabled"": true,
          ""strategies"": []
        },
        {
          ""name"": ""feature-three"",
          ""enabled"": true,
          ""strategies"": []
        }
      ]
    }";

    handler.Expect("*")
      .Respond("application/json", jsonResponse);

    var httpClient = handler.ToHttpClient();
    var client = new FeaturesClient(featuresClientSettings, httpClient, memoryCache);

    // Act
    var result = await client.IsEnabledAsync(toggleName, defaultValue);

    // Assert
    Assert.True(result);
  }
}

