
using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;

namespace appStore.Server.Data
{
	public class DatabaseContext( DbContextOptions<DatabaseContext> options ) : DbContext( options )
	{
		public DbSet<TaskItem> TaskItems { get; set; }

		public DbSet<Priority> Priorities { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Role> Roles { get; set; }

		public DbSet<UserRole> UserRoles { get; set; }

		public DbSet<Permission> Permissions { get; set; }

		public DbSet<RolePermission> RolePermissions { get; set; }

		public DbSet<RefreshToken> RefreshTokens { get; set; }

		public DbSet<PasswordPolicy> PasswordPolicies { get; set; }

		public DbSet<Project> Projects { get; set; }

		public DbSet<ProjectMember> ProjectMembers { get; set; }

		public DbSet<BoardColumn> BoardColumns { get; set; }

		public DbSet<TaskChecklist> TaskChecklists { get; set; }

		public DbSet<TaskComment> TaskComments { get; set; }

		public DbSet<TaskActivity> TaskActivities { get; set; }

		protected override void OnModelCreating( ModelBuilder modelBuilder )
		{
			modelBuilder.ApplyConfigurationsFromAssembly(
			typeof( DatabaseContext ).Assembly
			);
		}
	}
}