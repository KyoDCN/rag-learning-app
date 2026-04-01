namespace RagDemo.Api.DTOs;

public record QueryStreamResponse(string Delta, IReadOnlyList<string>? SourceChunks);
