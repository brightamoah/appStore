<script setup lang="ts">
interface Props {
  column: BoardColumn;
  tasks: Task[];
}

const { column, tasks } = defineProps<Props>();

const emit = defineEmits<{
  (event: "card-dropped", payload: { taskId: string; targetColumnId: string; targetOrderIndex: number }): void;
  (event: "select-task", taskItem: Task): void;
  (event: "add-task", columnId: string): void;
  (event: "edit-column", column: BoardColumn): void;
  (event: "delete-column", column: BoardColumn): void;
}>();

const isDragOver = ref<boolean>(false);

const isWipExceeded = computed<boolean>(() => {
  return column.wipLimit > 0 && tasks.length > column.wipLimit;
});

const columnColorClass = computed<string>(() => {
  const normalized = column.color?.toLowerCase();
  switch (normalized) {
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
});

function calculateDropIndex(dropEvent: DragEvent): number {
  const columnElement = dropEvent.currentTarget as HTMLElement;
  const cardElements = Array.from(columnElement.querySelectorAll<HTMLElement>("[data-task-id]"));
  if (cardElements.length === 0) return 0;

  for (let index = 0; index < cardElements.length; index++) {
    const card = cardElements[index];
    if (!card) continue;
    const rect = card.getBoundingClientRect();
    const midY = rect.top + rect.height / 2;
    if (dropEvent.clientY < midY) {
      return index;
    }
  }
  return cardElements.length;
}

function handleDragOver(dragEvent: DragEvent) {
  dragEvent.preventDefault();
  if (dragEvent.dataTransfer) {
    dragEvent.dataTransfer.dropEffect = "move";
  }
}

function handleDragEnter(dragEvent: DragEvent) {
  dragEvent.preventDefault();
  isDragOver.value = true;
}

function handleDragLeave() {
  isDragOver.value = false;
}

function handleDrop(dropEvent: DragEvent) {
  dropEvent.preventDefault();
  isDragOver.value = false;

  const rawData = dropEvent.dataTransfer?.getData("application/json");
  if (!rawData) {
    return;
  }

  try {
    const data = JSON.parse(rawData) as { taskId: string; sourceColumnId: string; sourceOrderIndex: number };
    const targetOrderIndex = calculateDropIndex(dropEvent);

    emit("card-dropped", {
      taskId: data.taskId,
      targetColumnId: column.id,
      targetOrderIndex,
    });
  }
  catch {
    // Ignore invalid drag payload
  }
}
</script>

<template>
  <div
    class="flex flex-col bg-muted/50 p-3.5 border border-default rounded-2xl w-80 h-full max-h-full transition-colors duration-200 shrink-0"
    :class="[
      isDragOver && 'border-primary bg-primary/5 ring-2 ring-primary/20',
      isWipExceeded && 'border-red-500/60 ring-1 ring-red-500/30',
    ]"
    @dragover="handleDragOver"
    @dragenter="handleDragEnter"
    @dragleave="handleDragLeave"
    @drop="handleDrop"
  >
    <div class="flex justify-between items-center gap-2 mb-2 px-1 shrink-0">
      <div class="flex items-center gap-2 min-w-0">
        <span
          class="rounded-full size-2.5 shrink-0"
          :class="columnColorClass"
        />

        <h2 class="font-semibold text-highlighted text-sm truncate">
          {{ column.name }}
        </h2>

        <UBadge
          v-if="column.wipLimit > 0"
          :label="`${tasks.length} / ${column.wipLimit}`"
          :color="isWipExceeded ? 'error' : 'neutral'"
          variant="subtle"
          size="xs"
          class="font-mono font-semibold"
          :class="isWipExceeded && 'animate-pulse'"
          :ui="{
            label: 'text-xs',
          }"
        />

        <UBadge
          v-else
          :label="`${tasks.length}`"
          variant="subtle"
          color="neutral"
          size="xs"
          class="font-mono"
          :ui="{
            label: 'text-xs',
          }"
        />
      </div>

      <div class="flex items-center gap-1">
        <UButton
          icon="i-lucide:plus"
          color="neutral"
          variant="ghost"
          size="xs"
          aria-label="Add task to column"
          :ui="{
            leadingIcon: 'size-4',
          }"
          @click="emit('add-task', column.id)"
        />

        <UDropdownMenu
          :items="[
            [
              {
                label: 'Edit Column',
                icon: 'i-lucide:pencil',
                onSelect: () => emit('edit-column', column),
              },
              {
                label: 'Delete Column',
                icon: 'i-lucide:trash-2',
                color: 'error' as const,
                disabled: column.isDefault,
                onSelect: () => emit('delete-column', column),
              },
            ],
          ]"
        >
          <UButton
            icon="i-lucide:more-vertical"
            color="neutral"
            variant="ghost"
            size="xs"
            aria-label="Column options"
            :ui="{
              leadingIcon: 'size-4',
            }"
          />
        </UDropdownMenu>
      </div>
    </div>

    <div
      v-auto-animate
      class="flex-1 space-y-2.5 p-0 min-h-36 overflow-y-auto"
    >
      <DashboardKanbanCard
        v-for="taskItem in tasks"
        :key="taskItem.id"
        :task-item
        @select="emit('select-task', $event)"
      />

      <div
        v-if="tasks.length === 0"
        class="flex flex-col justify-center items-center p-4 border-2 border-default/70 border-dashed rounded-xl h-32 text-muted text-xs text-center"
      >
        <UIcon
          name="i-lucide:arrow-down"
          class="opacity-50 mb-1 w-4 h-4"
        />

        Drop tasks here
      </div>
    </div>
  </div>
</template>

<style scoped>

</style>
