using Microsoft.AspNetCore.Authorization;

namespace appStore.Server.Authorization
{
	public sealed class PermissionRequirement( string permission ) : IAuthorizationRequirement
	{
		public string Permission { get; } = permission;
	}
}