<script setup lang="ts">
import type { DropdownMenuItem } from "@nuxt/ui";

const props = defineProps<{
  collapsed?: boolean;
}>();

const colorMode = useColorMode();
const { userInitials, avatarBgColor } = useUserItems();
const authStore = useAuthStore();

const currentUser = computed(() => authStore.user);

const user = computed(() => ({
  name: currentUser.value?.name || "User",
  avatar: {
    alt: currentUser.value?.name || "User",
    text: userInitials.value,
    ui: { fallback: "text-white font-medium" },
    style: `background-color: ${avatarBgColor.value}`,
  },
}));

const appearanceItems = computed<DropdownMenuItem[]>(() => [
  {
    label: "Light",
    icon: "i-lucide:sun",
    type: "checkbox" as const,
    checked: colorMode.value === "light",
    onSelect: (e: Event) => {
      e.preventDefault();
      colorMode.value = "light";
    },
  },
  {
    label: "Dark",
    icon: "i-lucide:moon",
    type: "checkbox" as const,
    checked: colorMode.value === "dark",
    onSelect: (e: Event) => {
      e.preventDefault();
      colorMode.value = "dark";
    },
  },
]);

const items = computed<DropdownMenuItem[][]>(() => [
  [
    {
      type: "label" as const,
      label: user.value.name,
      avatar: user.value.avatar,
      slot: "profile",
    },
  ],
  [
    {
      label: "Profile",
      icon: "i-lucide:user",
      to: "/tasks",
    },
    {
      label: "Team",
      icon: "i-lucide:users",
      to: "/team",
    },
    {
      label: "Settings",
      icon: "i-lucide:settings",
    },
  ],
  [
    {
      label: "Appearance",
      icon: "i-lucide:sun-moon",
      children: appearanceItems.value,
    },
  ],
  [
    {
      label: "Log out",
      icon: "i-lucide:log-out",
      color: "error",
      onSelect: async (e: Event) => {
        e.preventDefault();
        await authStore.logout();
      },
    },
  ],
]);

const menuContent = computed(() => {
  if (props.collapsed) {
    return {
      side: "right" as const,
      align: "end" as const,
      sideOffset: 12,
      collisionPadding: 16,
    };
  }
  return {
    side: "top" as const,
    align: "start" as const,
    sideOffset: 8,
    collisionPadding: 16,
  };
});
</script>

<template>
  <UDropdownMenu
    :items="items"
    :content="menuContent"
    :ui="{
      content: 'z-50 min-w-56 shadow-xl ring ring-muted',
      item: 'cursor-pointer rounded flex items-center gap-2',
    }"
  >
    <UButton
      v-bind="{
        ...user,
        label: collapsed ? undefined : user?.name,
        trailingIcon: collapsed ? undefined : 'i-lucide:chevrons-up-down',
      }"
      color="neutral"
      variant="link"
      block
      :square="collapsed"
      :ui="{
        trailingIcon: 'text-dimmed ms-auto',
        leadingAvatarSize: 'md',
        label: 'ml-0.5 text-sm font-medium truncate',
      }"
    />

    <template #profile>
      <UUser
        :name="user?.name"
        :description="currentUser?.email"
        size="lg"
        :avatar="{
          text: userInitials,
          style: `background-color: ${avatarBgColor}`,
          ui: { fallback: 'text-white font-medium' },
        }"
      >
        <template #description>
          <p class="text-muted text-xs truncate">
            {{ currentUser?.email }}
          </p>
        </template>
      </UUser>
    </template>

    <template #chip-leading="{ item }">
      <div class="flex items-center gap-2 text-center">
        <span
          :style="{
            '--chip-light': `var(--color-${(item as any).chip}-500)`,
            '--chip-dark': `var(--color-${(item as any).chip}-400)`,
          }"
          class="size-2.5 rounded-full bg-(--chip-light) dark:bg-(--chip-dark) shrink-0"
        />
      </div>
    </template>
  </UDropdownMenu>
</template>
