using RagDemo.Api.Managers;

namespace RagDemo.Api.HttpContexts;

public class UserSessionHttpContext(IHttpContextAccessor httpContextAccessor) : IUserSessionHttpContext
{
    public SessionId SessionId
    {
        get
        {
            if (field is not null)
                return field;

            string? xSessionId = httpContextAccessor.HttpContext!.Request.Headers["X-Session-Id"].ToString();

            if (string.IsNullOrWhiteSpace(xSessionId)) 
                throw new ArgumentException("Proper Session ID (Guid) must be provided.");

            if (!Guid.TryParse(xSessionId, out Guid sessionId))
                throw new ArgumentException("Session ID must be a valid Guid.");

            return field = new(sessionId);
        }
    }
}