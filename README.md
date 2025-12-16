# Feature Flags Client for .NET

A simple, open-source alternative client for **GitLab feature flags** in .NET applications.  

This library allows you to easily retrieve and work with GitLab feature flags in your .NET projects, without relying on the official Unleash client.

---

## ⚡ Features

- Fetch and deserialize GitLab feature flags
- Support for multiple strategies and parameters
- Lightweight and easy to integrate
- Fully open-source and customizable for your needs

---

## 🚀 Why This Exists

The official **Unleash client** can be difficult to work with in .NET environments, and sometimes doesn’t integrate smoothly with GitLab’s feature flag system.  

This project is a **community-driven alternative** to simplify working with feature flags in .NET applications.

> **Note:** I do **not** work for GitLab and this project is not affiliated with GitLab. It is a standalone, open-source alternative built for convenience and flexibility.

---

## 💡 Usage Example

Without context:
```csharp
var featureClientSettings = new FeaturesClientSettings()
{
    AppName = "environent",
    ProjectId = 1234455653,
    InstanceId = "glffct--instanceId",
};
var httpClient = new HttpClient();
var featureClient = new FeaturesClient(featureClientSettings, httpClient);
var defaultValue = true;

var result = await featureClient.IsEnabledAsync("test-flag", defaultValue);
```

With context:
```csharp
var featureClientSettings = new FeaturesClientSettings()
{
    AppName = "environent",
    ProjectId = 1234455653,
    InstanceId = "glffct--instanceId",
};
var httpClient = new HttpClient();
var featureClient = new FeaturesClient(featureClientSettings, httpClient);
var defaultValue = false;

var result = await featureClient.IsEnabledAsync("test-flag",  defaultValue, new FlagContext()
{
    UserId = "Some UserId"
});
```

The context can be enriched with additional properties as needed and as gitlab limitations.

## 📝 License

This project is open-source under the [Apache 2.0](https://github.com/sYnced7/dotnet-gitlab-feature-flags-clientsdk/blob/main/LICENSE). Feel free to use, modify, and contribute!

## 🙌 Contributing

Contributions, feedback, and suggestions are welcome! Whether it’s bug fixes, new features, or improving documentation, your help is appreciated.

## ⚠️ Disclaimer

This is not an official GitLab product, and the author is not affiliated with GitLab in any way. It’s purely a community project for easier .NET integration with GitLab feature flags.
