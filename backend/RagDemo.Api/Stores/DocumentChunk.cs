namespace RagDemo.Api.Stores;

public record DocumentChunk(string Text, string DocumentName, float[] Embedding);