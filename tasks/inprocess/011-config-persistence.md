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