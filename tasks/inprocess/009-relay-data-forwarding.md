# 中继数据转发引擎

优先级：P0 (核心)

## 任务细节

1. **转发服务 (`RelayService`)**：
    - 当收到发往非本机 ID 的数据包时，查询路由表。
    - 建立两条 TCP 连接：Source -> Me -> Target。
    - **零拷贝优化**：从 Source Socket 读取 Buffer，直接写入 Target Socket，不做应用层处理，不落盘。

2. **流量控制**：
    - 简单的背压机制（Backpressure）：如果 Target 写不进去，就停止从 Source 读。

3. **TDD 验证**：
    - 集成测试：启动三个 TCP 实例。A 发给 C（指定通过 B）。验证 B 的内存没有暴涨，且 C 收到了完整数据。