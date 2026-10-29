using System.Security.Claims;

using appStore.Server.Authorization;
using appStore.Server.Dto.Users;
using appStore.Server.Errors;
using appStore.Server.Extensions;
using appStore.Server.Services.Interfaces;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/auth")]
	[ApiController]
	public sealed class AuthController(
		IAuthenticationService authenticationService,
		IUserService userService,
		IRefreshTokenCookieService refreshTokenCookieService,
		IAntiforgery antiforgery,
		IProgressiveRateLimitTracker rateLimitTracker
	) : ControllerBase
	{
		[AllowAnonymous]
		[HttpGet("csrf")]
		public IActionResult GetCsrfToken()
		{
			var tokens = antiforgery.GetAndStoreTokens(HttpContext);

			return Ok(new
			{
				csrfToken = tokens.RequestToken
			});
		}

		[RequireSameUserOrPermission]
		[HttpGet("{id:guid}")]
		public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken)
		{
			var user = await userService.GetUserByIdAsync(id, cancellationToken);

			return Ok(user);
		}

		[Authorize]
		[HttpGet("me")]
		public async Task<ActionResult<UserDto>> GetCurrentUser(CancellationToken cancellationToken)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrWhiteSpace(userIdClaim))
			{
				throw AppException.Unauthorized(
					"The access token does not contain a valid user identifier.",
					AuthErrorCodes.TOKEN_INVALID
				);
			}

			if (!Guid.TryParse(userIdClaim, out var userId))
			{
				throw AppException.Unauthorized(
					"Invalid token: User ID claim is not valid.",
					AuthErrorCodes.TOKEN_INVALID
				);
			}

			var user = await userService.GetCurrentUserAsync(userId, cancellationToken);

			return Ok(user);
		}

		[AllowAnonymous]
		[HttpPost("signup")]
		[EnableRateLimiting(RateLimiterExtensions.AuthPolicy)]
		[ValidateAntiforgeryToken]
		public async Task<ActionResult<UserDto>> SignUp(
			CreateUserDto createUserDto,
			CancellationToken cancellationToken
		)
		{
			var user = await userService.CreateUserAsync(
				createUserDto,
				cancellationToken
			);

			return CreatedAtAction(
				nameof(GetUserById),
				new { id = user.Id },
				new
				{
					message = "Account created successfully",
					data = user
				}
			);
		}

		[AllowAnonymous]
		[HttpPost("login")]
		[EnableRateLimiting(RateLimiterExtensions.AuthPolicy)]
		[ValidateAntiforgeryToken]
		public async Task<ActionResult<UserDto>> Login(
			UserLoginDto loginDto,
			CancellationToken cancellationToken
		)
		{
			var result = await authenticationService.LoginAsync(loginDto, cancellationToken);

			var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";

			rateLimitTracker.Reset(clientIp);

			refreshTokenCookieService.SetRefreshTokenCookie(
				Response,
				result.RefreshToken,
				result.RefreshTokenExpiresAt
			);

			return Ok(new
			{
				message = "Login successful.",
				data = new
				{
					user = result.User,
					roles = result.Roles,
					permissions = result.Permissions,
				},
				accessToken = result.AccessToken,
				refreshToken = result.RefreshToken
			});
		}

		[AllowAnonymous]
		[HttpPost("refresh")]
		public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
		{
			var refreshToken = refreshTokenCookieService.GetRefreshToken(Request)
				?? Request.Headers["X-Refresh-Token"].FirstOrDefault();

			if (string.IsNullOrWhiteSpace(refreshToken))
			{
				throw AppException.Unauthorized(
					"Invalid refresh token.",
					AuthErrorCodes.REFRESH_TOKEN_INVALID
				);
			}

			var result = await authenticationService.RefreshAsync(
				refreshToken,
				cancellationToken
			);

			refreshTokenCookieService.SetRefreshTokenCookie(
				Response,
				result.RefreshToken,
				result.RefreshTokenExpiresAt
			);

			return Ok(new
			{
				message = "Token refreshed successfully.",
				accessToken = result.AccessToken,
				refreshToken = result.RefreshToken
			});
		}

		[AllowAnonymous]
		[HttpPost("logout")]
		public async Task<IActionResult> Logout(CancellationToken cancellationToken)
		{
			var refreshToken = refreshTokenCookieService.GetRefreshToken(Request)
				?? Request.Headers["X-Refresh-Token"].FirstOrDefault();

			if (!string.IsNullOrWhiteSpace(refreshToken))
			{
				await authenticationService.RevokeRefreshTokenAsync(
					refreshToken,
					cancellationToken
				);
			}

			refreshTokenCookieService.DeleteRefreshTokenCookie(Response);

			return NoContent();
		}
	}
}