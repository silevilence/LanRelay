# 配置持久化与用户偏好

优先级：P2

## 任务细节

1. **配置服务 (`ConfigService`)**：
    - 定义配置模型：`UserNickName`, `DownloadPath`, `LastUsedNetworkInterface`, `SecurityGroup`, `Theme`。
    - 使用 `System.Text.Json` 将配置保存到 AppData 目录 (`lanrelay.config.json`)。

2. **启动加载**：
    - App 启动时读取配置。
    - 如果是首次运行，生成随机昵称和默认下载路径。

3. **TDD 验证**：
    - 验证序列化与反序列化。
    - 验证文件不存在时的默认值回退逻辑。

## 功能说明 (Implementation Notes)

- 2026-01-29: 任务完成。
- **核心实现**:
  - `AppConfig.cs`: 配置模型类，包含 `UserNickName`、`DownloadPath`、`LastUsedNetworkInterface`、`SecurityGroup`、`Theme`、`DeviceId` 属性，均带 JSON 序列化特性。
  - `ConfigService.cs`: 配置服务，使用 `System.Text.Json` 实现 JSON 持久化。支持同步/异步读写方法 (`Load`/`LoadAsync`、`Save`/`SaveAsync`)。
  - 默认路径: `%AppData%\LanRelay\lanrelay.config.json`
  - 静态辅助方法: `GenerateRandomNickname()` 生成 `User-XXXX` 格式昵称，`GetDefaultDownloadPath()` 返回 `Downloads\LanRelay` 路径。
- **测试覆盖**: 10 个测试用例，覆盖属性默认值、序列化/反序列化、文件缺失回退、损坏文件处理、目录自动创建等场景。
- **变更**: 新增 `DeviceId` 属性用于设备唯一标识（原需求未列出，但对 P2P 场景有价值）。