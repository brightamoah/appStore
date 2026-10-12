namespace appStore.Server.Errors
{
	public static class ErrorCodes
	{
		public const string VALIDATION_ERROR = "VALIDATION_ERROR";
		public const string INVALID_REQUEST = "INVALID_REQUEST";
		public const string NOT_FOUND = "NOT_FOUND";
		public const string CONFLICT = "CONFLICT";
		public const string INTERNAL_ERROR = "INTERNAL_ERROR";


		public const string AUTHENTICATION_REQUIRED = "AUTHENTICATION_REQUIRED";
		public const string TOKEN_MISSING = "TOKEN_MISSING";
		public const string TOKEN_INVALID = "TOKEN_INVALID";
		public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
		public const string TOKEN_REVOKED = "TOKEN_REVOKED";
		public const string REFRESH_TOKEN_INVALID = "REFRESH_TOKEN_INVALID";
		public const string REFRESH_TOKEN_EXPIRED = "REFRESH_TOKEN_EXPIRED";


		public const string FORBIDDEN = "FORBIDDEN";
		public const string INSUFFICIENT_PERMISSION = "INSUFFICIENT_PERMISSION";


		public const string USER_NOT_FOUND = "USER_NOT_FOUND";
		public const string USER_ALREADY_EXISTS = "USER_ALREADY_EXISTS";
		public const string USER_INACTIVE = "USER_INACTIVE";

		public const string ROLE_NOT_FOUND = "ROLE_NOT_FOUND";
		public const string ROLE_ALREADY_EXISTS = "ROLE_ALREADY_EXISTS";

		public const string PERMISSION_NOT_FOUND = "PERMISSION_NOT_FOUND";
		public const string PERMISSION_ALREADY_EXISTS = "PERMISSION_ALREADY_EXISTS";
		public const string PERMISSION_IN_USE = "PERMISSION_IN_USE";

		public const string ROLE_PERMISSION_NOT_FOUND = "ROLE_PERMISSION_NOT_FOUND";
		public const string ROLE_PERMISSION_ALREADY_EXISTS = "ROLE_PERMISSION_ALREADY_EXISTS";

		public const string USER_ROLE_ALREADY_EXISTS = "USER_ROLE_ALREADY_EXISTS";
		public const string USER_ROLE_NOT_FOUND = "USER_ROLE_NOT_FOUND";

		public const string TASK_NOT_FOUND = "TASK_NOT_FOUND";

		public const string PRIORITY_NOT_FOUND = "PRIORITY_NOT_FOUND";
		public const string PRIORITY_ALREADY_EXISTS = "PRIORITY_ALREADY_EXISTS";

		public const string RATE_LIMIT_EXCEEDED = "RATE_LIMIT_EXCEEDED";
		public const string INVALID_STATUS_TRANSITION = "INVALID_STATUS_TRANSITION";

		public const string ROLE_IS_ASSIGNED_TO_USERS = "ROLE_IS_ASSIGNED_TO_USERS";

		public const string PROJECT_NOT_FOUND = "PROJECT_NOT_FOUND";
		public const string COLUMN_NOT_FOUND = "COLUMN_NOT_FOUND";
		public const string COLUMN_HAS_TASKS = "COLUMN_HAS_TASKS";
		public const string WIP_LIMIT_EXCEEDED = "WIP_LIMIT_EXCEEDED";
	}
}