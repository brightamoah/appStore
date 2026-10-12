using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
	public class TaskItem
	{
		public Guid Id { get; set; }

		[StringLength(20)]
		public string? TaskKey { get; set; }
		public required string Name { get; set; }

		[StringLength(255)]
		public string? Description { get; set; }

		public int PriorityId { get; set; }
		public Status Status { get; set; } = Status.NotStarted;
		public bool IsCompleted { get; set; } = false;
		public IssueType IssueType { get; set; } = IssueType.Task;
        
		public Priority? Priority { get; set; }
		public Guid? ProjectId { get; set; }
		public Guid? ColumnId { get; set; }
		public BoardColumn? Column { get; set; }
		public int OrderIndex { get; set; }

		public Guid? AssigneeId { get; set; }
		public User? Assignee { get; set; }
		public List<Guid> AssigneeIds { get; set; } = [];
		public Guid? ReporterId { get; set; }
		public User? Reporter { get; set; }
        
        public Guid? ParentTaskId { get; set; }
        public TaskItem? ParentTask { get; set; }
        public ICollection<TaskItem> Subtasks { get; set; } = [];
        
        public DateTime? DueDate { get; set; }
        public int? EstimatedMinutes { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
        public ICollection<TaskChecklist> Checklists { get; set; } = [];
        public ICollection<TaskComment> Comments { get; set; } = [];
        public ICollection<TaskActivity> Activities { get; set; } = [];
	}
}