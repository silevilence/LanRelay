namespace LanRelay.Core.State;

/// <summary>
/// Information about a group.
/// </summary>
public record GroupInfo
{
    /// <summary>
    /// Unique identifier for the group.
    /// </summary>
    public required string GroupId { get; init; }

    /// <summary>
    /// SHA256 hash of the group password, or null for public groups.
    /// </summary>
    public string? PasswordHash { get; init; }

    /// <summary>
    /// Indicates whether this is a public group (no password required).
    /// </summary>
    public bool IsPublic => PasswordHash is null;

    /// <summary>
    /// Display name of the group.
    /// </summary>
    public string DisplayName { get; init; } = "";

    /// <summary>
    /// When the group was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
