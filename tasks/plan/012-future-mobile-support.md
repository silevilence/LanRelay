# [V2.0] Android / iOS 移动端适配

优先级：P3 (Future)

## 任务细节
*本任务为 V2.0 规划，V1.0 阶段暂不执行。*

1.  **UI 响应式适配**：
    - 优化 Blazor 布局，使其在手机竖屏下可用。
    - 处理移动端特有的 SafeArea 和手势导航。

2.  **后台保活 (Background Service)**：
    - Android: 实现 Foreground Service，防止锁屏后网络连接断开。
    - iOS: 研究 Background Fetch 或后台任务限制（iOS 对 UDP 广播和后台 TCP 限制极其严格，需调研可行性）。

3.  **权限处理**：
    - 处理移动端的“本地网络权限” (Local Network Privacy) 申请逻辑。
    - 处理文件读写权限 (Scoped Storage)。