using appStore.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace appStore.Server.Data.Configurations
{
	public class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
	{
		public void Configure(EntityTypeBuilder<TaskItem> builder)
		{
			builder.HasKey(task => task.Id);

			builder.Property(task => task.TaskKey)
				.HasMaxLength(30);

			builder.HasIndex(task => task.TaskKey)
				.IsUnique();

			builder.Property(task => task.Name)
				.IsRequired()
				.HasMaxLength(200);

			builder.Property(task => task.Description)
				.HasMaxLength(4000);

			builder.Property(task => task.Status)
				.IsRequired();

			builder.Property(task => task.IssueType)
				.IsRequired()
				.HasDefaultValue(IssueType.Task);

			builder.Property(task => task.OrderIndex)
				.IsRequired()
				.HasDefaultValue(0);

			builder.Property(task => task.CreatedAt)
				.IsRequired();

			builder.Property(task => task.UpdatedAt)
				.IsRequired();

			builder.HasOne(task => task.Column)
				.WithMany(column => column.Tasks)
				.HasForeignKey(task => task.ColumnId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasOne(task => task.Assignee)
				.WithMany()
				.HasForeignKey(t => t.AssigneeId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.Property(task => task.AssigneeIds)
				.IsRequired();

			builder.HasOne(task => task.Reporter)
				.WithMany()
				.HasForeignKey(task => task.ReporterId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasOne<Project>()
				.WithMany()
				.HasForeignKey(task => task.ProjectId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasOne(task => task.ParentTask)
				.WithMany(parent => parent.Subtasks)
				.HasForeignKey(task => task.ParentTaskId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.HasIndex(task => new
			{
				task.ProjectId, 
				task.ColumnId, 
				task.OrderIndex
			});

			builder.HasIndex(task => task.AssigneeId);

			builder.HasIndex(task => task.Status);
		}
	}
}