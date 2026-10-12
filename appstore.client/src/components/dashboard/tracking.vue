<script setup lang="ts">
defineProps<{
  tasks: Task[];
}>();

const { formatted, isRunning, play, pause, stop } = useTimer();
</script>

<template>
  <UCard
    class="col-span-2 shadow-md border border-default rounded-xl h-fit"
    :ui="{
      header: 'p-2.5 sm:px-4',
      body: 'p-2 sm:p-2.5',
    }"
  >
    <template #header>
      <div class="flex justify-between items-center">
        <h3 class="font-manrope font-bold text-sm">
          Projects
        </h3>
      </div>
    </template>

    <template #default>
      <div
        v-for="task in tasks"
        :key="task.id"
        class="flex justify-between items-center gap-4 mb-2 last:mb-0 pb-2 last:pb-0 border-default border-b last:border-none"
      >
        <div class="flex items-center gap-3">
          <UIcon
            name="lucide:alarm-clock"
            class="size-4.5"
          />

          <p class="font-manrope font-medium text-sm truncate">
            {{ task.name }}
          </p>
        </div>

        <div class="flex items-center gap-2.5">
          <span class="font-mono tabular-nums text-sm">{{ formatted }}</span>

          <UButton
            v-if="!isRunning"
            icon="i-lucide-play"
            color="primary"
            size="sm"
            variant="link"
            @click="play"
          />

          <UButton
            v-else
            icon="i-lucide-pause"
            color="warning"
            size="sm"
            variant="link"
            @click="pause"
          />

          <UButton
            icon="i-lucide:ellipsis-vertical"
            color="neutral"
            size="sm"
            variant="link"
            @click="stop"
          />
        </div>
      </div>
    </template>
  </UCard>
</template>

<style scoped>

</style>
