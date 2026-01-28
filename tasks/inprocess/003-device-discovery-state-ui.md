# 设备发现状态管理与列表 UI

优先级：P0

## 任务细节

1. **状态容器 (`DeviceListState`)**：
    - 维护 `List<DeviceInfo>`。
    - 定义事件 `OnDeviceFound` 和 `OnDeviceLost`（超时机制）。
    - 编写逻辑处理收到的 UDP 广播包，更新列表，去重。

2. **UI 实现 (`DeviceList.razor`)**：
    - 注入 `DeviceListState`。
    - 使用 HTML/CSS (Tailwind) 绘制设备列表。
    - 显示设备名称、IP、以及“直连/中转”标记（UI 先做出来，逻辑后续完善）。

3. **TDD 验证**：
    - 编写测试：模拟 `UdpBroadcaster` 收到数据 -> `DeviceListState` 更新 -> 触发事件。
    - 验证超时逻辑：N秒未收到心跳，设备从列表移除。