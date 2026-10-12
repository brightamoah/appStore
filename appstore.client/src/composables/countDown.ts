const lockoutSeconds = ref(0);
let targetTimestamp: number | null = null;
let timer: ReturnType<typeof setInterval> | null = null;

function clearTimer() {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
}

function updateRemaining() {
  if (!targetTimestamp) {
    lockoutSeconds.value = 0;
    clearTimer();
    return;
  }

  const remaining = Math.max(0, Math.ceil((targetTimestamp - Date.now()) / 1000));
  lockoutSeconds.value = remaining;

  if (remaining <= 0) {
    targetTimestamp = null;
    clearTimer();
  }
}

/**
 * Starts or updates the global rate-limit countdown.
 * Uses wall-clock timestamp tracking so the countdown remains accurate
 * even if the tab is throttled or backgrounded.
 */
export function startCountdown(seconds: number) {
  const sec = Math.ceil(seconds);
  if (!sec || sec <= 0) {
    stopCountdown();
    return;
  }

  clearTimer();
  targetTimestamp = Date.now() + sec * 1000;
  lockoutSeconds.value = sec;

  timer = setInterval(updateRemaining, 1000);
}

/**
 * Stops the active countdown and resets remaining lockout seconds to 0.
 */
export function stopCountdown() {
  targetTimestamp = null;
  lockoutSeconds.value = 0;
  clearTimer();
}

/**
 * Formats a duration in seconds into human-readable format:
 * e.g. `1m 30s` or `45s`.
 */
export function formatLockout(seconds?: number): string {
  const s = Math.max(0, Math.floor(seconds ?? lockoutSeconds.value));
  const m = Math.floor(s / 60);
  const rem = s % 60;
  if (m > 0) {
    return `${m}m ${rem.toString().padStart(2, "0")}s`;
  }
  return `${rem}s`;
}

/**
 * Composable providing reactive rate-limit lockout countdown state and controls.
 * Shares timer state across all consuming components.
 */
export function useCountDown() {
  const isLockedOut = computed(() => lockoutSeconds.value > 0);
  const formattedRemaining = computed(() => formatLockout(lockoutSeconds.value));

  return {
    lockoutSeconds,
    isLockedOut,
    formattedRemaining,
    startCountdown,
    stopCountdown,
    formatLockout,
  };
}
