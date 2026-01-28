# 双网卡识别与绑定核心逻辑

优先级：P0

## 任务细节

1. **网卡枚举服务 (`NicService`)**：
    - 编写逻辑遍历 `NetworkInterface.GetAllNetworkInterfaces()`。
    - 过滤出有效的 IPv4 地址（排除回环、未启用的网卡）。
    - 识别每个 IP 对应的子网掩码。

2. **UDP 广播收发器 (`UdpBroadcaster`)**：
    - 实现 UDP 数据的发送和监听。
    - **关键约束**：必须支持绑定到指定的本地 IP（`IPEndPoint`），以支持在特定网卡上收发，而不是由系统随机路由。
    - 定义发现协议的数据包结构（JSON: UUID, Name, GroupID）。

3. **TDD 验证**：
    - 使用 Moq 或集成测试，验证 `NicService` 能正确返回本机 IP 列表。
    - 验证 UDP 广播包的序列化与反序列化是否正确。

## 功能说明 (Implementation Notes)

- **2026-01-28**: 任务完成。

### 核心实现

| 文件 | 描述 |
|------|------|
| `src/LanRelay.Core/Network/NicInfo.cs` | 网卡信息记录类型，包含 `Name`、`IPAddress`、`SubnetMask`，自动计算 `BroadcastAddress` |
| `src/LanRelay.Core/Network/NicService.cs` | 使用 `NetworkInterface.GetAllNetworkInterfaces()` 枚举活跃的 IPv4 网卡，过滤回环和 link-local 地址 |
| `src/LanRelay.Core/Network/DiscoveryPacket.cs` | 发现协议数据包，使用 `System.Text.Json` 序列化，包含 `DeviceId`、`DeviceName`、`GroupId`、`ProtocolVersion` |
| `src/LanRelay.Core/Network/UdpBroadcaster.cs` | 基于 `UdpClient` 封装，支持绑定到指定 `IPEndPoint`，启用广播和地址复用 |

### 测试覆盖

- `NicServiceTests` (6 tests): 验证网卡枚举、过滤回环、仅 IPv4、子网掩码、广播地址
- `DiscoveryPacketTests` (9 tests): 验证 JSON 序列化/反序列化、字节转换、协议版本
- `UdpBroadcasterTests` (5 tests): 验证绑定到指定 IP、发送、接收、取消、资源释放

### 改动说明

- 无偏离原计划的改动
- JSON 属性使用缩短的名称 (`v`, `id`, `name`, `group`) 以减少网络传输大小