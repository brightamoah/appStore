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
	[Route("api/v1/roles")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class RoleController(
		IRoleRepository roleRepository,
		IUserRoleRepository userRoleRepository
	) : ControllerBase
	{
		[RequirePermission(AppPermissions.Roles.Read)]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles(CancellationToken cancellationToken = default)
		{
			var roles = await roleRepository.GetAllAsync(cancellationToken);

			var roleDtos = roles.Select(RoleDto.FromModel);

			return Ok(roleDtos);
		}

		[RequirePermission(AppPermissions.Roles.Read)]
		[HttpGet("{id:int}")]
		public async Task<ActionResult<RoleDto>> GetRoleById(int id, CancellationToken cancellationToken = default)
		{
			var role = await roleRepository.GetByIdAsync(id, cancellationToken) ??

				throw AppException.NotFound(
					$"Role with ID '{id}' was not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);

			return Ok(RoleDto.FromModel(role));
		}

		[RequirePermission(AppPermissions.Roles.Manage)]
		[HttpPost]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<RoleDto>> CreateRole(CreateRoleDto roleDto, CancellationToken cancellationToken = default)
		{
			var roleName = roleDto.Name.Trim().ToLowerInvariant();

			var existingRole = await roleRepository.ExistsByNameAsync(roleName, null, cancellationToken);

			if(existingRole)
			{
				throw AppException.Conflict(
					$"Role with name '{roleName}' already exists.",
					ErrorCodes.ROLE_ALREADY_EXISTS
				);
			}

			var newRole = new Role
			{
				Name = roleName,
				Description = roleDto.Description
			};

			await roleRepository.AddAsync(newRole, cancellationToken);

			await roleRepository.SaveChangesAsync(cancellationToken);

			return CreatedAtAction(
				nameof(GetRoleById),
				new { id = newRole.Id },
				RoleDto.FromModel(newRole)
			);
		}

		[RequirePermission(AppPermissions.Roles.Manage)]
		[HttpPatch("{id:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> UpdateRole(int id, UpdateRoleDto updateRoleDto, CancellationToken cancellationToken = default)
		{
			var existingRole = await roleRepository.GetByIdAsync(id, cancellationToken) ??
				throw AppException.NotFound(
					$"Role with ID '{id}' was not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);

			var newName = updateRoleDto.Name?.Trim().ToLowerInvariant();

			if(!string.IsNullOrWhiteSpace(newName))
			{
				var conflicts = await roleRepository.ExistsByNameAsync(newName, id, cancellationToken);

				if(conflicts)
				{
					throw AppException.Conflict(
						$"Role with name '{newName}' already exists.",
						ErrorCodes.ROLE_ALREADY_EXISTS
					);
				}
			}

			updateRoleDto.PatchModel(existingRole);
			roleRepository.Update(existingRole);

			await roleRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}

		[RequirePermission(AppPermissions.Roles.Manage)]
		[HttpDelete("{id:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteRole(int id, CancellationToken cancellationToken = default)
		{
			var existingRole = await roleRepository.GetByIdAsync(id, cancellationToken) ??
				throw AppException.NotFound(
					$"Role with ID '{id}' was not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);

			var isAssigned = await userRoleRepository.IsRoleAssignedToAnyUserAsync(id, cancellationToken);

			if(isAssigned)
			{
				throw AppException.Conflict(
					$"Cannot delete role with ID '{id}' because it is assigned to users.",
					ErrorCodes.ROLE_IS_ASSIGNED_TO_USERS
				);
			}

			roleRepository.Remove(existingRole);
			await roleRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}
	}
}