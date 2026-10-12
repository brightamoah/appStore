import type z from "zod";

/**
 * Password policy details returned by GET /api/v1/security/password-policy and PUT /api/v1/security/password-policy
 */
export type PasswordPolicy = z.infer<typeof passwordPolicySchema>;

/**
 * Request body for PUT /api/v1/security/password-policy
 */
export type UpdatePasswordPolicyPayload = z.infer<typeof updatePasswordPolicySchema>;

// Backward-compatible aliases
export type PasswordPolicyDto = PasswordPolicy;
export type UpdatePasswordPolicyDto = UpdatePasswordPolicyPayload;
