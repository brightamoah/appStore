using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace appStore.Server.Authorization
{
	public sealed class PermissionPolicyProvider( IOptions<AuthorizationOptions> options ) : DefaultAuthorizationPolicyProvider( options )
	{
		public const string POLICY_PREFIX = "permission:";

		public override async Task<AuthorizationPolicy?> GetPolicyAsync( string policyName )
		{
			if (!policyName.StartsWith( POLICY_PREFIX, StringComparison.OrdinalIgnoreCase ))
			{
				return await base.GetPolicyAsync( policyName );
			}

			var permission = policyName[POLICY_PREFIX.Length..];

			if (string.IsNullOrWhiteSpace( permission )) return null;

			return new AuthorizationPolicyBuilder()
			.RequireAuthenticatedUser()
			.AddRequirements(
				new PermissionRequirement( permission )
			)
			.Build();
		}
	}
}