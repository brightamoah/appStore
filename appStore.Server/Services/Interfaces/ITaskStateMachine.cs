using appStore.Server.Models;

namespace appStore.Server.Services.Interfaces
{
    public interface ITaskStateMachine
    {
        /// <summary>
        /// Validates whether transitioning from currentStatus to targetStatus is allowed.
        /// Throws AppException if the transition is invalid.
        /// </summary>
        void ValidateTransition(TaskItem task, Status targetStatus, Guid currentUserId);

        /// <summary>
        /// Returns all valid statuses a task can currently transition to.
        /// </summary>
        IReadOnlyList<Status> GetAllowedTransitions(Status currentStatus);
    }
}