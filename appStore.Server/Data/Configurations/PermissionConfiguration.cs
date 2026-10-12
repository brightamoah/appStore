using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
	{
		public void Configure( EntityTypeBuilder<Permission> builder )
		{
			builder.HasIndex( permission => permission.Name )
				.IsUnique();

			builder.Property( permission => permission.Name )
				.IsRequired()
				.HasMaxLength( 150 );

			builder.Property( permission => permission.Description )
				.HasMaxLength( 500 );
		}
	}
}