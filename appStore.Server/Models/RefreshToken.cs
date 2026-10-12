using System.ComponentModel.DataAnnotations;

namespace appStore.Server.Models
{
	public class RefreshToken
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
        
        [StringLength(256)]
		public required string TokenHash { get; set; }
        
		public Guid TokenFamilyId { get; set; }
		public bool RememberMe { get; set; }
		public DateTime ExpiresAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? RevokedAt { get; set; }
		public Guid? ReplacedByTokenId { get; set; }
		public DateTime FamilyExpiresAt { get; set; }
		public User User { get; set; } = null!;
	}
}