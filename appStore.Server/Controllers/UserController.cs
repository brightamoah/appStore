using appStore.Server.Authorization;
using appStore.Server.Dto.Users;
using appStore.Server.Errors;
using appStore.Server.Extensions;
using appStore.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/users")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class UserController(IUserRepository userRepository) : ControllerBase
	{
		[RequirePermission(AppPermissions.Users.Read)]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(CancellationToken cancellationToken = default)
		{
			var users = await userRepository.GetAllAsync(cancellationToken);
			return Ok(users.Select(UserDto.FromModel));
		}

		[RequireSameUserOrPermission]
		[HttpGet("{id:guid}")]
		public async Task<ActionResult<UserDto>> GetUserById(Guid id, CancellationToken cancellationToken = default)
		{
			var existingUser = await userRepository.GetByIdAsync(id, cancellationToken);

			return existingUser == null
				? throw AppException.NotFound(
						$"User with ID '{id}' was not found.",
						ErrorCodes.USER_NOT_FOUND
					)
				: Ok(UserDto.FromModel(existingUser));
		}

		[HttpGet("test")]
		[RequirePermission(AppPermissions.Tasks.Read)]
		public IActionResult TestPermission()
		{
			return Ok(new
			{
				message = "You have the tasks.read permission."
			});
		}
	}
}
