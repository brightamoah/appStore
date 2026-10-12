using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
	public class User
	{
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public required string Email { get; set; }
        
		[StringLength(256)]
		public required string PasswordHash { get; set; }
        
		[StringLength(15)]
		public string? PhoneNumber { get; set; }
        
		public bool IsEmailVerified { get; set; } = false;
		public bool IsUserActive { get; set; } = true;
        
        [StringLength(500)]
		public string? ProfilePictureUrl { get; set; }
        
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? LastLoginAt { get; set; }
        
		public ICollection<UserRole> UserRoles { get; set; } = [];
		public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
        
        
		// Inverse collections commented out to eliminate dependency loops (queried directly via DbContext):
		// public ICollection<Project> OwnedProjects { get; set; } = [];
		// public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
        
        // public ICollection<TaskItem> AssignedTasks { get; set; } = [];
        // public ICollection<TaskItem> ReportedTasks { get; set; } = [];
        // public ICollection<TaskComment> Comments { get; set; } = [];
        // public ICollection<TaskActivity> Activities { get; set; } = [];
	}
}