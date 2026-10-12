<script setup lang="ts">
import type { NavigationMenuItem, SidebarProps } from "@nuxt/ui";

defineProps<Pick<SidebarProps, "variant" | "collapsible" | "side" | "rail">>();

const open = defineModel<boolean>("open", { default: false });

const items: NavigationMenuItem[] = [
  {
    label: "Dashboard",
    icon: "i-lucide:house",
    to: "/",
  },
  {
    label: "My Tasks",
    icon: "i-lucide:circle-check-big",
    to: "/tasks",
  },
  {
    label: "Team",
    icon: "i-lucide:users",
    to: "/team",
  },
];

const footerItems: NavigationMenuItem[] = [
  {
    label: "Settings",
    icon: "i-lucide:settings",
    to: "/settings",
  },
];
</script>

<template>
  <USidebar
    v-model:open="open"
    :variant
    :collapsible
    :side
    :rail
    :ui="{
      container: 'h-full',
      inner: 'bg-muted rounded-3xl',
      footer: 'p-2',
      body: 'border-b flex flex-col justify-between flex-1',
    }"
    color="primary"
    class=""
  >
    <template #header="{ state }">
      <RouterLink
        :to="{ name: '/' }"
        class="flex items-center gap-2.5 overflow-hidden transition-all duration-200"
        :class="state === 'collapsed' ? 'justify-center w-full' : 'px-1'"
      >
        <UColorModeImage
          dark="/images/favicon.jpg"
          light="/images/fav-light.jpg"
          alt="logo"
          class="shadow-xs rounded-full transition-all duration-300 ease-linear shrink-0"
          :class="state === 'expanded' ? 'size-9' : 'size-8'"
        />

        <span
          v-show="state === 'expanded'"
          class="font-mono font-bold text-primary text-xl truncate uppercase tracking-wider"
        >
          TaskFlow
        </span>
      </RouterLink>
    </template>

    <template #default="{ state }">
      <!-- <div class="flex flex-col justify-between"> -->
      <UNavigationMenu
        :items="items"
        orientation="vertical"
        :collapsed="state === 'collapsed'"
        color="primary"
        :ui="{
          link: [
            'transition-colors overflow-hidden rounded-tl-2xl rounded-br-2xl',
            state === 'collapsed' ? 'justify-center p-2' : 'justify-start px-2.5 py-2',
          ],
        }"
      />

      <UNavigationMenu
        :items="footerItems"
        orientation="vertical"
        :collapsed="state === 'collapsed'"
        color="primary"
        :ui="{
          link: [
            'transition-colors overflow-hidden font-mono rounded-tl-2xl rounded-br-2xl uppercase tracking-wider',
            state === 'collapsed' ? 'justify-center p-2' : 'justify-start px-2.5 py-2',
          ],
        }"
      />

      <!-- <UButton
        icon="i-lucide:settings"
        color="primary"
        variant="solid"
        size="sm"
        :to="{ name: '/tasks/[id]', params: { id: 1 } }"
        class="flex justify-center items-center shadow-xs rounded-tl-2xl rounded-br-2xl"
        :class="state === 'collapsed' ? 'size-8' : 'size-9'"
      /> -->
      <!-- </div> -->
    </template>

    <template #footer="{ state }">
      <UserMenu :collapsed="state === 'collapsed'" />
    </template>
  </USidebar>
</template>

<style scoped>

</style>
