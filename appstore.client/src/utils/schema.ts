import * as z from "zod";
import "zod/compile";

// ============================================================================
// Password Schemas & Policies
// ============================================================================

export interface PasswordPolicyOptions {
  minimumLength?: number;
  maximumLength?: number;
  requireUppercase?: boolean;
  requireLowercase?: boolean;
  requireDigit?: boolean;
  requireSpecialCharacter?: boolean;
}

/**
 * Creates a dynamic password validation schema matching any policy configuration.
 */
export function createPasswordSchema(options?: PasswordPolicyOptions) {
  const minLength = options?.minimumLength ?? 8;
  const maxLength = options?.maximumLength ?? 128;

  let schema = z
    .string("Password is required")
    .min(1, "Password is required")
    .min(minLength, `Password must be at least ${minLength} characters long`)
    .max(maxLength, `Password cannot exceed ${maxLength} characters`);

  if (options?.requireUppercase) {
    schema = schema.regex(/[A-Z]/, "Password must contain at least one uppercase letter");
  }

  if (options?.requireLowercase) {
    schema = schema.regex(/[a-z]/, "Password must contain at least one lowercase letter");
  }

  if (options?.requireDigit) {
    schema = schema.regex(/\d/, "Password must contain at least one number");
  }

  if (options?.requireSpecialCharacter) {
    schema = schema.regex(/[^a-z\d]/i, "Password must contain at least one special character");
  }

  return schema;
}

/**
 * Baseline/lenient password requirement:
 * Minimum 8 characters, maximum 128 characters.
 */
export const lenientPasswordSchema = createPasswordSchema({
  minimumLength: 8,
  maximumLength: 128,
  requireUppercase: false,
  requireLowercase: false,
  requireDigit: false,
  requireSpecialCharacter: false,
});

/**
 * Maximum password requirement (when admin pushes everything to the max):
 * Minimum 8 characters, maximum 128 characters, requires uppercase, lowercase, number, and special character.
 */
export const maxPasswordSchema = createPasswordSchema({
  minimumLength: 8,
  maximumLength: 128,
  requireUppercase: true,
  requireLowercase: true,
  requireDigit: true,
  requireSpecialCharacter: true,
});

export const strictPasswordSchema = maxPasswordSchema;

/**
 * Default password schema matching the backend SystemPasswordPolicyOptions.
 */
export const passwordSchema = maxPasswordSchema;

// ============================================================================
// Auth & User Schemas
// ============================================================================

export const loginSchema = z.object({
  email: z.email("Invalid email"),
  password: z.string("Password is required").min(1, "Password is required"),
  rememberMe: z.boolean().default(false),
});

export const forgotPasswordSchema = z.object({
  email: z.email("Invalid email"),
});

export const userSchema = z.object({
  id: z.string(),
  name: z.string("Name is required"),
  email: z.email("Invalid email"),
  phoneNumber: z.string().nullable(),
  isEmailVerified: z.boolean(),
  profilePictureUrl: z.string().nullable(),
  createdAt: z.string(),
  updatedAt: z.string(),
  lastLoginAt: z.string().nullable(),
});

export const createUserSchema = z.object({
  name: z
    .string("Name is required")
    .min(1, "Name is required")
    .min(2, "Name must be at least 2 characters"),
  email: z.email("Invalid email"),
  password: passwordSchema,
  phoneNumber: z.string().nullable().optional(),
});
export const signUpSchema = createUserSchema;

export const loginResponseSchema = z.object({
  message: z.string(),
  data: z.object({
    user: userSchema,
    roles: z.array(z.string()),
    permissions: z.array(z.string()),
  }),
  accessToken: z.string(),
});
export const authResponseSchema = loginResponseSchema;

export const signUpResponseSchema = z.object({
  message: z.string(),
  data: userSchema,
});

export const refreshResponseSchema = z.object({
  message: z.string(),
  accessToken: z.string(),
});

export const csrfResponseSchema = z.object({
  csrfToken: z.string(),
});

export const testPermissionResponseSchema = z.object({
  message: z.string(),
});

// ============================================================================
// Tasks Schemas
// ============================================================================

export const taskStatusSchema = z.enum([
  "NotStarted",
  "InProgress",
  "Completed",
  "ReadyForTest",
  "Closed",
  "UnderReview",
  "Cancelled",
  "OnHold",
  "Archived",
  "Pending",
]);

