namespace RagDemo.Api.DTOs;

public record QueryResponse(string Answer, IReadOnlyList<string> SourceChunks);
