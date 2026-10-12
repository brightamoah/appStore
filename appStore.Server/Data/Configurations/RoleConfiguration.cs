using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class RoleConfiguration : IEntityTypeConfiguration<Role>
	{
		public void Configure( EntityTypeBuilder<Role> builder )
		{
			builder.HasIndex( role => role.Name )
				.IsUnique();

			builder.Property( role => role.Name )
				.IsRequired()
				.HasMaxLength( 100 );

			builder.Property( role => role.Description )
				.HasMaxLength( 500 );

			builder.HasMany( role => role.UserRoles )
				.WithOne( userRole => userRole.Role )
				.HasForeignKey( userRole => userRole.RoleId )
				.OnDelete( DeleteBehavior.Restrict );

			builder.HasMany( role => role.RolePermissions )
				.WithOne( rolePermission => rolePermission.Role )
				.HasForeignKey( rolePermission => rolePermission.RoleId )
				.OnDelete( DeleteBehavior.Cascade );
		}
	}
}