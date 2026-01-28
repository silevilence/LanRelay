using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanRelay.Core.FileTransfer;

/// <summary>
/// Request to initiate a file transfer.
/// </summary>
public record FileTransferRequest
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Unique identifier for this transfer session.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid TransferId { get; init; }

    /// <summary>
    /// Name of the file being transferred.
    /// </summary>
    [JsonPropertyName("name")]
    public required string FileName { get; init; }

    /// <summary>
    /// Size of the file in bytes.
    /// </summary>
    [JsonPropertyName("size")]
    public required long FileSize { get; init; }

    /// <summary>
    /// MD5 hash of the file for verification.
    /// </summary>
    [JsonPropertyName("md5")]
    public required string Md5Hash { get; init; }

    public string Serialize() => JsonSerializer.Serialize(this, JsonOptions);

    public static FileTransferRequest? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FileTransferRequest>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
