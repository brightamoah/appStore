using appStore.Server.Dto.Users;

namespace appStore.Server.Services.Interfaces
{
	public interface IUserService
	{
		Task<UserDto> CreateUserAsync(
			CreateUserDto dto,
			CancellationToken cancellationToken = default
		);

		Task<UserDto> GetUserByIdAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		);

		Task<UserDto> GetCurrentUserAsync(
			Guid userId,
			CancellationToken cancellationToken = default
		);
	}
}