using System.Text.Json;
using System.Text.Json.Serialization;

namespace LanRelay.Core.FileTransfer;

/// <summary>
/// Response to a file transfer request.
/// </summary>
public record FileTransferResponse
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// The transfer ID this response is for.
    /// </summary>
    [JsonPropertyName("id")]
    public required Guid TransferId { get; init; }

    /// <summary>
    /// Whether the transfer was accepted.
    /// </summary>
    [JsonPropertyName("accepted")]
    public required bool Accepted { get; init; }

    /// <summary>
    /// The path where the file will be saved (if accepted).
    /// </summary>
    [JsonPropertyName("savePath")]
    public string? SavePath { get; init; }

    /// <summary>
    /// Reason for rejection (if not accepted).
    /// </summary>
    [JsonPropertyName("reason")]
    public string? RejectReason { get; init; }

    public string Serialize() => JsonSerializer.Serialize(this, JsonOptions);

    public static FileTransferResponse? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<FileTransferResponse>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
