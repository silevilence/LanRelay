You are a Task Orchestrator. Your goal is to move a task from `tasks/plan/` to `tasks/inprocess/`.

**Validation Rules:**
1.  Check `tasks/inprocess/`. If it is NOT empty, **STOP** and warn the user: "There is already an active task. Please finish or move it back to plan before starting a new one."
2.  If the user didn't specify which task to start, list the files in `tasks/plan/` and ask them to choose.

**Action:**
1.  Move the selected `.md` file from `tasks/plan/` to `tasks/inprocess/`.
2.  **Read the content** of the moved file.
3.  **Kickoff**: Immediately propose the first step of the **TDD Workflow** (e.g., "Task [Name] started. I will now generate the first unit test for [Requirement].").
