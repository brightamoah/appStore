using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
    public class TaskChecklist
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }

        [StringLength(200)]
        public required string Title { get; set; }
        
        public bool IsCompleted { get; set; } = false;
        public int OrderIndex { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TaskItem Task { get; set; } = null!;
    }
}