import type z from "zod";

/**
 * Permission details returned by GET /api/v1/permissions and GET /api/v1/permissions/{id}
 */
export type Permission = z.infer<typeof permissionSchema>;

/**
 * Request body for POST /api/v1/permissions
 */
export type CreatePermissionPayload = z.infer<typeof createPermissionSchema>;

/**
 * Request body for PATCH /api/v1/permissions/{id}
 */
export type UpdatePermissionPayload = z.infer<typeof updatePermissionSchema>;

// Backward-compatible aliases
export type PermissionDto = Permission;
export type CreatePermissionDto = CreatePermissionPayload;
export type UpdatePermissionDto = UpdatePermissionPayload;
