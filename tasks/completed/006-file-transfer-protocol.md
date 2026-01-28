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

## 功能说明 (Implementation Notes)
- **2026-01-28**: 任务完成。
- **核心实现**:
  - `FileTransferRequest`: 传输请求消息，包含 TransferId、FileName、FileSize、Md5Hash 属性，使用 JSON 序列化
  - `FileTransferResponse`: 传输响应消息，包含 Accepted、SavePath、RejectReason 属性
  - `FileTransferService`: 传输引擎核心类，提供以下功能：
    - `CopyWithProgressAsync()`: 流对流拷贝，支持 `IProgress<double>` 进度回调，按 81920 字节分块
    - `CalculateMd5Async()`: 异步计算 MD5 哈希值
    - `CheckDiskSpace()`: 磁盘空间检查（使用 `DriveInfo`）
  - 扩展 `Message` 类添加 `CreateFileRequest()`、`CreateFileResponse()`、`CreateFileData()`、`CreateFileAck()` 工厂方法
- **测试覆盖**: 12 个单元测试覆盖协议序列化、流拷贝、进度回调、MD5 计算、取消操作等场景
- **偏离说明**: 无，按计划实现