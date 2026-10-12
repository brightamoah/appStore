namespace appStore.Server.Errors
{
	public sealed class ErrorResponse
	{
		public int Status { get; init; }

		public string Code { get; init; } = string.Empty;

		public string Message { get; init; } = string.Empty;

		public string TraceId { get; init; } = string.Empty;

		public object? Details { get; init; }
	}
}