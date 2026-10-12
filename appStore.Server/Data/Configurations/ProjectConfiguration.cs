using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class ProjectConfiguration : IEntityTypeConfiguration<Project>
	{
		public void Configure( EntityTypeBuilder<Project> builder )
		{
			builder.HasKey( project => project.Id );

			builder.Property( project => project.Name )
				.IsRequired()
				.HasMaxLength( 150 );

			builder.Property( project => project.Description )
				.HasMaxLength( 1000 );

			builder.Property(project => project.Key)
				.IsRequired()
				.HasMaxLength(10)
				.HasDefaultValue("PROJ");

			builder.HasIndex(project => project.Key)
				.IsUnique();

			builder.Property(project => project.TaskCounter)
				.IsRequired()
				.HasDefaultValue(0);

			builder.Property( project => project.Icon )
				.HasMaxLength( 50 );

			builder.Property( project => project.CreatedAt )
				.IsRequired();

			builder.Property( project => project.UpdatedAt )
				.IsRequired();

			
			builder.HasOne( project => project.Owner )
				.WithMany()
				.HasForeignKey( project => project.OwnerId )
				.OnDelete( DeleteBehavior.Restrict );

			builder.HasMany( project => project.Members )
				.WithOne( projectMember => projectMember.Project )
				.HasForeignKey( projectMember => projectMember.ProjectId )
				.OnDelete( DeleteBehavior.Cascade );

			
			builder.HasIndex( project => project.OwnerId );
		}
	}
}