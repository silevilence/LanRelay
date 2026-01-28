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