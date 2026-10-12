<script setup lang="ts">
import type { AuthFormField } from "@nuxt/ui";

const signupFields: AuthFormField[] = [
  {
    name: "name",
    type: "text",
    label: "Full Name",
    placeholder: "Enter your full name",
    required: true,
  },
  {
    name: "email",
    type: "email",
    label: "Email",
    autocomplete: "username",
    placeholder: "Enter your email address",
    required: true,
  },
  {
    name: "password",
    type: "password",
    label: "Password",
    autocomplete: "new-password",
    placeholder: "Choose a secure password (min 8 characters)",
    required: true,
  },
];

const authStore = useAuthStore();

const { isLoading, error } = storeToRefs(authStore);
const { signup, clearError } = authStore;
</script>

<template>
  <Auth
    image-light="/images/signup.jpg"
    image-dark="/images/signup-dark.jpg"
    image-alt="Sign up visual"
  >
    <UAuthForm
      :schema="signUpSchema"
      title="Create Account"
      description="Enter your information to register a new account."
      icon="i-lucide:user-plus"
      :fields="signupFields"
      :loading="isLoading"
      :submit="{
        label: isLoading ? '' : 'Create Account',
        icon: 'i-lucide:user-plus',
        loading: isLoading,
        disabled: isLoading,
        class: 'cursor-pointer rounded-tl-3xl rounded-br-3xl',
      }"
      :ui="{
        title: 'font-mono text-primary',
        header: 'text-primary',
      }"
      @submit="signup"
    >
      <template #validation>
        <AuthAlert
          :error="error"
          @close="clearError"
        />
      </template>
    </UAuthForm>

    <template #footer>
      <p class="text-muted text-sm text-center">
        Already have an account?
        <ULink
          to="/auth/login"
          class="font-medium text-secondary hover:text-primary hover:underline"
        >
          Log in
        </ULink>
      </p>
    </template>
  </Auth>
</template>

<style scoped></style>
