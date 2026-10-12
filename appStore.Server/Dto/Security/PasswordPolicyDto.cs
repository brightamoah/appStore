using appStore.Server.Models;

namespace appStore.Server.Dto.Security
{
	public sealed record PasswordPolicyDto(
		int MinimumLength,
		bool RequireUppercase,
		bool RequireLowercase,
		bool RequireDigit,
		bool RequireSpecialCharacter,
		bool IsCustom
	)
	{
		public static PasswordPolicyDto FromModel(PasswordPolicy policy, bool isCustom)
		{
			return new PasswordPolicyDto(
				policy.MinimumLength,
				policy.RequireUppercase,
				policy.RequireLowercase,
				policy.RequireDigit,
				policy.RequireSpecialCharacter,
				isCustom
			);
		}
	};
}