import type z from "zod";

export type TaskStatus = z.infer<typeof taskStatusSchema>;
export type IssueType = z.infer<typeof issueTypeSchema>;

/**
 * Single task details returned by GET /api/v1/tasks/{id}, POST /api/v1/tasks, PATCH /api/v1/tasks/{id}
 */
export type Task = z.infer<typeof taskSchema>;

/**
 * Task item itemized for list view returned by GET /api/v1/tasks
 */
export type TaskListItem = z.infer<typeof taskListItemSchema>;

/**
 * Request body for POST /api/v1/tasks
 */
export type CreateTaskPayload = z.infer<typeof createTaskSchema>;

/**
 * Request body for PUT /api/v1/tasks/{id}
 */
export type UpdateTaskPayload = z.infer<typeof updateTaskSchema>;

/**
 * Request body for PATCH /api/v1/tasks/{id}
 */
export type PatchTaskPayload = z.infer<typeof patchTaskSchema>;

export type MoveTaskPayload = z.infer<typeof moveTaskSchema>;
export type StatusTransitionPayload = z.infer<typeof statusTransitionSchema>;

// Backward-compatible aliases
export type TaskDto = Task;
export type TasksListDto = TaskListItem;
export type CreateTaskDto = CreateTaskPayload;
export type UpdateTaskDto = UpdateTaskPayload;
export type PatchTaskDto = PatchTaskPayload;
export type MoveTaskDto = MoveTaskPayload;
export type StatusTransitionDto = StatusTransitionPayload;
