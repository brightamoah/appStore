<script setup lang="ts">
const authStore = useAuthStore();

onMounted(() => {
  authStore.initAuth();
});
</script>

<template>
  <UApp>
    <RouterView />
  </UApp>
</template>

<style scoped></style>
