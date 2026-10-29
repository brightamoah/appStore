import type { FetchOptions, FetchResponse, ResponseType } from "ofetch";
import type {
  ApiRequestOptions,
  HttpMethod,
  IApiClient,
} from "@/types/api";
import type { AuthResponse, CsrfResponse } from "@/types/auth";
import { createFetch } from "ofetch";

/**
 * Custom strongly typed error class for all API errors.
 * Inherits from AppError to ensure unified status, error code, human-readable message,
 * and validation details matching the ASP.NET Core backend.
 */
export class ApiError<T = unknown> extends AppError {
  readonly data?: T;
  readonly response?: Response;

  constructor(
    message: string,
    options: {
      status?: number;
      code?: string;
      title?: string;
      traceId?: string;
      details?: string[] | Record<string, unknown> | string | null;
      data?: T;
      response?: Response;
      cause?: unknown;
    } = {},
  ) {
    super({
      message,
      title: options.title,
      code: options.code,
      status: options.status,
      traceId: options.traceId,
      details: options.details,
      raw: options.cause,
    });
    this.name = "ApiError";
    this.data = options.data;
    this.response = options.response;
    Object.setPrototypeOf(this, ApiError.prototype);
  }

  /**
   * Helper to check if this error was caused by antiforgery / CSRF token validation failure.
   */
  get isCsrfError(): boolean {
    const msg = (this.message || "").toLowerCase();
    const detailsStr = typeof this.details === "string" ? this.details.toLowerCase() : "";
    return (
      (this.status === 400 || this.status === 403)
      && (msg.includes("antiforgery")
        || msg.includes("x-csrf-token")
        || msg.includes("csrf")
        || detailsStr.includes("antiforgery")
        || detailsStr.includes("x-csrf-token"))
    );
  }
}

/**
 * Converts any caught error into a standardized ApiError with human-friendly message,
 * status code, error code, and validation details.
 */
function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error;
  }

  const resolved = resolveError(error);
  const errObj = error && typeof error === "object" ? (error as Record<string, unknown>) : undefined;
  const data = (errObj?.data || (errObj?.response as Record<string, unknown> | undefined)?._data) as unknown;
  const response = errObj?.response as Response | undefined;

  return new ApiError(resolved.message, {
    status: resolved.status,
    code: resolved.code,
    title: resolved.title,
    traceId: resolved.traceId,
    details: resolved.details,
    data,
    response,
    cause: error,
  });
}

/**
 * Storage keys used to persist tokens in localStorage across reloads
 */
const ACCESS_TOKEN_KEY = "app_store_access_token";
const REFRESH_TOKEN_KEY = "app_store_refresh_token";

/**
 * Helpers to safely access localStorage in browser environments
 */
function getStoredToken(): string | null {
  if (typeof window !== "undefined" && window.localStorage) {
    try {
      return window.localStorage.getItem(ACCESS_TOKEN_KEY);
    }
    catch {
      return null;
    }
  }
  return null;
}

function setStoredToken(token: string | null): void {
  if (typeof window !== "undefined" && window.localStorage) {
    try {
      if (token) {
        window.localStorage.setItem(ACCESS_TOKEN_KEY, token);
      }
      else {
        window.localStorage.removeItem(ACCESS_TOKEN_KEY);
      }
    }
    catch {
      // Ignore storage errors
    }
  }
}

function getStoredRefreshToken(): string | null {
  if (typeof window !== "undefined" && window.localStorage) {
    try {
      return window.localStorage.getItem(REFRESH_TOKEN_KEY);
    }
    catch {
      return null;
    }
  }
  return null;
}

function setStoredRefreshToken(token: string | null): void {
  if (typeof window !== "undefined" && window.localStorage) {
    try {
      if (token) {
        window.localStorage.setItem(REFRESH_TOKEN_KEY, token);
      }
      else {
        window.localStorage.removeItem(REFRESH_TOKEN_KEY);
      }
    }
    catch {
      // Ignore storage errors
    }
  }
}

/**
 * Checks if a normalized endpoint path belongs to the auth route group
 * where 401 should NOT trigger automatic token refresh loops.
 */
