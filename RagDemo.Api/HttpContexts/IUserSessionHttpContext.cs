using RagDemo.Api.Managers;

namespace RagDemo.Api.HttpContexts;

public interface IUserSessionHttpContext
{
    SessionId SessionId { get; }
}
