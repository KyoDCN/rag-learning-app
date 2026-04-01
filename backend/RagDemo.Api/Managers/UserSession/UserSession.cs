using RagDemo.Api.Stores;

namespace RagDemo.Api.Managers;

public class UserSession
{
    public VectorStore VectorStore { get; } = new();
    public DateTime LastAccessedOn { get; private set; } = DateTime.UtcNow;

    public void RenewLastAccessedOn() => LastAccessedOn = DateTime.UtcNow;
}
