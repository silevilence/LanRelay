using LanRelay.Core.Config;

namespace LanRelay.Core.Tests.Config;

/// <summary>
/// Tests for configuration persistence service.
/// </summary>
public class ConfigServiceTests : IDisposable
{
    private readonly string _testConfigDir;
    private readonly string _testConfigPath;

    public ConfigServiceTests()
    {
        _testConfigDir = Path.Combine(Path.GetTempPath(), $"LanRelayTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testConfigDir);
        _testConfigPath = Path.Combine(_testConfigDir, "lanrelay.config.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testConfigDir))
        {
            Directory.Delete(_testConfigDir, true);
        }
    }

    [Fact]
    public void AppConfig_ShouldHaveRequiredProperties()
    {
        // Arrange & Act
        var config = new AppConfig
        {
            UserNickName = "TestUser",
            DownloadPath = @"C:\Downloads",
            LastUsedNetworkInterface = "Ethernet",
            SecurityGroup = "MyGroup",
            Theme = "Dark"
        };

        // Assert
        Assert.Equal("TestUser", config.UserNickName);
        Assert.Equal(@"C:\Downloads", config.DownloadPath);
        Assert.Equal("Ethernet", config.LastUsedNetworkInterface);
        Assert.Equal("MyGroup", config.SecurityGroup);
        Assert.Equal("Dark", config.Theme);
    }

    [Fact]
    public void AppConfig_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var config = new AppConfig();

        // Assert
        Assert.NotEmpty(config.UserNickName); // Should have random default
        Assert.NotEmpty(config.DownloadPath); // Should have default path
        Assert.Equal("Public", config.SecurityGroup); // Default group
        Assert.Equal("System", config.Theme); // Default theme
    }

    [Fact]
    public async Task ConfigService_ShouldSaveConfig()
    {
        // Arrange
        var service = new ConfigService(_testConfigPath);
        var config = new AppConfig
        {
            UserNickName = "SaveTest",
            DownloadPath = @"D:\MyDownloads"
        };

        // Act
        await service.SaveAsync(config);

        // Assert
        Assert.True(File.Exists(_testConfigPath));
        var content = await File.ReadAllTextAsync(_testConfigPath);
        Assert.Contains("SaveTest", content);
    }

    [Fact]
    public async Task ConfigService_ShouldLoadConfig()
    {
        // Arrange
        var service = new ConfigService(_testConfigPath);
        var original = new AppConfig
        {
            UserNickName = "LoadTest",
            DownloadPath = @"E:\Files",
            SecurityGroup = "TeamA",
            Theme = "Light"
        };
        await service.SaveAsync(original);

        // Act
        var loaded = await service.LoadAsync();

        // Assert
        Assert.Equal("LoadTest", loaded.UserNickName);
        Assert.Equal(@"E:\Files", loaded.DownloadPath);
        Assert.Equal("TeamA", loaded.SecurityGroup);
        Assert.Equal("Light", loaded.Theme);
    }

    [Fact]
    public async Task ConfigService_ShouldReturnDefaultWhenFileNotExists()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_testConfigDir, "nonexistent.json");
        var service = new ConfigService(nonExistentPath);

        // Act
        var config = await service.LoadAsync();

        // Assert
        Assert.NotNull(config);
        Assert.NotEmpty(config.UserNickName);
        Assert.Equal("Public", config.SecurityGroup);
    }

    [Fact]
    public async Task ConfigService_ShouldHandleCorruptedFile()
    {
        // Arrange
        await File.WriteAllTextAsync(_testConfigPath, "{ invalid json }}}");
        var service = new ConfigService(_testConfigPath);

        // Act
        var config = await service.LoadAsync();

        // Assert - Should return default config instead of throwing
        Assert.NotNull(config);
        Assert.NotEmpty(config.UserNickName);
    }

    [Fact]
    public void ConfigService_ShouldGenerateRandomNickname()
    {
        // Arrange & Act
        var nick1 = ConfigService.GenerateRandomNickname();
        var nick2 = ConfigService.GenerateRandomNickname();

        // Assert
        Assert.NotEmpty(nick1);
        Assert.NotEmpty(nick2);
        // They might be the same by chance, but format should be correct
        Assert.Matches(@"^User-[A-Z0-9]{4}$", nick1);
    }

    [Fact]
    public void ConfigService_ShouldGetDefaultDownloadPath()
    {
        // Arrange & Act
        var path = ConfigService.GetDefaultDownloadPath();

        // Assert
        Assert.NotEmpty(path);
        Assert.Contains("LanRelay", path);
    }

    [Fact]
    public async Task ConfigService_ShouldCreateDirectoryIfNotExists()
    {
        // Arrange
        var deepPath = Path.Combine(_testConfigDir, "sub", "folder", "config.json");
        var service = new ConfigService(deepPath);
        var config = new AppConfig { UserNickName = "DeepTest" };

        // Act
        await service.SaveAsync(config);

        // Assert
        Assert.True(File.Exists(deepPath));
    }

    [Fact]
    public async Task ConfigService_ShouldPreserveUnknownProperties()
    {
        // Arrange - Simulate future config with extra properties
        var jsonWithExtra = """
            {
                "userNickName": "TestUser",
                "downloadPath": "C:\\Downloads",
                "securityGroup": "Public",
                "theme": "Dark",
                "futureProperty": "should be ignored"
            }
            """;
        await File.WriteAllTextAsync(_testConfigPath, jsonWithExtra);
        var service = new ConfigService(_testConfigPath);

        // Act
        var config = await service.LoadAsync();

        // Assert - Should load successfully despite extra property
        Assert.Equal("TestUser", config.UserNickName);
    }
}
