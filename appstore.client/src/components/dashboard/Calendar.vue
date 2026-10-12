<script setup lang="ts">
import { getLocalTimeZone, today } from "@internationalized/date";

const date = shallowRef(today(getLocalTimeZone()));
</script>

<template>
  <div class="shadow-md p-2 border border-default rounded-xl">
    <UCalendar
      v-model="date"
      color="primary"
      size="xs"
    />

    <UButton
      color="primary"
      label="Today"
      variant="subtle"
      class="justify-center mt-2 rounded-tl-2xl rounded-br-2xl w-full"
      @click="date = today(getLocalTimeZone())"
    />
  </div>
</template>

<style scoped>

</style>
