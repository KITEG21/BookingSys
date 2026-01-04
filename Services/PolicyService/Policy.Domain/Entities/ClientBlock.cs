namespace Policy.Domain.Entities;

public class ClientBlock
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public Guid ClientId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public DateTime BlockedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    private ClientBlock() { }

    public ClientBlock(Guid clientId, string reason, DateTime? expiresAt = null)
    {
        ClientId = clientId;
        Reason = reason;
        BlockedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
