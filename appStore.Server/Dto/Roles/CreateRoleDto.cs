using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Dto.Roles
{
	public record CreateRoleDto(
		[Required]
		[StringLength(100)]
		string Name,

		[StringLength(256)]
		string Description
	);
}