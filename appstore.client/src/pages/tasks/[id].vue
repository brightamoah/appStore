<script setup lang="ts">
const route = useRoute("/tasks/[id]");
const taskStore = useTaskStore();

const taskId = computed<string>(() => String(route.params.id));
const isLoading = ref<boolean>(true);
const taskItem = ref<Task | null>(null);

onMounted(async () => {
  if (taskId.value) {
    try {
      taskItem.value = await taskStore.fetchTaskById(taskId.value);
    }
    catch {
      // Handled in store
    }
    finally {
      isLoading.value = false;
    }
  }
});
</script>

<template>
  <Dashboard
    variant="floating"
    collapsible="icon"
  >
    <template #header-title>
      <div class="flex items-center gap-2">
        <UButton
          to="/tasks"
          icon="i-lucide:arrow-left"
          color="neutral"
          variant="ghost"
          size="sm"
        />

        <h2 class="font-medium text-highlighted text-sm">
          {{ taskItem?.taskKey || "Task Details" }}
        </h2>
      </div>
    </template>

    <div class="space-y-6 max-w-4xl">
      <div
        v-if="isLoading"
        class="flex justify-center items-center py-20 text-muted"
      >
        <UIcon
          name="i-lucide:loader-2"
          class="w-6 h-6 animate-spin"
        />
      </div>

      <div
        v-else-if="taskItem"
        class="space-y-6 bg-elevated/40 p-6 border border-default rounded-xl"
      >
        <div class="flex justify-between items-center gap-2">
          <div class="flex items-center gap-2">
            <UBadge
              v-if="taskItem.taskKey"
              :label="taskItem.taskKey"
              variant="subtle"
              color="primary"
              size="sm"
              class="font-mono font-bold"
            />

            <UBadge
              :label="taskItem.issueType"
              variant="outline"
              color="neutral"
              size="xs"
            />
          </div>

          <UBadge
            :label="taskItem.status"
            variant="subtle"
            color="primary"
            size="xs"
          />
        </div>

        <h1 class="font-bold text-highlighted text-2xl tracking-tight">
          {{ taskItem.name }}
        </h1>

        <div class="space-y-2">
          <h3 class="font-semibold text-muted text-xs uppercase tracking-wider">
            Description
          </h3>

          <p class="whitespace-pre-wrap text-default text-sm">
            {{ taskItem.description || "No description provided." }}
          </p>
        </div>

        <div class="gap-4 grid grid-cols-2 bg-muted/40 p-4 border border-default rounded-xl text-xs">
          <div>
            <span class="block mb-1 text-muted">Created</span>

            <span class="font-mono text-default">
              {{ new Date(taskItem.createdAt).toLocaleString() }}
            </span>
          </div>

          <div>
            <span class="block mb-1 text-muted">Due Date</span>

            <span class="font-medium text-highlighted">
              {{ taskItem.dueDate ? new Date(taskItem.dueDate).toLocaleDateString() : "None" }}
            </span>
          </div>
        </div>
      </div>

      <div
        v-else
        class="py-16 text-muted text-center"
      >
        Task not found.
      </div>
    </div>
  </Dashboard>
</template>

<style scoped>

</style>
