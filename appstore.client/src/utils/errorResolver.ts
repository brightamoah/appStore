import { startCountdown } from "@/composables/countDown";

/**
 * Application Error Resolver and Standardized Error Types.
 *
 * Provides centralized error mapping, transformation, and user-friendly message resolution
 * mirroring the ASP.NET Core backend AppException / GlobalExceptionMiddleware architecture.
 */

/**
 * Standard Application Error Codes mirroring backend ErrorCodes & AuthErrorCodes.
 */
export const ErrorCodes = {
  // General & Validation
  VALIDATION_ERROR: "VALIDATION_ERROR",
  INVALID_REQUEST: "INVALID_REQUEST",
  NOT_FOUND: "NOT_FOUND",
  CONFLICT: "CONFLICT",
  INTERNAL_ERROR: "INTERNAL_ERROR",

  // Authentication & Session
  AUTHENTICATION_REQUIRED: "AUTHENTICATION_REQUIRED",
  INVALID_CREDENTIALS: "INVALID_CREDENTIALS",
  EMAIL_ALREADY_EXISTS: "EMAIL_ALREADY_EXISTS",
  TOKEN_MISSING: "TOKEN_MISSING",
  TOKEN_INVALID: "TOKEN_INVALID",
  TOKEN_EXPIRED: "TOKEN_EXPIRED",
  TOKEN_SIGNATURE_INVALID: "TOKEN_SIGNATURE_INVALID",
  TOKEN_ISSUER_INVALID: "TOKEN_ISSUER_INVALID",
  TOKEN_AUDIENCE_INVALID: "TOKEN_AUDIENCE_INVALID",
  TOKEN_REVOKED: "TOKEN_REVOKED",
  REFRESH_TOKEN_INVALID: "REFRESH_TOKEN_INVALID",
  REFRESH_TOKEN_EXPIRED: "REFRESH_TOKEN_EXPIRED",
  REFRESH_TOKEN_REVOKED: "REFRESH_TOKEN_REVOKED",

  // Authorization & Permissions
  FORBIDDEN: "FORBIDDEN",
  INSUFFICIENT_PERMISSION: "INSUFFICIENT_PERMISSION",

  // Users & Roles
  USER_NOT_FOUND: "USER_NOT_FOUND",
  USER_ALREADY_EXISTS: "USER_ALREADY_EXISTS",
  USER_INACTIVE: "USER_INACTIVE",
  ROLE_NOT_FOUND: "ROLE_NOT_FOUND",
  ROLE_ALREADY_EXISTS: "ROLE_ALREADY_EXISTS",
  PERMISSION_NOT_FOUND: "PERMISSION_NOT_FOUND",
  PERMISSION_ALREADY_EXISTS: "PERMISSION_ALREADY_EXISTS",
  PERMISSION_IN_USE: "PERMISSION_IN_USE",
  ROLE_PERMISSION_NOT_FOUND: "ROLE_PERMISSION_NOT_FOUND",
  ROLE_PERMISSION_ALREADY_EXISTS: "ROLE_PERMISSION_ALREADY_EXISTS",
  USER_ROLE_ALREADY_EXISTS: "USER_ROLE_ALREADY_EXISTS",
  USER_ROLE_NOT_FOUND: "USER_ROLE_NOT_FOUND",

  // Tasks & Domain Entities
  TASK_NOT_FOUND: "TASK_NOT_FOUND",
  PRIORITY_NOT_FOUND: "PRIORITY_NOT_FOUND",
  PRIORITY_ALREADY_EXISTS: "PRIORITY_ALREADY_EXISTS",

  // Password & Security Policies
  INVALID_PASSWORD_POLICY: "INVALID_PASSWORD_POLICY",
  WEAK_PASSWORD: "WEAK_PASSWORD",

  // Rate Limiting
  RATE_LIMIT_EXCEEDED: "RATE_LIMIT_EXCEEDED",

  // Client-specific & Network Errors
  NETWORK_ERROR: "NETWORK_ERROR",
  OFFLINE: "OFFLINE",
  TIMEOUT: "TIMEOUT",
  ABORTED: "ABORTED",
  CSRF_ERROR: "CSRF_ERROR",
  UNKNOWN_ERROR: "UNKNOWN_ERROR",
} as const;

