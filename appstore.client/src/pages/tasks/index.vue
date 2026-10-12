<script setup lang="ts">
const projectStore = useProjectStore();
const boardStore = useBoardStore();
const taskStore = useTaskStore();
const metadataStore = useMetadataStore();
const authStore = useAuthStore();
const toast = useToast();

const viewMode = useLocalStorage<"board" | "list">("app_store_tasks_view_mode", "board");
const isCreateTaskModalOpen = ref<boolean>(false);
const isCreateProjectModalOpen = ref<boolean>(false);
const isColumnModalOpen = ref<boolean>(false);
const selectedColumnForEdit = ref<BoardColumn | null>(null);
const isDrawerOpen = ref<boolean>(false);
const selectedTask = ref<Task | null>(null);
const defaultColumnForNewTask = ref<string | undefined>(undefined);

// Smooth reactive search
const searchInput = ref<string>(taskStore.searchQuery);

watch(() => taskStore.searchQuery, (newQuery) => {
  if (newQuery !== searchInput.value) {
    searchInput.value = newQuery;
  }
});

const debouncedSearch = useDebounceFn((term: string) => {
  if (projectStore.activeProjectId) {
    taskStore.fetchTasks(projectStore.activeProjectId, 1, 50, term);
  }
}, 300);

watch(searchInput, (newTerm) => {
  taskStore.searchQuery = newTerm;
  debouncedSearch(newTerm);
});

function resolveAssigneeName(assigneeId?: string | null) {
  if (!assigneeId) return null;
  return projectStore.activeProject?.members.find(member => member.id === assigneeId)?.name || null;
}

async function loadProjectData(projectId: string) {
  try {
    await Promise.all([
      boardStore.fetchBoard(projectId),
      taskStore.fetchTasks(projectId),
    ]);
  }
  catch {
    await taskStore.fetchTasks(projectId);
  }
}

async function initialize() {
  await Promise.all([
    projectStore.fetchProjects(),
    metadataStore.fetchMetadata(),
  ]);
  if (projectStore.activeProjectId) {
    await loadProjectData(projectStore.activeProjectId);
  }
}

onMounted(() => {
  initialize();
});

watch(() => projectStore.activeProjectId, (newProjectId) => {
  if (newProjectId) {
    loadProjectData(newProjectId);
  }
});

function handleSelectTask(taskItem: Task) {
  selectedTask.value = taskItem;
  isDrawerOpen.value = true;
}

function handleOpenCreateTaskModal(columnId?: string) {
  defaultColumnForNewTask.value = columnId;
  isCreateTaskModalOpen.value = true;
}

function handleOpenCreateColumnModal() {
  selectedColumnForEdit.value = null;
  isColumnModalOpen.value = true;
}

function handleOpenEditColumnModal(column: BoardColumn) {
  selectedColumnForEdit.value = column;
  isColumnModalOpen.value = true;
}

async function handleDeleteColumn(column: BoardColumn) {
  if (!projectStore.activeProjectId) return;
  const fallbackColumn = boardStore.columns.find(col => col.id !== column.id);
  await boardStore.deleteColumn(projectStore.activeProjectId, column.id, fallbackColumn?.id);
  await loadProjectData(projectStore.activeProjectId);
}

async function handleCardDropped(payload: { taskId: string; targetColumnId: string; targetOrderIndex: number }) {
  const targetColumn = boardStore.columns.find(column => column.id === payload.targetColumnId);
  const movedTask = taskStore.tasks.find(task => task.id === payload.taskId);

  if (!targetColumn || !movedTask) return;

  // Validate state machine transitions when moving across columns with different status mappings
  if (movedTask.columnId !== payload.targetColumnId && movedTask.status !== targetColumn.statusMapping) {
    const allowedTransitions = ALLOWED_STATUS_TRANSITIONS[movedTask.status] || [];
    if (!allowedTransitions.includes(targetColumn.statusMapping)) {
      toast.add({
        title: "Transition Not Allowed",
        description: `Cannot move directly from "${movedTask.status}" to "${targetColumn.statusMapping}". Allowed transitions: ${allowedTransitions.join(", ") || "None"}.`,
        color: "warning",
      });
      return;
    }
  }

  // Handle unassigned task being moved to In Progress column
  let assigneeId: string | undefined;
  const hasAssignee = movedTask.assigneeId || (movedTask.assigneeIds && movedTask.assigneeIds.length > 0);
  if (targetColumn.statusMapping === "InProgress" && !hasAssignee) {
    if (authStore.user?.id) {
      assigneeId = authStore.user.id;
      toast.add({
        title: "Assigned & Started",
        description: `Task assigned to ${authStore.user.name || "you"} and moved to In Progress.`,
        color: "info",
      });
    }
    else {
      toast.add({
        title: "Assignee Required",
        description: "A task must have an assigned user before it can be moved to In Progress.",
        color: "warning",
      });
      return;
    }
  }

  await taskStore.moveTaskOptimistic(
    payload.taskId,
    payload.targetColumnId,
    payload.targetOrderIndex,
    assigneeId,
  );
}

