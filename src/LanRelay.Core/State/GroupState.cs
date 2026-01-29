using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace LanRelay.Core.State;

/// <summary>
/// State container for managing groups and authentication.
/// Implements the State Container Pattern with C# events for UI notification.
/// </summary>
public class GroupState
{
    /// <summary>
    /// Default public group ID.
    /// </summary>
    public const string DefaultGroupId = "Public";

    private readonly ConcurrentDictionary<string, GroupInfo> _groups = new();

    /// <summary>
    /// Event raised when the current group changes.
    /// </summary>
    public event Action<string>? OnCurrentGroupChanged;

    /// <summary>
    /// Event raised when a new group is created.
    /// </summary>
    public event Action<GroupInfo>? OnGroupCreated;

    /// <summary>
    /// Gets the current group ID.
    /// </summary>
    public string CurrentGroupId { get; private set; } = DefaultGroupId;

    /// <summary>
    /// Gets all known groups.
    /// </summary>
    public IReadOnlyList<GroupInfo> Groups => _groups.Values.ToList();

    /// <summary>
    /// Creates a new GroupState with the default public group.
    /// </summary>
    public GroupState()
    {
        // Create default public group
        _groups[DefaultGroupId] = new GroupInfo
        {
            GroupId = DefaultGroupId,
            DisplayName = "Public",
            PasswordHash = null
        };
    }

    /// <summary>
    /// Creates a new group.
    /// </summary>
    /// <param name="groupId">The group identifier.</param>
    /// <param name="password">Optional password for private groups.</param>
    /// <returns>The created group info.</returns>
    public GroupInfo CreateGroup(string groupId, string? password = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupId);

        var group = new GroupInfo
        {
            GroupId = groupId,
            DisplayName = groupId,
            PasswordHash = password is not null ? HashPassword(password) : null
        };

        _groups[groupId] = group;
        OnGroupCreated?.Invoke(group);

        return group;
    }

    /// <summary>
    /// Validates a password for a group.
    /// </summary>
    /// <param name="groupId">The group to validate.</param>
    /// <param name="password">The password to check (null for public groups).</param>
    /// <returns>True if the password is valid or the group is public.</returns>
    public bool ValidatePassword(string groupId, string? password)
    {
        if (!_groups.TryGetValue(groupId, out var group))
        {
            return false; // Group doesn't exist
        }

        if (group.IsPublic)
        {
            return true; // Public groups accept anyone
        }

        if (password is null)
        {
            return false; // Private group requires password
        }

        var hash = HashPassword(password);
        return string.Equals(hash, group.PasswordHash, StringComparison.Ordinal);
    }

    /// <summary>
    /// Joins a group after validating the password.
    /// </summary>
    /// <param name="groupId">The group to join.</param>
    /// <param name="password">The password (null for public groups).</param>
    /// <returns>True if join was successful.</returns>
    public bool JoinGroup(string groupId, string? password = null)
    {
        if (!ValidatePassword(groupId, password))
        {
            return false;
        }

        if (CurrentGroupId != groupId)
        {
            CurrentGroupId = groupId;
            OnCurrentGroupChanged?.Invoke(groupId);
        }

        return true;
    }

    /// <summary>
    /// Gets a group by ID.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <returns>The group info, or null if not found.</returns>
    public GroupInfo? GetGroup(string groupId)
    {
        return _groups.TryGetValue(groupId, out var group) ? group : null;
    }

    /// <summary>
    /// Checks if a group exists.
    /// </summary>
    /// <param name="groupId">The group ID.</param>
    /// <returns>True if the group exists.</returns>
    public bool GroupExists(string groupId)
    {
        return _groups.ContainsKey(groupId);
    }

    /// <summary>
    /// Hashes a password using SHA256.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>The SHA256 hash as a hex string.</returns>
    public static string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
