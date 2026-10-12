using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class BoardColumnConfiguration : IEntityTypeConfiguration<BoardColumn>
	{
		public void Configure(EntityTypeBuilder<BoardColumn> builder)
		{
			builder.HasKey(boardColumn => boardColumn.Id);
            
			builder.Property(boardColumn => boardColumn.Name)
				.IsRequired()
				.HasMaxLength(60);
                
			builder.Property(boardColumn => boardColumn.StatusMapping)
				.IsRequired();
            
			builder.Property(boardColumn => boardColumn.OrderIndex)
				.IsRequired();
            
			builder.Property(boardColumn => boardColumn.WipLimit)
				.IsRequired()
				.HasDefaultValue(0);
            
			builder.Property(boardColumn => boardColumn.Color)
				.HasMaxLength(30);
            
			builder.Property(boardColumn => boardColumn.IsDefault)
				.IsRequired()
				.HasDefaultValue(false);
            
			builder.Property(boardColumn => boardColumn.CreatedAt)
				.IsRequired();
            
			builder.HasOne(boardColumn => boardColumn.Project)
				.WithMany(project => project.Columns)
				.HasForeignKey(boardColumn => boardColumn.ProjectId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasIndex(boardColumn => new { boardColumn.ProjectId, boardColumn.OrderIndex });
		}
	}
}