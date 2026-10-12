<script setup lang="ts">
defineProps<{
  tasks: Task[];
}>();
</script>

<template>
  <UCard
    class="col-span-2 shadow-md h-fit"
    :ui="{
      header: 'p-2.5 sm:px-4',
      body: 'p-2 sm:p-2.5',
    }"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <h3 class="font-manrope font-semibold text-sm">
          My Tasks ({{ 4 }})
        </h3>

        <UButton
          label="View All"
          color="primary"
          variant="subtle"
          size="xs"
          class="rounded-tl-2xl rounded-br-2xl"
          to="/tasks"
        />
      </div>
    </template>

    <template #default>
      <div
        v-for="task in tasks"
        :key="task.id"
        class="flex justify-between gap-2 py-2 border-default border-b last:border-b-0"
      >
        <UCheckbox
          v-model="task.isCompleted"
          :label="task.name"
          highlight
          disabled
          color="primary"
          size="md"
          class="w-full"
          :ui="{
            icon: 'font-bold',
            base: 'rounded-full cursor-default',
            root: 'group opacity-100',
            label: 'transition-all group-has-data-[state=checked]:line-through group-has-data-[state=checked]:text-muted cursor-default',
          }"
        />

        <p class="text-muted text-sm">
          Today
        </p>
      </div>
    </template>
  </UCard>
</template>

<style scoped>

</style>
