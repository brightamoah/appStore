export function useTimer() {
  const elapsed = ref(0); // in seconds
  const isRunning = ref(false);
  let intervalId: ReturnType<typeof setInterval> | null = null;

  const formatted = computed(() => {
    const h = Math.floor(elapsed.value / 3600).toString().padStart(2, "0");
    const m = Math.floor((elapsed.value % 3600) / 60).toString().padStart(2, "0");
    const s = Math.floor(elapsed.value % 60).toString().padStart(2, "0");
    return `${h}:${m}:${s}`;
  });

  function play() {
    if (isRunning.value) return;
    isRunning.value = true;
    intervalId = setInterval(() => {
      elapsed.value++;
    }, 1000);
  }

  function pause() {
    isRunning.value = false;
    if (intervalId) {
      clearInterval(intervalId);
      intervalId = null;
    }
  }

  function stop() {
    pause();
    elapsed.value = 0;
  }

  onUnmounted(() => {
    if (intervalId) clearInterval(intervalId);
  });

  return {
    elapsed,
    formatted,
    isRunning,
    play,
    pause,
    stop,
  };
}
