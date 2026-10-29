<script setup lang="ts">
import type { FormErrorEvent, FormSubmitEvent } from "@nuxt/ui";
import type * as z from "zod";

interface Props {
  taskItem: Task | null;
}

const { taskItem } = defineProps<Props>();

const emit = defineEmits<{
  (event: "updated", updatedTask: Task): void;
  (event: "deleted", taskId: string): void;
}>();

const open = defineModel<boolean>("open", { default: false });

const taskStore = useTaskStore();
const projectStore = useProjectStore();
const boardStore = useBoardStore();
const authStore = useAuthStore();
const toast = useToast();

const {
  updateTask,
  deleteTask,
  transitionStatus,
} = taskStore;

const isEditing = ref<boolean>(false);
const isUpdatingStatus = ref<boolean>(false);
const isAssigning = ref<boolean>(false);
const isSavingEdit = ref<boolean>(false);
const isConfirmingDelete = ref<boolean>(false);

// Edit state
const editState = reactive<{
  name: string;
  description: string;
  priorityId: number;
  issueType: IssueType;
  assigneeIds: string[];
  dueDate: string;
  estimatedMinutes?: number;
}>({
  name: "",
  description: "",
  priorityId: 2,
  issueType: "Task",
  assigneeIds: [],
  dueDate: "",
  estimatedMinutes: undefined,
});

const metadataStore = useMetadataStore();

const issueTypes = computed(() => metadataStore.issueTypes.map(item => item.value));
const priorityOptions = computed(() => metadataStore.priorities);

const projectMembers = computed(() => projectStore.activeProject?.members || []);

// Resolves all assigned members from taskItem
const assignedMembers = computed(() => {
  if (!taskItem) return [];

  const ids: string[] = taskItem.assigneeIds
    && taskItem.assigneeIds.length > 0
    ? taskItem.assigneeIds
    : (taskItem.assigneeId
        ? [taskItem.assigneeId]
        : []
      );

  return ids
    .map(id =>
      projectMembers.value.find(member => member.id === id),
    )
    .filter((member): member is NonNullable<typeof member> => !!member);
});

function initEditState() {
  if (!taskItem) return;
  editState.name = taskItem.name;
  editState.description = taskItem.description || "";

  editState.priorityId = taskItem.priorityId;
  editState.issueType = taskItem.issueType;

  editState.assigneeIds = taskItem.assigneeIds && taskItem.assigneeIds.length > 0
    ? [...taskItem.assigneeIds]
    : (taskItem.assigneeId ? [taskItem.assigneeId] : []);

  editState.dueDate = taskItem.dueDate ? taskItem.dueDate.slice(0, 10) : "";

  editState.estimatedMinutes = taskItem.estimatedMinutes ?? undefined;
  isEditing.value = true;
}

function cancelEdit() {
  isEditing.value = false;
}

function toggleEditAssignee(userId: string) {
  if (editState.assigneeIds.includes(userId)) {
    editState.assigneeIds = editState.assigneeIds.filter(id => id !== userId);
  }
  else {
    editState.assigneeIds.push(userId);
  }
}

const editAssigneeSelectItems = computed(() => {
  return projectMembers.value.map(member => ({
    id: member.id,
    label: member.id === authStore.user?.id ? `${member.name} (You)` : member.name,
    avatar: member.profilePictureUrl ? { src: member.profilePictureUrl } : undefined,
  }));
});

// Dropdown items for quick assign/reassign in view mode
const quickAssignDropdownItems = computed(() => {
  if (!taskItem) return [];
  const currentIds = taskItem.assigneeIds && taskItem.assigneeIds.length > 0
    ? taskItem.assigneeIds
    : (taskItem.assigneeId ? [taskItem.assigneeId] : []);

  const memberList = projectMembers.value.map(member => ({
    label: member.id === authStore.user?.id ? `${member.name} (You)` : member.name,
    icon: currentIds.includes(member.id) ? "i-lucide:check-square" : "i-lucide:square",
    active: currentIds.includes(member.id),
    onSelect: () => handleToggleMember(member.id),
  }));

  return [
    [
      {
        label: "Clear All",
        icon: "i-lucide:user-x",
        disabled: currentIds.length === 0,
        onSelect: () => handleClearAllAssignees(),
      },
    ],
    memberList,
  ];
});

