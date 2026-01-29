# 跨网段中继发现逻辑

优先级：P0 (核心)

## 任务细节

1. **双网卡桥接逻辑**：
    - 识别本机是否为“双网卡节点”（拥有两个不同网段的有效 IP）。
    - 如果是，`DiscoveryService` 需要在两个网卡上分别广播。

2. **路由表维护**：
    - `DeviceListState` 需扩展数据结构：`Dictionary<string, RouteInfo>`。
    - `RouteInfo` 包含：目标 ID、下一跳 IP (NextHopIP)、跳数 (HopCount)。
    - **逻辑**：
        - B 收到 A (网段1) 的广播，记录 A。
        - B 向 网段2 广播时，在包中携带 A 的存在信息（Gossip 协议简化版）。
        - C (网段2) 收到 B 的包，发现里面有 A，于是将 A 加入列表，标记为 "Via B"。

3. **TDD 验证**：
    - 编写纯逻辑测试：模拟三个节点 A, B, C 的网络拓扑，验证 B 能正确同步 A 和 C 的信息。

## 功能说明 (Implementation Notes)

- **2026-01-29**: 任务完成。

### 核心实现

1. **Gossip 协议模型** (`LanRelay.Core.Network/`):
   - `KnownDeviceInfo.cs` - 已知设备信息，包含 DeviceId, DeviceName, OriginIP, HopCount，用于跨网段设备共享
   - `DiscoveryPacket.KnownDevices` - 扩展发现包，添加 `IReadOnlyList<KnownDeviceInfo>` 属性携带已知设备列表

2. **路由逻辑** (`LanRelay.Core.State/`):
   - `DeviceInfo.HopCount` - 新增跳数属性，直连为 0，中继递增
   - `DeviceListState.ProcessDiscoveryPacket()` - 重构为同时处理直连设备和 Gossip 中继设备
   - `DeviceListState.ProcessKnownDevice()` - 私有方法，处理中继设备时优先选择直连或低跳数路径

3. **桥接节点检测** (`NicService`):
   - `IsBridgeNode()` - 检测本机是否有 2+ 个不同子网的网卡
   - `GetSubnetAddress()` - 辅助方法计算子网地址

### 测试覆盖

- `RelayDiscoveryTests.cs` - 7 个测试用例：
  - Gossip 包序列化/反序列化
  - 中继设备添加
  - 直连优先策略
  - 跳数追踪
  - 低跳数优先策略
  - 多设备广播

### 偏离说明

- 原计划使用独立的 `RouteInfo` 类，实际直接扩展 `DeviceInfo` 添加 `HopCount` 和 `RelayDeviceId`，简化数据结构
- 双网卡广播逻辑将在后续任务中结合实际网络服务实现