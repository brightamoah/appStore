<script setup lang="ts">
import type { AuthFormField } from "@nuxt/ui";

const loginFields: AuthFormField[] = [{
  name: "email",
  type: "email",
  label: "Email",
  placeholder: "Enter your email",
  autocomplete: "username",
  required: true,
}, {
  name: "password",
  label: "Password",
  type: "password",
  placeholder: "Enter your password",
  autocomplete: "current-password",
  required: true,
}, {
  name: "rememberMe",
  label: "Remember me",
  type: "checkbox",
}];

const authStore = useAuthStore();

const { isLoading, error } = storeToRefs(authStore);
const {
  login,
  clearError,
} = authStore;
</script>

<template>
  <Auth
    image-light="/images/login.jpg"
    image-dark="/images/login-dark.jpg"
  >
    <UAuthForm
      :key="$route.fullPath"
      :schema="loginSchema"
      title="Login"
      icon="lucide:user-lock"
      :fields="loginFields"
      :loading="isLoading"
      :submit="{
        label: isLoading ? '' : 'Log In',
        icon: 'i-lucide-log-in',
        loading: isLoading,
        disabled: isLoading,
        class: 'cursor-pointer rounded-tl-3xl rounded-br-3xl',
      }"
      :ui="{
        title: 'font-mono text-primary',
        header: 'text-primary',
      }"
      @submit="login"
    >
      <template #description>
        <p class="mb-4">
          Enter your credentials to access your account.
        </p>

        <AuthAlert
          :error
          @close="clearError"
        />
      </template>

      <template #password-hint>
        <ULink
          :to="{ name: '/auth/forgotPassword' }"
          class="font-medium text-secondary hover:text-primary text-sm hover:underline"
          tabindex="-1"
        >
          Forgot password?
        </ULink>
      </template>
    </UAuthForm>

    <template #footer>
      <p class="text-muted text-sm text-center">
        Don't have an account?
        <ULink
          to="/auth/signup"
          class="font-medium text-secondary hover:text-primary hover:underline"
        >
          Sign up
        </ULink>
      </p>
    </template>
  </Auth>
</template>
