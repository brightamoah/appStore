export const useBoardStore = defineStore("boardStore", () => {
  const toast = useToast();

  const board = ref<Board | null>(null);
  const isLoading = ref(false);
  const isSaving = ref(false);
  const error = ref<AppError | null>(null);

  const columns = computed<BoardColumn[]>(() => {
    if (!board.value?.columns) return [];
    return [...board.value.columns].sort((firstColumn, secondColumn) => firstColumn.orderIndex - secondColumn.orderIndex);
  });

  const clearError = () => {
    error.value = null;
  };

  const fetchBoard = async (projectId: string) => {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.get<{ message: string; data: Board }>(`/api/v1/projects/${projectId}/board`);
      board.value = response.data;
      return response.data;
    }
    catch (caughtError) {
      error.value = resolveError(caughtError);
      toast.add({
        title: "Failed to load board",
        description: error.value.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isLoading.value = false;
    }
  };

  const createColumn = async (projectId: string, payload: CreateColumnPayload) => {
    isSaving.value = true;
    try {
      const response = await api.post<{ message: string; data: BoardColumn }>(
        `/api/v1/projects/${projectId}/board/columns`,
        payload,
      );

      if (board.value) {
        board.value.columns.push(response.data);
      }

      toast.add({
        title: "Column created",
        description: `Column "${response.data.name}" added successfully.`,
        color: "success",
      });

      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to create column",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const updateColumn = async (projectId: string, columnId: string, payload: UpdateColumnPayload) => {
    isSaving.value = true;
    try {
      const response = await api.put<{ message: string; data: BoardColumn }>(
        `/api/v1/projects/${projectId}/board/columns/${columnId}`,
        payload,
      );

      if (board.value) {
        const columnIndex = board.value.columns.findIndex(existingColumn => existingColumn.id === columnId);
        if (columnIndex !== -1) {
          board.value.columns[columnIndex] = response.data;
        }
      }

      toast.add({
        title: "Column updated",
        description: `Column "${response.data.name}" settings saved.`,
        color: "success",
      });

      return response.data;
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to update column",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const reorderColumns = async (projectId: string, orderedColumnIds: string[]) => {
    if (!board.value) return;

    // Optimistically update orderIndex locally
    const previousColumnsSnapshot = [...board.value.columns];
    orderedColumnIds.forEach((columnId, newIndex) => {
      const column = board.value?.columns.find(existingColumn => existingColumn.id === columnId);
      if (column) {
        column.orderIndex = newIndex;
      }
    });

    try {
      const payload: ReorderColumnsPayload = { orderedColumnIds };
      await api.post(`/api/v1/projects/${projectId}/board/columns/reorder`, payload);
    }
    catch (caughtError) {
      // Rollback on failure
      board.value.columns = previousColumnsSnapshot;
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to reorder columns",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
  };

  const deleteColumn = async (projectId: string, columnId: string, fallbackColumnId?: string) => {
    isSaving.value = true;
    try {
      const url = fallbackColumnId
        ? `/api/v1/projects/${projectId}/board/columns/${columnId}?fallbackColumnId=${fallbackColumnId}`
        : `/api/v1/projects/${projectId}/board/columns/${columnId}`;

      await api.delete(url);

      if (board.value) {
        board.value.columns = board.value.columns.filter(existingColumn => existingColumn.id !== columnId);
      }

      toast.add({
        title: "Column deleted",
        description: "Column deleted successfully.",
        color: "success",
      });
    }
    catch (caughtError) {
      const resolvedError = resolveError(caughtError);
      toast.add({
        title: "Failed to delete column",
        description: resolvedError.message,
        color: "error",
      });
      throw caughtError;
    }
    finally {
      isSaving.value = false;
    }
  };

  const resetState = () => {
    board.value = null;
    isLoading.value = false;
    isSaving.value = false;
    error.value = null;
  };

  return {
    board,
    columns,
    isLoading,
    isSaving,
    error,
    clearError,
    fetchBoard,
    createColumn,
    updateColumn,
    reorderColumns,
    deleteColumn,
    resetState,
  };
}, {
  persist: {
    key: "app_store_board",
    storage: localStorage,
    pick: ["board"],
  },
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useBoardStore, import.meta.hot));
}
