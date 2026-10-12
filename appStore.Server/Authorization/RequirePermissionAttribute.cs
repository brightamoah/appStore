using Microsoft.AspNetCore.Authorization;

namespace appStore.Server.Authorization
{
	public sealed class RequirePermissionAttribute(string permission) : AuthorizeAttribute(
		$"{PermissionPolicyProvider.POLICY_PREFIX}{permission}"
	)
	{
		public string Permission { get; } = permission;
	}
}