using System.Collections.Concurrent;

namespace RagDemo.Api.Managers;

public class UserSessionManager
{
    private readonly ConcurrentDictionary<SessionId, UserSession> m_sessions = [];
    private readonly TimeSpan m_expiration;

    public UserSessionManager(IConfiguration config)
    {
        int expirationHours = config.GetValue<int>("UserSession:ExpirationHours", 1);
        m_expiration = TimeSpan.FromHours(expirationHours);
    }

    public bool AddSession(SessionId sessionId)
    {
        return m_sessions.TryAdd(sessionId, new());
    }

    public bool RemoveSession(SessionId sessionId)
    {
        return m_sessions.TryRemove(sessionId, out var _);
    }

    public UserSession GetOrAddSession(SessionId sessionId)
    {
        UserSession session = m_sessions.GetOrAdd(sessionId, (sessionId) => new UserSession());
        
        session.RenewLastAccessedOn();

        return session;
    }

    public UserSession? GetSession(SessionId sessionId)
    {
        UserSession? session = m_sessions.GetValueOrDefault(sessionId);

        if (session is null) return null;

        session.RenewLastAccessedOn();

        return session;
    }

    public void ClearExpiredSessions()
    {
        foreach ((SessionId sessionId, UserSession session) in m_sessions)
        {
            TimeSpan sinceLastAccessed = DateTime.UtcNow - session.LastAccessedOn;

            if (sinceLastAccessed >= m_expiration)
                m_sessions.TryRemove(sessionId, out _);
        }
    }
}