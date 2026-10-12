using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class PriorityConfiguration : IEntityTypeConfiguration<Priority>
	{
		public void Configure( EntityTypeBuilder<Priority> builder )
		{
			builder.HasKey( priority => priority.Id );

			builder.HasIndex( priority => priority.Name )
				.IsUnique();

			builder.Property( priority => priority.Name )
				.IsRequired()
				.HasMaxLength( 50 );
		}
	}
}