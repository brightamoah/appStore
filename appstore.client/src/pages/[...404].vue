<script setup lang="ts">
import { computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuthStore } from "@/stores/authStore";

export interface AppError {
  statusCode?: number | string;
  status?: number | string;
  statusMessage?: string;
  statusText?: string;
  message?: string;
  code?: string;
  traceId?: string;
  details?: unknown;
}

interface ErrorMeta {
  statusMessage: string;
  defaultMessage: string;
  icon: string;
}

const props = defineProps<{
  error?: AppError;
}>();

const ERROR_METADATA: Record<number | string, ErrorMeta> = {
  400: {
    statusMessage: "Bad Request",
    defaultMessage: "The request could not be understood or processed due to invalid parameters.",
    icon: "i-lucide:file-warning",
  },
  401: {
    statusMessage: "Unauthorized",
    defaultMessage: "You must be signed in with valid credentials to access this page.",
    icon: "i-lucide:lock-keyhole",
  },
  403: {
    statusMessage: "Access Denied",
    defaultMessage: "You do not have the required permissions to access this page or resource.",
    icon: "i-lucide:shield-ban",
  },
  404: {
    statusMessage: "Page Not Found",
    defaultMessage: "The page you are looking for does not exist, has been removed, or is temporarily unavailable.",
    icon: "i-lucide:compass",
  },
  408: {
    statusMessage: "Request Timeout",
    defaultMessage: "The server timed out waiting for the request. Please try again.",
    icon: "i-lucide:clock-alert",
  },
  429: {
    statusMessage: "Too Many Requests",
    defaultMessage: "You have sent too many requests in a short period. Please wait a moment and try again.",
    icon: "i-lucide:gauge",
  },
  500: {
    statusMessage: "Internal Server Error",
    defaultMessage: "Our server encountered an unexpected error while processing your request. Please try again later.",
    icon: "i-lucide:server-crash",
  },
  502: {
    statusMessage: "Bad Gateway",
    defaultMessage: "The server received an invalid response from an upstream server. Please try again later.",
    icon: "i-lucide:plug-zap",
  },
  503: {
    statusMessage: "Service Unavailable",
    defaultMessage: "The server is temporarily unable to handle the request. Please check back soon.",
    icon: "i-lucide:briefcase",
  },
  504: {
    statusMessage: "Gateway Timeout",
    defaultMessage: "The server, while acting as a gateway or proxy, did not receive a timely response from the upstream server. Please try again.",
    icon: "i-lucide:timer",
  },
  507: {
    statusMessage: "Insufficient Storage",
    defaultMessage: "The server is unable to store the representation needed to complete the request. Please clear some space or try again later.",
    icon: "i-lucide:database-crash",
  },
};

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();

const rawState = typeof window !== "undefined"
  ? (window.history?.state as Record<string, unknown> | null)
  : null;
const stateError = (rawState?.error && typeof rawState.error === "object")
  ? (rawState.error as Record<string, unknown>)
  : rawState;

const homePath = computed(() => (authStore.isAuthenticated ? "/tasks" : "/"));

const resolvedStatusCode = computed<number | string>(() => {
  if (props.error?.statusCode) return props.error.statusCode;
  if (props.error?.status) return props.error.status;
  if (stateError?.statusCode && (typeof stateError.statusCode === "number" || typeof stateError.statusCode === "string")) {
    return stateError.statusCode;
  }
  if (stateError?.status && (typeof stateError.status === "number" || typeof stateError.status === "string")) {
    return stateError.status;
  }
  if (route.query.code && (typeof route.query.code === "string" || typeof route.query.code === "number")) {
    return route.query.code;
  }
  if (route.query.status && (typeof route.query.status === "string" || typeof route.query.status === "number")) {
    return route.query.status;
  }
  if (route.query.statusCode && (typeof route.query.statusCode === "string" || typeof route.query.statusCode === "number")) {
    return route.query.statusCode;
  }
  return 404;
});

const is404 = computed(() => String(resolvedStatusCode.value) === "404");

