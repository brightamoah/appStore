using Microsoft.AspNetCore.Authorization;

namespace appStore.Server.Authorization
{
	public sealed class SameUserOrPermissionRequirement(
		string permission = AppPermissions.Users.Manage
	) : IAuthorizationRequirement
	{
		public string Permission { get; } = permission;
	}
}
