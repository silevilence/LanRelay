# 文件传输协议与流处理

优先级：P1

## 任务细节

1. **文件传输协议**：
    - 扩展 TCP 协议头，增加 `FileHeader` (FileName, Size, MD5)。
    - 握手流程：A 发送请求 -> B 弹窗确认 -> B 回复同意 -> A 开始推流。

2. **传输引擎 (`FileTransferService`)**：
    - 实现流对流的拷贝 (`NetworkStream` -> `FileStream`)。
    - 实现进度回调 (`IProgress<double>`)。
    - **约束**：接收端需要检查磁盘空间，支持另存为路径。

3. **TDD 验证**：
    - 单元测试：模拟一个内存流作为文件，验证分块传输逻辑和进度计算是否准确。