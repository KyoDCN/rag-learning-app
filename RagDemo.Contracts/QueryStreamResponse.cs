namespace RagDemo.Contracts;

public record QueryStreamResponse(string Delta, IReadOnlyList<string>? SourceChunks);
