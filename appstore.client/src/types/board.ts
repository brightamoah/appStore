import type z from "zod";

export type BoardColumn = z.infer<typeof boardColumnSchema>;
export type Board = z.infer<typeof boardSchema>;
export type CreateColumnPayload = z.infer<typeof createColumnSchema>;
export type UpdateColumnPayload = z.infer<typeof updateColumnSchema>;
export type ReorderColumnsPayload = z.infer<typeof reorderColumnsSchema>;

export type BoardColumnDto = BoardColumn;
export type BoardDto = Board;
export type CreateColumnDto = CreateColumnPayload;
export type UpdateColumnDto = UpdateColumnPayload;
export type ReorderColumnsDto = ReorderColumnsPayload;
