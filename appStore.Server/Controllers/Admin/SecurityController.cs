using appStore.Server.Authorization;
using appStore.Server.Dto.Security;
using appStore.Server.Extensions;
using appStore.Server.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers.Admin
{
	[Route("api/v1/security/password-policy")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class SecurityController(IPasswordPolicyService passwordPolicyService) : ControllerBase
	{
		[RequirePermission(AppPermissions.Security.Manage)]
		[HttpGet]
		public async Task<ActionResult<PasswordPolicyDto>> GetPasswordPolicy(CancellationToken cancellationToken)
		{
			var policy = await passwordPolicyService.GetPolicyAsync(cancellationToken);

			return Ok(policy);
		}

		[RequirePermission( AppPermissions.Security.Manage )]
		[HttpPut]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<PasswordPolicyDto>> UpdatePasswordPolicy(
			UpdatePasswordPolicyDto dto,
			CancellationToken cancellationToken
		)
		{
			var updated = await passwordPolicyService.UpdatePolicyAsync(dto, cancellationToken);

			return Ok(updated);
		}

		[RequirePermission( AppPermissions.Security.Manage )]
		[HttpPost("reset")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ResetPasswordPolicy(CancellationToken cancellationToken)
		{
			await passwordPolicyService.ResetToSystemDefaultAsync(cancellationToken);
			return NoContent();
		}
	}
}