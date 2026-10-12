using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
	{
		public void Configure(EntityTypeBuilder<TaskComment> builder)
		{
			builder.HasKey(taskComment => taskComment.Id);
            
			builder.Property(taskComment => taskComment.Content)
				.IsRequired()
				.HasMaxLength(4000);
			
			builder.Property(taskComment => taskComment.CreatedAt).IsRequired();
            
			builder.Property(taskComment => taskComment.UpdatedAt).IsRequired();
			
			builder.HasOne(taskComment => taskComment.Task)
				.WithMany(task => task.Comments)
				.HasForeignKey(taskComment => taskComment.TaskId)
				.OnDelete(DeleteBehavior.Cascade);
                
			builder.HasOne(taskComment => taskComment.Author)
				.WithMany()
				.HasForeignKey(taskComment => taskComment.AuthorId)
				.OnDelete(DeleteBehavior.Restrict);
                
			builder.HasIndex(taskComment => new { taskComment.TaskId, taskComment.CreatedAt });
		}
	}
}