<script setup lang="ts">
import type { AuthLayoutProps } from "@/types";

const {
  imageLight = "/images/login.jpg",
  imageDark = "/images/login-dark.jpg",
  imageAlt = "Authentication visual",
  backTo = { name: "/tasks/" },
  backLabel = "Go back",
  reverse = false,
  quote,
  showOverlay = true,
} = defineProps<AuthLayoutProps>();

defineSlots<{
  default?: () => any;
  overlay?: () => any;
  header?: () => any;
  footer?: () => any;
}>();
</script>

<template>
  <div class="relative bg-default min-h-svh text-default antialiased">
    <!-- Top-left Back Navigation -->
    <UButton
      :to="backTo"
      icon="i-lucide-arrow-left"
      variant="link"
      color="neutral"
      :aria-label="backLabel"
      class="top-4 sm:top-5 left-4 sm:left-5 z-30 absolute shadow-xs backdrop-blur-md rounded-full transition auth-back-btn"
    />

    <!-- Top-right Color Mode Toggle -->
    <UColorModeButton
      variant="link"
      color="neutral"
      class="top-4 sm:top-5 right-4 sm:right-5 z-30 absolute shadow-xs backdrop-blur-md rounded-full transition auth-theme-btn"
    />

    <main
      class="relative flex flex-col min-h-svh overflow-hidden"
      :class="reverse ? 'lg:flex-row-reverse' : 'lg:flex-row'"
    >
      <!-- Visual Banner: Takes at least 2.5/4 (62.5%+) of screen on desktop with smooth sliding transition -->
      <div class="hidden z-10 relative lg:flex flex-1 bg-muted lg:w-5/8 lg:min-w-5/8 overflow-hidden select-none auth-banner">
        <UColorModeImage
          :light="imageLight"
          :dark="imageDark"
          :alt="imageAlt"
          class="absolute inset-0 size-full object-center object-cover"
        />

        <!-- Subtle ambient gradient overlay for contrast -->
        <div
          v-show="showOverlay"
          class="absolute inset-0 bg-linear-to-t from-neutral-900/85 via-neutral-900/25 to-transparent pointer-events-none"
        />

        <!-- Optional Quote or Custom Overlay Slot -->
        <div
          v-if="quote || $slots.overlay"
          class="z-10 relative mt-auto p-10 xl:p-14 max-w-xl text-white"
        >
          <slot name="overlay">
            <blockquote
              v-if="quote"
              class="space-y-3"
            >
              <p class="font-medium text-white/95 text-lg xl:text-xl leading-relaxed tracking-tight">
                "{{ quote.text }}"
              </p>

              <footer
                v-if="quote.author"
                class="pt-1"
              >
                <div class="font-semibold text-white text-base">
                  {{ quote.author }}
                </div>

                <div
                  v-if="quote.role"
                  class="text-white/70 text-xs"
                >
                  {{ quote.role }}
                </div>
              </footer>
            </blockquote>
          </slot>
        </div>
      </div>

      <!-- Auth Form Container: Takes 1.5/4 (37.5%) on desktop, full-height vertical centering on mobile -->
      <div class="z-20 flex flex-col flex-1 justify-center items-center px-4 sm:px-6 lg:px-6 xl:px-8 lg:py-12 pt-20 sm:pt-24 pb-10 sm:pb-12 w-full lg:w-3/8 lg:max-w-3/8 min-h-svh lg:min-h-0 shrink-0">
        <div class="flex flex-col items-center gap-4 my-auto w-full max-w-sm auth-card">
          <slot name="header" />

          <div class="w-full">
            <UPageCard class="shadow-xs border-2 border-primary rounded-tr-4xl rounded-bl-4xl w-full">
              <slot />
            </UPageCard>
          </div>

          <slot name="footer" />
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped></style>
