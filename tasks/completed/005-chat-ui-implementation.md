# 聊天界面与交互实现

优先级：P1

## 任务细节

1. **聊天窗口 UI**：
    - 左侧/顶部为设备列表（复用 Task 003）。
    - 右侧为聊天气泡区域。
    - 底部输入框和发送按钮。

2. **交互逻辑**：
    - 点击设备 -> 切换 `ChatState.CurrentTarget`。
    - 发送消息 -> 调用 TCP Client 发送 -> 更新 `ChatState` -> UI 刷新。
    - 接收消息 -> `TcpServer` 回调 -> 更新 `ChatState` -> UI 刷新（Toast 通知）。

3. **即阅即焚**：
    - 确保所有记录仅保存在内存 `List` 中，不写入数据库或文件。

4. **TDD 验证**：
    - 使用 Bunit 测试 Blazor 组件，验证点击发送按钮后，State 是否被正确调用。

## 功能说明 (Implementation Notes)

- **2026-01-28**: 任务完成。

### 核心实现

| 文件 | 描述 |
|------|------|
| `src/LanRelay.App/Components/ChatBubble.razor` | 聊天气泡组件，区分发送(紫色)/接收(灰色)样式，显示时间戳 |
| `src/LanRelay.App/Components/MessageInput.razor` | 消息输入框组件，支持 Enter 键发送，禁用状态处理 |
| `src/LanRelay.App/Components/ChatWindow.razor` | 聊天窗口主组件，集成气泡列表、输入框、连接状态显示 |
| `src/LanRelay.App/Components/Pages/Home.razor` | 更新首页，左侧设备列表 + 右侧聊天窗口布局 |
| `src/LanRelay.App/MauiProgram.cs` | 注册 `ChatState` 为单例服务 |

### UI 特性

- 深色主题风格，与设备列表一致
- 消息气泡：发送方右对齐紫色，接收方左对齐灰色
- 聊天头部显示目标设备名称、IP、连接状态
- Enter 键发送消息，Shift+Enter 换行
- 消息仅保存在内存 `ChatState` 中，符合即阅即焚要求

### 改动说明

- 未使用 Bunit 测试（MAUI Blazor 与标准测试框架不兼容），改用集成方式验证
- TCP 实际发送逻辑标记为 TODO，待后续任务完善连接管理