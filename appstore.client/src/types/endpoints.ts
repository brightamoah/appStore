import type {
  CreatePermissionPayload,
  CreatePriorityPayload,
  CreateRolePayload,
  CreateTaskPayload,
  CreateUserPayload,
  CsrfResponse,
  LoginResponse,
  LoginSchema,
  PasswordPolicy,
  PatchTaskPayload,
  Permission,
  Priority,
  RefreshResponse,
  Role,
  RolePermission,
  SignUpResponse,
  Task,
  TaskListItem,
  TestPermissionResponse,
  UpdatePasswordPolicyPayload,
  UpdatePermissionPayload,
  UpdatePriorityPayload,
  UpdateRolePayload,
  UpdateTaskPayload,
  User,
  UserRole,
  WeatherForecast,
} from "./index";

/**
 * Single source of truth for all API routes, request bodies, and responses.
 * Add new endpoints here to automatically gain autocomplete and type inference across the app.
 */
export interface ApiRoutes {
  // Auth
  "/api/v1/auth/csrf": {
    GET: { response: CsrfResponse };
  };
  "/api/v1/auth/me": {
    GET: { response: User };
  };
  "/api/v1/auth/login": {
    POST: { body: LoginSchema; response: LoginResponse };
  };
  "/api/v1/auth/signup": {
    POST: { body: CreateUserPayload; response: SignUpResponse };
  };
  "/api/v1/auth/refresh": {
    POST: { body?: unknown; response: RefreshResponse };
  };
  "/api/v1/auth/logout": {
    POST: { body?: unknown; response: void };
  };
  "/api/v1/auth/{id}": {
    GET: { response: User };
  };
  "/api/v1/auth/:id": {
    GET: { response: User };
  };

  // Permission
  "/api/v1/permissions": {
    GET: { response: Permission[] };
    POST: { body: CreatePermissionPayload; response: Permission };
  };
  "/api/v1/permissions/{id}": {
    GET: { response: Permission };
    PATCH: { body: UpdatePermissionPayload; response: Permission };
    DELETE: { response: void };
  };
  "/api/v1/permissions/:id": {
    GET: { response: Permission };
    PATCH: { body: UpdatePermissionPayload; response: Permission };
    DELETE: { response: void };
  };

  // Priority
  "/api/v1/priorities": {
    GET: { response: Priority[] };
    POST: { body: CreatePriorityPayload; response: Priority };
  };
  "/api/v1/priorities/{id}": {
    GET: { response: Priority };
    PATCH: { body: UpdatePriorityPayload; response: void };
    DELETE: { response: void };
  };
  "/api/v1/priorities/:id": {
    GET: { response: Priority };
    PATCH: { body: UpdatePriorityPayload; response: void };
    DELETE: { response: void };
  };

  // Role
  "/api/v1/roles": {
    GET: { response: Role[] };
    POST: { body: CreateRolePayload; response: Role };
  };
  "/api/v1/roles/{id}": {
    GET: { response: Role };
    PATCH: { body: UpdateRolePayload; response: void };
    DELETE: { response: void };
  };
  "/api/v1/roles/:id": {
    GET: { response: Role };
    PATCH: { body: UpdateRolePayload; response: void };
    DELETE: { response: void };
  };

  // RolePermission
  "/api/v1/roles/{roleId}/permissions": {
    GET: { response: RolePermission[] };
  };
  "/api/v1/roles/:roleId/permissions": {
    GET: { response: RolePermission[] };
  };
  "/api/v1/roles/{roleId}/permissions/{permissionId}": {
    GET: { response: RolePermission };
    POST: { body?: unknown; response: RolePermission };
    DELETE: { response: void };
  };
  "/api/v1/roles/:roleId/permissions/:permissionId": {
    GET: { response: RolePermission };
    POST: { body?: unknown; response: RolePermission };
    DELETE: { response: void };
  };

  // Security
  "/api/v1/security/password-policy": {
    GET: { response: PasswordPolicy };
    PUT: { body: UpdatePasswordPolicyPayload; response: PasswordPolicy };
  };
  "/api/v1/security/password-policy/reset": {
    POST: { body?: unknown; response: void };
  };

  // TaskItems
  "/api/v1/tasks": {
    GET: { response: TaskListItem[] };
    POST: { body: CreateTaskPayload; response: Task };
  };
  "/api/v1/tasks/{id}": {
    GET: { response: Task };
    PUT: { body: UpdateTaskPayload; response: void };
    PATCH: { body: PatchTaskPayload; response: Task };
    DELETE: { response: void };
  };
  "/api/v1/tasks/:id": {
    GET: { response: Task };
    PUT: { body: UpdateTaskPayload; response: void };
    PATCH: { body: PatchTaskPayload; response: Task };
    DELETE: { response: void };
  };

  // User
  "/api/v1/users": {
    GET: { response: User[] };
  };
  "/api/v1/users/{id}": {
    GET: { response: User };
  };
  "/api/v1/users/:id": {
    GET: { response: User };
  };
  "/api/v1/users/test": {
    GET: { response: TestPermissionResponse };
  };

  // UserRole
  "/api/v1/users/{userId}/roles": {
    GET: { response: UserRole[] };
  };
  "/api/v1/users/:userId/roles": {
    GET: { response: UserRole[] };
  };
  "/api/v1/users/{userId}/roles/{roleId}": {
    GET: { response: UserRole };
    POST: { body?: unknown; response: UserRole };
    DELETE: { response: void };
  };
  "/api/v1/users/:userId/roles/:roleId": {
    GET: { response: UserRole };
    POST: { body?: unknown; response: UserRole };
    DELETE: { response: void };
  };

