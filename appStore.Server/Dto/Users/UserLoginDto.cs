using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Users
{
	public record UserLoginDto(
		[Required]
		[EmailAddress]
		string Email,

		[Required]
		string Password,

		[Required]
		bool RememberMe
	);
}