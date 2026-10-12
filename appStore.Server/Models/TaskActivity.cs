using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
    public class TaskActivity
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid ActorId { get; set; }
        public required ActionType ActionType { get; set; }

        [StringLength(1000)]
        public string? OldValue { get; set; }
        
        [StringLength(1000)]
        public string? NewValue { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public TaskItem Task { get; set; } = null!;
        public User Actor { get; set; } = null!;
    }
}
