using appStore.Server.Models;

namespace appStore.Server.Dto.Roles
{
	public record UserRoleDto(
		Guid UserId,
		int RoleId,
		string RoleName
	)
	{
		public static UserRoleDto FromModel( UserRole userRole )
		{
			return new UserRoleDto(
				userRole.UserId,
				userRole.RoleId,
				userRole.Role.Name
			);
		}
	};
}