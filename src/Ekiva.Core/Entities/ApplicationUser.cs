using Microsoft.AspNetCore.Identity;

namespace Ekiva.Core.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Relations
    public Guid? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
    
    public Guid? SubsidiaryId { get; set; }
    public virtual Subsidiary? Subsidiary { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";
}