const validNextStatuses = computed<TaskStatus[]>(() => {
  if (!taskItem) return [];
  return ALLOWED_STATUS_TRANSITIONS[taskItem.status] || [];
});

type BadgeColor = "primary" | "secondary" | "success" | "info" | "warning" | "error" | "neutral";

function normalizeColorToBadge(color?: string | null): BadgeColor {
  const normalized = color?.toLowerCase();
  switch (normalized) {
    case "primary": return "primary";
    case "secondary": return "secondary";
    case "success": return "success";
    case "info": return "info";
    case "warning": return "warning";
    case "error": return "error";
    case "neutral":
    default:
      return "neutral";
  }
}

function getColumnForStatus(status: TaskStatus): BoardColumn | undefined {
  return boardStore.columns.find(column => column.statusMapping === status);
}

function getStatusBadgeColor(status: TaskStatus, columnId?: string | null): BadgeColor {
  if (columnId) {
    const matchingColumn = boardStore.columns.find(column => column.id === columnId);
    if (matchingColumn?.color) {
      return normalizeColorToBadge(matchingColumn.color);
    }
  }

  const mappedColumn = getColumnForStatus(status);
  if (mappedColumn?.color) {
    return normalizeColorToBadge(mappedColumn.color);
  }

  switch (status) {
    case "Completed":
    case "Closed":
      return "success";
    case "InProgress":
      return "info";
    case "UnderReview":
    case "ReadyForTest":
      return "warning";
    case "Cancelled":
      return "error";
    default:
      return "neutral";
  }
}

function getStatusDotClass(status: TaskStatus, columnId?: string | null): string {
  const badgeColor = getStatusBadgeColor(status, columnId);
  switch (badgeColor) {
    case "primary": return "bg-primary";
    case "secondary": return "bg-secondary";
    case "success": return "bg-success";
    case "info": return "bg-sky-500";
    case "warning": return "bg-amber-500";
    case "error": return "bg-red-500";
    case "neutral":
    default:
      return "bg-neutral-400";
  }
}

const currentStatusBadgeColor = computed<BadgeColor>(() => {
  if (!taskItem) return "neutral";
  return getStatusBadgeColor(taskItem.status, taskItem.columnId);
});

const currentStatusDotClass = computed<string>(() => {
  if (!taskItem) return "bg-neutral-400";
  return getStatusDotClass(taskItem.status, taskItem.columnId);
});

async function handleToggleMember(memberId: string) {
  if (!taskItem) return;
  const currentIds = taskItem.assigneeIds && taskItem.assigneeIds.length > 0
    ? [...taskItem.assigneeIds]
    : (taskItem.assigneeId ? [taskItem.assigneeId] : []);

  const newIds = currentIds.includes(memberId)
    ? currentIds.filter(id => id !== memberId)
    : [...currentIds, memberId];

  isAssigning.value = true;
  try {
    const updated = await updateTask(taskItem.id, {
      assigneeIds: newIds,
      assigneeId: newIds[0] || null,
    });
    emit("updated", updated);
  }
  finally {
    isAssigning.value = false;
  }
}

async function handleClearAllAssignees() {
  if (!taskItem) return;
  isAssigning.value = true;
  try {
    const updated = await updateTask(taskItem.id, {
      assigneeIds: [],
      assigneeId: null,
    });
    emit("updated", updated);
  }
  finally {
    isAssigning.value = false;
  }
}

async function handleQuickAssignToMe() {
  if (!taskItem || !authStore.user?.id) return;
  await handleToggleMember(authStore.user.id);
}

type UpdateTaskFormValues = z.infer<typeof updateTaskSchema>;

async function onSaveEdit(event: FormSubmitEvent<UpdateTaskFormValues>) {
  if (!taskItem) return;

  isSavingEdit.value = true;
  try {
    const updated = await updateTask(taskItem.id, {
      name: event.data.name?.trim(),
      description: event.data.description?.trim(),
      priorityId: event.data.priorityId,
      issueType: event.data.issueType,
      assigneeIds: editState.assigneeIds,
      assigneeId: editState.assigneeIds[0] || null,
      dueDate: editState.dueDate ? new Date(editState.dueDate).toISOString() : null,
      estimatedMinutes: editState.estimatedMinutes ?? null,
    });

    emit("updated", updated);
    isEditing.value = false;
  }
  finally {
    isSavingEdit.value = false;
  }
}

