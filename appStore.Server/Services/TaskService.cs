using appStore.Server.Dto.Common;
using appStore.Server.Dto.Tasks;
using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
	public sealed class TaskService(
		ITaskRepository taskRepository,
		IProjectRepository projectRepository,
		IBoardRepository boardRepository,
		IPriorityRepository priorityRepository,
		ITaskStateMachine taskStateMachine
	) : ITaskService
	{
		public async Task<TaskDto> CreateTaskAsync(
			CreateTaskDto createTaskDto,
			Guid currentUserId,
			CancellationToken cancellationToken = default
		)
		{
			var priorityExists = await priorityRepository.AnyAsync(
				priority => priority.Id == createTaskDto.PriorityId,
				cancellationToken
			);

			if (!priorityExists)
			{
				throw AppException.NotFound(
					$"Priority with ID '{createTaskDto.PriorityId}' was not found.",
					ErrorCodes.PRIORITY_NOT_FOUND
				);
			}

			var project = await projectRepository.GetByIdAsync(
				createTaskDto.ProjectId,
				cancellationToken
			) ?? throw AppException.NotFound(
				$"Project with ID '{createTaskDto.ProjectId}' was not found.",
				ErrorCodes.PROJECT_NOT_FOUND
			);

			var assigneeIds = createTaskDto.AssigneeIds ?? (
				createTaskDto.AssigneeId.HasValue
					? [createTaskDto.AssigneeId.Value]
					: []
			);

			if (assigneeIds.Count > 0)
			{
				var areMembers = await projectRepository.AreAllUsersMembersAsync(
					createTaskDto.ProjectId,
					assigneeIds,
					cancellationToken
				);

				if (!areMembers)
				{
					throw AppException.BadRequest(
						"One or more assigned users are not members of this project.",
						ErrorCodes.VALIDATION_ERROR
					);
				}
			}

			project.TaskCounter++;
			projectRepository.Update(project);

			var taskKey = $"{project.Key}-{project.TaskCounter}";

			Guid columnId;
			Status initialStatus;
			int orderIndex;

			if (createTaskDto.ColumnId.HasValue)
			{
				var column = await boardRepository.GetByIdAsync(
					createTaskDto.ColumnId.Value,
					cancellationToken
				);

				if (column == null || column.ProjectId != createTaskDto.ProjectId)
				{
					throw AppException.NotFound(
						$"Column with ID '{createTaskDto.ColumnId}' was not found in project.",
						ErrorCodes.COLUMN_NOT_FOUND
					);
				}

				columnId = column.Id;
				initialStatus = column.StatusMapping;
				var maxOrder = await taskRepository.GetMaxOrderIndexInColumnAsync(columnId, cancellationToken);
				orderIndex = maxOrder + 1;
			}
			else
			{
				var firstColumn = await boardRepository.GetFirstColumnAsync(
					createTaskDto.ProjectId,
					cancellationToken
				);

				if (firstColumn == null)
				{
					var defaultColumns = BoardColumn.CreateDefaultColumns(createTaskDto.ProjectId);
					await boardRepository.AddRangeAsync(defaultColumns, cancellationToken);
					await boardRepository.SaveChangesAsync(cancellationToken);
					firstColumn = defaultColumns[0];
				}

				columnId = firstColumn.Id;
				initialStatus = firstColumn.StatusMapping;
				var maxOrder = await taskRepository.GetMaxOrderIndexInColumnAsync(columnId, cancellationToken);
				orderIndex = maxOrder + 1;
			}

			var now = DateTime.UtcNow;
			var primaryAssigneeId = assigneeIds.Count > 0 ? assigneeIds[0] : createTaskDto.AssigneeId;

			var taskItem = new TaskItem
			{
				Id = Guid.NewGuid(),
				TaskKey = taskKey,
				Name = createTaskDto.Name.Trim(),
				Description = createTaskDto.Description?.Trim() ?? string.Empty,
				PriorityId = createTaskDto.PriorityId,
				Status = initialStatus,
				IssueType = createTaskDto.IssueType,
				ProjectId = createTaskDto.ProjectId,
				ColumnId = columnId,
				OrderIndex = orderIndex,
				AssigneeId = primaryAssigneeId,
				AssigneeIds = assigneeIds,
				ReporterId = currentUserId,
				DueDate = createTaskDto.DueDate,
				EstimatedMinutes = createTaskDto.EstimatedMinutes,
				IsCompleted = initialStatus == Status.Completed,
				CompletedAt = initialStatus == Status.Completed ? now : null,
				CreatedAt = now,
				UpdatedAt = now
			};

			await taskRepository.AddAsync(taskItem, cancellationToken);
			await taskRepository.SaveChangesAsync(cancellationToken);

			return TaskDto.FromModel(taskItem);
		}

		public async Task<TaskDto> MoveTaskAsync(
			Guid taskId,
			MoveTaskDto moveTaskDto,
			Guid currentUserId,
			CancellationToken cancellationToken = default
		)
		{
			var taskItem = await taskRepository.GetByIdWithDetailsAsync(taskId, cancellationToken)
				?? throw AppException.NotFound(
					$"Task with ID '{taskId}' was not found.",
					ErrorCodes.TASK_NOT_FOUND
				);

			var targetColumn = await boardRepository.GetByIdAsync(moveTaskDto.TargetColumnId, cancellationToken)
				?? throw AppException.NotFound(
					$"Target column with ID '{moveTaskDto.TargetColumnId}' was not found.",
					ErrorCodes.COLUMN_NOT_FOUND
				);

			var sourceColumnId = taskItem.ColumnId;
			var oldOrderIndex = taskItem.OrderIndex;
			var isDifferentColumn = sourceColumnId != targetColumn.Id;

			if (isDifferentColumn)
			{
				if (moveTaskDto.AssigneeId.HasValue)
				{
					if (taskItem.ProjectId.HasValue)
					{
						var isMember = await projectRepository.IsMemberAsync(
							taskItem.ProjectId.Value,
							moveTaskDto.AssigneeId.Value,
							cancellationToken
						);

						if (!isMember)
						{
							throw AppException.BadRequest(
								$"User with ID '{moveTaskDto.AssigneeId.Value}' is not a member of this project.",
								ErrorCodes.VALIDATION_ERROR
							);
						}
					}

					taskItem.AssigneeId = moveTaskDto.AssigneeId.Value;
					if (!taskItem.AssigneeIds.Contains(moveTaskDto.AssigneeId.Value))
					{
						taskItem.AssigneeIds.Add(moveTaskDto.AssigneeId.Value);
					}
				}

				taskStateMachine.ValidateTransition(
					taskItem,
					targetColumn.StatusMapping,
					currentUserId
				);

				taskItem.Status = targetColumn.StatusMapping;
				taskItem.IsCompleted = targetColumn.StatusMapping == Status.Completed;
				taskItem.CompletedAt = targetColumn.StatusMapping == Status.Completed ? DateTime.UtcNow : null;
				taskItem.ColumnId = targetColumn.Id;

				// Shift source neighbors down
				if (sourceColumnId.HasValue)
				{
					var sourceNeighbors = await taskRepository.GetTasksInColumnAsync(sourceColumnId.Value, cancellationToken);
					foreach (var neighbor in sourceNeighbors.Where(task => task.OrderIndex > oldOrderIndex && task.Id != taskItem.Id))
					{
						neighbor.OrderIndex--;
					}
				}

				// Shift target neighbors up
				var targetNeighbors = await taskRepository.GetTasksInColumnAsync(targetColumn.Id, cancellationToken);
				foreach (var neighbor in targetNeighbors.Where(task => task.OrderIndex >= moveTaskDto.TargetOrderIndex && task.Id != taskItem.Id))
				{
					neighbor.OrderIndex++;
				}
			}
			else if (sourceColumnId.HasValue)
			{
				var columnTasks = await taskRepository.GetTasksInColumnAsync(sourceColumnId.Value, cancellationToken);

				if (oldOrderIndex < moveTaskDto.TargetOrderIndex)
				{
					// Moving down: shift items between old and new index up
					foreach (var shiftingTask in columnTasks.Where(task =>
						task.OrderIndex > oldOrderIndex &&
						task.OrderIndex <= moveTaskDto.TargetOrderIndex &&
						task.Id != taskItem.Id))
					{
						shiftingTask.OrderIndex--;
					}
				}
				else if (oldOrderIndex > moveTaskDto.TargetOrderIndex)
				{
					// Moving up: shift items between new and old index down
					foreach (var shiftingTask in columnTasks.Where(task =>
						task.OrderIndex >= moveTaskDto.TargetOrderIndex &&
						task.OrderIndex < oldOrderIndex &&
						task.Id != taskItem.Id))
					{
						shiftingTask.OrderIndex++;
					}
				}
			}

			taskItem.OrderIndex = moveTaskDto.TargetOrderIndex;
			taskItem.UpdatedAt = DateTime.UtcNow;

			await taskRepository.SaveChangesAsync(cancellationToken);

			return TaskDto.FromModel(taskItem);
		}

		public async Task<TaskDto> TransitionStatusAsync(
			Guid taskId,
			Status targetStatus,
			Guid currentUserId,
			CancellationToken cancellationToken = default
		)
		{
			var taskItem = await taskRepository.GetByIdWithDetailsAsync(taskId, cancellationToken)
				?? throw AppException.NotFound(
					$"Task with ID '{taskId}' was not found.",
					ErrorCodes.TASK_NOT_FOUND
				);

			taskStateMachine.ValidateTransition(taskItem, targetStatus, currentUserId);

			taskItem.Status = targetStatus;
			taskItem.IsCompleted = targetStatus == Status.Completed;
			taskItem.CompletedAt = targetStatus == Status.Completed ? DateTime.UtcNow : null;
			taskItem.UpdatedAt = DateTime.UtcNow;

			if (taskItem.ProjectId.HasValue)
			{
				var matchingColumn = await boardRepository.GetMatchingColumnAsync(
					taskItem.ProjectId.Value,
					targetStatus,
					cancellationToken
				);

				if (matchingColumn != null)
				{
					taskItem.ColumnId = matchingColumn.Id;
				}
			}

			await taskRepository.SaveChangesAsync(cancellationToken);

			return TaskDto.FromModel(taskItem);
		}

		public async Task<TaskDto> UpdateTaskAsync(
			Guid taskId,
			UpdateTaskDto updateTaskDto,
			Guid currentUserId,
			CancellationToken cancellationToken = default
		)
		{
			var taskItem = await taskRepository.GetByIdAsync(taskId, cancellationToken)
				?? throw AppException.NotFound(
					$"Task with ID '{taskId}' was not found.",
					ErrorCodes.TASK_NOT_FOUND
				);

			if (taskItem.ProjectId.HasValue)
			{
				var assigneesToCheck = updateTaskDto.AssigneeIds ?? (
					updateTaskDto.AssigneeId.HasValue ? [updateTaskDto.AssigneeId.Value] : null
				);

				if (assigneesToCheck != null && assigneesToCheck.Count > 0)
				{
					var areMembers = await projectRepository.AreAllUsersMembersAsync(
						taskItem.ProjectId.Value,
						assigneesToCheck,
						cancellationToken
					);

					if (!areMembers)
					{
						throw AppException.BadRequest(
							"One or more assigned users are not members of this project.",
							ErrorCodes.VALIDATION_ERROR
						);
					}
				}
			}

			if (!string.IsNullOrWhiteSpace(updateTaskDto.Name))
			{
				taskItem.Name = updateTaskDto.Name.Trim();
			}

			if (updateTaskDto.Description != null)
			{
				taskItem.Description = updateTaskDto.Description.Trim();
			}

			if (updateTaskDto.PriorityId.HasValue)
			{
				var priorityExists = await priorityRepository.AnyAsync(
					priority => priority.Id == updateTaskDto.PriorityId.Value,
					cancellationToken
				);

				if (!priorityExists)
				{
					throw AppException.NotFound(
						$"Priority with ID '{updateTaskDto.PriorityId.Value}' was not found.",
						ErrorCodes.PRIORITY_NOT_FOUND
					);
				}

				taskItem.PriorityId = updateTaskDto.PriorityId.Value;
			}

			if (updateTaskDto.IssueType.HasValue)
			{
				taskItem.IssueType = updateTaskDto.IssueType.Value;
			}

			if (updateTaskDto.AssigneeIds != null)
			{
				taskItem.AssigneeIds = updateTaskDto.AssigneeIds;
				taskItem.AssigneeId = updateTaskDto.AssigneeIds.Count > 0 ? updateTaskDto.AssigneeIds[0] : null;
			}
			else if (updateTaskDto.AssigneeId.HasValue)
			{
				taskItem.AssigneeId = updateTaskDto.AssigneeId.Value;
				if (!taskItem.AssigneeIds.Contains(updateTaskDto.AssigneeId.Value))
				{
					taskItem.AssigneeIds.Add(updateTaskDto.AssigneeId.Value);
				}
			}

			if (updateTaskDto.DueDate.HasValue)
			{
				taskItem.DueDate = updateTaskDto.DueDate;
			}

			if (updateTaskDto.EstimatedMinutes.HasValue)
			{
				taskItem.EstimatedMinutes = updateTaskDto.EstimatedMinutes;
			}

			taskItem.UpdatedAt = DateTime.UtcNow;

			await taskRepository.SaveChangesAsync(cancellationToken);

			return TaskDto.FromModel(taskItem);
		}

		public async Task<TaskDto> GetTaskByIdAsync(
			Guid taskId,
			CancellationToken cancellationToken = default
		)
		{
			var taskItem = await taskRepository.GetByIdWithDetailsAsync(taskId, cancellationToken)
				?? throw AppException.NotFound(
					$"Task with ID '{taskId}' was not found.",
					ErrorCodes.TASK_NOT_FOUND
				);

			return TaskDto.FromModel(taskItem);
		}

		public async Task<PagedResponse<TasksListDto>> GetTasksAsync(
			Guid? projectId,
			PaginationParams pagination,
			CancellationToken cancellationToken = default
		)
		{
			return await taskRepository.GetPagedTasksAsync(projectId, pagination, cancellationToken);
		}

		public async Task DeleteTaskAsync(
			Guid taskId,
			CancellationToken cancellationToken = default
		)
		{
			var taskItem = await taskRepository.GetByIdAsync(taskId, cancellationToken)
				?? throw AppException.NotFound(
					$"Task with ID '{taskId}' was not found.",
					ErrorCodes.TASK_NOT_FOUND
				);

			taskRepository.Remove(taskItem);
			await taskRepository.SaveChangesAsync(cancellationToken);
		}
	}
}