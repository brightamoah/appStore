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
	[Route("api/v1/roles/{roleId:int}/permissions")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class RolePermissionController(
		IRoleRepository roleRepository,
		IPermissionRepository permissionRepository,
		IRolePermissionRepository rolePermissionRepository
	) : ControllerBase
	{
		[RequirePermission(AppPermissions.Roles.Read)]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<RolePermissionDto>>> GetAllRolePermissions(int roleId, CancellationToken cancellationToken = default)
		{
			var roleExists = await roleRepository.ExistsByIdAsync(roleId, cancellationToken);

			if (!roleExists)
			{
				throw AppException.NotFound(
					$"Role with ID {roleId} not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);
			}

			var permissions = await rolePermissionRepository.GetByRoleIdAsync(roleId, cancellationToken);

			var dtos = permissions.Select(RolePermissionDto.FromModel);

			return Ok(dtos);
		}

		[RequirePermission(AppPermissions.Roles.Read)]
		[HttpGet("{permissionId:int}")]
		public async Task<ActionResult<RolePermissionDto>> GetRolePermission(int roleId, int permissionId, CancellationToken cancellationToken = default)
		{
			var roleExists = await roleRepository.ExistsByIdAsync(roleId, cancellationToken);

			if (!roleExists)
			{
				throw AppException.NotFound(
					$"Role with ID {roleId} not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);
			}

			var permissionExists = await permissionRepository.ExistsByIdAsync(permissionId, cancellationToken);

			if (!permissionExists)
			{
				throw AppException.NotFound(
					$"Permission with ID {permissionId} not found.",
					ErrorCodes.PERMISSION_NOT_FOUND
				);
			}

			var rolePermission = await rolePermissionRepository.GetAsync(roleId, permissionId, cancellationToken) ??
				throw AppException.NotFound(
					$"Role with ID {roleId} does not have permission with ID {permissionId}.",
					ErrorCodes.ROLE_PERMISSION_NOT_FOUND
				);

			return Ok(RolePermissionDto.FromModel(rolePermission));
		}

		[RequirePermission(AppPermissions.Roles.Manage)]
		[HttpPost("{permissionId:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<RolePermissionDto>> AssignPermissionToRole(int roleId, int permissionId, CancellationToken cancellationToken = default)
		{
			var roleExists = await roleRepository.ExistsByIdAsync(roleId, cancellationToken);

			if (!roleExists)
			{
				throw AppException.NotFound(
					$"Role with ID {roleId} not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);
			}

			var permission = await permissionRepository.GetByIdAsync(permissionId, cancellationToken) ??
				throw AppException.NotFound(
					$"Permission with ID {permissionId} not found.",
					ErrorCodes.PERMISSION_NOT_FOUND
				);

			var alreadyAssigned = await rolePermissionRepository.IsAssignedAsync(roleId, permissionId, cancellationToken);

			if (alreadyAssigned)
			{
				throw AppException.Conflict(
					$"Permission with ID {permissionId} is already assigned to role with ID {roleId}.",
					ErrorCodes.ROLE_PERMISSION_ALREADY_EXISTS
				);
			}

			var rolePermission = new RolePermission
			{
				RoleId = roleId,
				PermissionId = permissionId,
				Permission = permission
			};

			await rolePermissionRepository.AddAsync(rolePermission, cancellationToken);
			await rolePermissionRepository.SaveChangesAsync(cancellationToken);

			return CreatedAtAction(
				nameof(GetRolePermission),
				new { roleId, permissionId },
				RolePermissionDto.FromModel(rolePermission)
			);
		}

		[RequirePermission(AppPermissions.Roles.Manage)]
		[HttpDelete("{permissionId:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeleteRolePermission(int roleId, int permissionId, CancellationToken cancellationToken = default)
		{
			var roleExists = await roleRepository.ExistsByIdAsync(roleId, cancellationToken);

			if (!roleExists)
			{
				throw AppException.NotFound(
					$"Role with ID {roleId} not found.",
					ErrorCodes.ROLE_NOT_FOUND
				);
			}

			var permissionExists = await permissionRepository.ExistsByIdAsync(permissionId, cancellationToken);

			if (!permissionExists)
			{
				throw AppException.NotFound(
					$"Permission with ID {permissionId} not found.",
					ErrorCodes.PERMISSION_NOT_FOUND
				);
			}

			var rolePermission = await rolePermissionRepository.GetAsync(roleId, permissionId, cancellationToken) ??
				throw AppException.NotFound(
					$"Role with ID {roleId} does not have permission with ID {permissionId}.",
					ErrorCodes.ROLE_PERMISSION_NOT_FOUND
				);

			rolePermissionRepository.Remove(rolePermission);
			await rolePermissionRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}
	}
}