export type ErrorCode = (typeof ErrorCodes)[keyof typeof ErrorCodes] | (string & {});

interface ErrorMapping {
  title: string;
  message: string;
}

/**
 * Curated human-readable titles and messages for known error codes.
 */
const ERROR_CODE_MAPPINGS: Record<string, ErrorMapping> = {
  [ErrorCodes.RATE_LIMIT_EXCEEDED]: {
    title: "Too Many Requests",
    message: "Too many requests. Please slow down and try again later.",
  },

  // Auth
  INVALID_CREDENTIALS: {
    title: "Sign In Failed",
    message: "Invalid email or password. Please verify your credentials and try again.",
  },
  EMAIL_ALREADY_EXISTS: {
    title: "Account Already Exists",
    message: "An account with this email address already exists. Please sign in instead.",
  },
  USER_ALREADY_EXISTS: {
    title: "Account Already Exists",
    message: "An account with this email address or username already exists.",
  },
  USER_NOT_FOUND: {
    title: "Account Not Found",
    message: "No account found matching the provided information.",
  },
  USER_INACTIVE: {
    title: "Account Suspended",
    message: "This account is currently inactive. Please contact support.",
  },
  AUTHENTICATION_REQUIRED: {
    title: "Sign In Required",
    message: "You must be signed in to perform this action.",
  },
  TOKEN_MISSING: {
    title: "Sign In Required",
    message: "Authentication token is missing. Please sign in again.",
  },
  TOKEN_EXPIRED: {
    title: "Session Expired",
    message: "Your session has expired. Please sign in again.",
  },
  TOKEN_INVALID: {
    title: "Session Invalid",
    message: "Your authentication session is invalid. Please sign in again.",
  },
  TOKEN_REVOKED: {
    title: "Session Revoked",
    message: "Your session has been revoked. Please sign in again.",
  },
  REFRESH_TOKEN_INVALID: {
    title: "Session Expired",
    message: "Your session has expired. Please log in again to continue.",
  },
  REFRESH_TOKEN_EXPIRED: {
    title: "Session Expired",
    message: "Your session has expired. Please log in again.",
  },
  REFRESH_TOKEN_REVOKED: {
    title: "Session Ended",
    message: "Your session was terminated. Please log in again.",
  },
  FORBIDDEN: {
    title: "Access Denied",
    message: "You do not have permission to access this resource.",
  },
  INSUFFICIENT_PERMISSION: {
    title: "Permission Denied",
    message: "You do not have the required permissions to perform this action.",
  },

  // Validation & Inputs
  VALIDATION_ERROR: {
    title: "Validation Error",
    message: "One or more fields are invalid. Please check your information and try again.",
  },
  INVALID_REQUEST: {
    title: "Invalid Request",
    message: "The request could not be processed. Please check your input.",
  },
  NOT_FOUND: {
    title: "Not Found",
    message: "The requested item could not be found.",
  },
  CONFLICT: {
    title: "Conflict",
    message: "This action conflicts with an existing record.",
  },
  TASK_NOT_FOUND: {
    title: "Task Not Found",
    message: "The requested task does not exist or has been removed.",
  },
  WEAK_PASSWORD: {
    title: "Password Too Weak",
    message: "Your password does not meet the security policy requirements.",
  },
  INVALID_PASSWORD_POLICY: {
    title: "Password Policy Error",
    message: "The password provided does not meet our security criteria.",
  },
  INTERNAL_ERROR: {
    title: "Server Error",
    message: "An unexpected server error occurred. Please try again later.",
  },

  // Network & Transport
  NETWORK_ERROR: {
    title: "Connection Failed",
    message: "Unable to connect to the server. Please check your internet connection or verify the server is running.",
  },
  OFFLINE: {
    title: "You Are Offline",
    message: "No internet connection detected. Please check your network connection.",
  },
  TIMEOUT: {
    title: "Request Timeout",
    message: "The server took too long to respond. Please try again.",
  },
  ABORTED: {
    title: "Request Cancelled",
    message: "The request was cancelled before completion.",
  },
  CSRF_ERROR: {
    title: "Security Token Expired",
    message: "Your security verification token has expired. Please refresh the page.",
  },
  UNKNOWN_ERROR: {
    title: "Error",
    message: "An unexpected error occurred. Please try again.",
  },
};

