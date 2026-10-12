using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
	{
		public void Configure(EntityTypeBuilder<ProjectMember> builder)
		{
			builder.HasKey(
			projectMember => new
			{
				projectMember.ProjectId,
				projectMember.UserId
			});

			builder.Property(projectMember => projectMember.Role)
				.IsRequired()
				.HasDefaultValue(ProjectRole.Member)
				.HasSentinel(ProjectRole.None);


			builder.Property(projectMember => projectMember.JoinedAt)
				.IsRequired();

			builder.HasOne(projectMember => projectMember.Project)
				.WithMany(project => project.Members)
				.HasForeignKey(projectMember => projectMember.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasOne(projectMember => projectMember.User)
				.WithMany()
				.HasForeignKey(projectMember => projectMember.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}
}