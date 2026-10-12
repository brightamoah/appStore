namespace appStore.Server.Configuration
{
	public sealed class SystemPasswordPolicyOptions
	{
		public const int MinimumLength = 8;
		public const bool RequireUppercase = true;
		public const bool RequireLowercase = true;
		public const bool RequireDigit = true;
		public const bool RequireSpecialCharacter = true;
	}
}