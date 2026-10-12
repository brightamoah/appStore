export interface PriorityItem {
  id: number;
  name: string;
  label: string;
  color: "neutral" | "info" | "warning" | "error";
}

export interface StatusItem {
  value: TaskStatus;
  label: string;
}

export interface IssueTypeItem {
  value: IssueType;
  label: string;
}

export const useMetadataStore = defineStore("metadataStore", () => {
  const priorities = ref<PriorityItem[]>([
    { id: 1, name: "Low", label: "Low", color: "neutral" },
    { id: 2, name: "Medium", label: "Medium", color: "info" },
    { id: 3, name: "High", label: "High", color: "warning" },
    { id: 4, name: "Emergency", label: "Emergency", color: "error" },
  ]);

  const statuses = ref<StatusItem[]>([
    { value: "NotStarted", label: "Not Started" },
    { value: "InProgress", label: "In Progress" },
    { value: "UnderReview", label: "Under Review" },
    { value: "ReadyForTest", label: "Ready For Test" },
    { value: "Completed", label: "Completed" },
    { value: "OnHold", label: "On Hold" },
    { value: "Cancelled", label: "Cancelled" },
    { value: "Closed", label: "Closed" },
  ]);

  const issueTypes = ref<IssueTypeItem[]>([
    { value: "Task", label: "Task" },
    { value: "Bug", label: "Bug" },
    { value: "Story", label: "Story" },
    { value: "Epic", label: "Epic" },
  ]);

  const isLoading = ref<boolean>(false);
  const isLoaded = ref<boolean>(false);
  const error = ref<AppError | null>(null);

  function mapPriorityColor(priorityId: number, priorityName: string): "neutral" | "info" | "warning" | "error" {
    const normalizedName = priorityName.toLowerCase();
    if (priorityId === 4 || normalizedName.includes("emerg") || normalizedName.includes("crit") || normalizedName.includes("urg")) return "error";
    if (priorityId === 3 || normalizedName.includes("high")) return "warning";
    if (priorityId === 2 || normalizedName.includes("med")) return "info";
    return "neutral";
  }

  function deduplicatePriorities(rawPriorities: { id: number; name: string }[]): PriorityItem[] {
    const uniqueMap = new Map<string, PriorityItem>();
    for (const priority of rawPriorities) {
      const lower = priority.name.trim().toLowerCase();
      if (!uniqueMap.has(lower)) {
        uniqueMap.set(lower, {
          id: priority.id,
          name: priority.name.trim(),
          label: priority.name.trim(),
          color: mapPriorityColor(priority.id, priority.name),
        });
      }
    }
    return Array.from(uniqueMap.values());
  }

  async function fetchMetadata() {
    isLoading.value = true;
    error.value = null;

    try {
      const response = await api.get<{
        message: string;
        data: {
          statuses: { value: TaskStatus; label: string }[];
          issueTypes: { value: IssueType; label: string }[];
          priorities: { id: number; name: string }[];
        };
      }>("/api/v1/tasks/metadata");

      if (response?.data) {
        if (response.data.statuses && response.data.statuses.length > 0) {
          statuses.value = response.data.statuses;
        }
        if (response.data.issueTypes && response.data.issueTypes.length > 0) {
          issueTypes.value = response.data.issueTypes;
        }
        if (response.data.priorities && response.data.priorities.length > 0) {
          priorities.value = deduplicatePriorities(response.data.priorities);
        }
        isLoaded.value = true;
      }
    }
    catch (caughtError) {
      // Fallback to /api/v1/priorities if metadata endpoint fails
      try {
        const priorityList = await api.get<{ id: number; name: string }[]>("/api/v1/priorities");
        if (Array.isArray(priorityList) && priorityList.length > 0) {
          priorities.value = deduplicatePriorities(priorityList);
          isLoaded.value = true;
        }
      }
      catch {
        error.value = resolveError(caughtError);
      }
    }
    finally {
      isLoading.value = false;
    }
  }

  function getPriorityLabel(priorityId: number): string {
    const match = priorities.value.find(priority => priority.id === priorityId);
    return match ? match.name : "Low";
  }

  function getPriorityColor(priorityId: number): "neutral" | "info" | "warning" | "error" {
    const match = priorities.value.find(priority => priority.id === priorityId);
    return match ? match.color : "neutral";
  }

  function getStatusLabel(statusValue: TaskStatus): string {
    const match = statuses.value.find(status => status.value === statusValue);
    return match ? match.label : statusValue;
  }

  function resetState() {
    isLoaded.value = false;
    isLoading.value = false;
    error.value = null;
  }

  return {
    priorities,
    statuses,
    issueTypes,
    isLoading,
    isLoaded,
    error,
    fetchMetadata,
    getPriorityLabel,
    getPriorityColor,
    getStatusLabel,
    resetState,
  };
}, {
  persist: {
    key: "app_store_metadata",
    storage: localStorage,
    pick: ["priorities", "statuses", "issueTypes", "isLoaded"],
  },
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useMetadataStore, import.meta.hot));
}
