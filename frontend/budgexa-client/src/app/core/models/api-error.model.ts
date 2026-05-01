export interface ApiErrorResponse {
  tag: string;
  message: string;
  traceId: string;
  metadata?: Record<string, string>;
  detail?: string;
}