interface HttpStatusMapping {
  title: string;
  message: string;
  code: string;
}

/**
 * Fallback titles and messages by HTTP status code when the server does not supply custom JSON.
 */
const HTTP_STATUS_MAPPINGS: Record<number, HttpStatusMapping> = {
  400: {
    title: "Invalid Request",
    message: "The request was invalid or incomplete. Please check your input.",
    code: ErrorCodes.INVALID_REQUEST,
  },
  401: {
    title: "Unauthorized",
    message: "You must be signed in to perform this action.",
    code: ErrorCodes.AUTHENTICATION_REQUIRED,
  },
  403: {
    title: "Access Denied",
    message: "You do not have permission to perform this action.",
    code: ErrorCodes.FORBIDDEN,
  },
  404: {
    title: "Not Found",
    message: "The requested resource could not be found.",
    code: ErrorCodes.NOT_FOUND,
  },
  408: {
    title: "Request Timeout",
    message: "The request timed out. Please try again.",
    code: ErrorCodes.TIMEOUT,
  },
  409: {
    title: "Conflict",
    message: "The requested change conflicts with existing data.",
    code: ErrorCodes.CONFLICT,
  },
  422: {
    title: "Validation Error",
    message: "The data provided could not be processed.",
    code: ErrorCodes.VALIDATION_ERROR,
  },
  429: {
    title: "Too Many Requests",
    message: "Too many requests. Please slow down and try again later.",
    code: ErrorCodes.RATE_LIMIT_EXCEEDED,
  },
  500: {
    title: "Server Error",
    message: "An internal server error occurred. Please try again later.",
    code: ErrorCodes.INTERNAL_ERROR,
  },
  502: {
    title: "Bad Gateway",
    message: "The server is temporarily unreachable or starting up. Please try again shortly.",
    code: "BAD_GATEWAY",
  },
  503: {
    title: "Service Unavailable",
    message: "The server is currently unavailable. Please try again later.",
    code: "SERVICE_UNAVAILABLE",
  },
  504: {
    title: "Gateway Timeout",
    message: "The upstream server took too long to reply. Please try again.",
    code: "GATEWAY_TIMEOUT",
  },
};

function getCodeMapping(code: string): ErrorMapping {
  return ERROR_CODE_MAPPINGS[code] ?? {
    title: "Error",
    message: "An unexpected error occurred. Please try again.",
  };
}

function getHttpStatusMapping(status: number): HttpStatusMapping {
  return HTTP_STATUS_MAPPINGS[status] ?? {
    title: "Error",
    message: "An unexpected error occurred. Please try again.",
    code: ErrorCodes.UNKNOWN_ERROR,
  };
}

/**
 * Detects raw technical error messages (such as from ofetch, XMLHttpRequest, or browser fetch).
 */
export function isTechnicalOrFetchError(msg: string): boolean {
  if (!msg || typeof msg !== "string") return false;
  const lower = msg.toLowerCase();
  return (
    lower.includes("failed to fetch")
    || lower.includes("networkerror")
    || lower.includes("net::err_")
    || lower.includes("load failed")
    || lower.includes("<no response>")
    || lower.includes("fetch failed")
    || lower.includes("econnrefused")
    || lower.includes("etimedout")
    || /^\[(?:get|post|put|patch|delete|head|options)\]/i.test(msg.trim())
  );
}

/**
 * Extracts a flattened list of user-friendly validation error strings from
 * ASP.NET Core Validation ProblemDetails or string array.
 */
export function extractDetailList(
  details?: string[] | Record<string, unknown> | string | null,
): string[] {
  if (!details) return [];

  if (typeof details === "string") {
    return [details];
  }

  if (Array.isArray(details)) {
    return details.map(d => String(d)).filter(Boolean);
  }

  if (typeof details === "object") {
    const list: string[] = [];
    for (const [key, val] of Object.entries(details)) {
      if (Array.isArray(val)) {
        for (const item of val) {
          if (item) list.push(String(item));
        }
      }
      else if (typeof val === "string" && val) {
        list.push(val);
      }
      else if (val) {
        list.push(`${key}: ${JSON.stringify(val)}`);
      }
    }
    return list;
  }

  return [];
}

