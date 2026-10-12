using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
	public class Project
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;

		[StringLength(10)]
		public string Key { get; set; } = "PROJ";
		public string? Description { get; set; }
		public string? Icon { get; set; }
		public Guid OwnerId { get; set; }
		public int TaskCounter { get; set; } = 0;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
        
		public User Owner { get; set; } = null!;
		public ICollection<ProjectMember> Members { get; set; } = [];
		public ICollection<BoardColumn> Columns { get; set; } = [];
        
		// Tasks are accessed via Columns or queried directly by ProjectId:
		// public ICollection<TaskItem> Tasks { get; set; } = [];
	}
}