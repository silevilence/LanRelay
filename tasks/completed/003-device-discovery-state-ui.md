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

## 功能说明 (Implementation Notes)

- **2026-01-28**: 任务完成。

### 核心实现

| 文件 | 描述 |
|------|------|
| `src/LanRelay.Core/State/DeviceInfo.cs` | 设备信息记录类型，包含 `DeviceId`、`DeviceName`、`IPAddress`、`GroupId`、`LastSeen`、`IsDirectConnection`、`RelayDeviceId` |
| `src/LanRelay.Core/State/DeviceListState.cs` | 状态容器，使用 `ConcurrentDictionary` 存储设备，提供 `OnDeviceFound`、`OnDeviceLost`、`OnDevicesChanged` 事件 |
| `src/LanRelay.App/Components/DeviceList.razor` | 设备列表 Blazor 组件，注入 `DeviceListState`，订阅事件自动刷新 |
| `src/LanRelay.App/Components/DeviceList.razor.css` | 深色主题样式，支持直连/中转状态标记 |
| `src/LanRelay.App/Components/Pages/Home.razor` | 首页集成设备列表和设备详情面板 |

### 关键方法

- `DeviceListState.AddOrUpdateDevice()` - 添加或更新设备，自动更新 `LastSeen` 时间戳
- `DeviceListState.ProcessDiscoveryPacket()` - 将 `DiscoveryPacket` 转换为 `DeviceInfo` 并更新列表
- `DeviceListState.CleanupExpiredDevices()` - 清理超时设备（可配置超时时间，默认 10 秒）

### 测试覆盖

- `DeviceListStateTests` (10 tests): 验证添加、更新、去重、事件触发、超时清理、DiscoveryPacket 处理

### 改动说明

- 使用原生 CSS 替代 Tailwind（项目未配置 Tailwind）
- 添加了 `OnDevicesChanged` 事件用于 UI 统一刷新
- `DeviceInfo` 增加了 `RelayDeviceId` 字段为后续中转逻辑预留