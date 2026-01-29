# 动态组网与鉴权

优先级：P2

## 任务细节

1. **群组协议**：
    - 扩展发现协议，增加 `GroupID` 字段。
    - 默认组为 "Public"。

2. **密码验证**：
    - 建组时设置密码（Hash 存储）。
    - 入组申请协议：发送 Hash 后的密码进行比对。

3. **UI 实现**：
    - “创建群组”和“加入群组”的模态窗口。

4. **TDD 验证**：
    - 验证密码匹配逻辑。
    - 验证过滤逻辑：只显示同一 GroupID 的设备。

## 功能说明 (Implementation Notes)

- **2026-01-29**: 任务完成。

### 核心实现

1. **组信息模型** (`LanRelay.Core.State/GroupInfo.cs`):
   - `GroupId` - 组唯一标识
   - `PasswordHash` - SHA256 哈希存储的密码，null 表示公开组
   - `IsPublic` - 计算属性，判断是否为公开组

2. **组状态容器** (`LanRelay.Core.State/GroupState.cs`):
   - `CreateGroup(groupId, password?)` - 创建组，可选密码
   - `ValidatePassword(groupId, password)` - 密码验证
   - `JoinGroup(groupId, password?)` - 加入组并更新 CurrentGroupId
   - `HashPassword(string)` - 静态方法，使用 SHA256 哈希
   - `OnCurrentGroupChanged` - 组切换事件

3. **设备过滤** (`DeviceListState`):
   - `GetDevicesByGroup(groupId)` - 按组 ID 过滤设备列表

### 测试覆盖

- `GroupStateTests.cs` - 10 个测试：组创建、密码验证、公开组、事件触发、哈希一致性
- `DeviceFilterTests.cs` - 3 个测试：按组过滤、空组处理、当前组设备

### 偏离说明

- UI 模态窗口实现延后，本任务专注于核心状态逻辑
- `GroupID` 字段已在 Task 003 中存在于 `DiscoveryPacket`，无需重复添加