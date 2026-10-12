namespace appStore.Server.Errors
{
	public class AppException(
		int statusCode,
		string code,
		string message,
		object? details = null
	) : Exception(message)
	{
		public int StatusCode { get; } = statusCode;

		public string Code { get; } = code;

		public object? Details { get; } = details;

		public static AppException NotFound(
			string message,
			string code = ErrorCodes.NOT_FOUND,
			object? details = null
		) =>
			new(
				StatusCodes.Status404NotFound,
				code,
				message,
				details
			);

		public static AppException Conflict(
			string message,
			string code = ErrorCodes.CONFLICT,
			object? details = null
		) =>
			new(
				StatusCodes.Status409Conflict,
				code,
				message,
				details
			);

		public static AppException BadRequest(
			string message,
			string code = ErrorCodes.INVALID_REQUEST,
			object? details = null
		) =>
			new(
				StatusCodes.Status400BadRequest,
				code,
				message,
				details
			);

		public static AppException Unauthorized(
			string message,
			string code = AuthErrorCodes.AUTHENTICATION_REQUIRED,
			object? details = null
		) =>
			new(
				StatusCodes.Status401Unauthorized,
				code,
				message,
				details
			);

		public static AppException Forbidden(
			string message,
			string code = ErrorCodes.FORBIDDEN,
			object? details = null
		) =>
			new(
				StatusCodes.Status403Forbidden,
				code,
				message,
				details
			);

		public static AppException TooManyRequests(
			string message,
			string code = ErrorCodes.RATE_LIMIT_EXCEEDED,
			object? details = null
		) =>
			new(
				StatusCodes.Status429TooManyRequests,
				code,
				message,
				details
			);
	}
}