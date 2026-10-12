using appStore.Server.Models;

namespace appStore.Server.Dto.Roles
{
	public record RoleDto(
		int Id,
		string Name,
		string Description
	)
	{
		public static RoleDto FromModel( Role role )
		{
			return new RoleDto(
				role.Id,
				role.Name,
				role.Description
			);
		}
	};
}