import type { FormSubmitEvent } from "@nuxt/ui";

import type { RouteRecordName } from "vue-router";
import { acceptHMRUpdate, defineStore } from "pinia";

export const useAuthStore = defineStore("authStore", () => {
  const toast = useToast();
  const router = useRouter();

  const user = ref<User | null>(null);
  const roles = ref<string[]>([]);
  const permissions = ref<string[]>([]);

  const isAuthenticated = computed(() => Boolean(user.value));
  const isLoading = ref(false);
  const isInitializing = ref(false);
  const error = ref<AppError | null>(null);

  const clearError = () => {
    error.value = null;
  };

  const hasRole = (role: string) => roles.value.includes(role);
  const hasPermission = (permission: string) => permissions.value.includes(permission);

  function clearAuthState() {
    api.clearTokens();
    user.value = null;
    roles.value = [];
    permissions.value = [];
    error.value = null;

    if (typeof window !== "undefined") {
      localStorage.removeItem("app_store_tasks");
      localStorage.removeItem("app_store_project");
      localStorage.removeItem("app_store_projects");
      localStorage.removeItem("app_store_board");
      localStorage.removeItem("app_store_metadata");
      localStorage.removeItem("app_store_auth");
    }

    try {
      useTaskStore().resetState();
      useProjectStore().resetState();
      useBoardStore().resetState();
      useMetadataStore().resetState();
    }
    catch {
      // Stores may not be initialized yet in early app lifecycle
    }
  }

  api.onAuthFailure(() => {
    clearAuthState();
    router.push({ name: "/auth/login" });
  });

  /**
   * Revalidates session with the server by fetching the current user from GET /api/v1/auth/me
   */
  const fetchUser = async () => {
    if (!api.getAccessToken()) {
      clearAuthState();
      return null;
    }

    try {
      const response = await api.get("/api/v1/auth/me");
      user.value = response;
      return response;
    }
    catch (error) {
      if (error && typeof error === "object") {
        const err = error as { status?: number; isUnauthorized?: boolean };
        if (err.status === 401 || err.isUnauthorized) {
          clearAuthState();
        }
      }
      throw error;
    }
  };

  /**
   * Initializes auth: verifies session in the background on startup or refresh (Stale-While-Revalidate)
   */
  const initAuth = async () => {
    if (isInitializing.value) return;

    // Only revalidate if we have a token or persisted user
    if (!api.getAccessToken() && !user.value) return;

    isInitializing.value = true;
    try {
      await fetchUser();
    }
    catch {
      // Handled: 401 clears state; transient network errors leave stale data intact
    }
    finally {
      isInitializing.value = false;
    }
  };

  const signup = async (payload: FormSubmitEvent<SignUpSchema>) => {
    isLoading.value = true;
    clearError();

    try {
      const response = await api.post("/api/v1/auth/signup", payload.data);

      toast.add({
        title: "Signup Successful!",
        description: response.message,
        icon: "i-lucide:circle-check-big",
        color: "success",
      });

      router.push({ name: "/auth/login" });

      return response;
    }
    catch (err: unknown) {
      error.value = resolveError(err, "Failed to create account. Please check your information.");
      return null;
    }
    finally {
      isLoading.value = false;
    }
  };

  const login = async (payload: FormSubmitEvent<LoginSchema>) => {
    isLoading.value = true;
    clearError();

    try {
      const credentials = payload.data;

      const response = await api.post("/api/v1/auth/login", credentials);

      api.setAccessToken(response.accessToken);

      user.value = response.data.user;
      roles.value = response.data.roles;
      permissions.value = response.data.permissions;

      toast.add({
        title: "Login Successful!",
        description: response.message,
        icon: "i-lucide:circle-check-big",
        color: "success",
      });

      const redirectPath = (router.currentRoute.value.query.redirect as string) || "/" satisfies RouteRecordName;
      await router.push(redirectPath);

      return response;
    }
    catch (err: unknown) {
      error.value = resolveError(err, "Invalid email or password. Please try again.");
      return null;
    }
    finally {
      isLoading.value = false;
    }
  };

  const logout = async () => {
    isLoading.value = true;

    try {
      await api.post("/api/v1/auth/logout");
    }
    catch (err) {
      console.warn("Server logout request failed, proceeding with local signout:", err);
    }
    finally {
      isLoading.value = false;
      clearAuthState();
      await router.push({ name: "/auth/login" });
    }
  };

  return {
    user,
    roles,
    permissions,
    isAuthenticated,
    isLoading,
    isInitializing,
    error,
    clearError,
    hasRole,
    hasPermission,
    fetchUser,
    initAuth,
    login,
    logout,
    signup,
  };
}, {
  persist: {
    key: "app_store_auth",
    storage: localStorage,
    pick: ["user", "roles", "permissions"],
    afterHydrate: (ctx) => {
      (ctx.store as { initAuth?: () => void }).initAuth?.();
    },
  },
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useAuthStore, import.meta.hot));
}
