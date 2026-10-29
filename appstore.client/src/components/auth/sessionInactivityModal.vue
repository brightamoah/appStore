<script setup lang="ts">
import { useIdle, useIntervalFn } from "@vueuse/core";

const authStore = useAuthStore();
const toast = useToast();

const INACTIVITY_TIMEOUT_MS = 15 * 60 * 1000; // 15 minutes
const COUNTDOWN_SECONDS = 120; // 2 minutes grace period

const { idle, reset: resetIdleTracker } = useIdle(INACTIVITY_TIMEOUT_MS);

const isOpen = ref<boolean>(false);
const isRefreshing = ref<boolean>(false);
const secondsRemaining = ref<number>(COUNTDOWN_SECONDS);

// Countdown timer running while modal is displayed
const {
  pause: pauseCountdown,
  resume: resumeCountdown,
} = useIntervalFn(() => {
  if (secondsRemaining.value > 0) {
    secondsRemaining.value--;
  }
  else {
    pauseCountdown();
    handleAutoLogout();
  }
}, 1000, { immediate: false });

// Proactive refresh timer: refreshes token every 10 minutes while user is active
useIntervalFn(async () => {
  if (authStore.isAuthenticated && !idle.value) {
    try {
      await authStore.refreshToken();
    }
    catch (refreshError) {
      console.warn("Background proactive token refresh failed:", refreshError);
    }
  }
}, 10 * 60 * 1000, { immediate: false });

// Trigger modal when user becomes idle for 15 minutes
watch(idle, (isIdle) => {
  if (isIdle && authStore.isAuthenticated && !isOpen.value) {
    secondsRemaining.value = COUNTDOWN_SECONDS;
    isOpen.value = true;
    resumeCountdown();
  }
});

// Close modal if user logs out through other means
watch(() => authStore.isAuthenticated, (isAuthenticated) => {
  if (!isAuthenticated && isOpen.value) {
    isOpen.value = false;
    pauseCountdown();
  }
});

async function handleContinue() {
  isRefreshing.value = true;
  try {
    await authStore.refreshToken();
    pauseCountdown();
    isOpen.value = false;
    resetIdleTracker();

    toast.add({
      title: "Session Extended",
      description: "Your session has been successfully refreshed.",
      color: "success",
      icon: "i-lucide:shield-check",
    });
  }
  catch {
    toast.add({
      title: "Session Expired",
      description: "Could not refresh your session. Please log in again.",
      color: "error",
      icon: "i-lucide:alert-circle",
    });
    isOpen.value = false;
    pauseCountdown();
    await authStore.logout();
  }
  finally {
    isRefreshing.value = false;
  }
}

async function handleLogout() {
  pauseCountdown();
  isOpen.value = false;
  await authStore.logout();

  toast.add({
    title: "Logged Out",
    description: "You have been logged out securely.",
    color: "neutral",
    icon: "i-lucide:log-out",
  });
}

async function handleAutoLogout() {
  isOpen.value = false;
  await authStore.logout();

  toast.add({
    title: "Session Expired",
    description: "You were logged out due to 15 minutes of inactivity.",
    color: "warning",
    icon: "i-lucide:clock",
  });
}

const formattedCountdown = computed<string>(() => {
  const minutes = Math.floor(secondsRemaining.value / 60);
  const seconds = secondsRemaining.value % 60;
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
});
</script>

<template>
  <UModal
    v-model:open="isOpen"
    :dismissible="false"
    title="Session Inactivity Warning"
    description="You have been inactive for 15 minutes."
    :ui="{
      content: 'sm:max-w-lg',
      body: 'p-6 space-y-4',
      footer: 'justify-end gap-2',
    }"
  >
    <template #body>
      <div class="flex items-center gap-4">
        <UIcon
          name="i-eos-icons:hourglass"
          class="size-15 text-amber-500 shrink-0"
        />

        <div class="space-y-2">
          <p class="text-default">
            For your security, your session will automatically close due to inactivity. Would you like to continue working or log out now?
          </p>

          <div class="flex items-center gap-2 pt-1">
            <span class="text-muted text-sm">Automatic logout in:</span>

            <UBadge
              :label="formattedCountdown"
              color="warning"
              variant="subtle"
              class="font-mono font-bold"
            />
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <UButton
        label="Log Out"
        color="error"
        variant="subtle"
        icon="i-lucide:log-out"
        :disabled="isRefreshing"
        @click="handleLogout"
      />

      <UButton
        label="Continue Session"
        color="primary"
        variant="solid"
        icon="i-lucide:refresh-cw"
        :loading="isRefreshing"
        @click="handleContinue"
      />
    </template>
  </UModal>
</template>

<style scoped></style>
