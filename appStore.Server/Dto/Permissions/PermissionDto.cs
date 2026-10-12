using appStore.Server.Models;

namespace appStore.Server.Dto.Permissions
{
	public record PermissionDto(
		string Id,
		string Name,
		string Description
	)
	{
		public static PermissionDto FromModel( Permission permission )
		{
			return new PermissionDto(
				permission.Id.ToString(),
				permission.Name,
				permission.Description ?? string.Empty
			);
		}
	}
}