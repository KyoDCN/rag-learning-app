using RagDemo.Api.Managers;

namespace RagDemo.Api.HostedServices;

public class UserSessionExpiryService : BackgroundService
{
    private readonly UserSessionManager m_sessionManager;
    private readonly ILogger<UserSessionExpiryService> m_logger;

    public UserSessionExpiryService(UserSessionManager sessionManager, ILogger<UserSessionExpiryService> logger)
    {
        m_sessionManager = sessionManager;
        m_logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new (TimeSpan.FromMinutes(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            m_logger.LogInformation("Running expired session cleanup");

            m_sessionManager.ClearExpiredSessions();
            
            m_logger.LogInformation("Expired session cleanup complete");
        }
    }
}