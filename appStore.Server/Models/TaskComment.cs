using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
    public class TaskComment
    {
        public Guid Id { get; set; }
        public Guid TaskId { get; set; }
        public Guid AuthorId { get; set; }

        [StringLength(1000)]
        public required string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public TaskItem Task { get; set; } = null!;
        public User Author { get; set; } = null!;
    }
}