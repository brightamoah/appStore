<script setup lang="ts">
import type { CalendarDate } from "@internationalized/date";
import { DateFormatter, getLocalTimeZone, parseDate } from "@internationalized/date";

interface Props {
  placeholder?: string;
  disabled?: boolean;
  clearable?: boolean;
  size?: "xs" | "sm" | "md" | "lg" | "xl";
}

const {
  placeholder = "Select date",
  disabled = false,
  clearable = true,
  size = "sm",
} = defineProps<Props>();

const modelValue = defineModel<string | null>({ default: null });

const df = new DateFormatter("en-US", {
  dateStyle: "medium",
});

const isPopoverOpen = ref(false);

const calendarValue = computed<CalendarDate | undefined>({
  get() {
    if (!modelValue.value) return undefined;
    try {
      const datePart = modelValue.value.slice(0, 10);
      return parseDate(datePart);
    }
    catch {
      return undefined;
    }
  },
  set(val: CalendarDate | undefined) {
    if (!val) {
      modelValue.value = null;
    }
    else {
      modelValue.value = val.toString();
    }
    isPopoverOpen.value = false;
  },
});

const displayLabel = computed(() => {
  if (!calendarValue.value) return placeholder;
  try {
    return df.format(calendarValue.value.toDate(getLocalTimeZone()));
  }
  catch {
    return modelValue.value || placeholder;
  }
});

function handleClear(event: Event) {
  event.stopPropagation();
  modelValue.value = null;
  calendarValue.value = undefined;
}
</script>

<template>
  <UPopover v-model:open="isPopoverOpen">
    <UButton
      icon="i-lucide:calendar-days"
      :label="displayLabel"
      color="neutral"
      variant="outline"
      :size
      :disabled
      class="justify-between w-full"
      :class="!calendarValue ? 'text-muted' : 'text-highlighted'"
    >
      <template
        v-if="clearable && calendarValue && !disabled"
        #trailing
      >
        <span
          role="button"
          tabindex="0"
          class="inline-flex items-center p-0.5 hover:text-error transition-colors"
          @click="handleClear"
          @keydown.enter.prevent="handleClear"
        >
          <UIcon
            name="i-lucide:x"
            class="size-3.5"
          />
        </span>
      </template>
    </UButton>

    <template #content>
      <div class="p-2">
        <UCalendar v-model="calendarValue" />
      </div>
    </template>
  </UPopover>
</template>

<style scoped>
</style>
