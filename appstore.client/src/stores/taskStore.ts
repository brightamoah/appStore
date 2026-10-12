interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export const useTaskStore = defineStore("taskStore", () => {
  const toast = useToast();

  const tasks = ref<Task[]>([]);
  const currentTask = ref<Task | null>(null);
  const totalTasks = ref(0);
  const pageNumber = ref(1);
  const pageSize = ref(50);
  const searchQuery = ref("");
  const activeProjectId = ref<string | null>(null);

  const isLoading = ref(false);
  const isSaving = ref(false);
  const error = ref<AppError | null>(null);

  /**
   * Group tasks by columnId for instant Kanban board rendering.
   */
  const tasksByColumn = computed<Record<string, Task[]>>(() => {
    const groupedTasks: Record<string, Task[]> = {};

    for (const taskItem of tasks.value) {
      const columnKey = taskItem.columnId || "unassigned";
      if (!groupedTasks[columnKey]) {
        groupedTasks[columnKey] = [];
      }
      groupedTasks[columnKey].push(taskItem);
    }

    // Sort cards inside each column by orderIndex
    for (const columnKey in groupedTasks) {
      groupedTasks[columnKey]?.sort(
        (firstCard, secondCard) => firstCard.orderIndex - secondCard.orderIndex,
      );
    }

    return groupedTasks;
  });

  const clearError = () => {
    error.value = null;
  };

  /**
   * Fetch paginated tasks with search and project filtering.
   */
  const fetchTasks = async (
    projectId?: string,
    targetPageNumber = 1,
    targetPageSize = 50,
    search?: string,
  ) => {
    isLoading.value = true;
    error.value = null;

    if (projectId) activeProjectId.value = projectId;
    pageNumber.value = targetPageNumber;
    pageSize.value = targetPageSize;
    if (search !== undefined) searchQuery.value = search;

    try {
      const queryParameters = new URLSearchParams({
        pageNumber: targetPageNumber.toString(),
        pageSize: targetPageSize.toString(),
      });

      if (activeProjectId.value) {
        queryParameters.set("projectId", activeProjectId.value);
      }
      if (searchQuery.value.trim()) {
        queryParameters.set("search", searchQuery.value.trim());
      }

      const response = await api.get<{ message: string; data: PagedResult<Task> }>(
        `/api/v1/tasks?${queryParameters.toString()}`,
      );

      tasks.value = response.data.items;
      totalTasks.value = response.data.totalCount;
      return response.data;
    }
    catch (caughtError) {
      error.value = resolveError(caughtError);
      toast.add({
        title: "Failed to load tasks",
        description: error.value.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isLoading.value = false;
    }
  };

  /**
   * Fetch single task details by ID.
   */
  const fetchTaskById = async (taskId: string) => {
    isLoading.value = true;
    try {
      const response = await api.get<{ message: string; data: Task }>(`/api/v1/tasks/${taskId}`);
      currentTask.value = response.data;
      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to load task details",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isLoading.value = false;
    }
  };

  /**
   * Create a new task item with atomic Jira key generation.
   */
  const createTask = async (payload: CreateTaskPayload) => {
    isSaving.value = true;
    try {
      const response = await api.post<{ message: string; data: Task }>("/api/v1/tasks", payload);
      tasks.value.push(response.data);
      totalTasks.value++;

      toast.add({
        title: "Task created",
        description: `${response.data.taskKey ?? "Task"} created successfully.`,
        color: "success",
      });

      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to create task",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  /**
   * Optimistic drag-and-drop card movement with automatic rollback on rejection.
   */
  const moveTaskOptimistic = async (
    taskId: string,
    targetColumnId: string,
    targetOrderIndex: number,
    assigneeId?: string,
  ) => {
    const targetTask = tasks.value.find(
      taskItem => taskItem.id === taskId,
    );

    if (!targetTask) return;

    const previousTasksSnapshot = JSON.parse(JSON.stringify(tasks.value)) as Task[];
    const sourceColumnId = targetTask.columnId;
    const oldOrderIndex = targetTask.orderIndex;
    const isDifferentColumn = sourceColumnId !== targetColumnId;

    if (assigneeId) {
      targetTask.assigneeId = assigneeId;
    }

    if (isDifferentColumn) {
      tasks.value
        .filter(
          taskItem => taskItem.columnId === sourceColumnId && taskItem.orderIndex > oldOrderIndex,
        )
        .forEach(taskItem => taskItem.orderIndex--);

      tasks.value
        .filter(
          taskItem => taskItem.columnId === targetColumnId && taskItem.orderIndex >= targetOrderIndex,
        )
        .forEach(taskItem => taskItem.orderIndex++);

      targetTask.columnId = targetColumnId;
    }
    else {
      if (oldOrderIndex < targetOrderIndex) {
        tasks.value
          .filter(
            taskItem =>
              taskItem.columnId === sourceColumnId
              && taskItem.orderIndex > oldOrderIndex
              && taskItem.orderIndex <= targetOrderIndex
              && taskItem.id !== taskId,
          )
          .forEach(taskItem => taskItem.orderIndex--);
      }
      else if (oldOrderIndex > targetOrderIndex) {
        tasks.value
          .filter(
            taskItem =>
              taskItem.columnId === sourceColumnId
              && taskItem.orderIndex >= targetOrderIndex
              && taskItem.orderIndex < oldOrderIndex
              && taskItem.id !== taskId,
          )
          .forEach(taskItem => taskItem.orderIndex++);
      }
    }

    targetTask.orderIndex = targetOrderIndex;

    try {
      const payload: MoveTaskPayload = {
        targetColumnId,
        targetOrderIndex,
        assigneeId,
      };

      const response = await api.post<{ message: string; data: Task }>(
        `/api/v1/tasks/${taskId}/move`,
        payload,
      );

      // Synchronize exact server-side result (including updated status and timestamps)
      const taskIndex = tasks.value.findIndex(taskItem => taskItem.id === taskId);
      if (taskIndex !== -1) {
        tasks.value[taskIndex] = response.data;
      }
      if (currentTask.value && currentTask.value.id === taskId) {
        currentTask.value = response.data;
      }
      return response.data;
    }
    catch (caughtError) {
      // Rollback to snapshot on state machine failure or WIP limit violation
      tasks.value = previousTasksSnapshot;
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Movement rejected",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
  };

  /**
   * Update task fields (e.g. Assignee, Priority, Title, Due Date).
   */
  const updateTask = async (taskId: string, payload: UpdateTaskPayload) => {
    isSaving.value = true;
    try {
      const response = await api.patch<{ message: string; data: Task }>(
        `/api/v1/tasks/${taskId}`,
        payload,
      );

      const taskIndex = tasks.value.findIndex(taskItem => taskItem.id === taskId);
      if (taskIndex !== -1) {
        tasks.value[taskIndex] = response.data;
      }
      if (currentTask.value && currentTask.value.id === taskId) {
        currentTask.value = response.data;
      }

      toast.add({
        title: "Task updated",
        description: `${response.data.taskKey ?? "Task"} updated successfully.`,
        color: "success",
      });

      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to update task",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  /**
   * Transition state via the backend finite state machine.
   */
  const transitionStatus = async (taskId: string, targetStatus: TaskStatus) => {
    try {
      const payload: StatusTransitionPayload = { targetStatus };

      const response = await api.post<{ message: string; data: Task }>(
        `/api/v1/tasks/${taskId}/status`,
        payload,
      );

      const taskIndex = tasks.value.findIndex(taskItem => taskItem.id === taskId);
      if (taskIndex !== -1) {
        tasks.value[taskIndex] = response.data;
      }

      if (currentTask.value && currentTask.value.id === taskId) {
        currentTask.value = response.data;
      }

      toast.add({
        title: "Status updated",
        description: `Task status changed to ${targetStatus}.`,
        color: "success",
      });

      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Invalid status transition",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
  };

  /**
   * Delete a task item.
   */
  const deleteTask = async (taskId: string) => {
    try {
      await api.delete(`/api/v1/tasks/${taskId}`);
      tasks.value = tasks.value.filter(taskItem => taskItem.id !== taskId);
      totalTasks.value--;

      toast.add({
        title: "Task deleted",
        description: "Task removed successfully.",
        color: "success",
      });
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to delete task",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
  };

  const resetState = () => {
    tasks.value = [];
    currentTask.value = null;
    totalTasks.value = 0;
    pageNumber.value = 1;
    pageSize.value = 50;
    searchQuery.value = "";
    activeProjectId.value = null;
    isLoading.value = false;
    isSaving.value = false;
    error.value = null;
  };

  return {
    tasks,
    currentTask,
    totalTasks,
    pageNumber,
    pageSize,
    searchQuery,
    activeProjectId,
    isLoading,
    isSaving,
    error,
    tasksByColumn,
    clearError,
    fetchTasks,
    fetchTaskById,
    createTask,
    updateTask,
    moveTaskOptimistic,
    transitionStatus,
    deleteTask,
    resetState,
  };
}, {
  persist: {
    key: "app_store_tasks",
    storage: localStorage,
    pick: ["activeProjectId", "searchQuery", "pageNumber", "pageSize"],
  },
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useTaskStore, import.meta.hot));
}
