import type { RouteRecordName } from "vue-router";
import { nextTick } from "vue";
import { createRouter, createWebHistory } from "vue-router";
import { handleHotUpdate, routes } from "vue-router/auto-routes";

import { useAuthStore } from "@/stores/authStore";

export const router = createRouter({
  history: createWebHistory(),
  routes,
});

// Middleware: Route authentication guard
router.beforeEach((to) => {
  const authStore = useAuthStore();

  // Clear any residual form errors when switching routes
  authStore.clearError();

  const isAuthPage = to.path.startsWith("/auth");

  // Prevent logged-in users from accessing auth pages (login, signup, forgot-password)
  if (authStore.isAuthenticated && isAuthPage) {
    return { name: "/tasks/" };
  }

  const routeRequiresAuth: RouteRecordName[] = [
    "/",
    "/tasks/",
    "/tasks/[id]",
    "/team",
    "/settings",
  ];

  // Protect private routes (e.g. /tasks/*) from unauthenticated users
  if (!authStore.isAuthenticated && routeRequiresAuth.includes(to.name)) {
    return { name: "/auth/login", query: { redirect: to.fullPath } };
  }
});

// Enable native View Transitions API for seamless layout and page animations
router.beforeResolve((to, from) => {
  if (
    to.path !== from.path
    && typeof document !== "undefined"
    && "startViewTransition" in document
  ) {
    return new Promise<void>((resolve) => {
      (document as unknown as { startViewTransition: (cb: () => Promise<void>) => void }).startViewTransition(
        async () => {
          resolve();
          await nextTick();
        },
      );
    });
  }
});

if (import.meta.hot) {
  handleHotUpdate(router);
}