export const issueTypeSchema = z.enum([
  "Task",
  "Bug",
  "Story",
  "Epic",
]);

export const ALLOWED_STATUS_TRANSITIONS: Record<z.infer<typeof taskStatusSchema>, z.infer<typeof taskStatusSchema>[]> = {
  NotStarted: ["InProgress", "UnderReview", "ReadyForTest", "Completed", "OnHold", "Cancelled"],
  InProgress: ["NotStarted", "UnderReview", "ReadyForTest", "Completed", "OnHold", "Cancelled"],
  UnderReview: ["NotStarted", "InProgress", "ReadyForTest", "Completed", "Cancelled"],
  ReadyForTest: ["NotStarted", "InProgress", "UnderReview", "Completed", "Cancelled"],
  OnHold: ["NotStarted", "InProgress", "Cancelled"],
  Completed: ["NotStarted", "InProgress", "UnderReview", "ReadyForTest", "Closed"],
  Closed: ["InProgress", "Completed", "Archived"],
  Cancelled: ["NotStarted", "InProgress", "Archived"],
  Archived: ["NotStarted", "InProgress"],
  Pending: ["NotStarted", "InProgress", "Cancelled"],
};

export const moveTaskSchema = z.object({
  targetColumnId: z.string(),
  targetOrderIndex: z.number().int().min(0),
  assigneeId: z.string().optional(),
});
export const statusTransitionSchema = z.object({
  targetStatus: taskStatusSchema,
});

export const taskSchema = z.object({
  id: z.string(),
  taskKey: z.string().nullable().optional(),
  name: z.string(),
  description: z.string(),
  priorityId: z.number().int(),
  status: taskStatusSchema.default("NotStarted"),
  issueType: issueTypeSchema.default("Task"),
  isCompleted: z.boolean(),
  projectId: z.string().nullable().optional(),
  columnId: z.string().nullable().optional(),
  orderIndex: z.number().int().default(0),
  assigneeId: z.string().nullable().optional(),
  assigneeIds: z.array(z.string()).default([]),
  reporterId: z.string().nullable().optional(),
  dueDate: z.string().nullable().optional(),
  estimatedMinutes: z.number().int().nullable().optional(),
  createdAt: z.string(),
  updatedAt: z.string(),
  completedAt: z.string().nullable(),
});

export const taskListItemSchema = z.object({
  id: z.string(),
  taskKey: z.string().nullable().optional(),
  name: z.string(),
  description: z.string(),
  priority: z.string(),
  status: taskStatusSchema.default("NotStarted"),
  issueType: issueTypeSchema.default("Task"),
  isCompleted: z.boolean(),
  projectId: z.string().nullable().optional(),
  columnId: z.string().nullable().optional(),
  orderIndex: z.number().int().default(0),
  assigneeId: z.string().nullable().optional(),
  assigneeIds: z.array(z.string()).default([]),
  dueDate: z.string().nullable().optional(),
  createdAt: z.string(),
  updatedAt: z.string(),
  completedAt: z.string().nullable(),
});

export const createTaskSchema = z.object({
  name: z.string().min(1, "Task name is required"),
  description: z.string().default(""),
  priorityId: z.number().int("Priority ID is required"),
  projectId: z.string("Project ID is required"),
  columnId: z.string().optional(),
  issueType: issueTypeSchema.default("Task"),
  assigneeId: z.string().optional(),
  assigneeIds: z.array(z.string()).optional(),
  dueDate: z.string().optional(),
  estimatedMinutes: z.number().int().optional(),
});

export const updateTaskSchema = z.object({
  name: z.string().min(1, "Task name is required").optional(),
  description: z.string().optional(),
  priorityId: z.number().int().optional(),
  issueType: issueTypeSchema.optional(),
  assigneeId: z.string().nullable().optional(),
  assigneeIds: z.array(z.string()).optional(),
  dueDate: z.string().nullable().optional(),
  estimatedMinutes: z.number().int().nullable().optional(),
});

export const createProjectSchema = z.object({
  name: z.string().min(2, "Project name must be at least 2 characters").max(100, "Project name cannot exceed 100 characters"),
  key: z.string().min(2, "Key must be at least 2 characters").max(10, "Key cannot exceed 10 characters").regex(/^[\w-]+$/, "Key must contain only letters, numbers, hyphens, or underscores"),
  description: z.string().max(1000, "Description cannot exceed 1000 characters").optional().default(""),
  icon: z.string().max(50).optional().default("frontend"),
});

