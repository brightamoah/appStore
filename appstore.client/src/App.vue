<script setup lang="ts">
const authStore = useAuthStore();

onMounted(() => {
  authStore.initAuth();
});
</script>

<template>
  <UApp>
    <RouterView />

    <AuthSessionInactivityModal />
  </UApp>
</template>

<style scoped></style>
