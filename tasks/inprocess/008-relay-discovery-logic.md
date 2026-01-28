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