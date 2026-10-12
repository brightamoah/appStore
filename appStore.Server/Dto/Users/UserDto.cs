using appStore.Server.Models;

namespace appStore.Server.Dto.Users
{
	public record UserDto(
		Guid Id,
		string Name,
		string Email,
		string? PhoneNumber,
		bool IsEmailVerified,
		string ProfilePictureUrl,
		DateTime CreatedAt,
		DateTime UpdatedAt,
		DateTime? LastLoginAt = null
	)
	{
		public static UserDto FromModel( User user )
		{
			return new UserDto(
				user.Id,
				user.Name,
				user.Email,
				user.PhoneNumber,
				user.IsEmailVerified,
				user.ProfilePictureUrl ?? string.Empty,
				user.CreatedAt,
				user.UpdatedAt,
				user.LastLoginAt
			);
		}
	}
}