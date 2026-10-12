import type z from "zod";

/**
 * User representation returned by user & auth endpoints
 */
export type User = z.infer<typeof userSchema>;

/**
 * Body for POST /api/v1/auth/signup
 */
export type CreateUserPayload = z.infer<typeof createUserSchema>;
export type SignUpSchema = z.infer<typeof signUpSchema>;
export type PasswordSchema = z.infer<typeof passwordSchema>;

/**
 * Body for POST /api/v1/auth/login
 */
export type LoginSchema = z.infer<typeof loginSchema>;
export type ForgotPasswordPayload = z.infer<typeof forgotPasswordSchema>;

/**
 * Response returned by POST /api/v1/auth/login
 */
export type LoginResponse = z.infer<typeof loginResponseSchema>;
export type AuthResponse = LoginResponse;

/**
 * Response returned by POST /api/v1/auth/signup
 */
export type SignUpResponse = z.infer<typeof signUpResponseSchema>;

/**
 * Response returned by POST /api/v1/auth/refresh
 */
export type RefreshResponse = z.infer<typeof refreshResponseSchema>;

/**
 * Response returned by GET /api/v1/auth/csrf
 */
export type CsrfResponse = z.infer<typeof csrfResponseSchema>;

/**
 * Response returned by GET /api/v1/users/test
 */
export type TestPermissionResponse = z.infer<typeof testPermissionResponseSchema>;

// Backward-compatible aliases
export type UserDto = User;
export type CreateUserDto = CreateUserPayload;
export type UserLoginDto = LoginSchema;
export type LoginCredentials = LoginSchema;
