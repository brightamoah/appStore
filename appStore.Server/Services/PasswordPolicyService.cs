using appStore.Server.Configuration;
using appStore.Server.Dto.Security;
using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;
using appStore.Server.Services.Security;

namespace appStore.Server.Services
{
	public sealed class PasswordPolicyService(IPasswordPolicyRepository passwordPolicyRepository) : IPasswordPolicyService
	{
		private const int PolicyId = 1;

		public async Task<PasswordPolicyDefinition> GetEffectivePolicyAsync(
			CancellationToken cancellationToken = default
		)
		{
			var policy = await passwordPolicyRepository.GetPolicyAsync(
				asNoTracking: true,
				cancellationToken
			);

			if (policy is null) return GetSystemDefault();

			return new PasswordPolicyDefinition(
				policy.MinimumLength,
				policy.RequireUppercase,
				policy.RequireLowercase,
				policy.RequireDigit,
				policy.RequireSpecialCharacter
			);
		}

		public async Task<PasswordPolicyDto> GetPolicyAsync(
			CancellationToken cancellationToken = default
		)
		{
			var policy = await passwordPolicyRepository.GetPolicyAsync(
				asNoTracking: true,
				cancellationToken
			);

			if (policy is null)
			{
				return new PasswordPolicyDto(
					SystemPasswordPolicyOptions.MinimumLength,
					SystemPasswordPolicyOptions.RequireUppercase,
					SystemPasswordPolicyOptions.RequireLowercase,
					SystemPasswordPolicyOptions.RequireDigit,
					SystemPasswordPolicyOptions.RequireSpecialCharacter,
					false
				);
			}

			return PasswordPolicyDto.FromModel(policy, true);
		}

		public async Task<PasswordPolicyDto> UpdatePolicyAsync(
			UpdatePasswordPolicyDto dto,
			CancellationToken cancellationToken = default
		)
		{
			ValidatePolicyConfiguration(dto);

			var now = DateTime.UtcNow;

			var policy = await passwordPolicyRepository.GetPolicyAsync(
				asNoTracking: false,
				cancellationToken
			);

			if (policy is null)
			{
				policy = new PasswordPolicy
				{
					Id = PolicyId,
					CreatedAt = now
				};

				await passwordPolicyRepository.AddAsync(policy, cancellationToken);
			}

			policy.MinimumLength = dto.MinimumLength;
			policy.RequireUppercase = dto.RequireUppercase;
			policy.RequireLowercase = dto.RequireLowercase;
			policy.RequireDigit = dto.RequireDigit;
			policy.RequireSpecialCharacter = dto.RequireSpecialCharacter;
			policy.UpdatedAt = now;

			await passwordPolicyRepository.SaveChangesAsync(cancellationToken);

			return PasswordPolicyDto.FromModel(policy, true);
		}

		public async Task ResetToSystemDefaultAsync(
			CancellationToken cancellationToken = default
		)
		{
			var policy = await passwordPolicyRepository.GetPolicyAsync(
				asNoTracking: false,
				cancellationToken
			);

			if (policy is null) return;

			passwordPolicyRepository.Remove(policy);

			await passwordPolicyRepository.SaveChangesAsync(cancellationToken);
		}

		private static PasswordPolicyDefinition GetSystemDefault()
		{
			return new PasswordPolicyDefinition(
				SystemPasswordPolicyOptions.MinimumLength,
				SystemPasswordPolicyOptions.RequireUppercase,
				SystemPasswordPolicyOptions.RequireLowercase,
				SystemPasswordPolicyOptions.RequireDigit,
				SystemPasswordPolicyOptions.RequireSpecialCharacter
			);
		}

		private static void ValidatePolicyConfiguration(UpdatePasswordPolicyDto dto)
		{
			if (dto.MinimumLength < 8)
			{
				throw AppException.BadRequest(
					"Minimum password length cannot be less than 8.",
					AuthErrorCodes.INVALID_PASSWORD_POLICY
				);
			}

			if (dto.MinimumLength > 128)
			{
				throw AppException.BadRequest(
					"Minimum password length cannot exceed 128.",
					AuthErrorCodes.INVALID_PASSWORD_POLICY
				);
			}

			if (
				!dto.RequireUppercase &&
				!dto.RequireLowercase &&
				!dto.RequireDigit &&
				!dto.RequireSpecialCharacter
			)
			{
				throw AppException.BadRequest(
					"At least one password character requirement must be enabled.",
					AuthErrorCodes.INVALID_PASSWORD_POLICY
				);
			}
		}
	}
}