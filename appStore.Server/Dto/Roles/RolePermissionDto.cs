using appStore.Server.Models;

namespace appStore.Server.Dto.Roles
{
	public record RolePermissionDto(
		int RoleId,
		int PermissionId,
		string PermissionName
	)
	{

		public static RolePermissionDto FromModel( RolePermission rolePermission )
		{
			return new RolePermissionDto(
				rolePermission.RoleId,
				rolePermission.PermissionId,
				rolePermission.Permission.Name
			);
		}
	}
}