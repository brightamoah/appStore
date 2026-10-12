namespace appStore.Server.Errors
{
	public static class AuthErrorCodes
	{
		public const string AUTHENTICATION_REQUIRED = "AUTHENTICATION_REQUIRED";
		public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
		public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";

		public const string USER_NOT_FOUND = "USER_NOT_FOUND";
		public const string USER_ALREADY_EXISTS = "USER_ALREADY_EXISTS";
		public const string USER_INACTIVE = "USER_INACTIVE";

		public const string TOKEN_MISSING = "TOKEN_MISSING";
		public const string TOKEN_INVALID = "TOKEN_INVALID";
		public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
		public const string TOKEN_SIGNATURE_INVALID = "TOKEN_SIGNATURE_INVALID";
		public const string TOKEN_ISSUER_INVALID = "TOKEN_ISSUER_INVALID";
		public const string TOKEN_AUDIENCE_INVALID = "TOKEN_AUDIENCE_INVALID";

		public const string TOKEN_REVOKED = "TOKEN_REVOKED";

		public const string REFRESH_TOKEN_INVALID = "REFRESH_TOKEN_INVALID";
		public const string REFRESH_TOKEN_EXPIRED = "REFRESH_TOKEN_EXPIRED";
		public const string REFRESH_TOKEN_REVOKED = "REFRESH_TOKEN_REVOKED";

		public const string INVALID_PASSWORD_POLICY = "INVALID_PASSWORD_POLICY";

		public const string WEAK_PASSWORD = "WEAK_PASSWORD";
	}
}