# TCP 消息传输核心

优先级：P1

## 任务细节

1. **TCP 监听器 (`TcpServer`)**：
    - 在指定网卡上监听 TCP 端口。
    - 支持异步接收连接 (`AcceptTcpClientAsync`)。

2. **通信协议设计**：
    - 定义消息头（Header）：类型（文本/文件请求/心跳）、长度。
    - 定义消息体（Body）：UTF-8 文本或 JSON 控制信令。

3. **状态容器 (`ChatState`)**：
    - 管理当前选中的会话对象。
    - 维护消息历史记录 `List<ChatMessage>`。

4. **TDD 验证**：
    - 编写集成测试：启动 Server，启动 Client，发送一条 "Hello"，验证 Server 正确解析并触发回调。
    - 验证粘包/拆包处理（如果有自定义协议头）。

## 功能说明 (Implementation Notes)

- **2026-01-28**: 任务完成。

### 核心实现

| 文件 | 描述 |
|------|------|
| `src/LanRelay.Core/Network/MessageType.cs` | 消息类型枚举：Heartbeat(0), Text(1), FileRequest(2), FileData(3), FileAck(4) |
| `src/LanRelay.Core/Network/MessageHeader.cs` | 8 字节固定头，格式: [Type:1] [Reserved:3] [BodyLength:4]，Big-Endian 编码 |
| `src/LanRelay.Core/Network/Message.cs` | 完整消息封装，提供 `CreateText()`, `CreateHeartbeat()`, `CreateFileRequest()` 工厂方法 |
| `src/LanRelay.Core/Network/TcpClientConnection.cs` | 服务端连接封装，使用 `ReadExactAsync()` 处理粘包/拆包 |
| `src/LanRelay.Core/Network/TcpMessageServer.cs` | TCP 服务器，使用 `TcpListener` 绑定指定 `IPEndPoint`，支持多客户端 |
| `src/LanRelay.Core/Network/TcpMessageClient.cs` | TCP 客户端，异步连接、发送、接收消息 |
| `src/LanRelay.Core/State/ChatMessage.cs` | 聊天消息记录类型 |
| `src/LanRelay.Core/State/ChatState.cs` | 聊天状态容器，使用 `ConcurrentDictionary` 存储按设备分组的聊天历史 |

### 关键技术点

- **粘包/拆包处理**: 使用固定 8 字节头 + 变长 Body，`ReadExactAsync()` 确保完整读取
- **事件驱动**: `OnMessageReceived`, `OnClientConnected`, `OnDisconnected` 事件用于异步通知
- **线程安全**: 使用 `ConcurrentDictionary` 和 `lock` 保护共享状态

### 测试覆盖

- `MessageProtocolTests` (8 tests): 消息序列化/反序列化
- `TcpMessagingTests` (5 tests): TCP 连接、消息收发、多消息、断线检测
- `ChatStateTests` (8 tests): 聊天历史、设备选择、事件触发

### 改动说明

- 服务器类命名为 `TcpMessageServer` 而非 `TcpServer`，更明确表达消息协议处理
- 添加了 `TcpClientConnection` 封装服务端的单个连接，支持双向通信