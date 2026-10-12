<script setup lang="ts">
const {
  label = "Placeholder",
} = defineProps<{
  label?: string;
}>();

const value = defineModel<any>();
</script>

<template>
  <UInput
    v-model="value"
    placeholder=""
    :ui="{ base: 'peer' }"
  >
    <label class="-top-2.5 peer-focus:-top-2.5 peer-placeholder-shown:top-1.5 left-0 absolute px-1.5 peer-placeholder-shown:font-normal font-medium peer-focus:font-medium text-highlighted peer-focus:text-highlighted peer-placeholder-shown:text-dimmed text-xs peer-focus:text-xs peer-placeholder-shown:text-sm transition-all pointer-events-none">
      <span class="inline-flex bg-default px-1">{{ label }}</span>
    </label>
  </UInput>
</template>