/**
 * Options for constructing an AppError instance.
 */
export interface AppErrorOptions {
  message: string;
  title?: string;
  code?: string;
  status?: number;
  traceId?: string;
  details?: string[] | Record<string, unknown> | string | null;
  retryAfterSeconds?: number;
  raw?: unknown;
}

/**
 * Standard Application Error class.
 * Ensures all errors across the client have clean, human-readable titles and messages,
 * structured error codes, HTTP status, and validation details.
 */
export class AppError extends Error {
  readonly status: number;
  readonly code: string;
  readonly title: string;
  readonly traceId?: string;
  readonly details?: string[] | Record<string, unknown> | string | null;
  readonly detailList: string[];
  readonly retryAfterSeconds?: number;
  readonly raw?: unknown;

  constructor(options: AppErrorOptions) {
    super(options.message);
    this.name = "AppError";
    this.status = options.status ?? (options.code === ErrorCodes.NETWORK_ERROR ? 0 : 500);
    this.code = options.code ?? (this.status === 0 ? ErrorCodes.NETWORK_ERROR : ErrorCodes.UNKNOWN_ERROR);

    // Pick human-friendly title
    this.title
      = options.title
        || ERROR_CODE_MAPPINGS[this.code]?.title
        || HTTP_STATUS_MAPPINGS[this.status]?.title
        || "Error";

    this.traceId = options.traceId;
    this.details = options.details;
    this.detailList = extractDetailList(options.details);
    this.raw = options.raw;

    // Resolve retryAfterSeconds
    if (typeof options.retryAfterSeconds === "number" && options.retryAfterSeconds > 0) {
      this.retryAfterSeconds = Math.ceil(options.retryAfterSeconds);
    }
    else if (typeof options.details === "object" && options.details !== null && "retryAfterSeconds" in options.details) {
      const val = Number((options.details as Record<string, unknown>).retryAfterSeconds);
      if (!Number.isNaN(val) && val > 0) {
        this.retryAfterSeconds = Math.ceil(val);
      }
    }
    else if (this.status === 429 || this.code === ErrorCodes.RATE_LIMIT_EXCEEDED) {
      this.retryAfterSeconds = 60;
    }

    Object.setPrototypeOf(this, AppError.prototype);
  }

  get description(): string {
    return this.message;
  }

  get isNetworkError(): boolean {
    return this.code === ErrorCodes.NETWORK_ERROR || this.code === ErrorCodes.OFFLINE || this.status === 0;
  }

  get isTimeout(): boolean {
    return this.code === ErrorCodes.TIMEOUT || this.code === ErrorCodes.ABORTED;
  }

  get isUnauthorized(): boolean {
    return this.status === 401;
  }

  get isForbidden(): boolean {
    return this.status === 403;
  }

  get isNotFound(): boolean {
    return this.status === 404;
  }

  get isValidation(): boolean {
    return (
      this.status === 400
      || this.status === 422
      || this.code === ErrorCodes.VALIDATION_ERROR
      || this.code === ErrorCodes.INVALID_REQUEST
    );
  }

  get isConflict(): boolean {
    return (
      this.status === 409
      || this.code === ErrorCodes.CONFLICT
      || this.code === ErrorCodes.EMAIL_ALREADY_EXISTS
      || this.code === ErrorCodes.USER_ALREADY_EXISTS
    );
  }

  get isServerError(): boolean {
    return this.status >= 500;
  }

  get isRateLimited(): boolean {
    return this.status === 429 || this.code === ErrorCodes.RATE_LIMIT_EXCEEDED;
  }
}

/**
 * Resolves ANY caught error (unknown, Error, FetchError, ApiError, string, null, etc.)
 * into a strongly-typed, human-readable AppError.
 *
 * Guaranteed never to return technical strings like `[POST] "/api/v1/auth/login": <no response> Failed to fetch`.
 *
 * @param error - The caught error
 * @param fallbackMessage - Contextual fallback message to use if error message cannot be determined
 */