function isAuthEndpoint(url: string): boolean {
  const lower = url.toLowerCase();
  return (
    lower.includes("/api/v1/auth/login")
    || lower.includes("/api/v1/auth/refresh")
    || lower.includes("/api/v1/auth/signup")
    || lower.includes("/api/v1/auth/csrf")
  );
}

/**
 * Normalizes request URLs by ensuring the `/api` prefix without duplicating it
 * and preserving absolute URLs.
 */
function normalizeUrl(url: string, baseURL = "/api"): string {
  if (/^https?:\/\//i.test(url)) {
    return url;
  }
  if (!baseURL) {
    return url.startsWith("/") ? url : `/${url}`;
  }
  const cleanBase = baseURL.replace(/\/+$/, "");
  const cleanUrl = url.replace(/^\/+/, "");
  const baseWithoutSlash = cleanBase.replace(/^\/+/, "");

  if (cleanUrl === baseWithoutSlash || cleanUrl.startsWith(`${baseWithoutSlash}/`)) {
    return `/${cleanUrl}`;
  }
  return `${cleanBase}/${cleanUrl}`;
}

const MUTATING_METHODS = new Set<string>(["POST", "PUT", "PATCH", "DELETE"]);

export interface ApiClientConfig {
  baseURL?: string;
  defaultHeaders?: Record<string, string>;
}

/**
 * Factory creating a fully configured IApiClient instance.
 */