export const patchTaskSchema = z.object({
  name: z.string().nullable().optional(),
  description: z.string().nullable().optional(),
  priorityId: z.number().int().nullable().optional(),
  isCompleted: z.boolean().nullable().optional(),
  status: taskStatusSchema.default("NotStarted").nullable().optional(),
});

// ============================================================================
// Priorities Schemas
// ============================================================================

export const prioritySchema = z.object({
  id: z.number().int(),
  name: z.string(),
});

export const createPrioritySchema = z.object({
  name: z.string().min(1, "Priority name is required"),
});

export const updatePrioritySchema = z.object({
  name: z.string().min(1, "Priority name is required"),
});

// ============================================================================
// Roles Schemas
// ============================================================================

export const roleSchema = z.object({
  id: z.number().int(),
  name: z.string(),
  description: z.string(),
});

export const createRoleSchema = z.object({
  name: z.string().min(1, "Role name is required"),
  description: z.string().nullable().optional(),
});

export const updateRoleSchema = z.object({
  name: z.string().nullable().optional(),
  description: z.string().nullable().optional(),
});

export const userRoleSchema = z.object({
  userId: z.string(),
  roleId: z.number().int(),
  roleName: z.string(),
});

export const rolePermissionSchema = z.object({
  roleId: z.number().int(),
  permissionId: z.number().int(),
  permissionName: z.string(),
});

// ============================================================================
// Permissions Schemas
// ============================================================================

export const permissionSchema = z.object({
  id: z.union([z.number(), z.string()]),
  name: z.string(),
  description: z.string(),
});

export const createPermissionSchema = z.object({
  name: z.string().min(1, "Permission name is required"),
  description: z.string().min(1, "Description is required"),
});

export const updatePermissionSchema = z.object({
  name: z.string().nullable().optional(),
  description: z.string().nullable().optional(),
});

// ============================================================================
// Security (Password Policy) Schemas
// ============================================================================

export const passwordPolicySchema = z.object({
  minimumLength: z.number().int(),
  requireUppercase: z.boolean(),
  requireLowercase: z.boolean(),
  requireDigit: z.boolean(),
  requireSpecialCharacter: z.boolean(),
  isCustom: z.boolean(),
});

export const updatePasswordPolicySchema = z.object({
  minimumLength: z.number().int().min(8, "Minimum length must be at least 8").max(128, "Minimum length cannot exceed 128"),
  requireUppercase: z.boolean(),
  requireLowercase: z.boolean(),
  requireDigit: z.boolean(),
  requireSpecialCharacter: z.boolean(),
}).refine(
  data => data.requireUppercase || data.requireLowercase || data.requireDigit || data.requireSpecialCharacter,
  { message: "At least one password character requirement must be enabled" },
);

// ============================================================================
// Board & Column Schemas
// ============================================================================

export const boardColumnSchema = z.object({
  id: z.string(),
  projectId: z.string(),
  name: z.string().min(1, "Column name is required"),
  orderIndex: z.number().int(),
  statusMapping: taskStatusSchema,
  wipLimit: z.number().int().min(0).default(0),
  color: z.string().default("neutral"),
  isDefault: z.boolean().default(false),
  taskCount: z.number().int().default(0),
});

export const boardSchema = z.object({
  id: z.string(),
  name: z.string(),
  key: z.string(),
  columns: z.array(boardColumnSchema),
});

export const createColumnSchema = z.object({
  name: z.string().min(1, "Column name is required"),
  statusMapping: taskStatusSchema,
  wipLimit: z.number().int().min(0).default(0),
  color: z.string().default("neutral"),
});

export const updateColumnSchema = z.object({
  name: z.string().min(1, "Column name is required"),
  statusMapping: taskStatusSchema,
  wipLimit: z.number().int().min(0).default(0),
  color: z.string().default("neutral"),
});

export const reorderColumnsSchema = z.object({
  orderedColumnIds: z.array(z.string()),
});

// ============================================================================
// Weather Schemas
// ============================================================================

export const weatherForecastSchema = z.object({
  date: z.string(),
  temperatureC: z.number(),
  temperatureF: z.number(),
  summary: z.string().nullable(),
});
