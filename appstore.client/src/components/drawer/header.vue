<script setup lang="ts">
defineProps<{
  taskItem: Task | null;
  initEditState: () => void;
}>();

const open = defineModel<boolean>("open", { default: false });
const isEditing = defineModel<boolean>("isEditing", { default: false });
</script>

<template>
  <div class="flex justify-between items-center w-full">
    <div class="flex items-center gap-2">
      <span class="font-mono font-semibold text-default"> {{ taskItem?.taskKey }} </span>

      <UBadge
        v-if="taskItem?.issueType"
        :label="taskItem.issueType"
        variant="outline"
        color="neutral"
        size="sm"
      />
    </div>

    <div class="flex items-center gap-2">
      <UButton
        v-if="taskItem && !isEditing"
        icon="heroicons:pencil-square"
        label="Edit"
        color="neutral"
        variant="subtle"
        size="sm"
        @click="initEditState"
      />

      <UButton
        icon="i-lucide:x"
        color="neutral"
        variant="ghost"
        size="sm"
        aria-label="Close drawer"
        @click="open = false"
      />
    </div>
  </div>
</template>

<style scoped>

</style>
