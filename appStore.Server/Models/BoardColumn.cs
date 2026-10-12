using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
    public class BoardColumn
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public int OrderIndex { get; set; }
        public Status StatusMapping { get; set; }
        public int WipLimit { get; set; }

        
        public NuxtUiColor? Color { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = [];


        public static List<BoardColumn> CreateDefaultColumns(Guid projectId) =>
        [
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Backlog",
                OrderIndex = 0,
                StatusMapping = Status.NotStarted,
                WipLimit = 0,
                Color = NuxtUiColor.Neutral,
                IsDefault = true
            },

            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "In Progress",
                OrderIndex = 1,
                StatusMapping = Status.InProgress,
                WipLimit = 5,
                Color = NuxtUiColor.Info,
                IsDefault = true
            },

            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Review",
                OrderIndex = 2,
                StatusMapping = Status.UnderReview,
                WipLimit = 3,
                Color = NuxtUiColor.Warning,
                IsDefault = true
            },

            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = "Done",
                OrderIndex = 3,
                StatusMapping = Status.Completed,
                WipLimit = 0,
                Color = NuxtUiColor.Success,
                IsDefault = true
            }
        ];
    }
}