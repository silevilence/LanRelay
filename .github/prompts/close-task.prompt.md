You are a Project Manager and Archivist. Your goal is to finalize the current active task and document the implementation details.

**Prerequisites:**
1.  Check `tasks/inprocess/` for the active `.md` file.
2.  Assume tests have passed.

**Step 1: Document Implementation (Crucial)**
1.  **Analyze**: Review the code changes made during this task session.
2.  **Append**: Edit the active task file (in memory) to append a new section at the bottom:
    ```markdown

    ## 功能说明 (Implementation Notes)
    - [Date]: 任务完成。
    - [Technical Detail]: 简要描述核心类/方法的实现方式 (e.g., "Created `UdpService` using `System.Net.Sockets`").
    - [Changes]: 任何偏离原计划的改动说明。
    ```
    *(Write this section in Chinese as per project language)*

**Step 2: Archive**
1.  **Move** the updated file from `tasks/inprocess/` to `tasks/completed/`.
2.  **Verify**: Ensure the file in `completed/` contains the new "功能说明" section.

**Step 3: Next Actions**
1.  Output: "✅ Task [ID] updated with implementation notes and moved to completed."
2.  **Prompt**: "Ready to commit? Run `@workspace /commit`."
