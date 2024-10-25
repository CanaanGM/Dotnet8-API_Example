

using Microsoft.AspNetCore.Identity;

namespace Core.Identity;
public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpirationDateTime { get; set; }

}
