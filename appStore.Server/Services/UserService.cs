using appStore.Server.Dto.Users;
using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public sealed class UserService(
		IPasswordHasher passwordHasher,
		IPasswordPolicyService passwordPolicyService,
		IPasswordPolicyValidator passwordPolicyValidator,
		IUserRepository userRepository
	) : IUserService
	{
		public async Task<UserDto> CreateUserAsync(
			CreateUserDto dto,
			CancellationToken cancellationToken = default
	   )
		{
			var email = dto.Email.Trim().ToLowerInvariant();

			var existingUser = await userRepository.GetByEmailAsync(email, cancellationToken);

			if (existingUser != null)
			{
				throw AppException.Conflict(
					"A user with this email already exists.",
					AuthErrorCodes.EMAIL_ALREADY_EXISTS
				);
			}

			var policy = await passwordPolicyService
					.GetEffectivePolicyAsync(cancellationToken);

			passwordPolicyValidator.Validate(
				dto.Password,
				policy
			);

			var now = DateTime.UtcNow;

			var user = new User
			{
				Id = Guid.NewGuid(),
				Name = dto.Name.Trim(),
				Email = email,
				PasswordHash = passwordHasher.HashPassword(dto.Password),
				PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim(),
				IsEmailVerified = false,
				IsUserActive = true,
				CreatedAt = now,
				UpdatedAt = now
			};

			await userRepository.AddAsync(user, cancellationToken);

			await userRepository.SaveChangesAsync(cancellationToken);

			return UserDto.FromModel(user);
		}


		public async Task<UserDto> GetUserByIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			var user = await userRepository.GetByIdAsync(userId, cancellationToken);

			return user is null
				? throw AppException.NotFound(
					$"User with ID {userId} not found.",
					AuthErrorCodes.USER_NOT_FOUND
				)
				: UserDto.FromModel(user);
		}

		public async Task<UserDto> GetCurrentUserAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		)
		{
			return await GetUserByIdAsync(
				userId,
				cancellationToken
			);
		}
	}
}