<script setup lang="ts">
interface Props {
  taskItem: Task;
}

const { taskItem } = defineProps<Props>();

const emit = defineEmits<{
  (event: "select", selectedTask: Task): void;
}>();

const projectStore = useProjectStore();
const isDragging = ref<boolean>(false);

const assignedUsers = computed(() => {
  const ids: string[] = taskItem.assigneeIds && taskItem.assigneeIds.length > 0
    ? taskItem.assigneeIds
    : (taskItem.assigneeId ? [taskItem.assigneeId] : []);
  if (ids.length === 0) return [];
  const members = projectStore.activeProject?.members || [];
  return ids
    .map(id => members.find(member => member.id === id))
    .filter((member): member is NonNullable<typeof member> => !!member);
});

const issueTypeBadgeColor = computed<"error" | "success" | "primary" | "neutral">(() => {
  switch (taskItem.issueType) {
    case "Bug":
      return "error";
    case "Story":
      return "success";
    case "Epic":
      return "primary";
    case "Task":
    default:
      return "neutral";
  }
});

const priorityBadgeColor = computed<"error" | "warning" | "info" | "neutral">(() => {
  switch (taskItem.priorityId) {
    case 4:
      return "error";
    case 3:
      return "warning";
    case 2:
      return "info";
    case 1:
    default:
      return "neutral";
  }
});

const isOverdue = computed<boolean>(() => {
  if (!taskItem.dueDate || taskItem.isCompleted) {
    return false;
  }

  return new Date(taskItem.dueDate).getTime() < Date.now();
});

const formattedDueDate = computed<string | null>(() => {
  if (!taskItem.dueDate) {
    return null;
  }

  return new Date(taskItem.dueDate).toLocaleDateString(undefined, {
    month: "short",
    day: "numeric",
  });
});

function handleDragStart(dragEvent: DragEvent) {
  if (!dragEvent.dataTransfer) {
    return;
  }

  isDragging.value = true;
  dragEvent.dataTransfer.effectAllowed = "move";

  dragEvent.dataTransfer.setData(
    "application/json",
    JSON.stringify({
      taskId: taskItem.id,
      sourceColumnId: taskItem.columnId,
      sourceOrderIndex: taskItem.orderIndex,
    }),
  );
}

function handleDragEnd() {
  isDragging.value = false;
}
</script>

<template>
  <div
    draggable="true"
    :data-task-id="taskItem.id"
    class="group relative bg-muted hover:bg-elevated shadow-xs p-3 border border-default rounded-2xl transition-all duration-150 cursor-grab active:cursor-grabbing select-none"
    :class="[
      isDragging && 'opacity-40 scale-95 border-dashed border-primary',
      taskItem.isCompleted && 'opacity-75',
    ]"
    @dragstart="handleDragStart"
    @dragend="handleDragEnd"
    @click="emit('select', taskItem)"
  >
    <div class="flex justify-between items-center gap-2 mb-2">
      <div class="flex items-center gap-1.5">
        <UBadge
          v-if="taskItem.taskKey"
          :label="taskItem.taskKey"
          :color="issueTypeBadgeColor"
          variant="subtle"
          size="sm"
          class="font-mono font-semibold"
        />

        <UBadge
          :label="taskItem.issueType"
          variant="subtle"
          color="neutral"
          size="sm"
        />
      </div>

      <UBadge
        v-if="taskItem.priorityId"
        :color="priorityBadgeColor"
        variant="subtle"
        size="sm"
        :ui="{
          label: 'text-sm',
        }"
      >
        {{
          taskItem.priorityId === 4
            ? "Emergency" : taskItem.priorityId === 3
              ? "High" : taskItem.priorityId === 2
                ? "Medium" : "Low"
        }}
      </UBadge>
    </div>

    <h3
      class="font-medium group-hover:text-primary text-sm line-clamp-2 leading-snug transition-colors"
      :class="taskItem.isCompleted && 'line-through text-muted'"
    >
      {{ taskItem.name }}
    </h3>

    <div class="flex justify-between items-center gap-2 mt-3 pt-2.5 border-default border-t text-xs">
      <div class="flex items-center gap-1.5">
        <span
          v-if="formattedDueDate"
          class="flex items-center gap-1 px-1.5 py-0.5 rounded font-medium text-xs"
          :class="isOverdue ? 'text-error font-semibold' : 'text-muted'"
        >
          <UIcon
            :name="isOverdue ? 'i-lucide:calendar-clock' : 'i-lucide:calendar-days'"
            class="w-3.5 h-3.5"
          />

          {{ formattedDueDate }}
        </span>

        <span
          v-if="taskItem.estimatedMinutes"
          class="flex items-center gap-1 text-muted text-xs"
        >
          <UIcon
            name="i-lucide:clock"
            class="w-3.5 h-3.5"
          />

          {{ taskItem.estimatedMinutes }}m
        </span>
      </div>

      <div class="flex items-center gap-1.5">
        <UBadge
          :label="taskItem.status"
          variant="subtle"
          color="neutral"
          size="sm"
        />

        <UAvatarGroup
          v-if="assignedUsers.length > 0"
          size="2xs"
          :max="2"
        >
          <UTooltip
            v-for="user in assignedUsers"
            :key="user.id"
            :text="`Assigned to: ${user.name}`"
          >
            <UAvatar
              color="neutral"
              :src="user.profilePictureUrl ?? undefined"
              :text="generateInitials(user?.name)"
              class="bg-(--avatar-bg)"
              :style="{ '--avatar-bg': generateUserColor(user.id) }"
              :ui="{ fallback: 'text-white font-medium' }"
            />
          </UTooltip>
        </UAvatarGroup>

        <UTooltip
          v-else
          text="Unassigned"
        >
          <UAvatar
            icon="i-lucide:user-round-x"
            color="neutral"
            size="xs"
            class="bg-(--avatar-unassigned-bg)"
            :style="{ '--avatar-unassigned-bg': generateUserColor(taskItem.id) }"
            :ui="{ icon: 'text-white text-sm font-medium' }"
          />
        </UTooltip>
      </div>
    </div>
  </div>
</template>

<style scoped>

</style>