  // Board
  "/api/v1/projects/{projectId}/board": {
    GET: { response: { message: string; data: Board } };
  };
  "/api/v1/projects/:projectId/board": {
    GET: { response: { message: string; data: Board } };
  };
  "/api/v1/projects/{projectId}/board/columns": {
    GET: { response: { message: string; data: BoardColumn[] } };
    POST: { body: CreateColumnPayload; response: { message: string; data: BoardColumn } };
  };
  "/api/v1/projects/:projectId/board/columns": {
    GET: { response: { message: string; data: BoardColumn[] } };
    POST: { body: CreateColumnPayload; response: { message: string; data: BoardColumn } };
  };
  "/api/v1/projects/{projectId}/board/columns/{columnId}": {
    PUT: { body: UpdateColumnPayload; response: { message: string; data: BoardColumn } };
    DELETE: { response: { message: string; data: null } };
  };
  "/api/v1/projects/:projectId/board/columns/:columnId": {
    PUT: { body: UpdateColumnPayload; response: { message: string; data: BoardColumn } };
    DELETE: { response: { message: string; data: null } };
  };
  "/api/v1/projects/{projectId}/board/columns/reorder": {
    POST: { body: ReorderColumnsPayload; response: { message: string; data: null } };
  };
  "/api/v1/projects/:projectId/board/columns/reorder": {
    POST: { body: ReorderColumnsPayload; response: { message: string; data: null } };
  };

  // Task Items Movement & State
  "/api/v1/tasks/{id}/move": {
    POST: { body: MoveTaskPayload; response: { message: string; data: Task } };
  };
  "/api/v1/tasks/:id/move": {
    POST: { body: MoveTaskPayload; response: { message: string; data: Task } };
  };
  "/api/v1/tasks/{id}/status": {
    POST: { body: StatusTransitionPayload; response: { message: string; data: Task } };
  };
  "/api/v1/tasks/:id/status": {
    POST: { body: StatusTransitionPayload; response: { message: string; data: Task } };
  };

  // WeatherForecast
  "/api/weatherForecast": {
    GET: { response: WeatherForecast[] };
  };
  "/api/weatherforecast": {
    GET: { response: WeatherForecast[] };
  };
}

export type HttpMethodKey = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

/**
 * Strips the leading `/api` from a path string
 */
export type StripApiPrefix<P extends string> = P extends `/api/${infer Rest}` ? `/${Rest}` : P;

/**
 * Ensures a path string begins with `/api`
 */
export type EnsureApiPrefix<P extends string> = P extends `/api/${string}`
  ? P
  : `/api${P extends `/${string}` ? P : `/${P}`}`;

/**
 * All known endpoints that support method M (both with `/api` and shorthand without `/api`)
 */
export type RouteUrl<M extends HttpMethodKey> = {
  [K in keyof ApiRoutes]: ApiRoutes[K] extends Record<M, unknown>
    ? K | StripApiPrefix<K>
    : never;
}[keyof ApiRoutes];

/**
 * Converts dynamic parameter tokens `:param` or `{param}` into TypeScript template literal wildcards `${string | number}`
 */
export type RouteToPattern<P extends string>
  = P extends `${infer Start}/:${string}/${infer End}`
    ? `${Start}/${string | number}/${RouteToPattern<`/${End}`> extends `/${infer Rest}` ? Rest : never}`
    : P extends `${infer Start}/:${string}`
      ? `${Start}/${string | number}`
      : P extends `${infer Start}/{${string}}/${infer End}`
        ? `${Start}/${string | number}/${RouteToPattern<`/${End}`> extends `/${infer Rest}` ? Rest : never}`
        : P extends `${infer Start}/{${string}}`
          ? `${Start}/${string | number}`
          : P;

/**
 * Resolves the strongly typed response for a given endpoint and HTTP method.
 * If the route matches a known endpoint in ApiRoutes, its response type is returned.
 * Otherwise, it falls back to Default (or unknown).
 */
export type ResolveResponse<
  U extends string,
  M extends HttpMethodKey,
  Default = unknown,
> = EnsureApiPrefix<U> extends keyof ApiRoutes
  ? M extends keyof ApiRoutes[EnsureApiPrefix<U>]
    ? ApiRoutes[EnsureApiPrefix<U>][M] extends { response: infer R }
      ? R
      : Default
    : Default
  : {
      [K in keyof ApiRoutes]: EnsureApiPrefix<U> extends RouteToPattern<K>
        ? M extends keyof ApiRoutes[K]
          ? ApiRoutes[K][M] extends { response: infer R }
            ? R
            : never
          : never
        : never;
    }[keyof ApiRoutes] extends infer Matched
      ? [Matched] extends [never]
          ? Default
          : Matched
      : Default;

/**
 * Resolves the strongly typed request body for a given endpoint and HTTP method.
 */
export type ResolveBody<
  U extends string,
  M extends HttpMethodKey,
  Default = unknown,
> = EnsureApiPrefix<U> extends keyof ApiRoutes
  ? M extends keyof ApiRoutes[EnsureApiPrefix<U>]
    ? ApiRoutes[EnsureApiPrefix<U>][M] extends { body: infer B }
      ? B
      : Default
    : Default
  : {
      [K in keyof ApiRoutes]: EnsureApiPrefix<U> extends RouteToPattern<K>
        ? M extends keyof ApiRoutes[K]
          ? ApiRoutes[K][M] extends { body: infer B }
            ? B
            : never
          : never
        : never;
    }[keyof ApiRoutes] extends infer Matched
      ? [Matched] extends [never]
          ? Default
          : Matched
      : Default;
