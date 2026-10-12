using appStore.Server.Dto.Security;
using appStore.Server.Services.Security;

namespace appStore.Server.Services.Interfaces
{
	public interface IPasswordPolicyService
	{
		Task<PasswordPolicyDefinition> GetEffectivePolicyAsync(
			CancellationToken cancellationToken = default
		);

		Task<PasswordPolicyDto> GetPolicyAsync(
			CancellationToken cancellationToken = default
		);

		Task<PasswordPolicyDto> UpdatePolicyAsync(
			UpdatePasswordPolicyDto dto,
			CancellationToken cancellationToken = default
		);

		Task ResetToSystemDefaultAsync(
			CancellationToken cancellationToken = default
		);
	}
}