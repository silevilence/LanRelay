using System.Text.Json.Serialization;

namespace LanRelay.Core.Config;

/// <summary>
/// Application configuration model.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// User's display nickname.
    /// </summary>
    [JsonPropertyName("userNickName")]
    public string UserNickName { get; set; } = ConfigService.GenerateRandomNickname();

    /// <summary>
    /// Default download path for received files.
    /// </summary>
    [JsonPropertyName("downloadPath")]
    public string DownloadPath { get; set; } = ConfigService.GetDefaultDownloadPath();

    /// <summary>
    /// Last used network interface name.
    /// </summary>
    [JsonPropertyName("lastUsedNetworkInterface")]
    public string? LastUsedNetworkInterface { get; set; }

    /// <summary>
    /// Current security group (default: Public).
    /// </summary>
    [JsonPropertyName("securityGroup")]
    public string SecurityGroup { get; set; } = "Public";

    /// <summary>
    /// UI theme (System, Light, Dark).
    /// </summary>
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Unique device ID (generated on first run).
    /// </summary>
    [JsonPropertyName("deviceId")]
    public Guid DeviceId { get; set; } = Guid.NewGuid();
}
