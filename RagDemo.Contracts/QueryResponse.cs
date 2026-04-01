namespace RagDemo.Contracts;

public record QueryResponse(string Answer, IReadOnlyList<string> SourceChunks);
