<script setup lang="ts">
import type { AuthFormField } from "@nuxt/ui";

const toast = useToast();
const isLoading = ref(false);
const error = ref<AppError | null>(null);

function clearError() {
  error.value = null;
}

const forgotPasswordFields: AuthFormField[] = [{
  name: "email",
  type: "email",
  label: "Email",
  placeholder: "Enter your email address",
  required: true,
}];

async function handleForgotPassword() {
  isLoading.value = true;
  clearError();
  try {
    toast.add({
      title: "Reset Link Sent",
      description: "If an account exists with that email, a password reset link has been sent.",
      icon: "i-lucide:mail-check",
      color: "primary",
    });
  }
  catch (err: unknown) {
    error.value = resolveError(err, "Failed to send reset link. Please try again.");
  }
  finally {
    isLoading.value = false;
  }
}
</script>

<template>
  <Auth
    image-light="/images/login.jpg"
    image-dark="/images/login-dark.jpg"
    image-alt="Password recovery visual"
    :back-to="{ name: '/auth/login' }"
    back-label="Back to login"
  >
    <UAuthForm
      :schema="forgotPasswordSchema"
      title="Reset Password"
      description="Enter your email to receive instructions to reset your password."
      icon="lucide:key-round"
      :fields="forgotPasswordFields"
      :loading="isLoading"
      :submit="{
        label: isLoading ? '' : 'Send Reset Link',
        icon: 'i-lucide:send',
        loading: isLoading,
        disabled: isLoading,
        class: 'cursor-pointer',
      }"
      :ui="{
        title: 'font-mono',
      }"
      @submit="handleForgotPassword"
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
        Remember your password?
        <ULink
          to="/auth/login"
          class="font-medium text-secondary hover:text-primary hover:underline"
        >
          Back to login
        </ULink>
      </p>
    </template>
  </Auth>
</template>

<style scoped></style>
