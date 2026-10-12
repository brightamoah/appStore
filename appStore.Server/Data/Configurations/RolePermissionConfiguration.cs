using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
	{
		public void Configure( EntityTypeBuilder<RolePermission> builder )
		{
			builder.HasKey( rolePermission => new
			{
				rolePermission.RoleId,
				rolePermission.PermissionId
			} );

			builder.HasOne( rolePermission => rolePermission.Role )
			   .WithMany( role => role.RolePermissions )
			   .HasForeignKey( rolePermission => rolePermission.RoleId )
			   .OnDelete( DeleteBehavior.Cascade );

			builder.HasOne( rolePermission => rolePermission.Permission )
				.WithMany( permission => permission.RolePermissions )
				.HasForeignKey( rolePermission => rolePermission.PermissionId )
				.OnDelete( DeleteBehavior.Restrict );
		}
	}
}