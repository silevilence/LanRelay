
# Project Context: LAN Relay Transfer Software (V1.0)

You are an expert .NET Architect and Developer assisting in building a Local Area Network (LAN) file transfer and messaging application. The core feature is **Dual-NIC Relay (Multi-hop)** capabilities.

## 1. Tech Stack & Architecture (Strict Constraints)

*   **Framework**: .NET 10 (LTS).
*   **UI Framework**: MAUI Blazor Hybrid (Target: Windows Desktop).
*   **Language**: C# (Latest features).
*   **Test Framework**: **xUnit** + **Moq**.
*   **Tools**: **dotnet CLI** (Primary for project management), VS Code.
*   **Architecture Pattern**: **State Container Pattern** (Singleton State Services + C# Events).

## 2. Project Directory Structure

```text
/
├── .github/
│   └── copilot-instructions.md
├── src/                  # Source code
│   ├── LanRelay.Core/    # Class Library
│   └── LanRelay.App/     # MAUI Blazor Hybrid
├── tests/                # Unit Tests
│   ├── LanRelay.Core.Tests/
│   └── LanRelay.App.Tests/
├── tasks/                # Task Management System
│   ├── plan/             # Backlog
│   ├── inprocess/        # Active task (MAX 1)
│   └── completed/        # Finished tasks
├── LanRelay.slnx         # Solution file
└── demands.md            # 需求文档
```

## 3. CLI First Approach (Mandatory)

**Always use `dotnet` CLI commands for project manipulation:**
*   Create Solution: `dotnet new sln -n LanRelay`
*   Create Project: `dotnet new classlib -o src/LanRelay.Core`, `dotnet new maui-blazor -o src/LanRelay.App`
*   Add References: `dotnet add src/LanRelay.App reference src/LanRelay.Core`
*   Manage Nuget: `dotnet add package [PackageName]`
*   Run Tests: `dotnet test`
*   Build: `dotnet build`

## 4. Development Process: TDD Workflow (Mandatory)

**You must strictly follow the TDD cycle for every task in `inprocess/`:**

1.  **RED (Write Test)**:
    *   Create or update a test file in `tests/`.
    *   Write a test case that fails.
    *   *Output*: Show the user the test code.
2.  **GREEN (Write Code)**:
    *   Implement the *minimum* code in `src/` to make the test pass.
    *   *Output*: Show the user the implementation code.
3.  **REFACTOR**:
    *   Clean up the code while keeping tests passing.
4.  **USER VERIFICATION**:
    *   Ask user to run `dotnet test`.
    *   **Task State Rule**: Only the USER moves files between `inprocess` and `completed`.

## 5. Git Commit Convention

**You are responsible for generating commit messages, NOT pushing.**

### Format
```text
<Emoji> <Type>: <Brief Description (Max 50 chars)>

[Optional: Detailed description of changes]
```

### Type & Emoji Map
*   ✨ `feat`: New feature
*   🐛 `fix`: Bug fix
*   📚 `docs`: Documentation
*   💎 `style`: Code style
*   ♻️ `refactor`: Refactoring
*   🚨 `test`: Adding tests
*   🔧 `chore`: Build/Tooling

## 6. Task Management

**Always check `tasks/inprocess` first.**

### Task File Format (`.md`)
```markdown
# [Task Title]

优先级：P0/P1...

## 任务细节
[Detailed requirements]

## 功能说明
[如果完成后有功能使用说明则补充本节内容，如没有则不需要]
```

### Workflow
1.  **Read**: Read `tasks/inprocess/*.md`.
2.  **TDD**: Execute TDD workflow.
3.  **Verify**: User runs `dotnet test`.
4.  **Commit**: Generate commit message.
5.  **Close**: User moves file to `tasks/completed/`.

## 7. Coding Guidelines

### UI Layer (Blazor)
*   Inject State Services (`@inject`).
*   Subscribe to events in `OnInitialized` and unsubscribe in `Dispose`.

### Network Layer (Core)
*   **Dual-NIC Handling**: Logic must account for multiple `NetworkInterface`s.
*   **Sockets**: Explicitly bind `IPEndPoint` to specific local IPs.
*   **Async**: Use `async/await` for all I/O.
