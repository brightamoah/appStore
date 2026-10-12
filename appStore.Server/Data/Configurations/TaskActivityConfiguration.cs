using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class TaskActivityConfiguration : IEntityTypeConfiguration<TaskActivity>
	{
		public void Configure(EntityTypeBuilder<TaskActivity> builder)
		{
			builder.HasKey(taskActivity => taskActivity.Id);
            
			builder.Property(taskActivity => taskActivity.ActionType)
				.IsRequired()
				.HasMaxLength(50);
                
			builder.Property(taskActivity => taskActivity.OldValue)
				.HasMaxLength(500);

			builder.Property(taskActivity => taskActivity.NewValue)
				.HasMaxLength(500);
			
			builder.Property(taskActivity => taskActivity.CreatedAt).IsRequired();
			
			builder.HasOne(taskActivity => taskActivity.Task)
				.WithMany(task => task.Activities)
				.HasForeignKey(taskActivity => taskActivity.TaskId)
				.OnDelete(DeleteBehavior.Cascade);
            
			builder.HasOne(taskActivity => taskActivity.Actor)
				.WithMany()
				.HasForeignKey(taskActivity => taskActivity.ActorId)
				.OnDelete(DeleteBehavior.Restrict);
			
			builder.HasIndex(taskActivity => new { taskActivity.TaskId, taskActivity.CreatedAt });
		}
	}
}