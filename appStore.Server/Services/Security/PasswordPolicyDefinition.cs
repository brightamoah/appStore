namespace appStore.Server.Services.Security
{
	public sealed record PasswordPolicyDefinition(
		int MinimumLength,
		bool RequireUppercase,
		bool RequireLowercase,
		bool RequireDigit,
		bool RequireSpecialCharacter
	);
}