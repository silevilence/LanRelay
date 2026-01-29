using System.Text.Json;

namespace LanRelay.Core.Config;

/// <summary>
/// Service for persisting and loading application configuration.
/// </summary>
public class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _configPath;

    /// <summary>
    /// Creates a new ConfigService with the specified config file path.
    /// </summary>
    /// <param name="configPath">Path to the configuration file.</param>
    public ConfigService(string configPath)
    {
        _configPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
    }

    /// <summary>
    /// Creates a ConfigService using the default AppData location.
    /// </summary>
    public ConfigService() : this(GetDefaultConfigPath())
    {
    }

    /// <summary>
    /// Gets the default configuration file path.
    /// </summary>
    public static string GetDefaultConfigPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "LanRelay", "lanrelay.config.json");
    }

    /// <summary>
    /// Gets the default download path.
    /// </summary>
    public static string GetDefaultDownloadPath()
    {
        var downloads = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(downloads, "Downloads", "LanRelay");
    }

    /// <summary>
    /// Generates a random nickname for first-time users.
    /// </summary>
    public static string GenerateRandomNickname()
    {
        var suffix = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        return $"User-{suffix}";
    }

    /// <summary>
    /// Loads configuration from file, or returns default if not found.
    /// </summary>
    public async Task<AppConfig> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return new AppConfig();
            }

            var json = await File.ReadAllTextAsync(_configPath, cancellationToken);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);

            return config ?? new AppConfig();
        }
        catch (JsonException)
        {
            // Return default config if file is corrupted
            return new AppConfig();
        }
        catch (IOException)
        {
            return new AppConfig();
        }
    }

    /// <summary>
    /// Saves configuration to file.
    /// </summary>
    public async Task SaveAsync(AppConfig config, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(config);

        // Ensure directory exists
        var directory = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(_configPath, json, cancellationToken);
    }

    /// <summary>
    /// Loads configuration synchronously.
    /// </summary>
    public AppConfig Load()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return new AppConfig();
            }

            var json = File.ReadAllText(_configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);

            return config ?? new AppConfig();
        }
        catch
        {
            return new AppConfig();
        }
    }

    /// <summary>
    /// Saves configuration synchronously.
    /// </summary>
    public void Save(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var directory = Path.GetDirectoryName(_configPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(_configPath, json);
    }
}