function handleProjectCreated() {
  if (projectStore.activeProjectId) {
    loadProjectData(projectStore.activeProjectId);
  }
}
</script>

<template>
  <Dashboard
    variant="floating"
    collapsible="icon"
  >
    <template #header-title>
      <div class="flex items-center gap-2">
        <UDropdownMenu
          v-if="projectStore.projects.length > 0"
          :items="[
            projectStore.projects.map(project => ({
              label: `${project.name} (${project.key})`,
              icon: 'i-lucide:folder',
              active: project.id === projectStore.activeProjectId,
              onSelect: () => { projectStore.activeProjectId = project.id; },
            })),
            [{
              label: 'Create Project',
              icon: 'i-lucide:plus',
              onSelect: () => { isCreateProjectModalOpen = true; },
            }],
          ]"
        >
          <UButton
            color="neutral"
            variant="ghost"
            trailing-icon="i-lucide:chevrons-up-down"
            class="font-semibold"
          >
            {{ projectStore.activeProject?.name || "Select Project" }}

            <UBadge
              v-if="projectStore.activeProject?.key"
              :label="projectStore.activeProject.key"
              variant="subtle"
              color="primary"
              size="sm"
              class="ml-1.5 font-mono font-bold"
            />
          </UButton>
        </UDropdownMenu>

        <span
          v-else
          class="font-semibold text-highlighted text-sm"
        >
          Task Management
        </span>
      </div>
    </template>

    <template #header-actions>
      <div
        v-if="projectStore.activeProjectId"
        class="flex items-center bg-muted/60 p-0.5 border border-default rounded-lg"
      >
        <UButton
          icon="i-lucide:kanban"
          label="Board"
          size="xs"
          :variant="viewMode === 'board' ? 'solid' : 'ghost'"
          :color="viewMode === 'board' ? 'primary' : 'neutral'"
          @click="viewMode = 'board'"
        />

        <UButton
          icon="i-lucide:list"
          label="List"
          size="xs"
          :variant="viewMode === 'list' ? 'solid' : 'ghost'"
          :color="viewMode === 'list' ? 'primary' : 'neutral'"
          @click="viewMode = 'list'"
        />
      </div>

      <UButton
        v-if="projectStore.activeProjectId"
        label="New Task"
        icon="i-lucide:plus"
        color="primary"
        size="sm"
        @click="handleOpenCreateTaskModal()"
      />

      <UButton
        v-else
        label="New Project"
        icon="i-lucide:plus"
        color="primary"
        size="sm"
        @click="isCreateProjectModalOpen = true"
      />
    </template>

    <div
      v-if="!projectStore.activeProjectId && !projectStore.isLoading"
      class="flex flex-col flex-1 justify-center items-center p-8 text-center"
    >
      <UIcon
        name="i-lucide:folder-plus"
        class="mb-4 size-14 text-primary"
      />

      <h2 class="mb-2 font-bold text-highlighted text-xl">
        No Projects Found
      </h2>

      <p class="mb-6 max-w-sm text-muted text-sm">
        Create your first project to start managing tasks with Jira keys and Microsoft Planner Kanban boards.
      </p>

      <UButton
        label="Create Your First Project"
        icon="i-lucide:plus"
        color="primary"
        size="md"
        @click="isCreateProjectModalOpen = true"
      />
    </div>

    <div
      v-else
      class="flex flex-col flex-1 h-full min-h-0"
    >
      <div class="flex justify-between items-center gap-4 mb-4 px-1 shrink-0">
        <div class="flex flex-1 items-center gap-2 max-w-md">
          <UInput
            v-model="searchInput"
            icon="i-lucide:search"
            placeholder="Search by summary or issue key (e.g. CORE-1)..."
            class="w-full"
            @keyup.enter="debouncedSearch(searchInput)"
          />
        </div>

        <div class="flex items-center gap-2 text-muted text-xs">
          <span>{{ taskStore.totalTasks }} total tasks</span>
        </div>
      </div>

      <div
        v-if="viewMode === 'board'"
        v-auto-animate
        class="flex flex-1 items-stretch gap-4 pb-2 overflow-x-auto overflow-y-hidden select-none"
      >
        <DashboardKanbanColumn
          v-for="column in boardStore.columns"
          :key="column.id"
          :column
          :tasks="taskStore.tasksByColumn[column.id] || []"
          @card-dropped="handleCardDropped"
          @select-task="handleSelectTask"
          @add-task="handleOpenCreateTaskModal($event)"
          @edit-column="handleOpenEditColumnModal"
          @delete-column="handleDeleteColumn"
        />

        <div class="flex flex-col justify-start w-80 h-full shrink-0">
          <button
            type="button"
            class="flex justify-center items-center gap-2 hover:bg-primary/5 p-4 border-2 border-default hover:border-primary/60 border-dashed rounded-2xl h-20 font-medium text-muted hover:text-primary text-sm transition-all cursor-pointer"
            @click="handleOpenCreateColumnModal"
          >
            <UIcon
              name="i-lucide:plus"
              class="size-4"
            />
            Add Column
          </button>
        </div>

        <div
          v-if="boardStore.columns.length === 0 && !boardStore.isLoading"
          class="flex flex-col justify-center items-center w-full h-64 text-muted text-center"
        >
          <UIcon
            name="i-lucide:layout-grid"
            class="opacity-40 mb-2 w-10 h-10"
          />

          <p class="font-medium text-sm">
            No board columns provisioned yet.
          </p>
        </div>
      </div>

      <div
        v-else
        class="flex-1 bg-elevated/40 border border-default rounded-xl overflow-y-auto"
      >
        <table class="w-full text-sm text-left">
          <thead class="top-0 sticky bg-muted/50 border-default border-b text-muted text-xs uppercase tracking-wider">
            <tr>
              <th class="px-4 py-3">
                Key
              </th>

              <th class="px-4 py-3">
                Summary
              </th>

              <th class="px-4 py-3">
                Type
              </th>

              <th class="px-4 py-3">
                Status
              </th>

              <th class="px-4 py-3">
                Priority
              </th>

              <th class="px-4 py-3">
                Assignee
              </th>

              <th class="px-4 py-3">
                Due Date
              </th>
            </tr>
          </thead>

          <tbody class="divide-y divide-default">
            <tr
              v-for="taskItem in taskStore.tasks"
              :key="taskItem.id"
              class="hover:bg-elevated/80 transition-colors cursor-pointer"
              @click="handleSelectTask(taskItem)"
            >
              <td class="px-4 py-3 font-mono font-semibold text-primary text-xs">
                {{ taskItem.taskKey || "—" }}
              </td>

              <td class="px-4 py-3 font-medium text-highlighted">
                {{ taskItem.name }}
              </td>

              <td class="px-4 py-3">
                <UBadge
                  :label="taskItem.issueType"
                  variant="subtle"
                  color="neutral"
                  size="xs"
                />
              </td>

              <td class="px-4 py-3">
                <UBadge
                  :label="taskItem.status"
                  variant="subtle"
                  color="primary"
                  size="xs"
                />
              </td>

              <td class="px-4 py-3 text-xs">
                {{ metadataStore.getPriorityLabel(taskItem.priorityId) }}
              </td>

              <td class="px-4 py-3 text-xs">
                <span
                  v-if="resolveAssigneeName(taskItem.assigneeId)"
                  class="inline-flex items-center gap-1.5 font-medium text-highlighted"
                >
                  <span class="inline-flex justify-center items-center bg-primary/20 rounded-full w-5 h-5 font-bold text-primary text-xs">
                    {{ resolveAssigneeName(taskItem.assigneeId)?.slice(0, 2).toUpperCase() }}
                  </span>
                  {{ resolveAssigneeName(taskItem.assigneeId) }}
                </span>

                <span
                  v-else
                  class="text-muted text-xs italic"
                >
                  Unassigned
                </span>
              </td>

              <td class="px-4 py-3 text-muted text-xs">
                {{ taskItem.dueDate ? new Date(taskItem.dueDate).toLocaleDateString() : "—" }}
              </td>
            </tr>

            <tr v-if="taskStore.tasks.length === 0 && !taskStore.isLoading">
              <td
                colspan="7"
                class="py-12 text-muted text-center"
              >
                No tasks found matching your filter criteria.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <DashboardCreateTaskModal
      v-if="projectStore.activeProjectId"
      v-model:open="isCreateTaskModalOpen"
      :project-id="projectStore.activeProjectId"
      :default-column-id="defaultColumnForNewTask"
      @created="loadProjectData(projectStore.activeProjectId)"
    />

    <DashboardCreateProjectModal
      v-model:open="isCreateProjectModalOpen"
      @created="handleProjectCreated"
    />

    <DashboardTaskDrawer
      v-model:open="isDrawerOpen"
      :task-item="selectedTask"
      @updated="projectStore.activeProjectId && loadProjectData(projectStore.activeProjectId)"
      @deleted="projectStore.activeProjectId && loadProjectData(projectStore.activeProjectId)"
    />

    <DashboardColumnModal
      v-if="projectStore.activeProjectId"
      v-model:open="isColumnModalOpen"
      :project-id="projectStore.activeProjectId"
      :column="selectedColumnForEdit"
      @saved="loadProjectData(projectStore.activeProjectId)"
    />
  </Dashboard>
</template>

<style scoped>

</style>