export function resolveError(error: unknown, fallbackMessage?: string): AppError {
  // 1. If it's already an AppError, return it or enhance if missing message
  if (error instanceof AppError) {
    if (error.isRateLimited && error.retryAfterSeconds) {
      startCountdown(error.retryAfterSeconds);
    }

    if (fallbackMessage && (!error.message || isTechnicalOrFetchError(error.message))) {
      return new AppError({
        message: fallbackMessage,
        title: error.title,
        code: error.code,
        status: error.status,
        traceId: error.traceId,
        details: error.details,
        retryAfterSeconds: error.retryAfterSeconds,
        raw: error.raw,
      });
    }
    return error;
  }

  // 2. Check browser offline state
  if (typeof navigator !== "undefined" && navigator.onLine === false) {
    const offlineMapping = getCodeMapping(ErrorCodes.OFFLINE);
    return new AppError({
      title: offlineMapping.title,
      message: offlineMapping.message,
      code: ErrorCodes.OFFLINE,
      status: 0,
      raw: error,
    });
  }

  // 3. Inspect object-based errors (ofetch FetchError, standard Error, ApiError, etc.)
  if (error && typeof error === "object") {
    const err = error as Record<string, unknown>;
    const rawMessage = typeof err.message === "string" ? err.message : "";
    const name = typeof err.name === "string" ? err.name : "";

    // 3a. Handle AbortError / Timeout
    if (name === "AbortError" || name === "TimeoutError" || rawMessage.includes("timeout") || rawMessage.includes("aborted")) {
      const timeoutMapping = getCodeMapping(ErrorCodes.TIMEOUT);
      return new AppError({
        title: timeoutMapping.title,
        message: timeoutMapping.message,
        code: ErrorCodes.TIMEOUT,
        status: 408,
        raw: error,
      });
    }

    // 3b. Extract backend payload if present (e.g. ofetch error.data or error.response._data)
    const backendData = (err.data || (err.response as Record<string, unknown> | undefined)?._data) as
      | Record<string, unknown>
      | undefined;

    const response = err.response as Response | undefined;
    const status
      = (typeof err.statusCode === "number"
        ? err.statusCode
        : typeof err.status === "number"
          ? err.status
          : response?.status) ?? (isTechnicalOrFetchError(rawMessage) ? 0 : 500);

    // 3c. If backend returned structured JSON
    if (backendData && typeof backendData === "object") {
      const code = (typeof backendData.code === "string" ? backendData.code : undefined)
        || HTTP_STATUS_MAPPINGS[status]?.code
        || ErrorCodes.UNKNOWN_ERROR;
      const traceId = typeof backendData.traceId === "string" ? backendData.traceId : undefined;

      // Extract details & ASP.NET Validation errors
      const details = (backendData.details || backendData.errors) as
        | string[]
        | Record<string, unknown>
        | string
        | null
        | undefined;
      const detailList = extractDetailList(details);

      // Determine human-friendly message
      let message = typeof backendData.message === "string" ? backendData.message : "";

      const backendTitle = typeof backendData.title === "string" ? backendData.title : undefined;

      // If backend message was empty or technical, check known mappings or ASP.NET title
      if (!message || isTechnicalOrFetchError(message)) {
        if (backendTitle && !isTechnicalOrFetchError(backendTitle)) {
          message = backendTitle;
        }
        else if (ERROR_CODE_MAPPINGS[code]?.message) {
          message = ERROR_CODE_MAPPINGS[code].message;
        }
        else if (HTTP_STATUS_MAPPINGS[status]?.message) {
          message = HTTP_STATUS_MAPPINGS[status].message;
        }
        else if (fallbackMessage) {
          message = fallbackMessage;
        }
        else {
          message = "An error occurred while processing your request.";
        }
      }

      // If there are specific validation details, append the first detail if message is generic
      if (detailList.length > 0 && (message === "Validation Error" || message.includes("One or more validation errors"))) {
        message = detailList[0] || message;
      }

      const title
        = ERROR_CODE_MAPPINGS[code]?.title
          || backendTitle
          || HTTP_STATUS_MAPPINGS[status]?.title;

      // Extract retryAfterSeconds if present (e.g. from rate limiting details or response headers)
      let retryAfterSeconds: number | undefined;
      if (typeof details === "object" && details !== null && "retryAfterSeconds" in details) {
        const val = Number((details as Record<string, unknown>).retryAfterSeconds);
        if (!Number.isNaN(val) && val > 0) {
          retryAfterSeconds = Math.ceil(val);
        }
      }
      if (!retryAfterSeconds && response?.headers) {
        const headerVal = typeof response.headers.get === "function" ? response.headers.get("retry-after") : undefined;
        if (headerVal) {
          const parsed = parseInt(headerVal, 10);
          if (!Number.isNaN(parsed) && parsed > 0) {
            retryAfterSeconds = parsed;
          }
        }
      }
      if (!retryAfterSeconds && (status === 429 || code === ErrorCodes.RATE_LIMIT_EXCEEDED)) {
        retryAfterSeconds = 60;
      }

      const resolved = new AppError({
        message,
        title,
        code,
        status: (typeof backendData.status === "number" ? backendData.status : undefined) || status,
        traceId,
        details,
        retryAfterSeconds,
        raw: error,
      });

      if (resolved.isRateLimited && resolved.retryAfterSeconds) {
        startCountdown(resolved.retryAfterSeconds);
      }

      return resolved;
    }

    // 3d. If network failure (status 0, failed to fetch, server down)
    if (status === 0 || isTechnicalOrFetchError(rawMessage)) {
      const networkMapping = getCodeMapping(ErrorCodes.NETWORK_ERROR);
      return new AppError({
        title: networkMapping.title,
        message: fallbackMessage || networkMapping.message,
        code: ErrorCodes.NETWORK_ERROR,
        status: 0,
        raw: error,
      });
    }

    // 3e. If HTTP status is known (e.g. 502 Bad Gateway with HTML body, or 429 without body)
    if (HTTP_STATUS_MAPPINGS[status]) {
      const mapping = getHttpStatusMapping(status);
      let retryAfterSeconds: number | undefined;

      if (status === 429) {
        const headerVal = response?.headers && typeof response.headers.get === "function" ? response.headers.get("retry-after") : undefined;
        retryAfterSeconds = headerVal ? parseInt(headerVal, 10) || 60 : 60;
        startCountdown(retryAfterSeconds);
      }

      return new AppError({
        title: mapping.title,
        message: fallbackMessage || mapping.message,
        code: mapping.code,
        status,
        retryAfterSeconds,
        raw: error,
      });
    }

    // 3f. Standard JS Error
    if (rawMessage && !isTechnicalOrFetchError(rawMessage)) {
      return new AppError({
        message: rawMessage,
        title: "Error",
        code: ErrorCodes.UNKNOWN_ERROR,
        status,
        raw: error,
      });
    }
  }

  // 4. String error
  if (typeof error === "string" && error.trim().length > 0) {
    if (isTechnicalOrFetchError(error)) {
      const networkMapping = getCodeMapping(ErrorCodes.NETWORK_ERROR);
      return new AppError({
        title: networkMapping.title,
        message: fallbackMessage || networkMapping.message,
        code: ErrorCodes.NETWORK_ERROR,
        status: 0,
        raw: error,
      });
    }
    return new AppError({
      message: error,
      title: "Error",
      code: ErrorCodes.UNKNOWN_ERROR,
      status: 500,
      raw: error,
    });
  }

  // 5. Total fallback
  const unknownMapping = getCodeMapping(ErrorCodes.UNKNOWN_ERROR);
  return new AppError({
    message: fallbackMessage || unknownMapping.message,
    title: unknownMapping.title,
    code: ErrorCodes.UNKNOWN_ERROR,
    status: 500,
    raw: error,
  });
}

/**
 * Returns only the resolved user-friendly message string for convenient inline usage.
 */
export function resolveErrorMessage(error: unknown, fallbackMessage?: string): string {
  return resolveError(error, fallbackMessage).message;
}