const resolvedCode = computed<string | undefined>(() => {
  return props.error?.code
    ?? (typeof stateError?.code === "string" ? stateError.code : undefined)
    ?? (typeof route.query.errorCode === "string" ? route.query.errorCode : undefined);
});
const resolvedTraceId = computed<string | undefined>(() => {
  return props.error?.traceId
    ?? (typeof stateError?.traceId === "string" ? stateError.traceId : undefined)
    ?? (typeof route.query.traceId === "string" ? route.query.traceId : undefined);
});
const resolvedDetails = computed<unknown>(() => {
  return props.error?.details ?? stateError?.details;
});
const resolvedStatusMessage = computed<string>(() => {
  if (props.error?.statusMessage) return props.error.statusMessage;
  if (props.error?.statusText) return props.error.statusText;
  if (typeof stateError?.statusMessage === "string" && stateError.statusMessage) {
    return stateError.statusMessage;
  }
  if (typeof stateError?.statusText === "string" && stateError.statusText) {
    return stateError.statusText;
  }
  if (typeof route.query.statusMessage === "string" && route.query.statusMessage) {
    return route.query.statusMessage;
  }
  const code = Number(resolvedStatusCode.value);
  if (!Number.isNaN(code) && ERROR_METADATA[code]?.statusMessage) {
    return ERROR_METADATA[code].statusMessage;
  }
  return "Page Not Found";
});

const resolvedMessage = computed<string>(() => {
  if (props.error?.message) return props.error.message;
  if (typeof stateError?.message === "string" && stateError.message) {
    return stateError.message;
  }
  if (typeof route.query.message === "string" && route.query.message) {
    return route.query.message;
  }
  const code = Number(resolvedStatusCode.value);
  if (is404.value) {
    return `Sorry, the page "${route.path}" could not be found or does not exist.`;
  }
  if (!Number.isNaN(code) && ERROR_METADATA[code]?.defaultMessage) {
    return ERROR_METADATA[code].defaultMessage;
  }
  return "An unexpected error occurred while loading this page.";
});

const resolvedIcon = computed<string>(() => {
  const code = Number(resolvedStatusCode.value);
  if (!Number.isNaN(code) && ERROR_METADATA[code]?.icon) {
    return ERROR_METADATA[code].icon;
  }
  return "i-lucide:package-x";
});

const resolvedError = computed<AppError>(() => ({
  statusCode: resolvedStatusCode.value,
  statusMessage: resolvedStatusMessage.value,
  message: resolvedMessage.value,
  code: resolvedCode.value,
  traceId: resolvedTraceId.value,
  details: resolvedDetails.value,
}));

function goBack() {
  if (typeof window !== "undefined" && window.history.length > 1) {
    router.back();
  }
  else {
    router.push(homePath.value);
  }
}
</script>

<template>
  <div class="flex justify-center items-center p-4 w-full min-h-screen text-default">
    <UError
      :error="resolvedError"
      :icon="resolvedIcon"
      :ui="{

        leadingIcon: 'size-12 text-primary',
        statusCode: 'font-mono text-sm font-semibold tracking-wider text-primary uppercase',
        statusMessage: 'mt-2 text-2xl sm:text-3xl font-bold font-mono tracking-tight text-highlighted text-balance',
        message: 'mt-3 text-sm text-muted text-balance',
        links: 'mt-6 flex flex-wrap items-center justify-center gap-3',
      }"
    >
      <template #links>
        <UButton
          icon="i-lucide:arrow-left"
          label="Go back"
          size="sm"
          variant="ghost"
          color="neutral"
          class="rounded-tl-2xl rounded-br-2xl"
          @click="goBack"
        />

        <UButton
          :to="homePath"
          icon="i-lucide:layout-dashboard"
          :label="authStore.isAuthenticated ? 'Back to tasks' : 'Back to home'"
          color="primary"
          variant="solid"
          size="sm"
          class="rounded-tl-2xl rounded-br-2xl"
        />
      </template>
    </UError>
  </div>
</template>
