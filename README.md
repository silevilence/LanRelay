# LanRelay - 局域网中转传输软件

![.NET Version](https://img.shields.io/badge/.NET-10.0-purple)
![Platform](https://img.shields.io/badge/Platform-Windows-blue)
![License](https://img.shields.io/badge/License-MIT-green)

> 🚀 一款支持**双网卡中转(Multi-hop)**的局域网文件传输与即时通讯应用。

## 📊 项目状态

**当前版本**: V1.0 - 开发中 (Phase 2: 核心功能完成)

### 功能清单

| 功能 | 状态 | 任务编号 |
|------|------|----------|
| ✅ 项目初始化 | 已完成 | 001 |
| ✅ 多网卡扫描与识别 | 已完成 | 002 |
| ✅ UDP 设备发现与状态管理 | 已完成 | 003 |
| ✅ TCP 点对点消息传输 | 已完成 | 004 |
| ✅ 聊天界面实现 | 已完成 | 005 |
| ✅ 文件传输协议 | 已完成 | 006 |
| 🔄 文件传输 UI 控件 | 进行中 | 007 |
| 🔄 中继发现逻辑 | 进行中 | 008 |
| 🔄 中继数据转发 | 进行中 | 009 |
| 🔄 群组管理 | 进行中 | 010 |
| 🔄 配置持久化 | 进行中 | 011 |
| 📋 移动端支持 | 计划中 | 012 |
| 📋 文件夹传输 | 计划中 | 013 |
| 📋 断点续传 | 计划中 | 014 |
| 📋 端到端加密 | 计划中 | 015 |
| 📋 Mesh 路由 | 计划中 | 016 |

## 🏗️ 技术架构

### 技术栈
- **框架**: .NET 10 (LTS)
- **UI**: MAUI Blazor Hybrid (Windows Desktop)
- **语言**: C# (Latest)
- **测试**: xUnit + Moq
- **工具**: dotnet CLI, VS Code

### 架构模式
采用 **状态容器模式 (State Container Pattern)**:
- 单例 State 服务管理应用状态
- 通过 C# 事件通知 UI 更新
- 禁止使用传统 MVVM (INotifyPropertyChanged)

### 核心模块

```
LanRelay.Core/
├── Network/          # 网络通信层
│   ├── NicService        # 多网卡管理
│   ├── UdpBroadcaster    # UDP 广播发现
│   ├── TcpMessageServer  # TCP 服务端
│   └── TcpMessageClient  # TCP 客户端
├── State/            # 状态管理
│   ├── DeviceListState   # 设备列表状态
│   └── ChatState         # 聊天状态
└── FileTransfer/     # 文件传输
    └── FileTransferService

LanRelay.App/
└── Components/       # Blazor UI 组件
    ├── DeviceList.razor
    ├── ChatWindow.razor
    ├── ChatBubble.razor
    └── MessageInput.razor
```

## 🚀 快速开始

### 前置要求
- .NET 10 SDK
- Windows 10/11
- VS Code + C# Dev Kit (推荐)

### 构建与运行

```powershell
# 克隆仓库
git clone <repository-url>
cd LanRelay

# 还原依赖
dotnet restore

# 构建项目
dotnet build

# 运行测试
dotnet test

# 运行应用 (Windows)
dotnet run --project src/LanRelay.App
```

## 🔧 开发指南

### 项目结构

```
LanRelay/
├── .github/
│   ├── copilot-instructions.md   # AI 辅助开发指南
│   └── prompts/                  # Prompt 模板
├── src/
│   ├── LanRelay.Core/            # 核心类库
│   └── LanRelay.App/             # MAUI Blazor 应用
├── tests/
│   ├── LanRelay.Core.Tests/
│   └── LanRelay.App.Tests/
├── tasks/                        # 任务管理系统
│   ├── plan/                     # 待办任务
│   ├── inprocess/                # 进行中 (最多1个)
│   └── completed/                # 已完成
├── LanRelay.slnx                 # 解决方案文件
└── demands.md                    # 需求规格说明书
```

### TDD 工作流

本项目严格遵循 **测试驱动开发 (TDD)** 流程:

1. **RED**: 编写失败的测试用例
2. **GREEN**: 编写最小实现代码使测试通过
3. **REFACTOR**: 重构代码，保持测试通过

### CLI 优先原则

所有项目操作必须使用 `dotnet` CLI:

```powershell
# 添加项目引用
dotnet add src/LanRelay.App reference src/LanRelay.Core

# 添加 NuGet 包
dotnet add package <PackageName>

# 运行测试
dotnet test

# 构建发布
dotnet publish -c Release
```

## 📝 许可证

MIT License

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！
