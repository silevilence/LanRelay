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