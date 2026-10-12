using appStore.Server.Data;
using appStore.Server.Models;
using appStore.Server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Repositories
{
	public class ProjectRepository(DatabaseContext context) : Repository<Project, Guid>(context), IProjectRepository
	{
		public async Task<Project?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Include(project => project.Members)
					.ThenInclude(projectMember => projectMember.User)
				.FirstOrDefaultAsync(project => project.Id == id, cancellationToken);
		}

		public async Task<IReadOnlyList<Project>> GetProjectsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
		{
			return await DbSet
				.Include(project => project.Members)
					.ThenInclude(projectMember => projectMember.User)
				.Where(project => project.OwnerId == userId || project.Members.Any(member => member.UserId == userId))
				.OrderByDescending(project => project.UpdatedAt)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<bool> ExistsByKeyAsync(string key, CancellationToken cancellationToken = default)
		{
			var sanitizedKey = key.Trim().ToUpperInvariant();
			return await DbSet.AnyAsync(project => project.Key == sanitizedKey, cancellationToken);
		}

		public async Task<IReadOnlyList<ProjectMember>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken = default)
		{
			return await Context.ProjectMembers
				.Include(member => member.User)
				.Where(member => member.ProjectId == projectId)
				.OrderBy(member => member.JoinedAt)
				.AsNoTracking()
				.ToListAsync(cancellationToken);
		}

		public async Task<ProjectMember?> GetMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
		{
			return await Context.ProjectMembers
				.Include(member => member.User)
				.FirstOrDefaultAsync(member => member.ProjectId == projectId && member.UserId == userId, cancellationToken);
		}

		public async Task<bool> IsMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
		{
			var isOwner = await DbSet.AnyAsync(project => project.Id == projectId && project.OwnerId == userId, cancellationToken);
			if (isOwner)
			{
				return true;
			}

			return await Context.ProjectMembers.AnyAsync(
				member => member.ProjectId == projectId && member.UserId == userId,
				cancellationToken
			);
		}

		public async Task<bool> AreAllUsersMembersAsync(Guid projectId, IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
		{
			var uniqueUserIds = userIds.Distinct().ToList();
			if (uniqueUserIds.Count == 0)
			{
				return true;
			}

			var project = await DbSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
			if (project == null)
			{
				return false;
			}

			var memberIds = await Context.ProjectMembers
				.Where(member => member.ProjectId == projectId)
				.Select(member => member.UserId)
				.ToListAsync(cancellationToken);

			var allowedIds = new HashSet<Guid>(memberIds) { project.OwnerId };

			return uniqueUserIds.All(userId => allowedIds.Contains(userId));
		}

		public async Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default)
		{
			await Context.ProjectMembers.AddAsync(member, cancellationToken);
		}

		public void UpdateMember(ProjectMember member)
		{
			Context.ProjectMembers.Update(member);
		}

		public void RemoveMember(ProjectMember member)
		{
			Context.ProjectMembers.Remove(member);
		}
	}
}
