using appStore.Server.Authorization;
using appStore.Server.Dto.Priorities;
using appStore.Server.Errors;
using appStore.Server.Extensions;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace appStore.Server.Controllers
{
	[Route("api/v1/priorities")]
	[ApiController]
	[EnableRateLimiting(RateLimiterExtensions.ApiPolicy)]
	public class PriorityController(
		IPriorityRepository priorityRepository,
		ITaskRepository taskRepository
	) : ControllerBase
	{
		[Authorize]
		[HttpGet]
		public async Task<ActionResult<IEnumerable<Priority>>> GetAllPriorities(CancellationToken cancellationToken = default)
		{
			var priorities = await priorityRepository.GetAllAsync(cancellationToken);
			return Ok(priorities);
		}

		[RequirePermission(AppPermissions.Priority.Read)]
		[HttpGet("{id:int}")]
		public async Task<ActionResult<PriorityDto>> GetPriorityById(int id, CancellationToken cancellationToken = default)
		{
			var priority = await priorityRepository.GetByIdAsync(id, cancellationToken) ??
				throw AppException.NotFound(
					$"Priority with ID '{id}' was not found.",
					ErrorCodes.PRIORITY_NOT_FOUND
				);

			return Ok(PriorityDto.FromModel(priority));
		}

		[RequirePermission(AppPermissions.Priority.Create)]
		[HttpPost]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<ActionResult<PriorityDto>> CreatePriority(CreatePriorityDto priorityDto, CancellationToken cancellationToken = default)
		{
			var trimmedName = priorityDto.Name.Trim();

			var existingPriority = await priorityRepository.ExistsByNameAsync(trimmedName, cancellationToken: cancellationToken);

			if (existingPriority)
			{
				throw AppException.Conflict(
					$"Priority with name '{trimmedName}' already exists.",
					ErrorCodes.PRIORITY_ALREADY_EXISTS
				);
			}

			var priorityItem = new Priority
			{
				Name = trimmedName
			};

			await priorityRepository.AddAsync(priorityItem, cancellationToken);
			await priorityRepository.SaveChangesAsync(cancellationToken);

			return CreatedAtAction(
				nameof(GetPriorityById),
				new { id = priorityItem.Id },
				PriorityDto.FromModel(priorityItem)
			);
		}

		[RequirePermission(AppPermissions.Priority.Update)]
		[HttpPatch("{id:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditPriority(int id, UpdatePriorityDto priorityDto, CancellationToken cancellationToken = default)
		{
			var existingPriority = await priorityRepository.GetByIdAsync(id, cancellationToken) ??
				throw AppException.NotFound(
					$"Priority with ID '{id}' was not found.",
					ErrorCodes.PRIORITY_NOT_FOUND
				);

			var trimmedName = priorityDto.Name.Trim();

			if (!string.IsNullOrWhiteSpace(trimmedName))
			{
				var nameConflict = await priorityRepository.ExistsByNameAsync(
					trimmedName,
					excludeId: id,
					cancellationToken: cancellationToken
				);

				if (nameConflict)
				{
					throw AppException.Conflict(
						$"Priority with name '{trimmedName}' already exists.",
						ErrorCodes.PRIORITY_ALREADY_EXISTS
					);
				}
			}

			priorityDto.PatchModel(existingPriority);
			priorityRepository.Update(existingPriority);
			await priorityRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}

		[RequirePermission(AppPermissions.Priority.Delete)]
		[HttpDelete("{id:int}")]
		[EnableRateLimiting(RateLimiterExtensions.MutationPolicy)]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeletePriority(int id, CancellationToken cancellationToken = default)
		{
			var priorityItem = await priorityRepository.GetByIdAsync(id, cancellationToken) ??
				throw AppException.NotFound(
					$"Priority with ID '{id}' was not found.",
					ErrorCodes.PRIORITY_NOT_FOUND
				);

			var isReferenced = await taskRepository.AnyWithPriorityIdAsync(id, cancellationToken);

			if (isReferenced)
			{
				throw AppException.Conflict(
					$"Cannot delete priority with ID '{id}' because it is assigned to existing tasks."
				);
			}

			priorityRepository.Remove(priorityItem);
			await priorityRepository.SaveChangesAsync(cancellationToken);

			return NoContent();
		}
	}
}