function onEditError(event: FormErrorEvent) {
  if (event?.errors?.[0]?.id) {
    const element = document.getElementById(event.errors[0].id);
    element?.focus();
    element?.scrollIntoView({ behavior: "smooth", block: "center" });
  }
}

async function handleStatusChange(targetStatus: TaskStatus) {
  if (!taskItem || taskItem.status === targetStatus) {
    return;
  }

  isUpdatingStatus.value = true;
  try {
    const currentAssigneeCount = (taskItem.assigneeIds?.length || 0) + (taskItem.assigneeId ? 1 : 0);
    // If moving to InProgress without an assignee, auto-assign current user to satisfy state machine rule
    if (targetStatus === "InProgress" && currentAssigneeCount === 0) {
      if (authStore.user?.id) {
        await updateTask(taskItem.id, {
          assigneeId: authStore.user.id,
          assigneeIds: [authStore.user.id],
        });

        toast.add({
          title: "Assigned & Started",
          description: `Task assigned to ${authStore.user.name || "you"} before moving to In Progress.`,
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

    const updated = await transitionStatus(taskItem.id, targetStatus);
    emit("updated", updated);
  }
  finally {
    isUpdatingStatus.value = false;
  }
}

async function handleDeleteTask() {
  if (!taskItem) {
    return;
  }

  if (!isConfirmingDelete.value) {
    isConfirmingDelete.value = true;
    return;
  }

  try {
    await deleteTask(taskItem.id);
    emit("deleted", taskItem.id);
    open.value = false;
    isConfirmingDelete.value = false;
  }
  catch {
    // Handled in store
  }
}

// Reset editing when drawer closes
watch(open, (isOpen) => {
  if (!isOpen) {
    isEditing.value = false;
    isConfirmingDelete.value = false;
  }
});

const editForm = useTemplateRef("editForm");
</script>

<template>
  <USlideover
    v-model:open="open"
    title="Task Details"
    :ui="{
      content: 'max-w-xl',
      body: 'p-6 space-y-6',
      footer: 'justify-end',
    }"
  >
    <template #header>
      <DrawerHeader
        v-model:open="open"
        v-model:is-editing="isEditing"
        :task-item
        :init-edit-state
      />
    </template>

    <template #body>
      <div v-if="taskItem">
        <UForm
          v-if="isEditing"
          ref="editForm"
          :schema="updateTaskSchema"
          :state="editState"
          class="space-y-6"
          @submit="onSaveEdit"
          @error="onEditError"
        >
          <UFormField
            label="Title / Summary"
            name="name"
            required
          >
            <UInput
              v-model="editState.name"
              class="w-full"
            />
          </UFormField>

          <div class="gap-3 grid grid-cols-2">
            <UFormField
              label="Issue Type"
              name="issueType"
            >
              <div class="flex gap-1">
                <UButton
                  v-for="typeOption in issueTypes"
                  :key="typeOption"
                  :label="typeOption"
                  :variant="editState.issueType === typeOption ? 'solid' : 'subtle'"
                  :color="editState.issueType === typeOption ? 'primary' : 'neutral'"
                  size="xs"
                  @click="editState.issueType = typeOption"
                />
              </div>
            </UFormField>

            <UFormField
              label="Priority"
              name="priorityId"
            >
              <div class="flex flex-wrap gap-1">
                <UButton
                  v-for="priorityOption in priorityOptions"
                  :key="priorityOption.id"
                  :label="priorityOption.name"
                  :variant="editState.priorityId === priorityOption.id ? 'solid' : 'subtle'"
                  :color="editState.priorityId === priorityOption.id ? priorityOption.color : 'neutral'"
                  size="xs"
                  @click="editState.priorityId = priorityOption.id"
                />
              </div>
            </UFormField>
          </div>

          <UFormField
            label="Assignees"
            name="assigneeIds"
          >
            <template #hint>
              <UButton
                v-if="authStore.user && !editState.assigneeIds.includes(authStore.user.id)"
                label="Assign to me"
                color="primary"
                variant="link"
                size="xs"
                class="p-0 h-auto font-normal text-xs"
                @click="toggleEditAssignee(authStore.user.id)"
              />
            </template>

            <UInputMenu
              v-model="editState.assigneeIds"
              :items="editAssigneeSelectItems"
              multiple
              value-key="id"
              label-key="label"
              size="lg"
              icon="i-lucide:users"
              placeholder="Select assignees"
              class="w-full"
            />
          </UFormField>

          <UFormField
            label="Description"
            name="description"
          >
            <UTextarea
              v-model="editState.description"
              placeholder="Task details and acceptance criteria..."
              :rows="4"
              class="w-full"
            />
          </UFormField>

          <div class="gap-3 grid grid-cols-2">
            <UFormField
              label="Due Date"
              name="dueDate"
            >
              <DatePicker
                v-model="editState.dueDate"
                size="md"
                placeholder="Select due date"
              />
            </UFormField>

            <UFormField
              label="Estimated Time (Minutes)"
              name="estimatedMinutes"
            >
              <UInputNumber
                v-model="editState.estimatedMinutes"
                :min="0"
                placeholder="e.g. 60"
                size="md"
                class="w-full"
              />
            </UFormField>
          </div>
        </UForm>

        <!-- VIEW MODE -->
        <template v-else>
          <div class="space-y-3">
            <h2 class="font-bold text-highlighted text-xl leading-tight">
              {{ taskItem.name }}
            </h2>

            <div class="space-y-3">
              <div class="flex items-center gap-2">
                <span class="font-medium text-muted text-sm">Current Status:</span>

                <UBadge
                  :label="metadataStore.getStatusLabel(taskItem.status)"
                  :color="currentStatusBadgeColor"
                  variant="subtle"
                  class="gap-1.5 font-semibold"
                >
                  <template #leading>
                    <span
                      class="rounded-full size-2 shrink-0"
                      :class="currentStatusDotClass"
                    />
                  </template>
                </UBadge>
              </div>

              <div class="flex flex-col gap-1 mb-4">
                <span class="font-medium text-muted text-sm">Available Transitions:</span>

                <div
                  v-if="validNextStatuses.length > 0"
                  class="flex flex-wrap gap-1.5"
                >
                  <UButton
                    v-for="status in validNextStatuses"
                    :key="status"
                    :label="metadataStore.getStatusLabel(status)"
                    :loading="isUpdatingStatus"
                    variant="subtle"
                    :color="getStatusBadgeColor(status)"
                    size="xs"
                    @click="handleStatusChange(status)"
                  >
                    <template #leading>
                      <span
                        class="rounded-full size-1.5 shrink-0"
                        :class="getStatusDotClass(status)"
                      />
                    </template>
                  </UButton>
                </div>

                <span
                  v-else
                  class="text-muted text-xs italic"
                >
                  Final status reached (no further transitions allowed).
                </span>
              </div>
            </div>
          </div>

          <!-- ASSIGNEES SECTION -->
          <div class="space-y-1 mt-10">
            <div class="flex justify-between items-center">
              <div class="flex items-center gap-1.5">
                <UIcon
                  name="i-lucide:users"
                  class="size-4 text-muted"
                />

                <h3 class="font-semibold text-muted text-xs uppercase tracking-wider">
                  Assignees ({{ assignedMembers.length }})
                </h3>
              </div>

              <div class="flex items-center gap-1.5">
                <UButton
                  v-if="authStore.user && !taskItem.assigneeIds?.includes(authStore.user.id) && taskItem.assigneeId !== authStore.user.id"
                  icon="i-lucide:user-plus"
                  label="Assign to me"
                  color="primary"
                  variant="solid"
                  size="xs"
                  :loading="isAssigning"
                  @click="handleQuickAssignToMe"
                />

                <UDropdownMenu :items="quickAssignDropdownItems">
                  <UButton
                    icon="i-lucide:user-cog"
                    label="Manage"
                    color="neutral"
                    variant="ghost"
                    size="xs"
                    :loading="isAssigning"
                  />
                </UDropdownMenu>
              </div>
            </div>

            <div
              v-if="assignedMembers.length > 0"
              v-auto-animate
              class="gap-2 grid grid-cols-3"
            >
              <div
                v-for="member in assignedMembers"
                :key="member.id"
                class="group flex justify-between items-center gap-2 bg-muted/40 p-2 border border-default rounded-lg"
              >
                <UUser
                  :name="member.name"
                  :avatar="{
                    text: generateInitials(member.name),
                    src: member.profilePictureUrl ?? undefined,
                    style: `background-color: ${generateUserColor(member.id)}`,
                    ui: { fallback: 'text-white font-medium' },
                  }"
                  size="xs"
                >
                  <template #name>
                    <span class="font-semibold text-highlighted text-xs">{{ member.name }}</span>

                    <span class="block text-muted text-xs">{{ member.email }}</span>
                  </template>
                </UUser>

                <UButton
                  variant="link"
                  size="xs"
                  class="hidden group-hover:block p-1 text-muted hover:text-error transition-colors"
                  icon="i-lucide:x"
                  @click="handleToggleMember(member.id)"
                />
              </div>
            </div>

            <div
              v-else
              class="flex justify-between items-center bg-muted/30 p-3 border border-default border-dashed rounded-xl"
            >
              <div class="flex items-center gap-2 text-muted text-xs">
                <UIcon
                  name="i-lucide:user-x"
                  class="size-4 text-muted"
                />

                <span>No assignees currently assigned</span>
              </div>
            </div>
          </div>

          <div class="space-y-2 mt-10">
            <div class="flex items-center gap-1.5">
              <UIcon
                name="i-lucide:align-left"
                class="size-3.5 text-muted"
              />

              <h3 class="font-semibold text-muted text-xs uppercase tracking-wider">
                Description
              </h3>
            </div>

            <div class="bg-elevated/40 p-4 border border-default rounded-xl text-default text-sm leading-relaxed whitespace-pre-wrap">
              {{ taskItem.description || "No description provided." }}
            </div>
          </div>

          <div class="gap-5 grid grid-cols-2 bg-muted/40 mt-10 p-4 border border-default rounded-xl text-sm">
            <div class="space-y-1">
              <div class="flex items-center gap-1.5 text-muted">
                <UIcon
                  name="i-lucide:flag"
                  class="size-4 text-muted"
                />

                <span class="font-medium">Priority</span>
              </div>

              <div>
                <UBadge
                  :label="metadataStore.getPriorityLabel(taskItem.priorityId)"
                  :color="metadataStore.getPriorityColor(taskItem.priorityId)"
                  variant="subtle"
                  size="sm"
                />
              </div>
            </div>

            <div class="space-y-1">
              <div class="flex items-center gap-1.5 text-muted">
                <UIcon
                  name="i-lucide:calendar-days"
                  class="size-4 text-muted"
                />

                <span class="font-medium">Due Date</span>
              </div>

              <div class="font-medium text-highlighted">
                {{ taskItem.dueDate ? new Date(taskItem.dueDate).toLocaleDateString() : "None" }}
              </div>
            </div>

            <div class="space-y-1">
              <div class="flex items-center gap-1.5 text-muted">
                <UIcon
                  name="i-lucide:clock"
                  class="size-4 text-muted"
                />

                <span class="font-medium">Estimated Time</span>
              </div>

              <div class="font-medium text-highlighted">
                {{ taskItem.estimatedMinutes ? `${taskItem.estimatedMinutes} mins` : "None" }}
              </div>
            </div>

            <div class="space-y-1">
              <div class="flex items-center gap-1.5 text-muted">
                <UIcon
                  name="i-lucide:history"
                  class="size-4 text-muted"
                />

                <span class="font-medium">Created</span>
              </div>

              <div class="font-mono text-default">
                {{ new Date(taskItem.createdAt).toLocaleDateString() }}
              </div>
            </div>
          </div>
        </template>
      </div>
    </template>

    <template #footer>
      <div
        v-if="isEditing"
        class="flex justify-end gap-2 pt-3"
      >
        <UButton
          label="Cancel"
          color="neutral"
          variant="ghost"
          @click="cancelEdit"
        />

        <UButton
          label="Save Changes"
          type="submit"
          color="primary"
          :loading="isSavingEdit"
          @click="editForm?.submit"
        />
      </div>

      <div
        v-else
        class="flex justify-end gap-2 pt-4 border-default border-t"
      >
        <UButton
          v-if="isConfirmingDelete"
          label="Cancel"
          color="neutral"
          variant="ghost"
          @click="isConfirmingDelete = false"
        />

        <UButton
          :label="isConfirmingDelete ? 'Confirm Delete' : 'Delete Task'"
          :icon="isConfirmingDelete ? 'i-lucide:alert-triangle' : 'i-lucide:trash-2'"
          color="error"
          :variant="isConfirmingDelete ? 'solid' : 'subtle'"
          @click="handleDeleteTask"
        />
      </div>
    </template>
  </USlideover>
</template>

<style scoped>

</style>
