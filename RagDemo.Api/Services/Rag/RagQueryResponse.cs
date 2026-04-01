namespace RagDemo.Api.Services.Rag;

public record RagQueryResponse(string Answer, IReadOnlyList<string> SourceChunks);