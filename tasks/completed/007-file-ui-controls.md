# 文件传输 UI 与进度控制

优先级：P1

## 任务细节

1. **文件气泡组件**：
    - 在聊天窗口中展示文件消息：文件名、大小、图标。
    - 状态按钮：发送中(进度%)、接收中(进度%)、等待接收(接收/拒绝)、已完成。

2. **进度状态绑定**：
    - `TransferState` 容器需包含当前传输任务的进度字典。
    - UI 通过事件订阅实时刷新进度条 (Progress Bar)。

3. **弹窗确认**：
    - 收到文件请求时，使用 MAUI 原生 `DisplayAlert` 或 Blazor 模态框请求用户确认。

4. **TDD 验证**：
    - 测试 ViewModel/State 逻辑：更新 `Progress` 属性，验证 `OnChange` 事件是否触发。

## 功能说明 (Implementation Notes)

- **2026-01-29**: 任务完成。

### 核心实现

1. **状态层** (`LanRelay.Core.FileTransfer/`):
   - `TransferStatus.cs` - 传输状态枚举 (Pending, InProgress, Completed, Cancelled, Rejected, Failed)
   - `TransferInfo.cs` - 传输信息模型，包含 TransferId, FileName, FileSize, Progress, Status 等属性
   - `TransferState.cs` - 状态容器，使用 `ConcurrentDictionary` 管理传输任务，提供 `OnTransferAdded`, `OnTransferProgressChanged`, `OnTransferStatusChanged`, `OnTransferRemoved` 事件

2. **UI 层** (`LanRelay.App/Components/`):
   - `FileBubble.razor` - 文件气泡组件，支持：
     - 文件图标自动识别 (基于扩展名)
     - 进度条实时显示
     - 接收/拒绝按钮 (Pending 状态)
     - 状态文本显示 (已完成/已取消/已拒绝/传输失败)
   - `ChatWindow.razor` - 集成 TransferState，订阅所有传输事件并实时更新 UI

3. **DI 注册** (`MauiProgram.cs`):
   - `TransferState` 注册为 Singleton，遵循 State Container Pattern

### 测试覆盖

- `TransferStateTests.cs` - 10 个单元测试 (Add/Update/Remove 操作及事件触发)
- `TransferStateIntegrationTests.cs` - 7 个集成测试 (多传输场景、设备过滤等)

### 偏离说明

- 弹窗确认逻辑 (DisplayAlert) 暂未实现，待后续任务中结合实际传输流程完善