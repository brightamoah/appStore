using appStore.Server.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class TaskChecklistConfiguration : IEntityTypeConfiguration<TaskChecklist>
	{
		public void Configure(EntityTypeBuilder<TaskChecklist> builder)
		{
			builder.HasKey(taskChecklist => taskChecklist.Id);
            
			builder.Property(taskChecklist => taskChecklist.Title)
				.IsRequired()
				.HasMaxLength(250);
            
			builder.Property(taskChecklist => taskChecklist.IsCompleted)
				.IsRequired()
				.HasDefaultValue(false);
            
			builder.Property(taskChecklist => taskChecklist.OrderIndex)
				.IsRequired()
				.HasDefaultValue(0);
            
			builder.Property(taskChecklist => taskChecklist.CreatedAt).IsRequired();
            
			builder.HasOne(taskChecklist => taskChecklist.Task)
				.WithMany(task => task.Checklists)
				.HasForeignKey(taskChecklist => taskChecklist.TaskId)
				.OnDelete(DeleteBehavior.Cascade);
            
			builder.HasIndex(taskChecklist => new { taskChecklist.TaskId, taskChecklist.OrderIndex });
		}
	}
}