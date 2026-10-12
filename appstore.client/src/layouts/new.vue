<script setup lang="ts">
import type { SidebarProps } from "@nuxt/ui";

const {
  variant = "sidebar",
  collapsible = "icon",
  side = "left",
  rail = false,
} = defineProps<Pick<SidebarProps, "variant" | "collapsible" | "side" | "rail">>();

const open = useLocalStorage("taskFlow-sidebar-open", true);
const colorMode = useColorMode();

const isDark = computed({
  get: () => colorMode.value === "dark",
  set: (val: boolean) => {
    colorMode.value = val ? "dark" : "light";
  },
});

function toggleColorMode() {
  isDark.value = !isDark.value;
}
</script>

<template>
  <div
    class="flex bg-default w-full min-h-screen text-default"
    :class="[
      variant === 'inset' && 'bg-neutral-50 dark:bg-neutral-950',
      side === 'right' && 'flex-row-reverse',
    ]"
  >
    <DashboardSidebar
      v-model:open="open"
      :variant
      :collapsible
      :side
      :rail
    />

    <div class="flex flex-col flex-1 min-w-0 min-h-screen overflow-hidden transition-all duration-200">
      <!-- Dashboard Pill Header Container -->
      <div class="px-4 sm:px-6 pt-3.5 sm:pt-4 shrink-0">
        <header
          class="flex justify-between items-center bg-muted shadow-xs px-3 sm:px-4 border border-default rounded-2xl h-12 transition-all duration-200"
          :class="side === 'right' && 'flex-row-reverse'"
        >
          <div class="flex items-center gap-2">
            <UButton
              :icon="side === 'left'
                ? (open ? 'i-lucide:panel-left-close' : 'i-lucide:panel-left-open')
                : (open ? 'i-lucide:panel-right-close' : 'i-lucide:panel-right-open')"
              color="neutral"
              variant="ghost"
              size="sm"
              class="rounded-full"
              :aria-label="open ? 'Collapse sidebar' : 'Expand sidebar'"
              @click="open = !open"
            />

            <slot name="header-title" />
          </div>

          <div class="flex items-center gap-1.5 sm:gap-2">
            <slot name="header-actions" />

            <UButton
              :icon="isDark ? 'i-lucide:sun' : 'i-lucide:moon'"
              color="neutral"
              variant="ghost"
              size="sm"
              class="rounded-full"
              :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
              @click="toggleColorMode"
            />
          </div>
        </header>
      </div>

      <!-- Main Scrollable Content Area (Detached Content Section) -->
      <main
        class="flex-1 bg-default lg:shadow-xs mx-4 sm:mx-6 mt-5 mb-3 sm:mb-4 p-4 sm:p-6 lg:p-8 lg:rounded-xl lg:ring lg:ring-default min-h-0 overflow-y-auto"
        :class="[
          variant === 'inset' && 'rounded-xl ring ring-default shadow-xs',
          variant === 'floating' && 'rounded-xl ring ring-default shadow-xs',
        ]"
      >
        <slot />
      </main>
    </div>
  </div>
</template>
