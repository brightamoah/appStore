import type z from "zod";

/**
 * Priority details returned by GET /api/v1/priorities and GET /api/v1/priorities/{id}
 */
export type Priority = z.infer<typeof prioritySchema>;

/**
 * Request body for POST /api/v1/priorities
 */
export type CreatePriorityPayload = z.infer<typeof createPrioritySchema>;

/**
 * Request body for PATCH /api/v1/priorities/{id}
 */
export type UpdatePriorityPayload = z.infer<typeof updatePrioritySchema>;

// Backward-compatible aliases
export type PriorityDto = Priority;
export type CreatePriorityDto = CreatePriorityPayload;
export type UpdatePriorityDto = UpdatePriorityPayload;
