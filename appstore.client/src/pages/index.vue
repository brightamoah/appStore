<script setup lang="ts">
const projectStore = useProjectStore();
const taskStore = useTaskStore();

const searchTerm = ref<string>("");

onMounted(async () => {
  try {
    await Promise.all([
      projectStore.fetchProjects(),
      taskStore.fetchTasks(),
    ]);
  }
  catch {
    // Handled gracefully in stores
  }
});

const tasks = computed<Task[]>(() => taskStore.tasks);
const projects = computed<ProjectWithMembers[]>(() => projectStore.projects);
</script>

<template>
  <Dashboard
    variant="floating"
    collapsible="icon"
  >
    <template #header-title>
      <UInput
        v-model="searchTerm"
        class="w-64 font-mono outline-none border-0 focus:border-0 focus:outline-none"
        placeholder="Search..."
        size="sm"
        color="primary"
      />
    </template>

    <template #header-actions>
      <div class="flex gap-2">
        <UButton
          label="New Task"
          icon="i-lucide:plus"
          color="primary"
          size="sm"
          class="rounded-tl-2xl rounded-br-2xl"
          to="/tasks"
        />

        <UChip
          :text="5"
          size="2xl"
          color="error"
          inset
        >
          <UButton
            icon="i-lucide:bell-ring"
            color="neutral"
            variant="link"
            size="sm"
            class="rounded-tl-2xl rounded-br-2xl"
            to="/settings"
          />
        </UChip>
      </div>
    </template>

    <div class="gap-4 grid grid-cols-1 md:grid-cols-4">
      <DashboardCalendar />

      <DashboardTasksPreview :tasks />

      <DashboardCommentsPreview :tasks />

      <DashboardTracking :tasks="tasks.slice(0, 5)" />

      <DashboardProjects :projects />

      <UCard class="flex justify-center items-center w-full">
        <UButton
          label="Add Widget"
          icon="i-lucide:plus"
          variant="ghost"
          color="primary"
          class="rounded-tl-2xl rounded-br-2xl w-full"
        />
      </UCard>
    </div>
  </Dashboard>
</template>

<style scoped></style>
