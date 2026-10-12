using appStore.Server.Authorization;
using appStore.Server.Dto.Roles;
using appStore.Server.Errors;
using appStore.Server.Extensions;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/users/{userId:guid}/roles")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class UserRoleController(
		IUserRepository userRepository,
		IRoleRepository roleRepository,
		IUserRoleRepository userRoleRepository
	) : ControllerBase
	{
		[RequireSameUserOrPermission(AppPermissions.Users.Read, "userId")]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserRoleDto>>> GetAllUserRoles(Guid userId, CancellationToken cancellationToken = default)
		{
			var userExists = await userRepository.ExistsByIdAsync(userId, cancellationToken);

			if(!userExists)
			{
				throw AppException.NotFound(
					$"User with ID {userId} not found.",
					ErrorCodes.USER_NOT_FOUND
				);
			}

			var roles = await userRoleRepository.GetByUserIdAsync(userId, cancellationToken);

			var userRoleDtos = roles.Select(UserRoleDto.FromModel);

			return Ok(userRoleDtos);
		}

		[RequireSameUserOrPermission(AppPermissions.Users.Read, "userId")]
		[HttpGet("{roleId:int}")]
		public async Task<ActionResult<UserRoleDto>> GetUserRole(Guid userId, int roleId, CancellationToken cancellationToken = default)
		{
			var userExists = await userRepository.ExistsByIdAsync(userId, cancellationToken);

			if(!userExists)
			{
				throw AppException.NotFound(
					$"User with ID {userId} not found.",
					ErrorCodes.USER_NOT_FOUND
				);
			}

			var roleExists = await roleRepository.ExistsByIdAsync(roleId, cancellationToken);

			if(!roleExists)
			{
				throw AppException.NotFound(
					$"Role with ID {roleId} not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);
			}

			var userRole = await userRoleRepository.GetAsync(userId, roleId, cancellationToken) ??
				throw AppException.NotFound(
					$"User with ID {userId} does not have role with ID {roleId}.",
					ErrorCodes.USER_ROLE_NOT_FOUND
				);

			return Ok(UserRoleDto.FromModel(userRole));
		}

		[RequirePermission(AppPermissions.Users.Manage)]
		[HttpPost("{roleId:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<UserRoleDto>> AssignUserRole(Guid userId, int roleId, CancellationToken cancellationToken = default)
		{
			var userExists = await userRepository.ExistsByIdAsync(userId, cancellationToken);

			if(!userExists)
			{
				throw AppException.NotFound(
					$"User with ID {userId} not found.",
					ErrorCodes.USER_NOT_FOUND
				);
			}

			var role = await roleRepository.GetByIdAsync(roleId, cancellationToken) ??
				throw AppException.NotFound(
					$"Role with ID {roleId} not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);

			var alreadyAssigned = await userRoleRepository.IsAssignedAsync(userId, roleId, cancellationToken);

			if(alreadyAssigned)
			{
				throw AppException.Conflict(
					"The user already has this role",
					ErrorCodes.USER_ROLE_ALREADY_EXISTS
				);
			}

			var userRole = new UserRole
			{
				UserId = userId,
				RoleId = roleId,
				Role = role
			};

			await userRoleRepository.AddAsync(userRole, cancellationToken);

			await userRoleRepository.SaveChangesAsync(cancellationToken);

			return CreatedAtAction(
				nameof(GetUserRole),
				new { userId, roleId },
				UserRoleDto.FromModel(userRole)
			);
		}

		[RequirePermission(AppPermissions.Users.Manage)]
		[HttpDelete("{roleId:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteUserRole(Guid userId, int roleId, CancellationToken cancellationToken = default)
		{
			var userRole = await userRoleRepository.GetAsync(userId, roleId, cancellationToken)
						   ??
				throw AppException.NotFound(
					$"User with ID {userId} does not have role with ID {roleId}.",
					ErrorCodes.USER_ROLE_NOT_FOUND
				);

			userRoleRepository.Remove(userRole);

			await userRoleRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}
	}
}