export function createApiClient(config: ApiClientConfig = {}): IApiClient {
  const baseURL = config.baseURL || "/api";

  // Internal state
  let accessToken: string | null = getStoredToken();
  let fallbackRefreshToken: string | null = getStoredRefreshToken();
  let csrfToken: string | null = null;
  let csrfPromise: Promise<string> | null = null;
  let refreshPromise: Promise<string> | null = null;

  type AuthFailureHandler = (error: unknown) => void;
  type TokenRefreshedHandler = (token: string) => void;

  const authFailureListeners = new Set<AuthFailureHandler>();
  const tokenRefreshedListeners = new Set<TokenRefreshedHandler>();

  function notifyAuthFailure(error: unknown): void {
    for (const listener of authFailureListeners) {
      try {
        listener(error);
      }
      catch (listenerError) {
        console.error("Error in auth failure listener:", listenerError);
      }
    }
  }

  function notifyTokenRefreshed(token: string): void {
    for (const listener of tokenRefreshedListeners) {
      try {
        listener(token);
      }
      catch (listenerError) {
        console.error("Error in token refreshed listener:", listenerError);
      }
    }
  }

  // Base raw ofetch instance configured with credentials: 'include'
  const rawFetch = createFetch({
    defaults: {
      credentials: "include",
    },
  });

  /**
   * Retrieves CSRF token, deduplicating simultaneous requests via single in-flight promise.
   */
  async function fetchCsrfToken(force = false): Promise<string> {
    if (!force && csrfToken) {
      return csrfToken;
    }
    if (csrfPromise) {
      return csrfPromise;
    }

    csrfPromise = (async () => {
      try {
        const headers = new Headers();
        if (accessToken) {
          headers.set("Authorization", `Bearer ${accessToken}`);
        }

        const response = await rawFetch<CsrfResponse>("/api/v1/auth/csrf", {
          method: "GET",
          credentials: "include",
          headers,
        });
        if (response && response.csrfToken) {
          csrfToken = response.csrfToken;
          return csrfToken;
        }
        throw new ApiError("Failed to obtain CSRF token from server", {
          code: "CSRF_FETCH_FAILED",
          status: 500,
        });
      }
      catch (primaryError) {
        // If request with bearer token fails (e.g. token expired), retry csrf anonymously
        if (accessToken) {
          try {
            const anonymousResponse = await rawFetch<CsrfResponse>("/api/v1/auth/csrf", {
              method: "GET",
              credentials: "include",
            });
            if (anonymousResponse && anonymousResponse.csrfToken) {
              csrfToken = anonymousResponse.csrfToken;
              return csrfToken;
            }
          }
          catch {
            // Ignore fallback error and rethrow primary
          }
        }
        throw toApiError(primaryError);
      }
      finally {
        csrfPromise = null;
      }
    })();

    return csrfPromise;
  }

  /**
   * Refreshes access token via POST /api/v1/auth/refresh with single-flight mutex.
   * Primary transport is HttpOnly cookie with fallback to X-Refresh-Token header.
   */
  async function refreshAccessToken(): Promise<string> {
    if (refreshPromise) {
      return refreshPromise;
    }

    refreshPromise = (async () => {
      try {
        const headers = new Headers();
        if (fallbackRefreshToken) {
          headers.set("X-Refresh-Token", fallbackRefreshToken);
        }

        const response = await rawFetch<AuthResponse & { refreshToken?: string }>("/api/v1/auth/refresh", {
          method: "POST",
          credentials: "include",
          headers,
        });

        if (response && response.accessToken) {
          setAccessToken(response.accessToken);
          if (response.refreshToken) {
            setRefreshToken(response.refreshToken);
          }
          notifyTokenRefreshed(response.accessToken);
          return response.accessToken;
        }

        throw new ApiError("Refresh response did not contain an access token", {
          code: "REFRESH_TOKEN_INVALID",
          status: 401,
        });
      }
      catch (error) {
        clearTokens();
        const apiErr = toApiError(error);
        notifyAuthFailure(apiErr);
        throw apiErr;
      }
      finally {
        refreshPromise = null;
      }
    })();

    return refreshPromise;
  }

  function setAccessToken(token: string | null): void {
    const changed = accessToken !== token;
    accessToken = token;
    setStoredToken(token);
    if (changed) {
      csrfToken = null;
    }
  }

  function getAccessToken(): string | null {
    return accessToken;
  }

  function setRefreshToken(token: string | null): void {
    fallbackRefreshToken = token;
    setStoredRefreshToken(token);
  }

  function getRefreshToken(): string | null {
    return fallbackRefreshToken;
  }

  function setCsrfToken(token: string | null): void {
    csrfToken = token;
  }

  function getCsrfToken(): string | null {
    return csrfToken;
  }

  function clearTokens(): void {
    accessToken = null;
    csrfToken = null;
    fallbackRefreshToken = null;
    setStoredToken(null);
    setStoredRefreshToken(null);
  }

  /**
   * Main request pipeline with interceptors, token injection, and automatic retries.
   * Always returns the raw FetchResponse<T> with parsed ._data.
   */
  async function executeRequestRaw<T = unknown, R extends ResponseType = "json">(
    url: string,
    options: ApiRequestOptions<R> = {},
  ): Promise<FetchResponse<T>> {
    const requestBase = options.baseURL !== undefined ? options.baseURL : baseURL;
    const normalizedUrl = normalizeUrl(url, requestBase);
    const method = (options.method || "GET").toUpperCase() as HttpMethod;
    const headers = new Headers((options.headers as HeadersInit) || {});

    // 1. Inject default headers
    if (config.defaultHeaders) {
      for (const [key, val] of Object.entries(config.defaultHeaders)) {
        if (!headers.has(key)) {
          headers.set(key, val);
        }
      }
    }

    // 2. Inject Authorization Bearer token if present
    if (!options.skipAuth && accessToken && !headers.has("Authorization")) {
      headers.set("Authorization", `Bearer ${accessToken}`);
    }

    // 3. Inject X-CSRF-TOKEN on mutating requests
    if (!options.skipCsrf && MUTATING_METHODS.has(method)) {
      if (!headers.has("X-CSRF-TOKEN")) {
        const token = await fetchCsrfToken();
        headers.set("X-CSRF-TOKEN", token);
      }
    }

    const fetchOptions: ApiRequestOptions<R> = {
      credentials: "include",
      ...options,
      method,
      headers,
    };

    try {
      return (await rawFetch.raw<T, R>(normalizedUrl, fetchOptions as FetchOptions<R>)) as FetchResponse<T>;
    }
    catch (rawError) {
      const apiError = toApiError(rawError);

      // Retry condition 1: 401 Unauthorized -> Refresh access token and retry
      if (
        apiError.isUnauthorized
        && !options._isRetry
        && options.retryOn401 !== false
        && !isAuthEndpoint(normalizedUrl)
      ) {
        try {
          await refreshAccessToken();
          return await executeRequestRaw<T, R>(url, {
            ...options,
            _isRetry: true,
          });
        }
        catch {
          throw apiError;
        }
      }

      // Retry condition 2: Antiforgery / CSRF mismatch -> Force-refresh CSRF and retry once
      if (
        apiError.isCsrfError
        && !options._isRetry
        && options.retryOnCsrfFail !== false
        && MUTATING_METHODS.has(method)
      ) {
        try {
          const freshCsrf = await fetchCsrfToken(true);
          const retryHeaders = new Headers((options.headers as HeadersInit) || {});
          retryHeaders.set("X-CSRF-TOKEN", freshCsrf);

          return await executeRequestRaw<T, R>(url, {
            ...options,
            headers: retryHeaders,
            _isRetry: true,
          });
        }
        catch {
          throw apiError;
        }
      }

      throw apiError;
    }
  }

  /**
   * Helper that unwraps the parsed response body (_data) from executeRequestRaw.
   */
  async function executeRequest<T = unknown, R extends ResponseType = "json">(
    url: string,
    options: ApiRequestOptions<R> = {},
  ): Promise<T> {
    const response = await executeRequestRaw<T, R>(url, options);
    return response._data as T;
  }

  // Define callable client function
  const client = (async <T = unknown, R extends ResponseType = "json">(
    url: string,
    options?: ApiRequestOptions<R>,
  ): Promise<T> => {
    return executeRequest<T, R>(url, options);
  }) as IApiClient;

  // Ergonomic REST methods
  client.get = <T = unknown, R extends ResponseType = "json">(
    url: string,
    options?: ApiRequestOptions<R>,
  ): Promise<T> => {
    return executeRequest<T, R>(url, { ...options, method: "GET" });
  };

  client.post = <T = unknown, B = unknown, R extends ResponseType = "json">(
    url: string,
    body?: B,
    options?: ApiRequestOptions<R>,
  ): Promise<T> => {
    return executeRequest<T, R>(url, { ...options, method: "POST", body });
  };

  client.put = <T = unknown, B = unknown, R extends ResponseType = "json">(
    url: string,
    body?: B,
    options?: ApiRequestOptions<R>,
  ): Promise<T> => {
    return executeRequest<T, R>(url, { ...options, method: "PUT", body });
  };

  client.patch = <T = unknown, B = unknown, R extends ResponseType = "json">(
    url: string,
    body?: B,
    options?: ApiRequestOptions<R>,
  ): Promise<T> => {
    return executeRequest<T, R>(url, { ...options, method: "PATCH", body });
  };

  client.delete = <T = unknown, R extends ResponseType = "json">(
    url: string,
    options?: ApiRequestOptions<R>,
  ): Promise<T> => {
    return executeRequest<T, R>(url, { ...options, method: "DELETE" });
  };

  client.raw = <T = unknown, R extends ResponseType = "json">(
    url: string,
    options?: ApiRequestOptions<R>,
  ): Promise<FetchResponse<T>> => {
    return executeRequestRaw<T, R>(url, options);
  };

  // State & token management
  client.setAccessToken = setAccessToken;
  client.getAccessToken = getAccessToken;
  client.setRefreshToken = setRefreshToken;
  client.getRefreshToken = getRefreshToken;
  client.setCsrfToken = setCsrfToken;
  client.getCsrfToken = getCsrfToken;
  client.clearTokens = clearTokens;
  client.fetchCsrfToken = fetchCsrfToken;
  client.refreshToken = refreshAccessToken;

  // Listeners
  client.onAuthFailure = (handler: (error: unknown) => void): (() => void) => {
    authFailureListeners.add(handler);
    return () => {
      authFailureListeners.delete(handler);
    };
  };

  client.onTokenRefreshed = (handler: (token: string) => void): (() => void) => {
    tokenRefreshedListeners.add(handler);
    return () => {
      tokenRefreshedListeners.delete(handler);
    };
  };

  return client;
}

/**
 * Default pre-configured API client instance ready to use across the entire application.
 */
export const api = createApiClient();
export default api;
