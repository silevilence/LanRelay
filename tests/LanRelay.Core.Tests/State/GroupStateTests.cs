using LanRelay.Core.State;

namespace LanRelay.Core.Tests.State;

/// <summary>
/// Tests for group management and authentication.
/// </summary>
public class GroupStateTests
{
    [Fact]
    public void GroupState_ShouldCreateGroupWithPassword()
    {
        // Arrange
        var state = new GroupState();

        // Act
        var group = state.CreateGroup("MyTeam", "secret123");

        // Assert
        Assert.NotNull(group);
        Assert.Equal("MyTeam", group.GroupId);
        Assert.NotNull(group.PasswordHash);
        Assert.NotEmpty(group.PasswordHash);
        Assert.NotEqual("secret123", group.PasswordHash); // Should be hashed
    }

    [Fact]
    public void GroupState_ShouldCreatePublicGroupWithoutPassword()
    {
        // Arrange
        var state = new GroupState();

        // Act
        var group = state.CreateGroup("PublicRoom");

        // Assert
        Assert.NotNull(group);
        Assert.Equal("PublicRoom", group.GroupId);
        Assert.True(group.IsPublic);
        Assert.Null(group.PasswordHash);
    }

    [Fact]
    public void GroupState_ShouldValidateCorrectPassword()
    {
        // Arrange
        var state = new GroupState();
        var group = state.CreateGroup("SecureRoom", "myPassword");

        // Act
        var isValid = state.ValidatePassword("SecureRoom", "myPassword");

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void GroupState_ShouldRejectIncorrectPassword()
    {
        // Arrange
        var state = new GroupState();
        state.CreateGroup("SecureRoom", "myPassword");

        // Act
        var isValid = state.ValidatePassword("SecureRoom", "wrongPassword");

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void GroupState_ShouldAllowJoiningPublicGroup()
    {
        // Arrange
        var state = new GroupState();
        state.CreateGroup("PublicRoom");

        // Act
        var isValid = state.ValidatePassword("PublicRoom", null);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void GroupState_ShouldTrackCurrentGroup()
    {
        // Arrange
        var state = new GroupState();
        state.CreateGroup("TeamA", "pass");

        // Act
        state.JoinGroup("TeamA", "pass");

        // Assert
        Assert.Equal("TeamA", state.CurrentGroupId);
    }

    [Fact]
    public void GroupState_ShouldRaiseEventOnGroupChange()
    {
        // Arrange
        var state = new GroupState();
        state.CreateGroup("TeamA");
        string? receivedGroupId = null;
        state.OnCurrentGroupChanged += id => receivedGroupId = id;

        // Act
        state.JoinGroup("TeamA");

        // Assert
        Assert.Equal("TeamA", receivedGroupId);
    }

    [Fact]
    public void GroupState_ShouldUseDefaultPublicGroup()
    {
        // Arrange & Act
        var state = new GroupState();

        // Assert
        Assert.Equal(GroupState.DefaultGroupId, state.CurrentGroupId);
    }

    [Fact]
    public void GroupState_ShouldHashPasswordConsistently()
    {
        // Arrange
        var state = new GroupState();

        // Act
        var hash1 = GroupState.HashPassword("test123");
        var hash2 = GroupState.HashPassword("test123");

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GroupState_ShouldProduceDifferentHashesForDifferentPasswords()
    {
        // Arrange & Act
        var hash1 = GroupState.HashPassword("password1");
        var hash2 = GroupState.HashPassword("password2");

        // Assert
        Assert.NotEqual(hash1, hash2);
    }
}
