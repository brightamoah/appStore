using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Users
{
	public record CreateUserDto(
		[Required]
		[StringLength(maximumLength: 100)]
		string Name,

		[Required]
		[EmailAddress]
		string Email,

		[Required]
		string Password,

		string? PhoneNumber
	);
}