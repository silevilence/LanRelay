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