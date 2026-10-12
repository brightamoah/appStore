using appStore.Server.Models;

namespace appStore.Server.Repositories.Interfaces
{
	public interface IProjectRepository : IRepository<Project, Guid>
	{
		Task<Project?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<Project>> GetProjectsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
		Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default);
		Task<IReadOnlyList<ProjectMember>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken = default);
		Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
		Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
		Task<bool> AreAllUsersMembersAsync(Guid projectId, IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);
		Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default);
		void UpdateMember(ProjectMember member);
		void RemoveMember(ProjectMember member);
	}
}
