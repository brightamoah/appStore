export * from "./api";
export * from "./auth";
export * from "./board";
export * from "./endpoints";
export * from "./permissions";
export * from "./priorities";
export * from "./project";
export * from "./props";
export * from "./roles";
export * from "./security";
export * from "./tasks";
export * from "./weather";

// Legacy Forecasts types (used by HelloWorld.vue demo component)
export type Forecasts = {
  date: string;
  temperatureC: string;
  temperatureF: string;
  summary: string;
}[];

export interface Data {
  loading: boolean;
  post: null | Forecasts;
}

export type ColorType = "neutral" | "success" | "error" | "info" | "primary" | "warning";
