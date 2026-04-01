namespace RagDemo.Api.Services.Rag;

public record RagQueryStreamResponse(string Delta, IReadOnlyList<string>? SourceChunks);