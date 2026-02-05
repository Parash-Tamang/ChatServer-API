namespace AIChatbot.Domain.Entities;


/// Represents a refresh token for JWT authentication

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

  
    /// User to whom this token belongs
   
    public string UserId { get; set; } = default!;

   
    /// Hashed refresh token value
  
    public string TokenHash { get; set; } = default!;

   
    /// Expiration timestamp (UTC)
  
    public DateTime ExpiresAt { get; set; }

   
    /// Indicates whether the token has been revoked
  
    public bool IsRevoked { get; set; }

    
    /// Token creation timestamp (UTC)
  
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
}
