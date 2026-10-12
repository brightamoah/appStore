using appStore.Server.Errors;
using appStore.Server.Models;
using appStore.Server.Services.Interfaces;

namespace appStore.Server.Services
{
    public class TaskStateMachine : ITaskStateMachine
    {
        private static readonly Dictionary<Status, HashSet<Status>> AllowedTransitions = new()
        {
            [Status.NotStarted] =
            [
                Status.InProgress,
                Status.UnderReview,
                Status.ReadyForTest,
                Status.Completed,
                Status.OnHold,
                Status.Cancelled
            ],

            [Status.InProgress] =
            [
                Status.NotStarted,
                Status.UnderReview,
                Status.ReadyForTest,
                Status.Completed,
                Status.OnHold,
                Status.Cancelled
            ],

            [Status.UnderReview] =
            [
                Status.NotStarted,
                Status.InProgress,
                Status.ReadyForTest,
                Status.Completed,
                Status.Cancelled
            ],

            [Status.ReadyForTest] =
            [
                Status.NotStarted,
                Status.InProgress,
                Status.UnderReview,
                Status.Completed,
                Status.Cancelled
            ],

            [Status.OnHold] =
            [
                Status.NotStarted,
                Status.InProgress,
                Status.Cancelled
            ],

            [Status.Completed] =
            [
                Status.NotStarted,
                Status.InProgress,
                Status.UnderReview,
                Status.ReadyForTest,
                Status.Closed
            ],

            [Status.Closed] =
            [
                Status.InProgress,
                Status.Completed,
                Status.Archived
            ],

            [Status.Cancelled] =
            [
                Status.NotStarted,
                Status.InProgress,
                Status.Archived
            ],

            [Status.Archived] =
            [
                Status.NotStarted,
                Status.InProgress
            ]
        };

        public IReadOnlyList<Status> GetAllowedTransitions(Status currentStatus)
        {
            return AllowedTransitions
                .TryGetValue(currentStatus, out var allowed)
                ? allowed.ToList()
                : [];
        }

        public void ValidateTransition(TaskItem task, Status targetStatus, Guid currentUserId)
        {
            if (task.Status == targetStatus) return;

            if (
                !AllowedTransitions.TryGetValue(task.Status, out var allowed) ||
                !allowed.Contains(targetStatus)
            )
            {
                throw AppException.BadRequest(
                    $"Cannot transition task from '{task.Status}' to '{targetStatus}'. Allowed target statuses: [{string.Join(", ", GetAllowedTransitions(task.Status))}]",
                    ErrorCodes.INVALID_STATUS_TRANSITION
                );
            }


            if (targetStatus == Status.InProgress && task.AssigneeId is null && task.AssigneeIds.Count == 0)
            {
                throw AppException.BadRequest(
                    "A task must have an assigned user before it can be moved to In Progress.",
                    ErrorCodes.INVALID_STATUS_TRANSITION
                );
            }

            if (targetStatus == Status.ReadyForTest && !task.Checklists.All(checkList => checkList.IsCompleted))
            {
                var pendingCount = task.Checklists.Count(checkList => !checkList.IsCompleted);

                throw AppException.BadRequest(
                    $"Cannot submit task for testing. {pendingCount} checklist item(s) are still incomplete.",
                    ErrorCodes.INVALID_STATUS_TRANSITION
                );
            }
        }
    }
}