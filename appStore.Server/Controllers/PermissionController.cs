using appStore.Server.Authorization;
using appStore.Server.Dto.Permissions;
using appStore.Server.Errors;
using appStore.Server.Extensions;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/permissions")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class PermissionController(
		IPermissionRepository permissionRepository,
		IRolePermissionRepository rolePermissionRepository
	) : ControllerBase
	{
		[RequirePermission(AppPermissions.Permissions.Read)]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<PermissionDto>>> GetAllPermissions(CancellationToken cancellationToken = default)
		{
			var permissions = await permissionRepository.GetAllAsync(cancellationToken);

			var permissionDtos = permissions.Select(PermissionDto.FromModel);

			return Ok(permissionDtos);
		}

		[RequirePermission(AppPermissions.Permissions.Read)]
		[HttpGet("{id:int}")]
		public async Task<ActionResult<PermissionDto>> GetPermissionById(int id, CancellationToken cancellationToken = default)
		{
			var permission = await permissionRepository.GetByIdAsync(id, cancellationToken) ??

				throw AppException.NotFound(
					$"Permission with id {id} not found.",
					ErrorCodes.PERMISSION_NOT_FOUND
				);

			return Ok(PermissionDto.FromModel(permission));
		}

		[RequirePermission(AppPermissions.Permissions.Manage)]
		[HttpPost]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<PermissionDto>> CreatePermission(CreatePermissionDto permissionDto, CancellationToken cancellationToken = default)
		{
			var permissionName = permissionDto.Name.Trim().ToLowerInvariant();
			var description = permissionDto.Description.Trim();

			var alreadyExists = await permissionRepository.ExistsByNameAsync(permissionName, cancellationToken);

			if(alreadyExists)
			{
				throw AppException.Conflict(
					$"Permission with name '{permissionName}' already exists.",
					ErrorCodes.PERMISSION_ALREADY_EXISTS
				);
			}

			var permission = new Permission
			{
				Name = permissionName,
				Description = description,
			};

			await permissionRepository.AddAsync(permission, cancellationToken);
			await permissionRepository.SaveChangesAsync(cancellationToken);

			return CreatedAtAction(
				nameof(GetPermissionById),
				new { id = permission.Id },
				PermissionDto.FromModel(permission)
			);
		}

		[RequirePermission(AppPermissions.Permissions.Manage)]
		[HttpPatch("{id:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<PermissionDto>> UpdatePermission(int id, UpdatePermissionDto permissionDto, CancellationToken cancellationToken = default)
		{
			var permission = await permissionRepository.GetByIdAsync(id, cancellationToken) ??

				throw AppException.NotFound(
					$"Permission with id {id} not found.",
					ErrorCodes.PERMISSION_NOT_FOUND
				);

			if(permissionDto.Name is not null)
			{
				var newName = permissionDto.Name.Trim().ToLowerInvariant();

				if(permission.Name != newName)
				{
					var alreadyExists = await permissionRepository.ExistsByNameAsync(newName, cancellationToken);

					if(alreadyExists)
					{
						throw AppException.Conflict(
							$"Permission with name '{newName}' already exists.",
							ErrorCodes.PERMISSION_ALREADY_EXISTS
						);
					}
				}
			}

			permissionDto.PatchModel(permission);
			permissionRepository.Update(permission);

			await permissionRepository.SaveChangesAsync(cancellationToken);

			return Ok(PermissionDto.FromModel(permission));
		}

		[RequirePermission(AppPermissions.Permissions.Manage)]
		[HttpDelete("{id:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult> DeletePermission(int id, CancellationToken cancellationToken = default)
		{
			var permission = await permissionRepository.GetByIdAsync(id, cancellationToken) ??

				throw AppException.NotFound(
					$"Permission with id {id} not found.",
					ErrorCodes.PERMISSION_NOT_FOUND
				);

			var isAssigned = await rolePermissionRepository.IsPermissionAssignedToAnyRoleAsync(id, cancellationToken);

			if(isAssigned)
			{
				throw AppException.Conflict(
					$"Permission with id {id} is assigned to one or more roles and cannot be deleted.",
					ErrorCodes.PERMISSION_IN_USE
				);
			}

			permissionRepository.Remove(permission);

			await permissionRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}
	}
}