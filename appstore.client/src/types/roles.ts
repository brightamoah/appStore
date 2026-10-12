import type z from "zod";

/**
 * Role details returned by GET /api/v1/roles and GET /api/v1/roles/{id}
 */
export type Role = z.infer<typeof roleSchema>;

/**
 * Request body for POST /api/v1/roles
 */
export type CreateRolePayload = z.infer<typeof createRoleSchema>;

/**
 * Request body for PATCH /api/v1/roles/{id}
 */
export type UpdateRolePayload = z.infer<typeof updateRoleSchema>;

/**
 * User-role assignment details returned by GET /api/v1/users/{userId}/roles and POST /api/v1/users/{userId}/roles/{roleId}
 */
export type UserRole = z.infer<typeof userRoleSchema>;

/**
 * Role-permission assignment details returned by GET /api/v1/roles/{roleId}/permissions and POST /api/v1/roles/{roleId}/permissions/{permissionId}
 */
export type RolePermission = z.infer<typeof rolePermissionSchema>;

// Backward-compatible aliases
export type RoleDto = Role;
export type CreateRoleDto = CreateRolePayload;
export type UpdateRoleDto = UpdateRolePayload;
export type UserRoleDto = UserRole;
export type RolePermissionDto = RolePermission;
