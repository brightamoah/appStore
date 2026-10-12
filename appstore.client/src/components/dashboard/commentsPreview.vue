<script setup lang="ts">
defineProps<{
  tasks: Task[];
}>();
</script>

<template>
  <UCard
    class="shadow-md border border-default rounded-xl h-fit"
    :ui="{
      header: 'p-2.5 sm:px-4',
      body: 'p-2 sm:p-2.5',
    }"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <h3 class="font-manrope font-bold text-sm">
          New Comments
        </h3>
      </div>
    </template>

    <template #default>
      <div
        v-for="task in tasks.slice(0, 4)"
        :key="task.id"
        class="flex justify-between gap-4 py-1 first:py-1 last:py-1 border-default border-b last:border-b-0 h-full"
      >
        <div class="flex flex-col overflow-hidden">
          <h3 class="font-semibold text-sm truncate">
            {{ task.name }}
          </h3>

          <p class="text-xs truncate">
            {{ task.description }}
          </p>
        </div>

        <div class="flex justify-end items-center p-0">
          <UButton
            icon="i-lucide:chevrons-right"
            size="xs"
            variant="link"
            :ui="{
              base: 'p-0',
            }"
          />
        </div>
      </div>

      <UButton
        label="Add Comment"
        icon="i-lucide:plus"
        variant="subtle"
        class="justify-center items-center mt-3 rounded-tl-2xl rounded-br-2xl w-full"
      />
    </template>
  </UCard>
</template>

<style scoped>

</style>
