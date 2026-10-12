namespace appStore.Server.Dto.Security
{
	public sealed record UpdatePasswordPolicyDto(
		int MinimumLength,
		bool RequireUppercase,
		bool RequireLowercase,
		bool RequireDigit,
		bool RequireSpecialCharacter
	);
}