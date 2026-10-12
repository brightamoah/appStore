import type z from "zod";

/**
 * Weather forecast item returned by GET /api/weatherForecast
 */
export type WeatherForecast = z.infer<typeof weatherForecastSchema>;
