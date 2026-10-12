<script setup lang="ts">
export interface AlertErrorPayload {
  message?: string;
  title?: string;
  description?: string;
  code?: string;
  status?: number;
  retryAfterSeconds?: number;
  isRateLimited?: boolean;
  details?: string[] | Record<string, unknown> | string | null;
  detailList?: string[];
}

const { error } = defineProps<{
  error?: AppError | AlertErrorPayload | string | null;
}>();

const emit = defineEmits<{
  (e: "close"): void;
}>();

const { lockoutSeconds, formatLockout, startCountdown } = useCountDown();

const isRateLimited = computed(() => {
  if (!error || typeof error === "string") return false;
  return (
    error.status === 429
    || error.code === ErrorCodes.RATE_LIMIT_EXCEEDED
    || ("isRateLimited" in error && Boolean(error.isRateLimited))
  );
});

// Watch error to ensure countdown is running for 429 rate-limited errors
watch(
  () => error,
  (newError) => {
    if (!newError || typeof newError === "string") return;
    if (newError.status === 429 || newError.code === ErrorCodes.RATE_LIMIT_EXCEEDED) {
      const seconds
        = ("retryAfterSeconds" in newError && typeof newError.retryAfterSeconds === "number" && newError.retryAfterSeconds > 0)
          ? newError.retryAfterSeconds
          : (typeof newError.details === "object" && newError.details !== null && "retryAfterSeconds" in newError.details)
              ? Number((newError.details as Record<string, unknown>).retryAfterSeconds) || 60
              : 60;

      if (seconds > 0 && lockoutSeconds.value <= 0) {
        startCountdown(seconds);
      }
    }
  },
  { immediate: true },
);

const title = computed(() => {
  if (!error) return "";
  if (typeof error === "string") return error;

  if (isRateLimited.value) {
    if (lockoutSeconds.value > 0) {
      return `Too many attempts. Please wait ${formatLockout(lockoutSeconds.value)} before trying again.`;
    }
    return "Lockout period has expired. You may try again now.";
  }

  return error.message || error.description || error.title || "An error occurred.";
});

const icon = computed(() => {
  return isRateLimited.value ? "i-lucide:clock" : "i-lucide:circle-alert";
});
</script>

<template>
  <div
    v-auto-animate
    class="w-full"
    role="alert"
    aria-live="assertive"
  >
    <UAlert
      v-if="error"
      color="error"
      variant="subtle"
      :title
      :icon
      close
      class="shadow-xs border border-error/25"
      :ui="{
        close: 'text-error hover:bg-error hover:text-white',
      }"
      @update:open="emit('close')"
    />
  </div>
</template>
