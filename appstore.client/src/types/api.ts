import type { FetchOptions, FetchResponse, ResponseType } from "ofetch";

import type { ResolveBody, ResolveResponse, RouteUrl } from "./endpoints";

/**
 * Standard backend error response format returned by ASP.NET Core AppException/ExceptionHandler
 */
export interface BackendErrorResponse {
  status: number;
  code: string;
  message: string;
  traceId?: string;
  details?: string[] | Record<string, unknown> | string | null;
}

/**
 * HTTP methods supported by the API client
 */
export type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE" | "HEAD" | "OPTIONS";

/**
 * Custom request options extending ofetch FetchOptions
 */
export interface ApiRequestOptions<R extends ResponseType = "json"> extends Omit<FetchOptions<R>, "body"> {
  /**
   * Request body (automatically serialized as JSON if object)
   */
  body?: unknown;

  /**
   * Skip automatic injection of Authorization Bearer header
   * @default false
   */
  skipAuth?: boolean;

  /**
   * Skip automatic injection of X-CSRF-TOKEN header on mutating requests
   * @default false
   */
  skipCsrf?: boolean;

  /**
   * Automatically attempt to refresh access token on 401 response and retry
   * @default true
   */
  retryOn401?: boolean;

  /**
   * Automatically attempt to fetch a fresh CSRF token if antiforgery validation fails and retry
   * @default true
   */
  retryOnCsrfFail?: boolean;

  /**
   * Internal flag to track retried requests and avoid retry loops
   */
  _isRetry?: boolean;
}

/**
 * Interface defining the methods available on the API client.
 * Features Nitro-style endpoint autocompletion and automatic response/body typing.
 */
export interface IApiClient {
  // Callable client
  <
    U extends RouteUrl<"GET"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    options?: ApiRequestOptions<R>
  ): Promise<ResolveResponse<U, "GET">>;

  <
    T = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    options?: ApiRequestOptions<R>
  ): Promise<T>;

  // GET
  get: (<
    U extends RouteUrl<"GET"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    options?: ApiRequestOptions<R>,
  ) => Promise<ResolveResponse<U, "GET">>) & (<
    T = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    options?: ApiRequestOptions<R>,
  ) => Promise<T>);

  // POST
  post: (<
    U extends RouteUrl<"POST"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    body?: ResolveBody<U, "POST">,
    options?: ApiRequestOptions<R>,
  ) => Promise<ResolveResponse<U, "POST">>) & (<
    T = unknown,
    B = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    body?: B,
    options?: ApiRequestOptions<R>,
  ) => Promise<T>);

  // PUT
  put: (<
    U extends RouteUrl<"PUT"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    body?: ResolveBody<U, "PUT">,
    options?: ApiRequestOptions<R>,
  ) => Promise<ResolveResponse<U, "PUT">>) & (<
    T = unknown,
    B = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    body?: B,
    options?: ApiRequestOptions<R>,
  ) => Promise<T>);

  // PATCH
  patch: (<
    U extends RouteUrl<"PATCH"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    body?: ResolveBody<U, "PATCH">,
    options?: ApiRequestOptions<R>,
  ) => Promise<ResolveResponse<U, "PATCH">>) & (<
    T = unknown,
    B = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    body?: B,
    options?: ApiRequestOptions<R>,
  ) => Promise<T>);

  // DELETE
  delete: (<
    U extends RouteUrl<"DELETE"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    options?: ApiRequestOptions<R>,
  ) => Promise<ResolveResponse<U, "DELETE">>) & (<
    T = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    options?: ApiRequestOptions<R>,
  ) => Promise<T>);

  // RAW
  raw: (<
    U extends RouteUrl<"GET"> | (string & {}),
    R extends ResponseType = "json",
  >(
    url: U,
    options?: ApiRequestOptions<R>,
  ) => Promise<FetchResponse<ResolveResponse<U, "GET">>>) & (<
    T = unknown,
    R extends ResponseType = "json",
  >(
    url: string,
    options?: ApiRequestOptions<R>,
  ) => Promise<FetchResponse<T>>);

  setAccessToken: (token: string | null) => void;
  getAccessToken: () => string | null;
  setRefreshToken: (token: string | null) => void;
  getRefreshToken: () => string | null;
  setCsrfToken: (token: string | null) => void;
  getCsrfToken: () => string | null;
  clearTokens: () => void;

  fetchCsrfToken: (force?: boolean) => Promise<string>;
  refreshToken: () => Promise<string>;

  onAuthFailure: (handler: (error: unknown) => void) => () => void;
  onTokenRefreshed: (handler: (token: string) => void) => () => void;
}
